namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The DAA instruction.
        /// </summary>
        byte DAA()
        {
            const byte CF_NF = 3;
            
            FetchFinished();

            //Algorithm borrowed from MAME:
            //https://github.com/mamedev/mame/blob/master/src/emu/cpu/z80/z80.c

            var oldValue = Registers.A;
            var newValue = oldValue;

            if(Registers.HF || (oldValue & 0x0F) > 9) newValue = (byte)(newValue + (Registers.NF ? -0x06 : 0x06)); //FA
            if(Registers.CF || oldValue > 0x99) newValue = (byte)(newValue + (Registers.NF ? -0x60 : 0x60)); //A0

            Registers.CF |= (oldValue > 0x99);
            Registers.HF = ((oldValue ^ newValue) & 0x10);
            Registers.SF = (newValue & 0x80);
            Registers.ZF = (newValue == 0);
            Registers.PF = Parity[newValue];
            SetFlags3and5From(newValue);

            Registers.A = newValue;

            return 4;
        }
    }
}
