using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace ZXBasicStudio.DocumentEditors.NextDows
{
    public partial class ZXFormsControl : UserControl
    {
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set{
                _IsSelected = value;
                Refresh();
            }
        }

        private bool _IsSelected = false;
        private bool IsHover = false;

        private static IBrush color_Normal = new SolidColorBrush(new Color(255, 32, 32, 32));
        private static IBrush color_Hover = new SolidColorBrush(new Color(255, 48, 48, 48));
        private static IBrush color_Selected = new SolidColorBrush(new Color(255, 64, 64, 64));

        private Action<ZXFormsControl, string> callBackCommand = null;


        public ZXFormsControl()
        {
            InitializeComponent();

            this.PointerEntered += ZXFormsControl_PointerEntered;
            this.PointerExited += ZXFormsControl_PointerExited;
            this.Tapped += ZXFormsControl_Tapped;
        }


        private void ZXFormsControl_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            IsSelected = true;
            callBackCommand(this, "SELECTED");
        }


        private void ZXFormsControl_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            IsHover = false;
            Refresh();
        }


        private void ZXFormsControl_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            IsHover = true;
            Refresh();
        }


        public void Initialize(string imageName, string title, string description, Action<ZXFormsControl,string> callBackCommand)
        {
            try
            {
                this.callBackCommand = callBackCommand;

                txtTitle.Text= title;
                txtDescription.Text= description;

                var assetLoader = new AssetLoader();
                var bitmap = new Bitmap(assetLoader.Open(new Uri("avares://ZXBasicStudio/DocumentEditors/NextDows/images/"+imageName)));
                imgImage.Source = bitmap;
                IsHover = false;
                _IsSelected = false;
                Refresh();
            }
            catch(Exception ex)
            {

            }
        }


        public void Refresh()
        {
            if (_IsSelected)
            {
                this.Background = color_Selected;
            }else if (IsHover)
            {
                this.Background = color_Hover;
            }
            else
            {
                this.Background = color_Normal;
            }
        }


        public void Resize(double size)
        {
            this.Width = size;            
        }
    }
}
