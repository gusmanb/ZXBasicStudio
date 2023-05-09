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
    public class ZXGrip : UserControl
    {
        public static StyledProperty<double> DotRadiusProperty = StyledProperty<double>.Register<ZXGrip, double>("DotRadius", 1);
        public static StyledProperty<Size> DotSpacingProperty = StyledProperty<Size>.Register<ZXGrip, Size>("DotSpacing", new Size(5,5));
        public static StyledProperty<IBrush> DotColorProperty = StyledProperty<IBrush>.Register<ZXGrip, IBrush>("DotRadius", Brushes.Black);
        public static StyledProperty<Thickness> DotMarginProperty = StyledProperty<Thickness>.Register<ZXGrip, Thickness>("DotMargin", new Thickness(5));

        bool _showDots = false;

        public double DotRadius 
        {
            get => GetValue(DotRadiusProperty);
            set => SetValue(DotRadiusProperty, value); 
        }
        public Size DotSpacing 
        {
            get => GetValue(DotSpacingProperty);
            set => SetValue(DotSpacingProperty, value);
        }
        public IBrush DotColor 
        {
            get => GetValue(DotColorProperty);
            set => SetValue(DotColorProperty, value);
        }
        public Thickness DotMargin
        {
            get => GetValue(DotMarginProperty);
            set => SetValue(DotMarginProperty, value);
        }

        internal bool ShowDots 
        { 
            get { return _showDots; } 
            set 
            { 
                if (_showDots != value) 
                { 
                    _showDots = value;
                    InvalidateVisual();
                } 
            } 
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (!_showDots)
                return;

            for (double y = DotMargin.Top; y < Bounds.Height - DotMargin.Bottom; y += DotSpacing.Height)
            {
                double offset = y % 2 == 0 ? 0 : DotSpacing.Width / 2.0;
                
                for (double x = DotMargin.Left; x < Bounds.Width - DotMargin.Right; x += DotSpacing.Width)
                {
                    context.DrawEllipse(DotColor, null, new Avalonia.Point(x + offset, y), DotRadius, DotRadius);                    
                }
            }
        }
        
    }
}
