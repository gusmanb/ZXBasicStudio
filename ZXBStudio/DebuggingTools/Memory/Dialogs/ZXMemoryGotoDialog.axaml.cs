using Avalonia.Controls;
using Konamiman.Z80dotNet;
using Markdown.Avalonia.Utils;
using System;
using System.Text;
using Tmds.DBus.Protocol;
using ZXBasicStudio.DebuggingTools.Memory.Classes;
using ZXBasicStudio.DebuggingTools.Memory.Controls;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.DebuggingTools.Memory.Dialogs
{
    public partial class ZXMemoryGotoDialog : Window
    {
        IMemory? mem;
        ZXMemoryView? view;
        ushort currentAddr = 0;
        public ZXMemoryGotoDialog()
        {
            InitializeComponent();
            nudAddress.Value = currentAddr;
            btnGoto.Click += BtnGoto_Click;
            btnClose.Click += BtnClose_Click;
        }
        
        private void BtnClose_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnGoto_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Goto((ushort)(nudAddress.Value ?? 0));
        }

        async void Goto(ushort address)
        {
            if (mem == null || view == null)
                return;

            try
            {
                ZXMemoryRange rng = new ZXMemoryRange { StartAddress = address , EndAddress = address };

                view.HighlightedRange = rng;
                nudAddress.Value = address;
                view.GoToAddress(address);
                view.Update();
                currentAddr = (ushort)(address < 65535 ? address + 1 : address);
            }
            catch { await this.ShowError("Error", "Error, possible invalid input."); }
        }
        
        public void Initialize(IMemory Memory, ZXMemoryView View)
        {
            mem = Memory;
            view = View;
        }
    }
}
