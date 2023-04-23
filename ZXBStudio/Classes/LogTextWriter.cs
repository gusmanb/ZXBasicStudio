using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class LogTextWriter : TextWriter
    {
        TextBlock target;
        ScrollViewer scroll;

        public LogTextWriter(TextBlock Target, ScrollViewer Scroller) 
        {
            this.target = Target;
            this.scroll = Scroller;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                target.Text = "";
                scroll.ScrollToHome();
            });
        }
        public override Encoding Encoding
        {
            get
            {
                return Encoding.Unicode;
            }
        }

        public override void Write(char value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value; scroll.ScrollToEnd(); }));
            
        }

        public override void WriteLine()
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(bool value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(char value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += new string(buffer, index, count) + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(char[]? buffer)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += new string(buffer ?? new char[0]) + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(decimal value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }
        public override void WriteLine(double value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }
        public override void WriteLine(float value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }
        public override void WriteLine(int value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(long value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(object? value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += (value?.ToString() ?? "") + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            string data = new string(buffer);
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += data + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine([StringSyntax("CompositeFormat")] string format, object? arg0)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg0) + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg0, arg1) + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1, object? arg2)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg0, arg1, arg2) + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine([StringSyntax("CompositeFormat")] string format, params object?[] arg)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg) + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(string? value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += (value ?? "") + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(StringBuilder? value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += (value?.ToString() ?? "") + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(uint value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }

        public override void WriteLine(ulong value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString() + Environment.NewLine; scroll.ScrollToEnd(); }));
        }
        public override Task WriteLineAsync()
        {
            throw new NotSupportedException();
        }

        public override Task WriteLineAsync(char value)
        {
            throw new NotSupportedException();
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            throw new NotSupportedException();
        }
        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
        public override Task WriteLineAsync(string? value)
        {
            throw new NotSupportedException();
        }
        public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public override void Write(bool value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += new string(buffer, index, count); scroll.ScrollToEnd(); }));
        }
        public override void Write(char[]? buffer)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += new string(buffer ?? new char[0]); scroll.ScrollToEnd(); }));
        }

        public override void Write(decimal value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }
        public override void Write(double value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }
        public override void Write(float value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }
        public override void Write(int value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }
        public override void Write(long value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }
        public override void Write(object? value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value?.ToString() ?? ""; scroll.ScrollToEnd(); }));
        }
        public override void Write(ReadOnlySpan<char> buffer)
        {
            string data = new string(buffer);
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += data; scroll.ScrollToEnd(); }));
        }
        public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg0); scroll.ScrollToEnd(); }));
        }
        public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg0, arg1); scroll.ScrollToEnd(); }));
        }
        public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1, object? arg2)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg0, arg1, arg2); scroll.ScrollToEnd(); }));
        }
        public override void Write([StringSyntax("CompositeFormat")] string format, params object?[] arg)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += string.Format(format, arg); scroll.ScrollToEnd(); }));
        }
        public override void Write(string? value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value ?? ""; scroll.ScrollToEnd(); }));
        }
        public override void Write(StringBuilder? value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value?.ToString() ?? ""; scroll.ScrollToEnd(); }));
        }

        public override void Write(uint value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }

        public override void Write(ulong value)
        {
            Dispatcher.UIThread.InvokeAsync(new Action(() => { target.Text += value.ToString(); scroll.ScrollToEnd(); }));
        }
        public override Task WriteAsync(char value)
        {
            throw new NotSupportedException();
        }
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            throw new NotSupportedException();
        }
        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
        public override Task WriteAsync(string? value)
        {
            throw new NotSupportedException();
        }
        public override Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
