using Avalonia;
using Avalonia.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.Dialogs
{
    public partial class ZXNewFileDialog : Window
    {
        static StyledProperty<string?> CurrentDocumentDescriptionProperty = StyledProperty<string?>.Register<ZXNewFileDialog, string?>("CurrentDocumentDescription");
        static StyledProperty<bool> HasFileNameProperty = StyledProperty<bool>.Register<ZXNewFileDialog, bool>("HasFileName");
        public ObservableCollection<string> CurrentCategories { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<IZXDocumentType> CurrentDocuments { get; private set; } = new ObservableCollection<IZXDocumentType>();
        public string? CurrentDocumentDescription 
        {
            get => GetValue(CurrentDocumentDescriptionProperty);
            set => SetValue(CurrentDocumentDescriptionProperty, value);
        }

        public bool HasFileName
        {
            get => GetValue(HasFileNameProperty);
            set => SetValue(HasFileNameProperty, value);
        }

        public ZXNewFileDialog()
        {
            DataContext = this;
            InitializeComponent();
            lstCategories.SelectionChanged += LstCategories_SelectionChanged;
            lstDocumentTypes.SelectionChanged += LstDocumentTypes_SelectionChanged;
            txtName.TextChanged += TxtName_TextChanged;
            btnAccept.Click += BtnAccept_Click;
            btnCancel.Click += BtnCancel_Click;
            UpdateCategories();
            UpdateDocuments();
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(null);
        }

        private async void BtnAccept_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var item = lstDocumentTypes.SelectedItem as IZXDocumentType;

            if (item == null)
            {
                await this.ShowError("No type selected", "You must select a document type in order to create a new file.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text)) 
            {
                await this.ShowError("Missing name", "You must provide a file name in order to create it.");
                return;
            }

            string name = txtName.Text;
            string ext = Path.GetExtension(name);

            if (string.IsNullOrWhiteSpace(ext) || !item.DocumentExtensions.Any(ex => ex.ToLower() == ext.ToLower()))
                name += item.DocumentExtensions.FirstOrDefault();

            Close((item, name));

        }

        private void TxtName_TextChanged(object? sender, TextChangedEventArgs e)
        {
            HasFileName = !string.IsNullOrWhiteSpace(txtName.Text);
        }

        private void LstDocumentTypes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var item = lstDocumentTypes.SelectedItem as IZXDocumentType;

            if (item == null)
            {
                CurrentDocumentDescription = null;
                txtName.Text = "";
            }
            else
            {
                CurrentDocumentDescription = item.DocumentDescription;

                if (string.IsNullOrWhiteSpace(txtName.Text))
                    txtName.Text = "NewDocument" + item.DocumentExtensions.FirstOrDefault();
                else
                {
                    string name = Path.GetFileNameWithoutExtension(txtName.Text);
                    txtName.Text = name + item.DocumentExtensions.FirstOrDefault();
                }
            }
        }

        private void LstCategories_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateDocuments();
        }

        void UpdateCategories()
        {
            CurrentCategories.Clear();
            var cats = ZXDocumentProvider.GetDocumentCategories();
            foreach(var cat in cats)
                CurrentCategories.Add(cat);
            lstCategories.SelectedIndex = 0;
        }

        void UpdateDocuments()
        {
            CurrentDocuments.Clear();

            var category = lstCategories.SelectedItem as string;

            if (category == null)
                return;

            var docs = ZXDocumentProvider.GetDocumentsInCategory(category).Where(d => d.CanCreate);

            foreach(var doc in docs)
                CurrentDocuments.Add(doc);
        }
    }
}
