using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Dto;

namespace ZXBasicStudio.Extensions
{
    public static class TopLevelExtensions
    {
        public static async Task ShowError(this TopLevel Source, string Title, string Text)
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            var box = MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = Title,
                    ContentMessage = Text,
                    Icon = Icon.Error,
                    WindowIcon = window.Icon,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });

            //var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            //var win = prop.GetValue(box) as Window;

            //win.Icon = window.Icon;
            await box.ShowWindowDialogAsync(window);
        }
        public static async Task ShowInfo(this TopLevel Source, string Title, string Text)
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = Title,
                ContentMessage = Text,
                Icon = Icon.Info,
                WindowIcon = window.Icon,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

            await box.ShowWindowDialogAsync(window);
        }
        public static async Task<bool> ShowConfirm(this TopLevel Source, string Title, string Text)
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.YesNo,
                ContentTitle = Title,
                ContentMessage = Text,
                Icon = Icon.Warning,
                WindowIcon = window.Icon,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

            var result = await box.ShowWindowDialogAsync(window);

            if (result == ButtonResult.No)
                return false;

            return true;
        }
        public static async Task<string?> ShowInput(this TopLevel Source, string Title, string Text, string Label, string DefaultValue = "")
        {
            if (Source is not Window)
                throw new ArgumentException("Dialog source must be a window.");

            var window = (Window)Source;

            var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                ContentTitle = Title,
                ContentMessage = Text,
                Icon = Icon.Warning,
                WindowIcon = window.Icon,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                InputParams = new InputParams { Label = Label, DefaultValue = DefaultValue }
            });

            var result = await box.ShowWindowDialogAsync(window);

            if (result == ButtonResult.Cancel)
                return null;

            return box.InputValue;
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
