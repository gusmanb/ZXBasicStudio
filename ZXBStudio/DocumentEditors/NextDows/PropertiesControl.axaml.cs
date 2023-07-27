using Avalonia.Controls;
using System;
using System.Collections.Generic;
using ZXBasicStudio.DocumentEditors.NextDows.neg;
using System.Linq;
using Avalonia.Data;

namespace ZXBasicStudio.DocumentEditors.NextDows
{
    public partial class PropertiesControl : UserControl
    {
        public List<ControlItem> Controls
        {
            get
            {
                return _controls;
            }
            set
            {
                _controls = value;
                RefreshList();
            }
        }

        private List<ControlItem> _controls = null;

        public ControlItem Control
        {
            get
            {
                return _control;
            }
            set
            {
                _control = value;
                Refresh();
            }
        }

        private ControlItem _control = null;

        private Action<string, ControlItem> callBackCommand = null;


        public PropertiesControl()
        {
            InitializeComponent();
            cmbControls.SelectedValueBinding = new Binding(nameof(ComboBoxItem.Tag));
        }


        public void Initilize(Action<string, ControlItem> callBackCommand)
        {
            this.callBackCommand = callBackCommand;
        }


        private void RefreshList()
        {
            var lst = from d in _controls
                      orderby d.Id
                      select new ComboBoxItem()
                      {
                          Tag = d.Id,
                          Content = string.Format("{0}: {1} ({2})", d.Id, d.Name, d.ControlType.ToString())
                      };
            cmbControls.ItemsSource = lst;
        }


        private void Refresh()
        {
            var cs = _controls.FirstOrDefault(d => d.Id == _control.Id);
            if (cs != null)
            {
                cmbControls.SelectedValue = _control.Id;
                UpdateProperties();
            }
        }


        private void UpdateProperties()
        {
            txtId.Text = _control.Id.ToString();
            txtName.Text = _control.Name;
            txtControlType.Text = _control.ControlType.ToString();
            cmbVisible.SelectedIndex = _control.Visible ? 0 : 1;
            txtContainerId.Text = _control.ContainerId.ToString();
            txtLeft.Text = _control.Left.ToString();
            txtTop.Text = _control.Top.ToString();
            txtWidth.Text = _control.Width.ToString();
            txtHeight.Text = _control.Height.ToString();
            txtInk.Text=_control.Ink.ToString();
            txtPaper.Text = _control.Paper.ToString();

            // Optional properties
            lblTextAlign.IsVisible = false;
            cmbTextAlign.IsVisible = false;
            lblText.IsVisible = false;
            txtText.IsVisible = false;            
        }
    }
}
