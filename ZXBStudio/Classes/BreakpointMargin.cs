using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;

namespace ZXBasicStudio.Classes
{
    public class BreakPointMargin : AbstractMargin, IDisposable
    {
        private int previewLine;
        private bool previewPointVisible;

        List<ZXBreakPoint> _breakpoints = new List<ZXBreakPoint>();
        AvaloniaEdit.TextEditor _editor;

        public event EventHandler<BreakpointEventArgs>? BreakpointAdded;
        public event EventHandler<BreakpointEventArgs>? BreakpointRemoved;

        public List<ZXBreakPoint> Breakpoints { get { return _breakpoints; } }

        static BreakPointMargin()
        {
            FocusableProperty.OverrideDefaultValue(typeof(BreakPointMargin), true);
        }

        public BreakPointMargin(AvaloniaEdit.TextEditor Editor)
        {
            _editor= Editor;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
        }

        protected override void OnTextViewVisualLinesChanged()
        {
            base.OnTextViewVisualLinesChanged();
            var line = TextView.VisualLines.FirstOrDefault();

            if (line != null)
                Width = line.Height / line.TextLines.Count;
        }

        public override void Render(DrawingContext context)
        {
            if (TextView.VisualLinesValid)
            {
                context.FillRectangle(Brush.Parse("#ff202020"), Bounds);
                context.DrawLine(new Pen(Brushes.Gray, 0.5), Bounds.TopRight, Bounds.BottomRight);

                if (TextView.VisualLines.Count > 0)
                {
                    var firstLine = TextView.VisualLines.FirstOrDefault();
                    var rectSize = (firstLine.GetTextLineVisualYPosition(firstLine.TextLines[0], VisualYPosition.TextBottom) - firstLine.GetTextLineVisualYPosition(firstLine.TextLines[0], VisualYPosition.TextTop)) * 0.75;

                    var lineSize = firstLine.Height / firstLine.TextLines.Count;
                    var offset = (lineSize - rectSize) / 2;

                    //Width = height;
                    var textView = TextView;

                    foreach (var breakPoint in _breakpoints)
                    {
                        var visualLine = TextView.VisualLines.FirstOrDefault(vl => vl.FirstDocumentLine.LineNumber == breakPoint.Line);

                        if (visualLine != null)
                        {
                            context.FillRectangle(Brush.Parse("#FF3737"),
                            new Rect(offset,
                                 visualLine.GetTextLineVisualYPosition(visualLine.TextLines[0], AvaloniaEdit.Rendering.VisualYPosition.TextTop) + offset - TextView.VerticalOffset,
                                rectSize, rectSize), (float)rectSize);
                        }
                    }

                    if (previewPointVisible)
                    {
                        var visualLine = TextView.VisualLines.FirstOrDefault(vl => vl.FirstDocumentLine.LineNumber == previewLine);

                        if (visualLine != null)
                        {
                            context.FillRectangle(Brush.Parse("#E67466"),
                            new Rect(offset,
                                 visualLine.GetTextLineVisualYPosition(visualLine.TextLines[0], AvaloniaEdit.Rendering.VisualYPosition.TextTop) + offset - TextView.VerticalOffset,
                                rectSize, rectSize), (float)rectSize);
                        }
                    }
                }
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            previewPointVisible = true;

            var textView = TextView;

            var offset = GetOffsetFromPoint(e.GetPosition(this));

            if (offset != -1)
            {
                previewLine = textView.Document.GetLineByOffset(offset).LineNumber; // convert from text line to visual line.
            }

            InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            previewPointVisible = true;

            var textView = TextView;

            var offset = GetOffsetFromPoint(e.GetPosition(this));

            if (offset != -1)
            {
                var lineClicked = -1;
                lineClicked = textView.Document.GetLineByOffset(offset).LineNumber; // convert from text line to visual line.

                var currentBreakPoint =
                    _breakpoints.FirstOrDefault(bp => bp.Line == lineClicked);

                if (currentBreakPoint != null)
                {
                    if (BreakpointRemoved != null)
                    {
                        var args = new BreakpointEventArgs(currentBreakPoint);
                        BreakpointRemoved(this, args);

                        if(!args.Cancel)
                            _breakpoints.Remove(currentBreakPoint);
                    }
                }
                else
                {
                    if (BreakpointAdded != null)
                    {
                        var bp = new ZXBreakPoint("", lineClicked);
                        var args = new BreakpointEventArgs(bp);
                        BreakpointAdded(this, args);

                        if (!args.Cancel)
                            _breakpoints.Add(bp);
                    }
                }
            }

            InvalidateVisual();
        }
        private int GetOffsetFromPoint(Point point)
        {
            var position = _editor.GetPositionFromPoint(point);

            var offset = position != null ? Document.GetOffset(position.Value.Location) : -1;

            return offset;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (TextView != null)
            {
                return new Size(TextView.DefaultLineHeight, 0);
            }

            return new Size(0, 0);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            previewPointVisible = false;
            InvalidateVisual();
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            InvalidateVisual();
            e.Handled = true;
        }

        public void Dispose()
        {
            _editor = null;
        }

        public class BreakpointEventArgs
        {
            public BreakpointEventArgs(ZXBreakPoint Break)
            {
                BreakPoint = Break;
            }
            public ZXBreakPoint BreakPoint { get; set; }
            public string? FileName { get; set; }
            public bool Cancel { get; set; }
        }
    }
}
