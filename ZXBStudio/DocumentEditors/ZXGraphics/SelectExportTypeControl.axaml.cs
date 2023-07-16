using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class SelectExportTypeControl : UserControl
    {
        public Action<ExportTypes> SelectionChanged;

        public ExportTypes ExportType
        {
            get
            {
                try
                {
                    if (lstOptions.SelectedItem == null)
                    {
                        return ExportTypes.None;
                    }
                    else
                    {
                        return ((ExportTypeDescrioptionItem)lstOptions.SelectedItem).ExportType;
                    }
                }
                catch
                {
                    return ExportTypes.None;
                }
            }
            set
            {
                var option = ExportTypesList.FirstOrDefault(d => d.ExportType == value);
                lstOptions.SelectedItem = option;
            }
        }
        private ExportTypes _ExportType = ExportTypes.Bin;

        private List<ExportTypeDescrioptionItem> ExportTypesList = null;
      

        public SelectExportTypeControl()
        {
            InitializeComponent();
        }


        public bool Initialize(Action<ExportTypes> callbackSelectionChanged)
        {
            this.SelectionChanged = callbackSelectionChanged;

            ExportTypesList = new List<ExportTypeDescrioptionItem>();
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "No conversion is performed, the file can be included in an asm with \"incbin\".",
                ExportType = ExportTypes.Bin,
                Image = "/Svg/binary.svg",
                Name = "Raw data"
            });
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "It is exported as .tap, which allows loading with LOAD \"\" CODE.",
                ExportType = ExportTypes.Tap,
                Image = "/Svg/cassette-solid.svg",
                Name = ".tap format"
            });
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "The data is integrated into the inline assembler.",
                ExportType = ExportTypes.Asm,
                Image = "/Svg/text-asm.svg",
                Name = "ASM format"
            });
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "An array is created and data is included in the initialisation of the array.",
                ExportType = ExportTypes.Dim,
                Image = "/Svg/text-dim.svg",
                Name = "DIM format"
            });
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "The data are included in traditional DATA lines.",
                ExportType = ExportTypes.Data,
                Image = "/Svg/text-data.svg",
                Name = "DATA format"
            });

            lstOptions.ItemsSource = ExportTypesList;

            lstOptions.SelectionChanged += LstOptions_SelectionChanged;

            return true;
        }


        private void LstOptions_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(ExportType);
        }
    }
}
