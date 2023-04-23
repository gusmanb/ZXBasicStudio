using Avalonia.Media;
using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class PausedLineBackgroundRender : IBackgroundRenderer
    {
        static Brush highlight = new SolidColorBrush(Color.Parse("#443008"));
        public int? Line { get; set; }
        public KnownLayer Layer
        {
            get
            {
                return KnownLayer.Background;
            }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (Line == null)
                return;

            var lines = textView.VisualLines.Where(l => l.FirstDocumentLine.LineNumber == Line).ToArray();

            foreach (var line in lines)
            {
                var rects = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, line, 0, 1000000);

                double lastY = double.MinValue;
                foreach (var rect in rects)
                {
                    if (lastY == rect.Y)
                        continue;
                    lastY = rect.Y;
                    drawingContext.DrawRectangle(highlight, new Pen(Brushes.Wheat, 2), rect);
                }
            }
        }
    }
}
