using Avalonia;
using Avalonia.Controls;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Threading;
using MessageBox.Avalonia.Models;
using Avalonia.Controls.Primitives;

namespace ZXBasicStudio.Common
{
    public class UI
    {
        private static Window MainWindow = null;
        private static UI Instance = null;
        private static WindowIcon? Icon = null;


        public bool Initialize(Window ownnerWindow, WindowIcon? icon)
        {
            Instance = this;
            MainWindow = ownnerWindow;
            UI.Icon = icon;
            return true;
        }


        public async Task<bool> ShowConfirm(Window ownnerWindow, string Title, string Text)
        {
            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, @enum: ButtonEnum.YesNo, icon: MessageBox.Avalonia.Enums.Icon.Warning);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = Icon;
            var result = await box.ShowDialog(MainWindow);

            if (result == ButtonResult.No)
                return false;

            return true;
        }


        public static void ShowError(string Title, string Text)
        {
            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, icon: MessageBox.Avalonia.Enums.Icon.Error);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = Icon;
            box.ShowDialog(MainWindow);
        }
    }
}
