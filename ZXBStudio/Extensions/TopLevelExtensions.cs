using Avalonia.Controls;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using MessageBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace ZXBasicStudio.Extensions
{
    public static class TopLevelExtensions
    {
        public static async Task ShowError(this TopLevel Source, string Title, string Text)
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, icon: MessageBox.Avalonia.Enums.Icon.Error);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = window.Icon;
            await box.ShowDialog(window);
        }
        public static async Task ShowInfo(this TopLevel Source, string Title, string Text)
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, icon: MessageBox.Avalonia.Enums.Icon.Info);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = window.Icon;
            await box.ShowDialog(window);
        }
        public static async Task<bool> ShowConfirm(this TopLevel Source, string Title, string Text)
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, @enum: ButtonEnum.YesNo, icon: MessageBox.Avalonia.Enums.Icon.Warning);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = window.Icon;
            var result = await box.ShowDialog(window);

            if (result == ButtonResult.No)
                return false;

            return true;
        }
        public static async Task<string?> ShowInput(this TopLevel Source, string Title, string Text, string Label, string DefaultValue = "")
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            List<ButtonDefinition> buttons = new List<ButtonDefinition>();
            buttons.Add(new ButtonDefinition { Name = "Accept", IsDefault = true });
            buttons.Add(new ButtonDefinition { Name = "Cancel", IsCancel = true });
            var box = MessageBoxManager.GetMessageBoxInputWindow(new MessageBox.Avalonia.DTO.MessageBoxInputParams { ContentTitle = Title, ContentMessage = Label, ContentHeader = Text, ButtonDefinitions = buttons, Icon = MessageBox.Avalonia.Enums.Icon.Setting, WindowStartupLocation = WindowStartupLocation.CenterOwner, InputDefaultValue = DefaultValue });

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = window.Icon;
            var result = await box.ShowDialog(window);

            if (result.Button == "Cancel" || string.IsNullOrWhiteSpace(result.Message))
                return null;

            return result.Message;
        }
        public static IStorageProvider GetStorageProvider(this TopLevel Source)
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            return window.StorageProvider;
        }
    }
}
