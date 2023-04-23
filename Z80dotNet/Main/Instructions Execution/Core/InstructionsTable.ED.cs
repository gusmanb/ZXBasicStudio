using System;

namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        private Func<byte>[] ED_InstructionExecutors;
        private Func<byte>[] ED_Block_InstructionExecutors;

        private void Initialize_ED_InstructionsTable()
        {
            ED_InstructionExecutors = new Func<byte>[]
            {
                IN_B_C,    //40
                OUT_C_B,    //41
                SBC_HL_BC,    //42
                LD_aa_BC,    //43
                NEG,    //44
                RETN,    //45
                IM_0,    //46
                LD_I_A,    //47
                IN_C_C,    //48
                OUT_C_C,    //49
                ADC_HL_BC,    //4A
                LD_BC_aa,    //4B
                NEG,    //4C
                RETI,    //4D
                IM_0,    //4E
                LD_R_A,    //4F
                IN_D_C,    //50
                OUT_C_D,    //51
                SBC_HL_DE,    //52
                LD_aa_DE,    //53
                NEG,    //54
                RETN,    //55
                IM_1,    //56
                LD_A_I,    //57
                IN_E_C,    //58
                OUT_C_E,    //59
                ADC_HL_DE,    //5A
                LD_DE_aa,    //5B
                NEG,    //5C
                RETI,    //5D
                IM_2,    //5E
                LD_A_R,    //5F
                IN_H_C,    //60
                OUT_C_H,    //61
                SBC_HL_HL,    //62
                LD_aa_HL,    //63
                NEG,    //64
                RETN,    //65
                IM_0,    //66
                RRD,    //67
                IN_L_C,    //68
                OUT_C_L,    //69
                ADC_HL_HL,    //6A
                LD_HL_aa,    //6B
                NEG,    //6C
                RETI,    //6D
                IM_0,    //6E
                RLD,    //6F
                IN_F_C,    //70
                OUT_C_0,    //71
                SBC_HL_SP,    //72
                LD_aa_SP,    //73
                NEG,    //74
                RETN,    //75
                IM_1,    //76
                NOP2,    //77
                IN_A_C,    //78
                OUT_C_A,    //79
                ADC_HL_SP,    //7A
                LD_SP_aa,    //7B
                NEG,    //7C
                RETI,    //7D
                IM_2,    //7E
                NOP2    //7F
            };

            ED_Block_InstructionExecutors = new Func<byte>[]
            {
                LDI,         //A0
                CPI,         //A1
                INI,         //A2
                OUTI,        //A3
                null, null, null, null,
                LDD,         //A8
                CPD,         //A9
                IND,         //AA
                OUTD,        //AB
                null, null, null, null,
                LDIR,        //B0
                CPIR,        //B1
                INIR,        //B2
                OTIR,        //B3
                null, null, null, null,
                LDDR,        //B8
                CPDR,        //B9
                INDR,        //BA
                OTDR,        //BB
            };
        }
    }
}
