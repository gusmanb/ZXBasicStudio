using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ZXBasicStudio.Controls.DockSystem
{
    public class ZXDockingContainer : Grid, IZXDockingContainer
    {
        public static StyledProperty<ZXStackOrientation> StackOrientationProperty = StyledProperty<ZXStackOrientation>.Register<ZXDockingContainer, ZXStackOrientation>("StackOrientation", ZXStackOrientation.Vertical);
        public static StyledProperty<string?> DockingGroupProperty = StyledProperty<string?>.Register<ZXDockingContainer, string?>("DockingGroup", null);

        public event EventHandler? DockingControlsChanged;

        public ZXStackOrientation StackOrientation
        {
            get => GetValue<ZXStackOrientation>(StackOrientationProperty);
            set 
            {
                var oldValue = GetValue<ZXStackOrientation>(StackOrientationProperty);

                if (value == oldValue)
                    return;

                SetValue(StackOrientationProperty, value);
                ReorderContent();
            }
        }
        public string? DockingGroup 
        {
            get => GetValue(DockingGroupProperty);
            set => SetValue(DockingGroupProperty, value);
        }
        public IEnumerable<ZXDockingControl> DockingControls { get { return Children.Where(c => c is ZXDockingControl).Cast<ZXDockingControl>(); } }

        Panel dropTargetPanel;

        public ZXDockingContainer() : base()
        {
            this.Background = new SolidColorBrush(Colors.Transparent);

            DragDrop.SetAllowDrop(this, true);

            dropTargetPanel = new Panel { Background = new SolidColorBrush(Colors.LightCyan, 0.5) };

            AddHandler(DragDrop.DragOverEvent, (sender, e) => 
            {
                CheckDrag(sender, e);
            });
            AddHandler(DragDrop.DragEnterEvent, (sender, e) =>
            {
                CheckDrag(sender, e);
            });
            AddHandler(DragDrop.DragLeaveEvent, (sender, e) =>
            {
                CheckDrag(sender, e);
            });
            AddHandler(DragDrop.DropEvent, (sender, e) => 
            {
                if (!e.Data.Contains("DockedControl"))
                    return;

                var dragged = e.Data.Get("DockedControl") as ZXDockingControl;

                if (dragged == null || dragged.DockingGroup != DockingGroup)
                    return;

                if (Children.Count == 0 || (Children.Count == 1 && Children[0] == dropTargetPanel))
                    AddToStart(dragged, null);
                else
                {
                    var target = FindDropTarget(e, dragged);

                    if (target == null)
                    {
                        if (dropTargetPanel.Parent != null)
                            Children.Remove(dropTargetPanel);

                        return;
                    }

                    GridLength length = new GridLength(target.OverSize.Value / 2, target.OverSize.GridUnitType);

                    if (StackOrientation == ZXStackOrientation.Vertical)
                        RowDefinitions[target.OverIndex].Height = length;
                    else
                        RowDefinitions[target.OverIndex].Height = length;

                    InsertAt(target.DropIndex, dragged, length);


                }
            });

        }

        public void AddToStart(ZXDockingControl Element) => AddToStart(Element, null);
        public void AddToEnd(ZXDockingControl Element) => AddToEnd(Element, null);
        public void InsertAt(int Index, ZXDockingControl Element) => InsertAt(Index, Element, null);
        private void AddToStart(ZXDockingControl Element, GridLength? Length)
        {
            BeginAdd(Element);

            if (Children.Count == 0)
            {
                if (StackOrientation == ZXStackOrientation.Vertical)
                {
                    Element.SetValue(Grid.RowProperty, 0);
                    RowDefinitions = new RowDefinitions("*");
                }
                else
                {
                    Element.SetValue(Grid.ColumnProperty, 0);
                    ColumnDefinitions = new ColumnDefinitions("*");
                }
                Children.Add(Element);

                if(DockingControlsChanged != null)
                    DockingControlsChanged(this, EventArgs.Empty);
            }
            else
            {
                if (StackOrientation == ZXStackOrientation.Vertical)
                {
                    foreach (var item in Children)
                        item.SetValue(Grid.RowProperty, item.GetValue(Grid.RowProperty) + 2);

                    var rowDefs = RowDefinitions;

                    if (Length == null)
                    {
                        var stars = rowDefs.Where(d => d.Height.IsStar);
                        var starSum = stars.Sum(d => d.Height.Value);
                        var size = starSum / (stars.Count() + 1);

                        rowDefs.Insert(0, new RowDefinition(size, GridUnitType.Star));
                    }
                    else
                        rowDefs.Insert(0, new RowDefinition(Length.Value.Value, Length.Value.GridUnitType));

                    rowDefs.Insert(1, new RowDefinition(4, GridUnitType.Pixel));
                    
                    Element.SetValue(Grid.RowProperty, 0);
                    Children.Add(Element);

                    var splitter = new GridSplitter();
                    splitter.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                    splitter.ResizeDirection = GridResizeDirection.Rows;
                    splitter.SetValue(Grid.RowProperty, 1);
                    Children.Add(splitter);

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);
                }
                else
                {
                    foreach (var item in Children)
                        item.SetValue(Grid.ColumnProperty, item.GetValue(Grid.ColumnProperty) + 2);

                    var colDefs = ColumnDefinitions;

                    if (Length == null)
                    {
                        var stars = colDefs.Where(d => d.Width.IsStar);
                        var starSum = stars.Sum(d => d.Width.Value);
                        var size = starSum / (stars.Count() + 1);

                        colDefs.Insert(0, new ColumnDefinition(size, GridUnitType.Star));
                    }
                    else
                        colDefs.Insert(0, new ColumnDefinition(Length.Value.Value, Length.Value.GridUnitType));

                    colDefs.Insert(1, new ColumnDefinition(4, GridUnitType.Pixel));
                    

                    Element.SetValue(Grid.ColumnProperty, 0);
                    Children.Add(Element);

                    var splitter = new GridSplitter();
                    splitter.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                    splitter.ResizeDirection = GridResizeDirection.Columns;
                    splitter.SetValue(Grid.ColumnProperty, 1);
                    Children.Add(splitter);

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);
                }
            }
        }
        private void AddToEnd(ZXDockingControl Element, GridLength? Length)
        {
            BeginAdd(Element);

            if (Children.Count == 0)
            {
                if (StackOrientation == ZXStackOrientation.Vertical)
                {
                    Element.SetValue(Grid.RowProperty, 0);
                    RowDefinitions = new RowDefinitions("*");
                }
                else
                {
                    Element.SetValue(Grid.ColumnProperty, 0);
                    ColumnDefinitions = new ColumnDefinitions("*");
                }
                Children.Add(Element);

                if (DockingControlsChanged != null)
                    DockingControlsChanged(this, EventArgs.Empty);
            }
            else
            {
                int realIndex = Children.Count;

                if (StackOrientation == ZXStackOrientation.Vertical)
                {

                    foreach (var item in Children)
                    {
                        if (item.GetValue(Grid.RowProperty) >= realIndex)
                            item.SetValue(Grid.RowProperty, item.GetValue(Grid.RowProperty) + 2);
                    }

                    var rowDefs = RowDefinitions;

                    rowDefs.Insert(realIndex, new RowDefinition(4, GridUnitType.Pixel));

                    if (Length == null)
                    {
                        var stars = rowDefs.Where(d => d.Height.IsStar);
                        var starSum = stars.Sum(d => d.Height.Value);
                        var size = starSum / (stars.Count() + 1);

                        rowDefs.Insert(realIndex + 1, new RowDefinition(size, GridUnitType.Star));
                    }
                    else
                        rowDefs.Insert(realIndex + 1, new RowDefinition(Length.Value.Value, Length.Value.GridUnitType));

                    var splitter = new GridSplitter();
                    splitter.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                    splitter.ResizeDirection = GridResizeDirection.Rows;
                    splitter.SetValue(Grid.RowProperty, realIndex);
                    Children.Add(splitter);

                    Element.SetValue(Grid.RowProperty, realIndex + 1);
                    Children.Add(Element);

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);
                }
                else
                {
                    foreach (var item in Children)
                    {
                        if (item.GetValue(Grid.ColumnProperty) >= realIndex)
                            item.SetValue(Grid.ColumnProperty, item.GetValue(Grid.ColumnProperty) + 2);
                    }

                    var colDefs = ColumnDefinitions;

                    colDefs.Insert(realIndex, new ColumnDefinition(4, GridUnitType.Pixel));

                    if (Length == null)
                    {
                        var stars = colDefs.Where(d => d.Width.IsStar);
                        var starSum = stars.Sum(d => d.Width.Value);
                        var size = starSum / (stars.Count() + 1);

                        colDefs.Insert(realIndex + 1, new ColumnDefinition(size, GridUnitType.Star));
                    }
                    else
                        colDefs.Insert(realIndex + 1, new ColumnDefinition(Length.Value.Value, Length.Value.GridUnitType));

                    var splitter = new GridSplitter();
                    splitter.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                    splitter.ResizeDirection = GridResizeDirection.Columns;
                    splitter.SetValue(Grid.ColumnProperty, realIndex);
                    Children.Add(splitter);

                    Element.SetValue(Grid.ColumnProperty, realIndex + 1);
                    Children.Add(Element);

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);
                }
            }
        }
        private void InsertAt(int Index, ZXDockingControl Element, GridLength? Length)
        {
            if (Index < 0)
                Index = 0;

            if(Index > (Children.Count + 1) / 2)
                Index = (Children.Count + 1) / 2;

            if (Index == 0)
                AddToStart(Element, Length);
            else if (Index == (Children.Count + 1) / 2)
                AddToEnd(Element, Length);
            else
            {
                BeginAdd(Element);

                int realIndex = Index * 2 - 1;

                if (StackOrientation == ZXStackOrientation.Vertical)
                {
                    foreach (var item in Children)
                    {
                        if (item.GetValue(Grid.RowProperty) >= realIndex)
                            item.SetValue(Grid.RowProperty, item.GetValue(Grid.RowProperty) + 2);
                    }

                    var rowDefs = RowDefinitions;

                    rowDefs.Insert(realIndex, new RowDefinition(4, GridUnitType.Pixel));

                    if (Length == null)
                    {
                        var stars = rowDefs.Where(d => d.Height.IsStar);
                        var starSum = stars.Sum(d => d.Height.Value);
                        var size = starSum / (stars.Count() + 1);

                        rowDefs.Insert(realIndex + 1, new RowDefinition(size, GridUnitType.Star));
                    }
                    else
                        rowDefs.Insert(realIndex + 1, new RowDefinition(Length.Value.Value, Length.Value.GridUnitType));

                    var splitter = new GridSplitter();
                    splitter.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                    splitter.ResizeDirection = GridResizeDirection.Rows;
                    splitter.SetValue(Grid.RowProperty, realIndex);
                    Children.Add(splitter);

                    Element.SetValue(Grid.RowProperty, realIndex + 1);
                    Children.Add(Element);

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);
                }
                else
                {
                    foreach (var item in Children)
                    {
                        if (item.GetValue(Grid.ColumnProperty) >= realIndex)
                            item.SetValue(Grid.ColumnProperty, item.GetValue(Grid.ColumnProperty) + 2);
                    }

                    var colDefs = ColumnDefinitions;

                    colDefs.Insert(realIndex, new ColumnDefinition(4, GridUnitType.Pixel));

                    if (Length == null)
                    {
                        var stars = colDefs.Where(d => d.Width.IsStar);
                        var starSum = stars.Sum(d => d.Width.Value);
                        var size = starSum / (stars.Count() + 1);

                        colDefs.Insert(realIndex + 1, new ColumnDefinition(size, GridUnitType.Star));
                    }
                    else
                        colDefs.Insert(realIndex + 1, new ColumnDefinition(Length.Value.Value, Length.Value.GridUnitType));

                    var splitter = new GridSplitter();
                    splitter.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                    splitter.ResizeDirection = GridResizeDirection.Columns;
                    splitter.SetValue(Grid.ColumnProperty, realIndex);
                    Children.Add(splitter);

                    Element.SetValue(Grid.ColumnProperty, realIndex + 1);
                    Children.Add(Element);

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);
                }
            }
        }
        public void Remove(ZXDockingControl Element)
        {
            if (!Children.Contains(Element))
                return;

            AttachedProperty<int> prop;

            if (StackOrientation == ZXStackOrientation.Vertical)
                prop = Grid.RowProperty;
            else
                prop = Grid.ColumnProperty;

            int itmIdx = 0;
            int sepIdx = -1;

            itmIdx = Element.GetValue(prop);

            Control? sep = null;

            if (itmIdx == 0)
            {
                if (Children.Count > 1)
                    sepIdx = 1;
            }
            else
            {
                if (Children.Count > 1)
                    sepIdx = itmIdx - 1;
            }

            if(sepIdx != -1)
                sep = Children.First(c => c.GetValue(prop) == sepIdx);

            Children.Remove(Element);
            if(sep != null)
                Children.Remove(sep);

            foreach (var child in Children)
            {
                if (child.GetValue(prop) > itmIdx)
                    child.SetValue(prop, child.GetValue(prop) - 2);
            }

            if (sepIdx == -1)
            {
                if (StackOrientation == ZXStackOrientation.Vertical)
                    RowDefinitions.RemoveAt(itmIdx);
                else
                    ColumnDefinitions.RemoveAt(itmIdx);
            }
            else
            {
                int minIndex = Math.Min(sepIdx, itmIdx);
                if (StackOrientation == ZXStackOrientation.Vertical)
                {

                    GridLength len = RowDefinitions[itmIdx].Height;

                    if (itmIdx == 0 && Children.Count > 2)
                    {
                        var rowVal = RowDefinitions[2].Height;
                        RowDefinitions[2].Height = new GridLength(len.Value + rowVal.Value, rowVal.GridUnitType);
                    }
                    else if (itmIdx > 0)
                    {
                        var rowVal = RowDefinitions[itmIdx - 2].Height;
                        RowDefinitions[itmIdx - 2].Height = new GridLength(len.Value + rowVal.Value, rowVal.GridUnitType);
                    }

                    RowDefinitions.RemoveAt(minIndex);
                    RowDefinitions.RemoveAt(minIndex);
                }
                else
                {
                    GridLength len = ColumnDefinitions[itmIdx].Width;

                    if (itmIdx == 0 && Children.Count > 2)
                    {
                        var rowVal = ColumnDefinitions[2].Width;
                        ColumnDefinitions[2].Width = new GridLength(len.Value + rowVal.Value, rowVal.GridUnitType);
                    }
                    else if (itmIdx > 0)
                    {
                        var rowVal = ColumnDefinitions[itmIdx - 2].Width;
                        ColumnDefinitions[itmIdx - 2].Width = new GridLength(len.Value + rowVal.Value, rowVal.GridUnitType);
                    }

                    ColumnDefinitions.RemoveAt(minIndex);
                    ColumnDefinitions.RemoveAt(minIndex);
                }
            }

            if (DockingControlsChanged != null)
                DockingControlsChanged(this, EventArgs.Empty);
        }
        private void BeginAdd(ZXDockingControl Element)
        {
            if (dropTargetPanel.Parent != null)
                Children.Remove(dropTargetPanel);

            if (Element.Parent != null)
            {
                var parent = Element.Parent as IZXDockingContainer;

                if (parent == null)
                    throw new InvalidOperationException("Only controls without parent or in a dock container can be moved to another dock container.");

                parent.Remove(Element);
            }

            Element.IsVisible = true;
            Element.TabMode = false;
        }
        private void ReorderContent()
        {
            List<(int,Control)> childs = new List<(int,Control)> ();

            var oldProp = StackOrientation == ZXStackOrientation.Vertical ? Grid.ColumnProperty : Grid.RowProperty;
            var newProp = StackOrientation == ZXStackOrientation.Vertical ? Grid.RowProperty : Grid.ColumnProperty;

            foreach(var child in Children) 
            {
                child.SetValue(newProp, child.GetValue(oldProp));
                child.SetValue(oldProp, 0);
            }

            if (StackOrientation == ZXStackOrientation.Vertical)
            {
                string cells = ColumnDefinitions.ToString();
                RowDefinitions = new RowDefinitions(cells);
                ColumnDefinitions = new ColumnDefinitions("*");
            }
            else
            {
                string cells = RowDefinitions.ToString();
                ColumnDefinitions = new ColumnDefinitions(cells);
                RowDefinitions = new RowDefinitions("*");
                
            }
        }
        private void CheckDrag(object? sender, DragEventArgs e)
        {
            if (e.Data?.Contains("DockedControl") ?? false)
            {
                var control = e.Data.Get("DockedControl") as ZXDockingControl;

                if (control == null || control.DockingGroup != DockingGroup)
                {
                    if (dropTargetPanel.Parent != null)
                        Children.Remove(dropTargetPanel);

                    e.DragEffects = DragDropEffects.None;
                }
                else
                {
                    if (control.Parent is IZXDockingContainer)
                    {
                        var dropTarget = FindDropTarget(e, control);
                        if (dropTarget == null)
                        {
                            var boundsRect = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);

                            if (boundsRect.Contains(e.GetPosition(this)) &&
                                (Children.Count == 0 ||
                                (Children.Count == 1 &&
                                Children[0] == dropTargetPanel)))
                            {
                                if (dropTargetPanel.Parent == null)
                                    Children.Add(dropTargetPanel);

                                dropTargetPanel.SetValue(Grid.ColumnProperty, 0);
                                dropTargetPanel.SetValue(Grid.RowProperty, 0);
                                dropTargetPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                                dropTargetPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                                dropTargetPanel.Width = double.NaN;
                                dropTargetPanel.Height = double.NaN;
                            }
                            else
                            {
                                e.DragEffects = DragDropEffects.None;
                                if (dropTargetPanel.Parent != null)
                                    Children.Remove(dropTargetPanel);
                            }
                        }
                        else
                        {
                            if (dropTargetPanel.Parent == null)
                                Children.Add(dropTargetPanel);

                            if (StackOrientation == ZXStackOrientation.Vertical)
                            {
                                dropTargetPanel.SetValue(Grid.ColumnProperty, 0);
                                dropTargetPanel.SetValue(Grid.RowProperty, dropTarget.OverIndex);
                                dropTargetPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                                dropTargetPanel.Height = dropTarget.Bounds.Height / 2;
                                if (dropTarget.Location == DropLocation.Top)
                                    dropTargetPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
                                else
                                    dropTargetPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                            }
                            else
                            {
                                dropTargetPanel.SetValue(Grid.ColumnProperty, dropTarget.OverIndex);
                                dropTargetPanel.SetValue(Grid.RowProperty, 0);
                                dropTargetPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                                dropTargetPanel.Width = dropTarget.Bounds.Width / 2;
                                if (dropTarget.Location == DropLocation.Left)
                                    dropTargetPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                                else
                                    dropTargetPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                            }

                            e.DragEffects = DragDropEffects.Move;

                        }

                    }
                    else
                    {
                        if (dropTargetPanel.Parent != null)
                            Children.Remove(dropTargetPanel);

                        e.DragEffects = DragDropEffects.None;
                    }
                }
            }
            else
            {
                if(dropTargetPanel.Parent != null)
                    Children.Remove(dropTargetPanel);

                e.DragEffects = DragDropEffects.None;
            }
        }
        private int GetControlIndex(ZXDockingControl Control)
        {
            if (Control == null || Control.Parent != this)
                return -1;

            if (StackOrientation == ZXStackOrientation.Vertical)
                return Control.GetValue(Grid.RowProperty) / 2;
            else
                return Control.GetValue(Grid.ColumnProperty) / 2;
        }
        private DropTarget? FindDropTarget(DragEventArgs e, ZXDockingControl DraggedControl)
        {
            DropTarget target = new DropTarget();

            var pos = e.GetPosition(this);

            foreach (var child in this.Children)
            {
                if (child is GridSplitter || child == dropTargetPanel || child == DraggedControl)
                    continue;

                if (child.Bounds.Contains(pos))
                {

                    if (child is Panel)
                        continue;

                    int idx = -1;

                    target.Bounds = child.Bounds;

                    if (StackOrientation == ZXStackOrientation.Vertical)
                    {
                        target.OverIndex = child.GetValue(Grid.RowProperty);
                        target.OverSize = RowDefinitions[target.OverIndex].Height;

                        var top = pos.Y - child.Bounds.Top;
                        bool inTopHalf = top < child.Bounds.Height / 2;

                        if (inTopHalf)
                        {
                            target.DropIndex = target.OverIndex / 2;
                            target.Location = DropLocation.Top;
                        }
                        else
                        {
                            target.DropIndex = (target.OverIndex / 2) + 1;
                            target.Location = DropLocation.Bottom;
                        }
                    }
                    else
                    {
                        target.OverIndex = child.GetValue(Grid.ColumnProperty);
                        target.OverSize = ColumnDefinitions[target.OverIndex].Width;

                        var left = pos.X - child.Bounds.Left;
                        bool InLeftHalf = left < child.Bounds.Width / 2;

                        if (InLeftHalf)
                        {
                            target.DropIndex = target.OverIndex / 2;
                            target.Location = DropLocation.Left;
                        }
                        else
                        {
                            target.DropIndex = (target.OverIndex / 2) + 1;
                            target.Location = DropLocation.Right;
                        }
                    }

                    if (DraggedControl.Parent == this && target.DropIndex > GetControlIndex(DraggedControl))
                        target.DropIndex -= 1;

                    return target;
                }
            }

            return null;
        }

        public void Select(ZXDockingControl Element)
        {
            
        }

        private class DropTarget
        {
            public Rect Bounds { get; set; }
            public DropLocation Location { get; set; }
            public int OverIndex { get; set; }
            public GridLength OverSize { get; set; }
            public int DropIndex { get; set; }
        }
        private enum DropLocation
        {
            Top,
            Bottom,
            Left,
            Right
        }
    }

    public enum ZXStackOrientation
    {
        Vertical,
        Horizontal
    }
}
