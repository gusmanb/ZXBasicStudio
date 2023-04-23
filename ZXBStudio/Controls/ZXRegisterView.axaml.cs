using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ExCSS;
using System;
using System.Globalization;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXRegisterView : UserControl
    {
        public static StyledProperty<ZXRegister?> ItemProperty = StyledProperty<ZXRegister?>.Register<ZXRegisterView, ZXRegister?>("Item", null);
        public static StyledProperty<bool> AllowEditProperty = StyledProperty<bool>.Register<ZXRegisterView, bool>("AllowEdit", false);

        public bool AllowEdit
        {
            get { return GetValue<bool>(AllowEditProperty); }
            set { SetValue<bool>(AllowEditProperty, value); }
        }

        public event EventHandler<EventArgs>? ValueChanged;

        public ZXRegister? Item
        {
            get { return GetValue<ZXRegister?>(ItemProperty); }
            set { SetValue<ZXRegister?>(ItemProperty, value); }
        }

        public ZXRegisterView()
        {
            DataContext = this;
            InitializeComponent();
            txtName.PointerReleased += TxtName_PointerReleased;
            txtEdit.KeyUp += TxtEdit_KeyUp;
            txtEdit.LostFocus += TxtEdit_LostFocus;
        }

        private void TxtEdit_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                if (Item?.IsByte ?? false)
                {
                    byte value;

                    if (!byte.TryParse(txtEdit.Text, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                    {
                        txtEdit.Background = Brushes.MistyRose;
                        return;
                    }
                    Item?.SetValue(value);
                }
                else
                {
                    ushort value;

                    if (!ushort.TryParse(txtEdit.Text, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                    {
                        txtEdit.Background = Brushes.MistyRose;
                        return;
                    }
                    Item?.SetValue(value);
                }
                
                brdEdit.IsVisible = false;

                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
            else if (e.Key == Avalonia.Input.Key.Escape)
            { brdEdit.IsVisible = false; }
            else
            {
                txtEdit.Background = Brushes.White;
            }
        }

        private void TxtEdit_LostFocus(object? sender, RoutedEventArgs e)
        {
            brdEdit.IsVisible = false;
        }

        private void TxtName_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (AllowEdit)
            {
                txtEdit.Text = Item?.Hex;
                txtEdit.Background = Brushes.White;
                brdEdit.IsVisible = true;
                txtEdit.Focus();
            }
        }
    }
}
