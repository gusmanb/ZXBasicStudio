using Avalonia.Controls;
using System.Diagnostics;
using System;
using System.Linq;
using ZXBasicStudio.Common;
using Avalonia;
using ZXBasicStudio.DocumentModel.Classes;
using System.IO;
using ZXBasicStudio.Classes;
using AvaloniaEdit.Folding;
using Avalonia.Threading;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Folding;
using System.Collections.Generic;
using ZXBasicStudio.DocumentEditors.NextDows;

namespace ZXBasicStudio.DocumentEditors.NextDows
{
    /// <summary>
    /// Editor for GDUs and Fonts
    /// </summary>
    public partial class ZXFormsEditor : ZXDocumentEditorBase, IObserver<AvaloniaPropertyChangedEventArgs>
    {
        #region Events

        public override event EventHandler? DocumentRestored;
        public override event EventHandler? DocumentModified;
        public override event EventHandler? DocumentSaved;
        public override event EventHandler? RequestSaveDocument;

        #endregion


        #region ZXDocumentBase properties

        public override string DocumentName
        {
            get
            {
                return Path.GetFileName(FileName);
            }
        }


        public override string DocumentPath
        {
            get
            {
                return Path.GetFullPath(FileName);
            }
        }

        public override bool Modified
        {
            get
            {
                return _Modified;
            }
        }

        private bool _Modified = false;

        protected virtual AbstractFoldingStrategy? foldingStrategy { get { return null; } }

        #endregion


        #region IObserver implementation for modified document notifications

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(AvaloniaPropertyChangedEventArgs value)
        {

        }

        #endregion


        #region Private variables

        private string FileName = "";

        /// <summary>
        /// last zoom value
        /// </summary>
        private int lastZoom = 0;

        /// <summary>
        /// Zooms values
        /// </summary>
        private int[] zooms = new int[]
        {
            1,2,4,8,16,24,32,48,64
        };

        private FoldingManager? fManager;
        private DispatcherTimer? updateFoldingsTimer;

        private ZXFormsControl[] toolboxControls = new ZXFormsControl[0];

        #endregion


        #region ZXDocumentBase functions

        public override bool SaveDocument(TextWriter OutputLog)
        {
            try
            {
                // TODO: Implement save

                //if (!Modified)
                //{
                //    return true;
                //}


                //if (!ServiceLayer.Files_Save_GDUorFont(fileType, patterns.Select(d => d.Pattern)))
                //{
                //    OutputLog.WriteLine($"Error saving file {fileType.FileName}: " + ServiceLayer.LastError);
                //    return false;
                //}
                //_Modified = false;

                //OutputLog.WriteLine($"File {fileType.FileName} saved successfully.");
                //DocumentSaved?.Invoke(this, EventArgs.Empty);
                return false;
            }
            catch (Exception ex)
            {
                OutputLog.WriteLine($"Error saving file {FileName}: {ex.Message}");
                return false;
            }
        }


        public override bool RenameDocument(string NewName, TextWriter OutputLog)
        {
            FileName = NewName;
            return true;
        }


        public override bool CloseDocument(TextWriter OutputLog, bool ForceClose)
        {
            return true;
        }


        public override void Dispose()
        {
        }

        #endregion





        /// <summary>
        /// Constructor, without parameters is mandatory to cal Initialize
        /// </summary>
        public ZXFormsEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public ZXFormsEditor(string fileName)
        {
            InitializeComponent();
            Initialize(fileName);
            stpControlList.SizeChanged += StpControlList_SizeChanged;
        }


        /// <summary>
        /// Initialize the system
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if OK, or False if error</returns>
        public bool Initialize(string fileName)
        {
            _Modified = false;

            //ServiceLayer.Initialize();

            FileName = fileName;

            toolboxControls = new ZXFormsControl[]{
                Toolbox_AddControl("Pointer32.png", "Select control", "Select a control to edit it."),
                Toolbox_AddControl("Box32.png", "Box", "Draw a box."),
                Toolbox_AddControl("Button32.png", "Button", "A button that can be pressed by the user."),
                Toolbox_AddControl("Check32.png", "CheckBox", "A box that can be activated by the user."),
                Toolbox_AddControl("Circle32.png", "Circle", "Draw a circle."),
                Toolbox_AddControl("Image32.png", "Image", "Show an image."),
                Toolbox_AddControl("Label32.png", "Label", "Print text."),
                Toolbox_AddControl("Line32.png", "Line", "Draw a line."),
                Toolbox_AddControl("List32.png", "List", "List of selecteable items."),
                Toolbox_AddControl("Modal32.png", "Modal dialog", "A modal dialog window."),
                Toolbox_AddControl("Panel32.png", "Panel", "A box that may contain other controls."),
                Toolbox_AddControl("Radio32.png", "Radio", "One selectable control. Only one of them can be activated."),
                Toolbox_AddControl("Select32.png", "Select", "A drop-down to select an item."),
                Toolbox_AddControl("Table32.png", "Table", "A table representing tabulated data."),
                Toolbox_AddControl("TextBox32.png", "Input", "Allows the user to enter a text or numerical value.")
            };
            foreach (var ctrl in toolboxControls)
            {
                stpControlList.Children.Add(ctrl);
            }
            StpControlList_SizeChanged(null, null);

            this.ctrEditor.Initialize();

            return true;
        }


        private ZXFormsControl Toolbox_AddControl(string imageName, string title, string description)
        {
            var ctrl = new ZXFormsControl();
            ctrl.Initialize(imageName, title, description, Toolbox_Command);
            return ctrl;
        }


        private void StpControlList_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            double w = stpControlList.Bounds.BottomRight.X - 16;
            foreach (ZXFormsControl ctrl in toolboxControls)
            {
                ctrl.Resize(w);
            }
        }


        private void Toolbox_Command(ZXFormsControl control, string command)
        {
            switch (command)
            {
                case "SELECTED":
                    Toolbox_Selected(control);
                    break;
            }
        }


        private void Toolbox_Selected(ZXFormsControl control)
        {
            foreach (var ctrl in toolboxControls)
            {
                if (control != ctrl)
                {
                    ctrl.IsSelected = false;
                }
            }
            if (!control.IsSelected)
            {
                control.IsSelected = true;
            }
        }
    }
}
