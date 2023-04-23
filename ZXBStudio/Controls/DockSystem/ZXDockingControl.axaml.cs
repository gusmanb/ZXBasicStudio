using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace ZXBasicStudio.Controls.DockSystem
{
    public partial class ZXDockingControl : UserControl
    {
        public static StyledProperty<string> TitleProperty = StyledProperty<string>.Register<ZXDockingControl, string>("Title", "Title");
        public static StyledProperty<Control?> DockedControlProperty = StyledProperty<Control?>.Register<ZXDockingControl, Control?>("DockedControl");
        public static StyledProperty<bool> CanCloseProperty = StyledProperty<bool>.Register<ZXDockingControl, bool>("CanClose", false);
        public static StyledProperty<bool> TabModeProperty = StyledProperty<bool>.Register<ZXDockingControl, bool>("TabMode", false);
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public Control? DockedControl
        {
            get => GetValue(DockedControlProperty);
            set => SetValue(DockedControlProperty, value);
        }
        public bool CanClose
        {
            get => GetValue(CanCloseProperty);
            set => SetValue(CanCloseProperty, value);
        }
        public bool TabMode
        {
            get => GetValue(TabModeProperty);
            set => SetValue(TabModeProperty, value);
        }

        public event EventHandler<CloseEventArgs>? Closing;

        public ZXDockingControl()
        {
            InitializeComponent();
            DataContext = this;
            grip.AddHandler(PointerMovedEvent, (sender, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    DataObject d = new DataObject();
                    d.Set("DockedControl", this);
                    DragDrop.DoDragDrop(e, d, DragDropEffects.Move);
                }
            });
            mnuFloat.Click += MnuFloat_Click;
            btnClose.Click += Close;
        }

        private void MnuFloat_Click(object? sender, RoutedEventArgs e)
        {
            ZXFloatController.MakeFloating(this);
        }

        private void Close(object? sender, RoutedEventArgs e)
        {
            if (this.Parent is not IZXDockingContainer)
                return;

            if (Closing != null)
            {
                var args = new CloseEventArgs();
                Closing(this, args);

                if (args.Cancel)
                    return;

                (this.Parent as IZXDockingContainer)?.Remove(this);
            }
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == TitleProperty)
                titleBlock.Text = change.NewValue as string;
            else if (change.Property == DockedControlProperty)
                content.Content = change.NewValue;

            base.OnPropertyChanged(change);
        }
        internal void RequestClose()
        {
            Close(null, null);
        }
        public void Select()
        {
            (Parent as IZXDockingContainer)?.Select(this);
        }
    }

    public class CloseEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }
}
