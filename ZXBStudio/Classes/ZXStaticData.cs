using Avalonia.Data.Converters;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public static class ZXStaticData
    {
        static ZXStaticData()
        {
            Keys = Enum.GetValues<Key>();
            Modifiers = Enum.GetValues<KeyModifiers>().Skip(1).ToArray();
        }
        public static Key[] Keys { get; private set; }
        public static KeyModifiers[] Modifiers { get; private set; }
    }

    public class KeyModifiersConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var cmd = (ZXKeybCommand)value;

            var values = Enum.GetValues(typeof(KeyModifiers));

            ObservableCollection<KeyModifiers> list = new ObservableCollection<KeyModifiers>();

            foreach (KeyModifiers v in values ) 
            {
                if(cmd.Modifiers.HasFlag(v) && v != KeyModifiers.None)
                    list.Add(v);
            }

            list.CollectionChanged += (s, e) => 
            {
                var value = KeyModifiers.None;

                foreach (var item in list)
                    value |= item;

                cmd.Modifiers = value;
            };

            return list;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
