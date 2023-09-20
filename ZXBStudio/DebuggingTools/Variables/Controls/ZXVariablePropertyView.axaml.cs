using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Konamiman.Z80dotNet;
using System.Globalization;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Tmds.DBus;
using Avalonia.Media;
using CoreSpectrum.Hardware;
using ZXBasicStudio.BuildSystem;

namespace ZXBasicStudio.DebuggingTools.Variables.Controls
{
    public partial class ZXVariablePropertyView : UserControl
    {
        public static StyledProperty<string> SvgPathProperty = StyledProperty<string>.Register<ZXVariablePropertyView, string>("SvgPath");
        public static StyledProperty<string> PropertyNameProperty = StyledProperty<string>.Register<ZXVariablePropertyView, string>("PropertyName");
        public static StyledProperty<string> PropertyTypeProperty = StyledProperty<string>.Register<ZXVariablePropertyView, string>("PropertyType");
        public static StyledProperty<string> PropertyValueProperty = StyledProperty<string>.Register<ZXVariablePropertyView, string>("PropertyValue");
        public static StyledProperty<bool> EditableProperty = StyledProperty<bool>.Register<ZXVariablePropertyView, bool>("Editable");
        public static StyledProperty<ZXVariable> VariableProperty = StyledProperty<ZXVariable>.Register<ZXVariablePropertyView, ZXVariable>("Variable");
        public static StyledProperty<int[]> ArrayPathProperty = StyledProperty<int[]>.Register<ZXVariablePropertyView, int[]>("ArrayPath");
        public static StyledProperty<IMemory> MemoryProperty = StyledProperty<IMemory>.Register<ZXVariablePropertyView, IMemory>("Memory");
        public static StyledProperty<IZ80Registers> RegistersProperty = StyledProperty<IZ80Registers>.Register<ZXVariablePropertyView, IZ80Registers>("Registers");
        public string SvgPath
        {
            get { return GetValue<string>(SvgPathProperty); }
            set { SetValue<string>(SvgPathProperty, value); }
        }
        public string PropertyName
        {
            get { return GetValue<string>(PropertyNameProperty); }
            set { SetValue<string>(PropertyNameProperty, value); }
        }
        public string PropertyType
        {
            get { return GetValue<string>(PropertyTypeProperty); }
            set { SetValue<string>(PropertyTypeProperty, value); }
        }
        public string PropertyValue
        {
            get { return GetValue<string>(PropertyValueProperty); }
            set { SetValue<string>(PropertyValueProperty, value); }
        }
        public bool Editable
        {
            get { return GetValue<bool>(EditableProperty); }
            set { SetValue<bool>(EditableProperty, value); }
        }
        public required ZXVariable Variable
        {
            get { return GetValue<ZXVariable>(VariableProperty); }
            set { SetValue<ZXVariable>(VariableProperty, value); }
        }
        public int[] ArrayPath
        {
            get { return GetValue<int[]>(ArrayPathProperty); }
            set { SetValue<int[]>(ArrayPathProperty, value); }
        }
        public IMemory Memory
        {
            get { return GetValue<IMemory>(MemoryProperty); }
            set { SetValue<IMemory>(MemoryProperty, value); }
        }
        public IZ80Registers Registers
        {
            get { return GetValue<IZ80Registers>(RegistersProperty); }
            set { SetValue<IZ80Registers>(RegistersProperty, value); }
        }
        bool editing = false;

        public ZXVariablePropertyView()
        {
            DataContext = this;
            AddHandler<PointerPressedEventArgs>(PointerPressedEvent, CheckPressed, handledEventsToo: true);
            PropertyChanged += ZXVariablePropertyView_PropertyChanged;
            InitializeComponent();
            txtEdit.KeyUp += TxtEdit_KeyUp; ;
            txtEdit.LostFocus += TxtEdit_LostFocus;
        }
        protected void CheckPressed(object? sender, PointerPressedEventArgs e)
        {
            if (Editable && e.ClickCount == 2 && !editing)
            {
                brdEdit.IsVisible = true;
                editing = true;
                e.Handled = true;
                txtEdit.Text = PropertyType == "STR" ? PropertyValue.Substring(1, PropertyValue.Length - 2) : PropertyValue;
                txtEdit.Focus();
            }
        }
        private void TxtEdit_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            editing = false;
            brdEdit.IsVisible = false;
        }

        private void TxtEdit_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                bool parsed = false;
                object? finalValue = null;

                switch (Variable.StorageType)
                {
                    case ZXVariableStorage.I8:
                        {
                            sbyte value;
                            parsed = sbyte.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.U8:
                    case ZXVariableStorage.LA8:
                        {
                            byte value;
                            parsed = byte.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.I16:
                        {
                            short value;
                            parsed = short.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.U16:
                    case ZXVariableStorage.LA16:
                        {
                            ushort value;
                            parsed = ushort.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.I32:
                        {
                            int value;
                            parsed = int.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.U32:
                    case ZXVariableStorage.LA32:
                        {
                            uint value;
                            parsed = uint.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.F16:
                        {
                            float value;
                            parsed = float.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.F:
                    case ZXVariableStorage.LAF:
                        {
                            double value;
                            parsed = double.TryParse(txtEdit.Text, out value);
                            if (parsed)
                                finalValue = value;
                        }
                        break;
                    case ZXVariableStorage.STR:
                        {
                            finalValue = txtEdit.Text ?? "";
                            parsed = true;
                        }
                        break;
                }

                if (!parsed || finalValue == null)
                {
                    txtEdit.Background = Brushes.MistyRose;
                    return;
                }

                if (Variable.VariableType == ZXVariableType.Flat)
                    Variable.SetValue(Memory, Registers, finalValue);
                else
                    Variable.SetArrayValue(Memory, Registers, ArrayPath, finalValue);

                PropertyValue = PropertyType == "STR" ? $"\"{finalValue}\"" : finalValue.ToString() ?? "";

                brdEdit.IsVisible = false;
                editing = false;
            }
            else if (e.Key == Avalonia.Input.Key.Escape)
            {
                brdEdit.IsVisible = false;
                editing = false;
            }
            else
            {
                txtEdit.Background = Brushes.White;
            }
        }

        private void ZXVariablePropertyView_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == EditableProperty.Name)
            {
                if (!Editable)
                {
                    brdEdit.IsVisible = false;
                    editing = false;
                }
            }
        }
    }
}
