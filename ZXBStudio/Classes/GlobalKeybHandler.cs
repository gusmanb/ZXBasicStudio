using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public static class GlobalKeybHandler
    {
        public static event EventHandler<KeyEventArgs>? KeyDown;
        public static event EventHandler<KeyEventArgs>? KeyUp;
        public static event EventHandler<TextInputEventArgs>? TextInput;

        static GlobalKeybHandler()
        {
            InputElement.KeyDownEvent.AddClassHandler<TopLevel>((o, e) => 
            { 
                if (KeyDown != null) 
                    KeyDown(o, e); 

            }, handledEventsToo: true);

            InputElement.KeyUpEvent.AddClassHandler<TopLevel>((o, e) =>
            {
                if (KeyUp != null)
                    KeyUp(o, e);

            }, handledEventsToo: true);

            InputElement.TextInputEvent.AddClassHandler<TopLevel>((o, e) =>
            {
                if (TextInput != null)
                    TextInput(o, e);

            }, handledEventsToo: true);
        }
    }
}
