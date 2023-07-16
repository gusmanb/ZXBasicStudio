using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.Dialogs
{
    public partial class ZXKeybMappingDialog : Window
    {
        Timer tmrReset;
        public ZXKeybMappingDialog()
        {
            InitializeComponent();
            tmrReset = new Timer((e) => 
            {
                Dispatcher.UIThread.Invoke(() => 
                {
                    scrMaps.ScrollChanged -= ScrMaps_ScrollChanged;
                });
            });
            selSource.SelectionChanged += SelSource_SelectionChanged;
            DataContext = ZXKeybMapper.GetKeybCommands();
            btnAccept.Click += BtnAccept_Click;
            btnCancel.Click += BtnCancel_Click;
            btnRestore.Click += BtnRestore_Click;
        }

        private async void BtnRestore_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!await this.ShowConfirm("Confirm restore", "Are you sure you want to restore the keyboard configuration to its default?"))
                return;

            ZXKeybMapper.RestoreDefaults();
            DataContext = ZXKeybMapper.GetKeybCommands();
        }

        private void SelSource_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            scrMaps.ScrollChanged += ScrMaps_ScrollChanged;
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAccept_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var sources = DataContext as ZXKeybMapperSource[];

            if (sources == null)
                return;

            foreach (var source in sources)
                ZXKeybMapper.UpdateCommands(source.SourceId, source.KeybCommands);

            this.Close();
        }

        private void ScrMaps_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            scrMaps.ScrollToHome();
            tmrReset.Change(100, Timeout.Infinite);
        }
    }
}
