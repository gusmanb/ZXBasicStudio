using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Konamiman.Z80dotNet;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZXBasicStudio.BuildSystem;

namespace ZXBasicStudio.DebuggingTools.Variables.Controls
{
    public partial class ZXVariablesView : UserControl
    {
        const string globalVarBG = "#314158";
        const string localVarBG = "#4a353e";
        const string localParamBG = "#3a4a35";

        private ObservableCollection<ZXVariablePropertyModel> variables { get; set; } = new ObservableCollection<ZXVariablePropertyModel>();

        ZXVariableMap? map;
        IMemory? mem;
        IZ80Registers? regs;

        public ZXVariablesView()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void Initialize(ZXVariableMap Map, IMemory Memory, IZ80Registers Registers)
        {
            map = Map;
            mem = Memory;
            regs = Registers;
        }

        public void BeginEdit()
        {
            if (map == null || mem == null || regs == null)
                return;

            var headerModel = new ZXVariablePropertyModel { Variable = null };
            headerModel.Background = SolidColorBrush.Parse("#404040");
            headerModel.PropertyName= "Name";
            headerModel.PropertyValue = "Value";
            headerModel.PropertyType = "Type";
            variables.Add(headerModel);

            var vars = map.VariablesInRange(regs.PC).OrderBy(v => v.Scope.ScopeName == "GLOBAL" ? 2 : v.IsParameter ? 0 : 1).ThenBy(v => v.Name);

            foreach (var variable in vars)
            {
                ZXVariablePropertyModel model = new ZXVariablePropertyModel { Variable = variable };
                model.PropertyName = variable.Name;

                if (variable.VariableType == ZXVariableType.Flat)
                {
                    model.SvgPath = "/Svg/White/box-solid.svg";
                    model.PropertyValue = variable.GetValue(mem, regs)?.ToString() ?? "{null}";
                    model.PropertyType = variable.StorageType.ToString();
                    model.Editable = true;
                    model.Memory = mem;
                    model.Registers = regs;

                    if (variable.Scope == ZXVariableScope.GlobalScope)
                        model.Background = SolidColorBrush.Parse(globalVarBG);
                    else if(variable.IsParameter)
                        model.Background = SolidColorBrush.Parse(localParamBG);
                    else
                        model.Background = SolidColorBrush.Parse(localVarBG);

                    variables.Add(model);
                }
                else
                {
                    var descriptor = variable.GetArrayDescriptor(mem, regs);

                    if (descriptor == null)
                    {
                        model.SvgPath = "/Svg/White/boxes-stacked-solid.svg";
                        model.PropertyValue = $"{{ {variable.StorageType}(?) }}";
                        model.PropertyType = $"{variable.StorageType}(?)";

                        if (variable.Scope == ZXVariableScope.GlobalScope)
                            model.Background = SolidColorBrush.Parse(globalVarBG);
                        else if (variable.IsParameter)
                            model.Background = SolidColorBrush.Parse(localParamBG);
                        else
                            model.Background = SolidColorBrush.Parse(localVarBG);

                        variables.Add(model);

                        continue;
                    }

                    string[] dimStrings = descriptor.DimensionSizes.Select(d => d.ToString()).ToArray();
                    string valueString = $"{{ {variable.StorageType}({string.Join(',', dimStrings)}) }}";
                    string typeString = $"{variable.StorageType}({new string(',', descriptor.Dimensions - 1)})";

                    model.SvgPath = "/Svg/White/boxes-stacked-solid.svg";
                    model.PropertyValue = valueString;
                    model.PropertyType = typeString;

                    if (variable.Scope == ZXVariableScope.GlobalScope)
                        model.Background = SolidColorBrush.Parse(globalVarBG);
                    else
                        model.Background = SolidColorBrush.Parse(localVarBG);

                    ScanArrayElements(variable, 0, null, model.ChildProperties, descriptor);
                    variables.Add(model);
                }
            }
        }

