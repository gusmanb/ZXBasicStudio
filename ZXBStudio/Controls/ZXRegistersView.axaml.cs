using Avalonia;
using Avalonia.Controls;
using Konamiman.Z80dotNet;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXRegistersView : UserControl
    {
        public static StyledProperty<bool> AllowEditProperty = StyledProperty<bool>.Register<ZXRegistersView, bool>("AllowEdit", false);

        public bool AllowEdit
        {
            get { return GetValue<bool>(AllowEditProperty); }
            set { SetValue<bool>(AllowEditProperty, value); }
        }

        ZXRegister regAF = new ZXRegister { Name = "AF" };
        ZXRegister regBC = new ZXRegister { Name = "BC" };
        ZXRegister regDE = new ZXRegister { Name = "DE" };
        ZXRegister regHL = new ZXRegister { Name = "HL" };
        ZXRegister regIX = new ZXRegister { Name = "IX" };
        ZXRegister regIY = new ZXRegister { Name = "IY" };
        ZXRegister regPC = new ZXRegister { Name = "PC" };
        ZXRegister regSP = new ZXRegister { Name = "SP" };
        ZXRegister regA = new ZXRegister { Name = "A", IsByte = true };
        ZXRegister regF = new ZXRegister { Name = "F", IsByte = true };
        ZXRegister regB = new ZXRegister { Name = "B", IsByte = true };
        ZXRegister regC = new ZXRegister { Name = "C", IsByte = true };
        ZXRegister regD = new ZXRegister { Name = "D", IsByte = true };
        ZXRegister regE = new ZXRegister { Name = "E", IsByte = true };
        ZXRegister regH = new ZXRegister { Name = "H", IsByte = true };
        ZXRegister regL = new ZXRegister { Name = "L", IsByte = true };
        ZXRegister regIXH = new ZXRegister { Name = "IXH", IsByte = true };
        ZXRegister regIXL = new ZXRegister { Name = "IXL", IsByte = true };
        ZXRegister regIYH = new ZXRegister { Name = "IYH", IsByte = true };
        ZXRegister regIYL = new ZXRegister { Name = "IYL", IsByte = true };
        ZXRegister regAFx = new ZXRegister { Name = "AF'" };
        ZXRegister regBCx = new ZXRegister { Name = "BC'" };
        ZXRegister regDEx = new ZXRegister { Name = "DE'" };
        ZXRegister regHLx = new ZXRegister { Name = "HL'" };
        ZXRegister regAx = new ZXRegister { Name = "A'", IsByte = true };
        ZXRegister regFx = new ZXRegister { Name = "F'", IsByte = true };
        ZXRegister regBx = new ZXRegister { Name = "B'", IsByte = true };
        ZXRegister regCx = new ZXRegister { Name = "C'", IsByte = true };
        ZXRegister regDx = new ZXRegister { Name = "D'", IsByte = true };
        ZXRegister regEx = new ZXRegister { Name = "E'", IsByte = true };
        ZXRegister regHx = new ZXRegister { Name = "H'", IsByte = true };
        ZXRegister regLx = new ZXRegister { Name = "L'", IsByte = true };

        public IZ80Registers? Registers { get; set; }

        public ZXRegistersView()
        {
            InitializeComponent();

            RegAF.Item = regAF;
            RegBC.Item = regBC;
            RegDE.Item = regDE;
            RegHL.Item = regHL;
            RegIX.Item = regIX;
            RegIY.Item = regIY;
            RegPC.Item = regPC;
            RegSP.Item = regSP;
            RegA.Item = regA;
            RegF.Item = regF;
            RegB.Item = regB;
            RegC.Item = regC;
            RegD.Item = regD;
            RegE.Item = regE;
            RegH.Item = regH;
            RegL.Item = regL;
            RegIXH.Item = regIXH;
            RegIXL.Item = regIXL;
            RegIYH.Item = regIYH;
            RegIYL.Item = regIYL;
            RegAFx.Item = regAFx;
            RegBCx.Item = regBCx;
            RegDEx.Item = regDEx;
            RegHLx.Item = regHLx;
            RegAx.Item = regAx;
            RegFx.Item = regFx;
            RegBx.Item = regBx;
            RegCx.Item = regCx;
            RegDx.Item = regDx;
            RegEx.Item = regEx;
            RegHx.Item = regHx;
            RegLx.Item = regLx;

            var edit = this.GetObservable<bool>(AllowEditProperty);
            
            RegAF.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegBC.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegDE.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegHL.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegIX.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegIY.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegPC.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegSP.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegA.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegF.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegB.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegC.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegD.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegE.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegH.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegL.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegIXH.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegIXL.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegIYH.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegIYL.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegAFx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegBCx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegDEx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegHLx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegAx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegFx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegBx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegCx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegDx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegEx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegHx.Bind(ZXRegisterView.AllowEditProperty, edit);
            RegLx.Bind(ZXRegisterView.AllowEditProperty, edit);

            RegAF.ValueChanged += RegisterChanged;
            RegBC.ValueChanged += RegisterChanged;
            RegDE.ValueChanged += RegisterChanged;
            RegHL.ValueChanged += RegisterChanged;
            RegIX.ValueChanged += RegisterChanged;
            RegIY.ValueChanged += RegisterChanged;
            RegPC.ValueChanged += RegisterChanged;
            RegSP.ValueChanged += RegisterChanged;
            RegA.ValueChanged += RegisterChanged;
            RegF.ValueChanged += RegisterChanged;
            RegB.ValueChanged += RegisterChanged;
            RegC.ValueChanged += RegisterChanged;
            RegD.ValueChanged += RegisterChanged;
            RegE.ValueChanged += RegisterChanged;
            RegH.ValueChanged += RegisterChanged;
            RegL.ValueChanged += RegisterChanged;
            RegIXH.ValueChanged += RegisterChanged;
            RegIXL.ValueChanged += RegisterChanged;
            RegIYH.ValueChanged += RegisterChanged;
            RegIYL.ValueChanged += RegisterChanged;
            RegAFx.ValueChanged += RegisterChanged;
            RegBCx.ValueChanged += RegisterChanged;
            RegDEx.ValueChanged += RegisterChanged;
            RegHLx.ValueChanged += RegisterChanged;
            RegAx.ValueChanged += RegisterChanged;
            RegFx.ValueChanged += RegisterChanged;
            RegBx.ValueChanged += RegisterChanged;
            RegCx.ValueChanged += RegisterChanged;
            RegDx.ValueChanged += RegisterChanged;
            RegEx.ValueChanged += RegisterChanged;
            RegHx.ValueChanged += RegisterChanged;
            RegLx.ValueChanged += RegisterChanged;
        }

        private void RegisterChanged(object? sender, System.EventArgs e)
        {
            if (Registers == null)
                return;

            var regView = sender as ZXRegisterView;

            if (regView == null)
                return;

            if (regView.Item == null)
                return;

            switch (regView.Item.Name)
            {
                case "AF":
                    Registers.AF = regAF.Signed;
                    break;
                case "BC":
                    Registers.BC = regBC.Signed;
                    break;
                case "DE":
                    Registers.DE = regDE.Signed;
                    break;
                case "HL":
                    Registers.HL = regHL.Signed;
                    break;
                case "IX":
                    Registers.IX = regIX.Signed;
                    break;
                case "IY":
                    Registers.IY = regIY.Signed;
                    break;
                case "PC":
                    Registers.PC = regPC.Unsigned;
                    break;
                case "SP":
                    Registers.SP = regSP.Signed;
                    break;
                case "A":
                    Registers.A = (byte)regA.Unsigned;
                    break;
                case "F":
                    Registers.F = (byte)regF.Unsigned;
                    break;
                case "B":
                    Registers.B = (byte)regB.Unsigned;
                    break;
                case "C":
                    Registers.C = (byte)regC.Unsigned;
                    break;
                case "D":
                    Registers.D = (byte)regD.Unsigned;
                    break;
                case "E":
                    Registers.E = (byte)regE.Unsigned;
                    break;
                case "H":
                    Registers.H = (byte)regH.Unsigned;
                    break;
                case "L":
                    Registers.L = (byte)regL.Unsigned;
                    break;
                case "IXH":
                    Registers.IXH = (byte)regIXH.Unsigned;
                    break;
                case "IXL":
                    Registers.IXL = (byte)regIXL.Unsigned;
                    break;
                case "IYH":
                    Registers.IYH = (byte)regIYH.Unsigned;
                    break;
                case "IYL":
                    Registers.IYL = (byte)regIYL.Unsigned;
                    break;
                case "AF'":
                    Registers.Alternate.AF = regAFx.Signed;
                    break;
                case "BC'":
                    Registers.Alternate.BC = regBCx.Signed;
                    break;
                case "DE'":
                    Registers.Alternate.DE = regDEx.Signed;
                    break;
                case "HL'":
                    Registers.Alternate.HL = regHLx.Signed;
                    break;
                case "A'":
                    Registers.Alternate.A = (byte)regAx.Unsigned;
                    break;
                case "F'":
                    Registers.Alternate.F = (byte)regFx.Unsigned;
                    break;
                case "B'":
                    Registers.Alternate.B = (byte)regBx.Unsigned;
                    break;
                case "C'":
                    Registers.Alternate.C = (byte)regCx.Unsigned;
                    break;
                case "D'":
                    Registers.Alternate.D = (byte)regDx.Unsigned;
                    break;
                case "E'":
                    Registers.Alternate.E = (byte)regEx.Unsigned;
                    break;
                case "H'":
                    Registers.Alternate.H = (byte)regHx.Unsigned;
                    break;
                case "L'":
                    Registers.Alternate.L = (byte)regLx.Unsigned;
                    break;
            }

            Update();
        }

        public void Update()
        {
            if (Registers == null)
                return;

            regAF.SetValue(Registers.AF);
            regBC.SetValue(Registers.BC);
            regDE.SetValue(Registers.DE);
            regHL.SetValue(Registers.HL);
            regIX.SetValue(Registers.IX);
            regIY.SetValue(Registers.IY);
            regPC.SetValue(Registers.PC);
            regSP.SetValue(Registers.SP);
            regA.SetValue(Registers.A);
            regF.SetValue(Registers.F);
            regB.SetValue(Registers.B);
            regC.SetValue(Registers.C);
            regD.SetValue(Registers.D);
            regE.SetValue(Registers.E);
            regH.SetValue(Registers.H);
            regL.SetValue(Registers.L);
            regIXH.SetValue(Registers.IXH);
            regIXL.SetValue(Registers.IXL);
            regIYH.SetValue(Registers.IYH);
            regIYL.SetValue(Registers.IYL);
            regAFx.SetValue(Registers.Alternate.AF);
            regBCx.SetValue(Registers.Alternate.BC);
            regDEx.SetValue(Registers.Alternate.DE);
            regHLx.SetValue(Registers.Alternate.HL);
            regAx.SetValue(Registers.Alternate.A);
            regFx.SetValue(Registers.Alternate.F);
            regBx.SetValue(Registers.Alternate.B);
            regCx.SetValue(Registers.Alternate.C);
            regDx.SetValue(Registers.Alternate.D);
            regEx.SetValue(Registers.Alternate.E);
            regHx.SetValue(Registers.Alternate.H);
            regLx.SetValue(Registers.Alternate.L);
        }
    }

}
