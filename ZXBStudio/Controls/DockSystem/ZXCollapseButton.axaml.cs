using Avalonia;
using Avalonia.Controls;
using System.Data;

namespace ZXBasicStudio.Controls.DockSystem
{
    public partial class ZXCollapseButton : UserControl
    {
        public static StyledProperty<CollapseDirection> CollapseDirectionProperty = StyledProperty<CollapseDirection>.Register<ZXCollapseButton, CollapseDirection>("CollapseDirection", CollapseDirection.Left);

        public static StyledProperty<int> GridColumnProperty = StyledProperty<int>.Register<ZXCollapseButton, int>("GridColumn");

        public static StyledProperty<double> ExpandedSizeProperty = StyledProperty<double>.Register<ZXCollapseButton, double>("ExpandedSize", 1);

        public CollapseDirection CollapseDirection
        {
            get => GetValue(CollapseDirectionProperty);
            set { SetValue(CollapseDirectionProperty, value); UpdateLocation(); }
        }

        public int GridColumn
        {
            get => GetValue(GridColumnProperty);
            set { SetValue(GridColumnProperty, value); UpdateLocation(); }
        }

        public double ExpandedSize
        {
            get => GetValue(ExpandedSizeProperty);
            set => SetValue(ExpandedSizeProperty, value);
        }

        public ZXCollapseButton()
        {
            InitializeComponent();
            UpdateLocation();
            btnCollapse.Click += BtnCollapse_Click;
        }

        private void BtnCollapse_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var parentGrid = Parent as Grid;

            if (parentGrid == null)
                return;

            var cols = parentGrid.ColumnDefinitions;

            if (cols.Count <= GridColumn)
                return;

            var colSize = cols[GridColumn];
            var width = colSize.Width;
            if (!width.IsStar)
                return;

            if (width.Value == 0)
                colSize.Width = new GridLength(ExpandedSize, GridUnitType.Star);
            else
            {
                ExpandedSize = width.Value;
                colSize.Width = new GridLength(0, GridUnitType.Star);
            }

            UpdateStatus();
        }

        void UpdateLocation()
        {
            if (CollapseDirection == CollapseDirection.Left)
                Margin = new Thickness(0, 0, -10, 0);
            else
                Margin = new Thickness(-10, 0, 0, 0);

            UpdateStatus();
        }

        void UpdateStatus()
        {
            var parentGrid = Parent as Grid;

            if (parentGrid == null)
                return;

            var cols = parentGrid.ColumnDefinitions;

            if (cols.Count <= GridColumn)
                return;

            var colSize = cols[GridColumn];
            var width = colSize.Width;
            if (!width.IsStar)
                return;

            if (width.Value == 0)
            {
                if (CollapseDirection == CollapseDirection.Left)
                    svgImg.Path = "/Svg/White/forward-step-solid.svg";
                else
                    svgImg.Path = "/Svg/White/backward-step-solid.svg";
            }
            else
            {
                if (CollapseDirection == CollapseDirection.Left)
                    svgImg.Path = "/Svg/White/backward-step-solid.svg";
                else
                    svgImg.Path = "/Svg/White/forward-step-solid.svg";
            }
        }
    }

    public enum CollapseDirection
    {
        Left,
        Right
    }
}