        void ScanArrayElements(ZXVariable Variable, int CurrentDimension, int[]? PreviousDimensions, ICollection<ZXVariablePropertyModel> Elements, ZXArrayDescriptor Descriptor)
        {
            for (int buc = 0; buc < Descriptor.DimensionSizes[CurrentDimension]; buc++)
            {
                int[] path = PreviousDimensions == null ? new int[] { buc } : PreviousDimensions.Concat(new int[] { buc }).ToArray();

                if (CurrentDimension == Descriptor.Dimensions - 1)
                {
                    string name = $"({string.Join(',', path.Select(p => p.ToString()))})";
                    ZXVariablePropertyModel model = new ZXVariablePropertyModel { Variable = Variable };
                    model.SvgPath = "/Svg/White/box-solid.svg";
                    model.PropertyName = name;
                    model.PropertyValue = Variable.GetArrayValue(mem, Descriptor, path)?.ToString() ?? "{null}";
                    model.PropertyType = Variable.StorageType.ToString();
                    model.Editable = true;
                    model.ArrayPath = path;
                    model.Memory = mem;
                    model.Registers = regs;
                    if (Variable.Scope == ZXVariableScope.GlobalScope)
                        model.Background = SolidColorBrush.Parse(globalVarBG);
                    else
                        model.Background = SolidColorBrush.Parse(localVarBG);

                    Elements.Add(model);
                }
                else
                    ScanArrayElements(Variable, CurrentDimension + 1, path, Elements, Descriptor);

            }
        }

        public void EndEdit() 
        {
            variables.Clear();
        }
    }

    public class ZXVariablePropertyModel : AvaloniaObject
    {
        public static StyledProperty<string> SvgPathProperty = StyledProperty<string>.Register<ZXVariablePropertyModel, string>("SvgPath");
        public static StyledProperty<IBrush> BackgroundProperty = StyledProperty<IBrush>.Register<ZXVariablePropertyModel, IBrush>("Background", Brushes.Transparent);
        public static StyledProperty<string> PropertyNameProperty = StyledProperty<string>.Register<ZXVariablePropertyModel, string>("PropertyName");
        public static StyledProperty<string> PropertyTypeProperty = StyledProperty<string>.Register<ZXVariablePropertyModel, string>("PropertyType");
        public static StyledProperty<string> PropertyValueProperty = StyledProperty<string>.Register<ZXVariablePropertyModel, string>("PropertyValue");
        public static StyledProperty<bool> EditableProperty = StyledProperty<bool>.Register<ZXVariablePropertyModel, bool>("Editable");
        public static StyledProperty<ObservableCollection<ZXVariablePropertyModel>> ChildPropertiesProperty = StyledProperty<ObservableCollection<ZXVariablePropertyModel>>.Register<ZXVariablePropertyModel, ObservableCollection<ZXVariablePropertyModel>>("ChildProperties");

        public static StyledProperty<ZXVariable> VariableProperty = StyledProperty<ZXVariable>.Register<ZXVariablePropertyModel, ZXVariable>("Variable");
        public static StyledProperty<int[]> ArrayPathProperty = StyledProperty<int[]>.Register<ZXVariablePropertyModel, int[]>("ArrayPath");
        public static StyledProperty<IMemory> MemoryProperty = StyledProperty<IMemory>.Register<ZXVariablePropertyModel, IMemory>("Memory");
        public static StyledProperty<IZ80Registers> RegistersProperty = StyledProperty<IZ80Registers>.Register<ZXVariablePropertyModel, IZ80Registers>("Registers");

        public string SvgPath
        {
            get { return GetValue<string>(SvgPathProperty); }
            set { SetValue<string>(SvgPathProperty, value); }
        }
        public IBrush Background
        {
            get { return GetValue<IBrush>(BackgroundProperty);}
            set { SetValue<IBrush>(BackgroundProperty, value); }
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
        public ObservableCollection<ZXVariablePropertyModel> ChildProperties
        {
            get { return GetValue<ObservableCollection<ZXVariablePropertyModel>>(ChildPropertiesProperty); }
            set { SetValue<ObservableCollection<ZXVariablePropertyModel>>(ChildPropertiesProperty, value); }
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

        public ZXVariablePropertyModel()
        {
            ChildProperties = new ObservableCollection<ZXVariablePropertyModel>();
        }
    }
}
