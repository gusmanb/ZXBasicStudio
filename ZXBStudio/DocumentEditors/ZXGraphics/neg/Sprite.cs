using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    /// <summary>
    /// Represents an sprite with his properties
    /// </summary>
    public class Sprite
    {
        /// <summary>
        /// Id of the sprite
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the sprite
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Graphics mode of the sprite
        /// </summary>
        public GraphicsModes GraphicMode { get; set; }
        /// <summary>
        /// Width of the sprite in pixels
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height in pixels
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// True if the sprite is masked (not suported in all modes)
        /// </summary>
        public bool Masked { get; set; }
        /// <summary>
        /// Number of frames for the sprite
        /// </summary>
        public byte Frames { get; set; }
        /// <summary>
        /// Current frame
        /// </summary>
        public byte CurrentFrame { get; set; }
        /// <summary>
        /// Patterns for the sprite (one pattern for frame)
        /// </summary>
        public List<Pattern> Patterns { get; set; }
        /// <summary>
        /// Default transparent or background color
        /// </summary>
        public byte DefaultColor { get; set; }
        /// <summary>
        /// Palete color for the sprite
        /// </summary>
        public PaletteColor[] Palette { get; set; }
        /// <summary>
        /// True when the sprite was auto-exported
        /// </summary>
        public bool Export { get; set; }
    }
}
