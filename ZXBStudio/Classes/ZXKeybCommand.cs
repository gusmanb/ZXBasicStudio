using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;

namespace ZXBasicStudio.Classes
{
    public partial class ZXKeybCommand : ObservableObject
    {
        public required Guid CommandId { get; set; }

        [ObservableProperty]
        string commandName = "";

        [ObservableProperty]
        Key key = Key.None;

        [ObservableProperty]
        KeyModifiers modifiers = KeyModifiers.None;
    }
}
