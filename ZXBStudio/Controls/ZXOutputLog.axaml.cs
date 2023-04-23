using Avalonia.Controls;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXOutputLog : UserControl
    {
        LogTextWriter _writer;

        public LogTextWriter Writer { get { return _writer; } }

        public ZXOutputLog()
        {
            InitializeComponent();
            _writer = new LogTextWriter(tbOutput, scrOutput);
        }

        public void Clear()
        {
            tbOutput.Text = "";
        }
    }
}
