namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    /// <summary>
    /// Enumerates the types of graphics supported
    /// </summary>
    public enum GraphicsModes
    {
        /// <summary>
        /// Black and white, no attributes
        /// </summary>
        Monochrome = 0,
        /// <summary>
        /// Classic ZX Spectrum attributes, 15 colors for every 8x8 cell
        /// </summary>
        ZXSpectrum = 1,
        /// <summary>
        /// ZX Spectrum Next 16x16 (256 colors)
        /// </summary>
        Next = 2,
    }
}