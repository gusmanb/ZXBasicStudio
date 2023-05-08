using Avalonia;
using Avalonia.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.Dialogs
{
    public partial class ZXNewFileDialog : Window
    {
        static StyledProperty<string?> CurrentDocumentDescriptionProperty = StyledProperty<string?>.Register<ZXNewFileDialog, string?>("CurrentDocumentDescription");
        public ObservableCollection<string> CurrentCategories { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<IZXDocumentType> CurrentDocuments { get; private set; } = new ObservableCollection<IZXDocumentType>();
        public string? CurrentDocumentDescription 
        {
            get => GetValue(CurrentDocumentDescriptionProperty);
            set => SetValue(CurrentDocumentDescriptionProperty, value);
        }

        public ZXNewFileDialog()
        {
            DataContext = this;
            InitializeComponent();
            lstCategories.SelectionChanged += LstCategories_SelectionChanged;
            lstDocumentTypes.SelectionChanged += LstDocumentTypes_SelectionChanged;
            UpdateCategories();
            UpdateDocuments();
        }

        private void LstDocumentTypes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var item = lstDocumentTypes.SelectedItem as IZXDocumentType;

            if (item == null)
                CurrentDocumentDescription = null;
            else
                CurrentDocumentDescription = item.DocumentDescription;
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
