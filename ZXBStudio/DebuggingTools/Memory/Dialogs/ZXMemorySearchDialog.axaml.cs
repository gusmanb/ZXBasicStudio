using Avalonia.Controls;
using Konamiman.Z80dotNet;
using Markdown.Avalonia.Utils;
using System;
using System.Text;
using ZXBasicStudio.DebuggingTools.Memory.Classes;
using ZXBasicStudio.DebuggingTools.Memory.Controls;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.DebuggingTools.Memory.Dialogs
{
    public partial class ZXMemorySearchDialog : Window
    {
        IMemory? mem;
        ZXMemoryView? view;
        ushort currentAddr = 0;
        public ZXMemorySearchDialog()
        {
            InitializeComponent();
            btnFind.Click += BtnFind_Click;
            btnFindNext.Click += BtnFindNext_Click;
            btnClose.Click += BtnClose_Click;
        }

        private void BtnFindNext_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Find((ushort)(Math.Max(nudStart.Value ?? 0, currentAddr)), (ushort)(nudEnd.Value ?? 65535));
        }

        private void BtnClose_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnFind_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Find((ushort)(nudStart.Value ?? 0), (ushort)(nudEnd.Value ?? 65535));
        }

        async void Find(ushort start, ushort end)
        {
            if (mem == null || view == null)
                return;

            try
            {
                byte[]? searchData = null;
                if (rbByte.IsChecked ?? false)
                {
                    if (cbHex.IsChecked ?? false)
                        searchData = new byte[] { byte.Parse(txtValue.Text ?? "", System.Globalization.NumberStyles.HexNumber) };
                    else
                    {
                        if (cbSigned.IsChecked ?? false)
                            searchData = new byte[] { (byte)sbyte.Parse(txtValue.Text ?? "") };
                        else
                            searchData = new byte[] { byte.Parse(txtValue.Text ?? "") };
                    }
                }
                else if (rbShort.IsChecked ?? false)
                {

                    if (cbHex.IsChecked ?? false)
                        searchData = BitConverter.GetBytes(ushort.Parse((txtValue.Text ?? "").Replace(" ", ""), System.Globalization.NumberStyles.HexNumber));
                    else
                    {
                        if (cbSigned.IsChecked ?? false)
                            searchData = BitConverter.GetBytes(short.Parse(txtValue.Text ?? ""));
                        else
                            searchData = BitConverter.GetBytes(ushort.Parse(txtValue.Text ?? ""));
                    }
                }
                else if (rbInt.IsChecked ?? false)
                {
                    if (cbHex.IsChecked ?? false)
                        searchData = BitConverter.GetBytes(uint.Parse((txtValue.Text ?? "").Replace(" ", ""), System.Globalization.NumberStyles.HexNumber));
                    else
                    {
                        if (cbSigned.IsChecked ?? false)
                            searchData = BitConverter.GetBytes(int.Parse(txtValue.Text ?? ""));
                        else
                            searchData = BitConverter.GetBytes(uint.Parse(txtValue.Text ?? ""));
                    }
                }
                else if (rbString.IsChecked ?? false)
                {
                    searchData = Encoding.ASCII.GetBytes(txtValue.Text ?? "");
                }

                if (searchData == null || searchData.Length == 0)
                    return;

                byte[] range = mem.GetContents(start, end - start + 1);

                int findResult = SearchBytes(range, searchData);

                if (findResult == -1)
                {
                    await this.ShowInfo("Not found", "Cannot find specified value.");
                    return;
                }

                ZXMemoryRange rng = new ZXMemoryRange { StartAddress = start + findResult, EndAddress = start + findResult + searchData.Length - 1 };

                view.HighlightedRange = rng;
                view.GoToAddress((ushort)rng.StartAddress);
                view.Update();
                currentAddr = (ushort)(rng.StartAddress < 65535 ? rng.StartAddress + 1 : rng.StartAddress);
            }
            catch { await this.ShowError("Error", "Error searching value, possible invalid input."); }
        }

        int SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        public void Initialize(IMemory Memory, ZXMemoryView View)
        {
            mem = Memory;
            view = View;
        }
    }
}
