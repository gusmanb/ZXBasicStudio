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

        private List<ExportTypeDescrioptionItem> ExportTypesList = null;

        public SelectExportTypeControl()
        {
            InitializeComponent();
        }


        public bool InitializeSprite()
        {
            ExportTypesList = new List<ExportTypeDescrioptionItem>();
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "Boriel Basic's PutChars embeded library.",
                ExportType = ExportTypes.PutChars,
                Image = "/Svg/Seal.svg",
                Name = "PutChars"
            });
            /*
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "Dr. Gusman GuSprites library.",
                ExportType = ExportTypes.GUSprite,
                Image = "/Svg/binary.svg",
                Name = "GuSprites"
            });
            ExportTypesList.Add(new ExportTypeDescrioptionItem()
            {
                Description = "FourSpriter from Mojon Twins allow to draw up to four sprites of 16x16 pixels with background preservation.",
                ExportType = ExportTypes.FourSprites,
                Image = "/Svg/binary.svg",
                Name = "FourSpriter"
            });
            */
            lstOptions.ItemsSource = ExportTypesList;

            lstOptions.SelectionChanged += LstOptions_SelectionChanged;

            return true;
        }


        public bool InitializeFontGDU()
        {
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
                Description = "It is built as .tap, which allows loading with LOAD \"\" CODE.",
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

        public bool Initialize(Action<ExportTypes> callbackSelectionChanged)
        {
            this.SelectionChanged = callbackSelectionChanged;
            return true;
        }


        private void LstOptions_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(ExportType);
        }
    }
}
