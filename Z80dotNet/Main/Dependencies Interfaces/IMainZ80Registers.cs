namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a set of the Z80 main registers (AF, BC, DE, HL)
    /// </summary>
    public interface IMainZ80Registers
    {
        /// <summary>
        /// The AF register pair
        /// </summary>
        short AF { get; set; }

        /// <summary>
        /// The BC register pair
        /// </summary>
        short BC { get; set; }

        /// <summary>
        /// The DE register pair
        /// </summary>
        short DE { get; set; }

        /// <summary>
        /// The HL register pair
        /// </summary>
        short HL { get; set; }

        /// <summary>
        /// The A register.
        /// </summary>
        byte A { get; set; }

        /// <summary>
        /// The F register.
        /// </summary>
        byte F { get; set; }

        /// <summary>
        /// The B register.
        /// </summary>
        byte B { get; set; }

        /// <summary>
        /// The C register.
        /// </summary>
        byte C { get; set; }

        /// <summary>
        /// The D register.
        /// </summary>
        byte D { get; set; }

        /// <summary>
        /// The E register.
        /// </summary>
        byte E { get; set; }

        /// <summary>
        /// The H register.
        /// </summary>
        byte H { get; set; }

        /// <summary>
        /// The E register.
        /// </summary>
        byte L { get; set; }

        /// <summary>
        /// The carry (C) flag.
        /// </summary>
        Bit CF { get; set; }

        /// <summary>
        /// The addition/substraction (N) flag.
        /// </summary>
        Bit NF { get; set; }

        /// <summary>
        /// The parity/overflow (P/V) flag.
        /// </summary>
        Bit PF { get; set; }

        /// <summary>
        /// The unused flag at bit 3 of F.
        /// </summary>
        Bit Flag3 { get; set; }

        /// <summary>
        /// The half carry (H) flag.
        /// </summary>
        Bit HF { get; set; }

        /// <summary>
        /// The unused flag at bit 5 of F.
        /// </summary>
        Bit Flag5 { get; set; }

        /// <summary>
        /// The zero (Z) flag.
        /// </summary>
        Bit ZF { get; set; }

        /// <summary>
        /// The sign (S) flag.
        /// </summary>
        Bit SF { get; set; }
    }
}