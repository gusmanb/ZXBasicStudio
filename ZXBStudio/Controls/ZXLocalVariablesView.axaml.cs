using Avalonia.Controls;
using AvaloniaEdit.Utils;
using Konamiman.Z80dotNet;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXLocalVariablesView : UserControl
    {
        public ObservableCollection<ZXVariableProperty> Variables { get; set; } = new ObservableCollection<ZXVariableProperty>();
        IMemory? mem;
        IZ80Registers? regs;
        ZXLocalVariableMap? map;

        public bool HasVariables { get { return Variables.Count > 0; } }

        public ZXLocalVariablesView()
        {
            DataContext = this;
            InitializeComponent();
        }
        public void Initialize(IMemory Memory, IZ80Registers Registers, ZXLocalVariableMap LocalMap)
        {
            Variables.Clear();
            mem = Memory;
            regs = Registers;
            map = LocalMap;
        }
        public void Update()
        {
            Variables.Clear();

            if (mem == null || regs == null || map == null)
                return;

            var localVars = map.GetVariablesInRange(regs.PC);

            if (localVars == null)
                txtFunction.Text = "No local vars in range";
            else
            {
                txtFunction.Text = $"Local vars of function {localVars.FunctionName}";
                foreach (var localVar in localVars.Variables.OrderByDescending(v => v.StackOffset))
                {
                    if (localVar.VariableType == ZXVariableType.Flat)
                    {
                        ZXVariableProperty varProp = new ZXVariableProperty();
                        varProp.PropertyName = $"{localVar.Name ?? localVar.StackOffset.ToString()}";
                        varProp.PropertyValue = $"{localVar.GetValue(mem, regs)?.ToString() ?? "{null}"}\t({localVar.StorageType})";
                        varProp.SvgPath = "/Svg/White/box-open-solid.svg";
                        Variables.Add(varProp);
                    }
                    else
                    {
                        ZXVariableProperty varProp = new ZXVariableProperty();
                        varProp.PropertyName = $"{localVar.Name ?? localVar.StackOffset.ToString()}";
                        varProp.PropertyValue = $"{((ushort)localVar.GetValue(mem, regs)).ToString("X4")}\t({localVar.StorageType})";
                        varProp.SvgPath = "/Svg/White/boxes-stacked-solid.svg";
                        Variables.Add(varProp);

                        var descriptor = localVar.GetArrayDescriptor(mem, regs);
                        List<ZXVariableProperty> arrProps = new List<ZXVariableProperty>();
                        ScanArrayElements(mem, localVar, 0, null, arrProps, descriptor);
                        Variables.AddRange(arrProps);
                    }
                }
            }

            tvVariables.Items = Variables;
        }

        void ScanArrayElements(IMemory Memory, ZXLocalVariable Variable, int CurrentDimension, int[]? PreviousDimensions, List<ZXVariableProperty> Properties, ZXArrayDescriptor Descriptor)
        {
            for (int buc = 0; buc < Descriptor.DimensionSizes[CurrentDimension]; buc++)
            {
                int[] path = PreviousDimensions == null ? new int[] { buc } : PreviousDimensions.Concat(new int[] { buc }).ToArray();

                if (CurrentDimension == Descriptor.Dimensions - 1)
                    Properties.Add(new ZXVariableProperty { PropertyName = "\tItem(" + string.Join(", ", path.Select(p => p.ToString())) + ")", PropertyValue = ZXVariableHelper.GetArrayValue(Memory, Descriptor, Variable.StorageType, path).ToString() ?? "", SvgPath = "/Svg/White/list-ol-solid.svg", ArrayPath = path });
                else
                    ScanArrayElements(Memory, Variable, CurrentDimension + 1, path, Properties, Descriptor);

            }
        }
    }
}
