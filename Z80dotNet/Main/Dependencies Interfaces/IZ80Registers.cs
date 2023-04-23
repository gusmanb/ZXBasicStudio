namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a full set of Z80 registers.
    /// </summary>
    public interface IZ80Registers : IMainZ80Registers
    {
        /// <summary>
        /// The alternate register set (AF', BC', DE', HL')
        /// </summary>
        IMainZ80Registers Alternate { get; set; }

        /// <summary>
        /// The IX register pair
        /// </summary>
        short IX { get; set; }

        /// <summary>
        /// The IY register pair
        /// </summary>
        short IY { get; set; }

        /// <summary>
        /// The program counter
        /// </summary>
        ushort PC { get; set; }

        /// <summary>
        /// The stack pointer
        /// </summary>
        short SP { get; set; }

        /// <summary>
        /// The interrupt and refresh register
        /// </summary>
        short IR { get; set; }

        /// <summary>
        /// The IFF1 flag. It has always the value 0 or 1.
        /// </summary>
        Bit IFF1 { get; set; }

        /// <summary>
        /// The IFF2 flag. It has always the value 0 or 1.
        /// </summary>
        Bit IFF2 { get; set; }

        /// <summary>
        /// The IXH register.
        /// </summary>
        byte IXH { get; set; }

        /// <summary>
        /// The IXL register.
        /// </summary>
        byte IXL { get; set; }

        /// <summary>
        /// The IYH register.
        /// </summary>
        byte IYH { get; set; }

        /// <summary>
        /// The IYL register.
        /// </summary>
        byte IYL { get; set; }

        /// <summary>
        /// The I register.
        /// </summary>
        byte I { get; set; }

        /// <summary>
        /// The R register.
        /// </summary>
        byte R { get; set; }
    }
}