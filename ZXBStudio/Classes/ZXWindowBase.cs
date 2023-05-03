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

namespace ZXBasicStudio.Classes
{
    public class ZXWindowBase : Window
    {
        static JsonSerializerSettings jSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented };

        static AppConfig config;
        static ZXWindowBase()
        {
            if (File.Exists("AppSettings.json"))
                config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText("AppSettings.json"), jSettings) ?? new AppConfig();

            if (config == null)
                config = new AppConfig();
        }
        protected virtual bool PersistBounds { get { return false; } }
        public ZXWindowBase() : base() { }
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (OperatingSystem.IsWindows())
            {
                // Not needed for Windows
                return;
            }

            var scale = PlatformImpl?.DesktopScaling ?? 1.0;
            var pOwner = Owner?.PlatformImpl;
            if (pOwner != null)
            {
                scale = pOwner.DesktopScaling;
            }
            var rect = new PixelRect(PixelPoint.Origin,
                PixelSize.FromSize(ClientSize, scale));

            if (WindowStartupLocation == WindowStartupLocation.CenterScreen)
            {
                var screen = Screens.ScreenFromPoint(pOwner?.Position ?? Position);
                if (screen == null)
                {
                    return;
                }
                Position = screen.WorkingArea.CenterRect(rect).Position;
            }
            else
            {
                if (pOwner == null ||
                    WindowStartupLocation != WindowStartupLocation.CenterOwner)
                {
                    return;
                }

                Position = new PixelRect(pOwner.Position,
                    PixelSize.FromSize(pOwner.ClientSize, scale)).CenterRect(rect).Position;
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (!Design.IsDesignMode && PersistBounds && config.WindowSettings.ContainsKey(this.GetType().FullName))
            {
                var settings = config.WindowSettings[this.GetType().FullName];
                this.Width = settings.Width;
                this.Height = settings.Height;
                this.WindowState = settings.State;
            }
        }

        /*
        protected async Task ShowError(string Title, string Text)
        {
            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, icon: MessageBox.Avalonia.Enums.Icon.Error);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = Icon;
            await box.ShowDialog(this);
        }
        protected async Task ShowInfo(string Title, string Text)
        {
            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, icon: MessageBox.Avalonia.Enums.Icon.Info);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = Icon;
            await box.ShowDialog(this);
        }
        protected async Task<bool> ShowConfirm(string Title, string Text)
        {
            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, @enum: ButtonEnum.YesNo, icon: MessageBox.Avalonia.Enums.Icon.Warning);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = Icon;
            var result = await box.ShowDialog(this);

            if (result == ButtonResult.No)
                return false;

            return true;
        }
        protected async Task<string?> ShowInput(string Title, string Text, string Label, string DefaultValue = "")
        {
            List<ButtonDefinition> buttons = new List<ButtonDefinition>();
            buttons.Add(new ButtonDefinition { Name = "Accept", IsDefault = true });
            buttons.Add(new ButtonDefinition { Name = "Cancel", IsCancel = true }) ;
            var box = MessageBoxManager.GetMessageBoxInputWindow(new MessageBox.Avalonia.DTO.MessageBoxInputParams { ContentTitle = Title, ContentMessage = Label, ContentHeader = Text, ButtonDefinitions = buttons, Icon = MessageBox.Avalonia.Enums.Icon.Setting, WindowStartupLocation = WindowStartupLocation.CenterOwner, InputDefaultValue = DefaultValue });

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = Icon;
            var result = await box.ShowDialog(this);

            if (result.Button == "Cancel" || string.IsNullOrWhiteSpace(result.Message))
                return null;

            return result.Message;
        }
        */

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            if (PersistBounds)
            {
                WindowStatus status = new WindowStatus { Height = this.Height, Width = this.Width, State = this.WindowState };
                config.WindowSettings[this.GetType().FullName] = status;
                File.WriteAllText("AppSettings.json", JsonConvert.SerializeObject(config, jSettings));
            }
        }

        class AppConfig
        {
            public string AppName { get; set; } = Assembly.GetEntryAssembly().FullName;
            public Dictionary<string, WindowStatus> WindowSettings { get; set; } = new Dictionary<string, WindowStatus>();
        }

        public class WindowStatus
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public WindowState State { get; set; }
        }
    }
}
