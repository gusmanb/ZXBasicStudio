using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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
            mnuClearOutputWindow.Click += ClearOutputWindow;
            mnuCopyToClipboard.Click += CopyToClipboard;
            btnClearOutputWindow.Click += ClearOutputWindow;
            btnCopyToClipboard.Click += CopyToClipboard;
        }

        public void Clear()
        {
            tbOutput.Text = "";
        }
        
        private async void ClearOutputWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            tbOutput.Text = "";
        }
        
        private async void CopyToClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await DoSetClipboardTextAsync(tbOutput.Text);
        }
        
        private async Task DoSetClipboardTextAsync(string? text)
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.Clipboard is not { } provider)
                throw new NullReferenceException("Missing Clipboard instance.");
            
            await provider.SetTextAsync(text);
        }
    }
}
