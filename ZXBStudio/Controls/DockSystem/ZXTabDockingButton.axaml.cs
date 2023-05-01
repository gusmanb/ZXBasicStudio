using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace ZXBasicStudio.Controls.DockSystem
{
    public partial class ZXTabDockingButton : UserControl
    {
        public static StyledProperty<bool> IsSelectedProperty = StyledProperty<bool>.Register<ZXTabDockingButton, bool>("IsSelected");
        public static StyledProperty<IBrush> SelectedBackgroundProperty = StyledProperty<bool>.Register<ZXTabDockingButton, IBrush>("SelectedBackground", SolidColorBrush.Parse("#444444"));
        public static StyledProperty<ZXTabDockingButtonPosition> ButtonPositionProperty = StyledProperty<ZXTabDockingButtonPosition>.Register<ZXTabDockingButton, ZXTabDockingButtonPosition>("ButtonPosition");
        public static StyledProperty<ZXDockingControl?> AssociatedControlProperty = StyledProperty<ZXDockingControl?>.Register<ZXTabDockingButton, ZXDockingControl?>("AssociatedControl");
        public static StyledProperty<string> TitleProperty = StyledProperty<string>.Register<ZXTabDockingButton, string>("Title", "Title");
        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public IBrush SelectedBackground
        {
            get => GetValue(SelectedBackgroundProperty);
            set => SetValue(SelectedBackgroundProperty, value);
        }
        public ZXTabDockingButtonPosition ButtonPosition
        {
            get => GetValue(ButtonPositionProperty);
            set => SetValue(ButtonPositionProperty, value);
        }
        public ZXDockingControl? AssociatedControl 
        {
            get => GetValue(AssociatedControlProperty);
            set => SetValue(AssociatedControlProperty, value);
        }
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public event EventHandler<EventArgs>? Click;
        public event EventHandler<EventArgs>? Close;
        public ZXTabDockingButton()
        {
            DataContext = this;
            InitializeComponent();
            AddHandler(PointerReleasedEvent, OnClick, Avalonia.Interactivity.RoutingStrategies.Direct | Avalonia.Interactivity.RoutingStrategies.Bubble | Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            AddHandler(PointerMovedEvent, (sender, e) =>
            {
                if (AssociatedControl == null)
                    return;

                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    DataObject d = new DataObject();
                    d.Set("DockedControl", AssociatedControl);
                    DragDrop.DoDragDrop(e, d, DragDropEffects.Move);
                }
            });
            mnuFloat.Click += MnuFloat_Click;
            btnClose.Click += BtnClose_Click;
        }

        private void BtnClose_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Close != null)
                Close(this, EventArgs.Empty);
        }

        private void MnuFloat_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(AssociatedControl != null)
                ZXFloatController.MakeFloating(AssociatedControl);
        }

        private void OnClick(object? sender, PointerReleasedEventArgs e)
        {
            if (Click != null)
                Click(this, EventArgs.Empty);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsSelectedProperty ||
                change.Property == SelectedBackgroundProperty ||
                change.Property == BackgroundProperty ||
                change.Property == ButtonPositionProperty)
                UpdateAppearance();
            else if (change.Property == AssociatedControlProperty)
            {
                if (change.NewValue != null)
                {
                    ZXDockingControl? ctl = change.NewValue as ZXDockingControl;
                    if (ctl != null)
                        ctl.PropertyChanged += DockTitleChanged;
                }

                if (change.OldValue != null)
                {
                    ZXDockingControl? ctl = change.OldValue as ZXDockingControl;
                    if (ctl != null)
                        ctl.PropertyChanged -= DockTitleChanged;
                }
            }
        }
        void DockTitleChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            Title = AssociatedControl?.Title ?? "";
        }
        void UpdateAppearance()
        {
            if (!IsSelected)
            {
                rectBottom.IsVisible = false;
                rectTop.IsVisible = false;
                grdMain.Background = Background ?? Brushes.Transparent;
                txtButton.FontWeight = FontWeight.Regular;
            }
            else
            {
                if (ButtonPosition == ZXTabDockingButtonPosition.Bottom)
                {
                    rectBottom.IsVisible = true;
                    rectTop.IsVisible = false;
                }
                else
                {
                    rectBottom.IsVisible = false;
                    rectTop.IsVisible = true;
                }

                grdMain.Background = SelectedBackground;
                txtButton.FontWeight = FontWeight.ExtraBold;
            }
        }
    }
    public enum ZXTabDockingButtonPosition
    {
        Top,
        Bottom
    }
}
