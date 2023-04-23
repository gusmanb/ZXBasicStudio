using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Controls.DockSystem
{
    public class ZXGrip : Border
    {
        public static StyledProperty<double> DotRadiusProperty = StyledProperty<double>.Register<ZXGrip, double>("DotRadius", 1);
        public static StyledProperty<double> DotSpacingProperty = StyledProperty<double>.Register<ZXGrip, double>("DotSpacing", 5);
        public static StyledProperty<IBrush> DotColorProperty = StyledProperty<IBrush>.Register<ZXGrip, IBrush>("DotRadius", Brushes.Black);

        public double DotRadius 
        {
            get => GetValue(DotRadiusProperty);
            set => SetValue(DotRadiusProperty, value); 
        }
        public double DotSpacing 
        {
            get => GetValue(DotSpacingProperty);
            set => SetValue(DotSpacingProperty, value);
        }
        public IBrush DotColor 
        {
            get => GetValue(DotColorProperty);
            set => SetValue(DotColorProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            for (double y = DotRadius * 2; y < Bounds.Height; y += DotSpacing)
            {
                double offset = y % 2 == 0 ? 0 : DotSpacing / 2.0;
                
                for (double x = DotRadius * 2; x < Bounds.Width; x += DotSpacing)
                {
                    context.DrawEllipse(DotColor, null, new Avalonia.Point(x + offset, y), DotRadius, DotRadius);                    
                }
            }
        }
    }
}
