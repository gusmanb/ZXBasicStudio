using Avalonia.Controls;
using System.Diagnostics;
using ZXGraphics.log;
using ZXGraphics.neg;

namespace ZXGraphics.ui
{
    public partial class Main : UserControl
    {
        private FileTypeConfig fileType = null;
        private byte[] fileData = null;
        private PatternControl[] patterns = null;


        /// <summary>
        /// Constructor, without parameters is mandatory to cal Initialize
        /// </summary>
        public Main()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public Main(string fileName)
        {
            InitializeComponent();
            Initialize(fileName);
        }


        /// <summary>
        /// Initialize the system
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if OK, or False if error</returns>
        public bool Initialize(string fileName)
        {
            ServiceLayer.Initialize();

            fileType = ServiceLayer.GetFileType(fileName);
            fileData = ServiceLayer.GetFileData(fileName);

            UpdatePatterns();
            return true;
        }


        public void UpdatePatterns()
        {
            if (patterns == null)
            {
                // Create patterns if not exist
                patterns = new PatternControl[fileType.NumerOfPatterns];
                int x = 0;
                int y = 0;
                for (int n = 0; n < fileType.NumerOfPatterns; n++)
                {
                    var ctrl = new PatternControl();                    
                    cnvPatterns.Children.Add(ctrl);
                    Canvas.SetTop(ctrl, y);
                    Canvas.SetLeft(ctrl, x);
                    x = x + 36;
                    if (x > 139)
                    {
                        x = 0;
                        y = y + 56;
                    }
                    patterns[n] = ctrl;
                }
            }

            // Update patterns
            for (int n = 0; n < patterns.Length; n++)
            {
                var p = new Pattern();
                p.Id = n;
                p.Number = "";
                switch (fileType.FileType)
                {
                    case FileTypes.GDU:
                        {
                            var id = n;
                            p.Number = id.ToString();
                            var c = Convert.ToChar(n + 65);
                            p.Name = c.ToString();
                        }
                        break;
                    case FileTypes.Font:
                        {
                            var id = n + 32;
                            p.Number = id.ToString();
                            var c = Convert.ToChar(n + 32);
                            p.Name = c.ToString();
                        }
                        break;
                    default:
                        p.Number = n.ToString();
                        p.Name = "";
                        break;
                }
                p.Data = ServiceLayer.Binary2PointData(n, fileData, 0, 0);
                patterns[n].Data = p;
                patterns[n].Refresh();
            }
        }
    }
}
