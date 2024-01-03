namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    /// <summary>
    /// Types of data format for sprite export
    /// </summary>
    public enum ExportDataTypes
    {
        /// <summary>
        /// Array in Basic
        /// </summary>
        DIM = 0,
        /// <summary>
        /// DEFB inside ASM block
        /// </summary>
        ASM = 1,
        /// <summary>
        /// .bin file (INCBIN)
        /// </summary>
        BIN = 2,
        // .tap file (LOAD "" CODE)
        TAP = 3
    }
}