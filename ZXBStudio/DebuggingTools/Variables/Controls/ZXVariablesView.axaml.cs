using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
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

                    if (variable.StorageType == ZXVariableStorage.STR)
                        model.PropertyValue = $"\"{model.PropertyValue}\"";

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

                    if (Variable.StorageType == ZXVariableStorage.STR)
                        model.PropertyValue = $"\"{model.PropertyValue}\"";

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

    public partial class ZXVariablePropertyModel : ObservableObject
    {
        [ObservableProperty]
        string? svgPath;

        [ObservableProperty]
        IBrush background = Brushes.Transparent;

        [ObservableProperty]
        string? propertyName;

        [ObservableProperty]
        string? propertyType;

        [ObservableProperty]
        string? propertyValue;

        [ObservableProperty]
        bool editable;

        [ObservableProperty]
        ObservableCollection<ZXVariablePropertyModel> childProperties = new ObservableCollection<ZXVariablePropertyModel>();

        [ObservableProperty]
        ZXVariable? variable;

        [ObservableProperty]
        int[]? arrayPath;

        [ObservableProperty]
        IMemory? memory;

        [ObservableProperty]
        IZ80Registers? registers;
    }
}
