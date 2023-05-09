using Avalonia.Controls;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXOutputLog : UserControl
    {
        ZXLogTextWriter _writer;

        public ZXLogTextWriter Writer { get { return _writer; } }

        public ZXOutputLog()
        {
            InitializeComponent();
            _writer = new ZXLogTextWriter(tbOutput, scrOutput);
        }

        public void Clear()
        {
            tbOutput.Text = "";
        }
    }
}
