using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ZXBasicStudio.Controls.DockSystem
{
    public partial class ZXTabDockingContainer : UserControl, IZXDockingContainer
    {
        public static StyledProperty<ZXTabDockingButtonPosition> TabsPositionProperty = StyledProperty<ZXTabDockingButtonPosition>.Register<ZXTabDockingContainer, ZXTabDockingButtonPosition>("TabsPosition", ZXTabDockingButtonPosition.Top);

        public event EventHandler? DockingControlsChanged;
        public ZXTabDockingButtonPosition TabsPosition
        {
            get => GetValue(TabsPositionProperty);
            set => SetValue(TabsPositionProperty, value);
        }
        public Avalonia.Controls.Controls DockedControls { get; private set; }
        public IEnumerable<ZXDockingControl> DockingControls { get { return DockedControls.Where(c => c is ZXDockingControl).Cast<ZXDockingControl>(); } }
        public ZXTabDockingContainer()
        {
            InitializeComponent();
            DragDrop.SetAllowDrop(this, true);
            tabContent.mainContainer= this;
            DockedControls = tabContent.Children;
            DockedControls.CollectionChanged += DockedControls_CollectionChanged;
            dropTargetPanel.Background = new SolidColorBrush(Colors.LightCyan, 0.5);

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

                if (dragged == null)
                    return;

                AddToEnd(dragged);
            });
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TabsPositionProperty)
                UpdateTabsPosition();
        }
        private void UpdateTabsPosition()
        {
            if (TabsPosition == ZXTabDockingButtonPosition.Top)
                tabButtons.SetValue(Grid.RowProperty, 0);
            else
                tabButtons.SetValue(Grid.RowProperty, 2);

            foreach (var child in tabButtons.Children)
            {
                if (child is not ZXTabDockingButton)
                    continue;

                var btn = (ZXTabDockingButton)child;
                btn.ButtonPosition = TabsPosition;
            }
        }
        private void CheckDrag(object? sender, DragEventArgs e)
        {
            if (e.Data?.Contains("DockedControl") ?? false)
            {
                var control = e.Data.Get("DockedControl") as ZXDockingControl;

                if (control == null)
                {
                    dropTargetPanel.IsVisible = false;
                    e.DragEffects = DragDropEffects.None;
                }
                else
                {
                    if (control.Parent is IZXDockingContainer)
                    {
                        var point = e.GetPosition(this);

                        if (tabContent.Bounds.Contains(point))
                        {
                            dropTargetPanel.IsVisible = true;
                            e.DragEffects = DragDropEffects.Move;
                        }
                        else
                        {
                            dropTargetPanel.IsVisible = false;
                            e.DragEffects = DragDropEffects.None;
                        }

                    }
                    else
                    {
                        dropTargetPanel.IsVisible = false;
                        e.DragEffects = DragDropEffects.None;
                    }
                }
            }
            else
            {
                dropTargetPanel.IsVisible = false;
                e.DragEffects = DragDropEffects.None;
            }
        }
        private void DockedControls_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    if (e.NewItems == null)
                        break;

                    int idx = e.NewStartingIndex;

                    foreach (var item in e.NewItems)
                    {
                        if (item is not ZXDockingControl)
                            throw new InvalidOperationException();

                        var dockControl = (ZXDockingControl)item;
                        
                        ZXTabDockingButton btn = new ZXTabDockingButton();
                        btn.AssociatedControl = dockControl;
                        btn.ButtonPosition = TabsPosition;
                        btn.Click += (sender, e) => 
                        {
                            if (sender == null)
                                return;

                            SelectTab((ZXTabDockingButton)sender);  
                        };
                        dockControl.TabMode = true;
                        tabButtons.Children?.Insert(idx, btn);
                        SelectTab(btn);
                        idx++;
                    }

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);

                    break;             
                    
                case NotifyCollectionChangedAction.Remove:

                    if (e.OldItems == null)
                        break;

                    foreach (var item in e.OldItems)
                    {
                        if (item is not ZXDockingControl)
                            throw new InvalidOperationException();

                        if (tabButtons.Children == null)
                            throw new InvalidOperationException();

                        ZXTabDockingButton btn = (ZXTabDockingButton)tabButtons.Children[e.OldStartingIndex];
                        btn.AssociatedControl = null;
                        tabButtons.Children.Remove(btn);

                        if (btn.IsSelected)
                        {
                            if (tabButtons.Children.Count > e.OldStartingIndex)
                            {
                                btn = (ZXTabDockingButton)tabButtons.Children[e.OldStartingIndex];
                                SelectTab(btn);
                            }
                            else if (tabButtons.Children.Count > 0)
                            {
                                btn = (ZXTabDockingButton)tabButtons.Children[tabButtons.Children.Count - 1];
                                SelectTab(btn);
                            }
                        }
                    }

                    if (DockingControlsChanged != null)
                        DockingControlsChanged(this, EventArgs.Empty);

                    break;
            }
        }
        private void SelectTab(ZXTabDockingButton Button)
        {
            foreach (var button in tabButtons.Children)
            {
                if (button is not ZXTabDockingButton)
                    continue;

                ZXTabDockingButton btn = (ZXTabDockingButton)button;
                if (btn == Button)
                { 
                    btn.IsSelected = true;
                    if (btn.AssociatedControl == null)
                        continue;
                    btn.AssociatedControl.IsVisible = true;
                }
                else
                {
                    btn.IsSelected = false;
                    if (btn.AssociatedControl == null)
                        continue;
                    btn.AssociatedControl.IsVisible = false;
                }
                
            }
        }
        public void AddToStart(ZXDockingControl Element)
        {
            BeginAdd(Element);
            DockedControls.Insert(0, Element);
        }
        public void AddToEnd(ZXDockingControl Element)
        {
            BeginAdd(Element);
            DockedControls.Insert(DockedControls.Count, Element);
        }
        public void InsertAt(int Index, ZXDockingControl Element)
        {
            BeginAdd(Element);
            DockedControls.Insert(Index, Element);
        }
        public void Remove(ZXDockingControl Element)
        {
            DockedControls.Remove(Element);
        }
        private void BeginAdd(ZXDockingControl Element)
        {
            dropTargetPanel.IsVisible = false;

            if (Element.Parent != null)
            {
                var parent = Element.Parent as IZXDockingContainer;

                if (parent == null)
                    throw new InvalidOperationException("Only controls without parent or in a dock container can be moved to another dock container.");

                parent.Remove(Element);
            }

            Element.IsVisible = true;
            Element.TabMode = true;
        }
        public void Select(ZXDockingControl Element)
        {
            var tabButton = (ZXTabDockingButton?)tabButtons.Children.FirstOrDefault(c => (c as ZXTabDockingButton)?.AssociatedControl == Element);

            if (tabButton != null)
                SelectTab(tabButton);
        }
    }

    public class ZXTabDockingInnerContainer : Panel, IZXDockingContainer
    {
        internal ZXTabDockingContainer? mainContainer;

        public IEnumerable<ZXDockingControl> DockingControls
        {
            get
            {
                return mainContainer?.DockingControls;
            }
        }

        public event EventHandler? DockingControlsChanged 
        {
            add { mainContainer.DockingControlsChanged += value; }
            remove { mainContainer.DockingControlsChanged -= value; }
        }

        public void AddToEnd(ZXDockingControl Element) => mainContainer?.AddToEnd(Element);

        public void AddToStart(ZXDockingControl Element) => mainContainer?.AddToStart(Element);

        public void InsertAt(int Index, ZXDockingControl Element) => mainContainer?.InsertAt(Index, Element);

        public void Remove(ZXDockingControl Element) => mainContainer?.Remove(Element);

        public void Select(ZXDockingControl Element) => mainContainer?.Select(Element);
    }
}
