namespace ZXGraphics.neg
{
    /// <summary>
    /// Defines a point into the GDU/Font/Sprite/Tile data
    /// </summary>
    public class PointData
    {
        /// <summary>
        /// X coord
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y coord
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Color index
        /// For ZX classic 0 or 1
        /// For others like Next, color index
        /// </summary>
        public int ColorIndex { get; set; }
    }
}