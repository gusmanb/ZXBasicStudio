using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DebuggingTools.Memory.Binding
{
    public class ZXMemoryRow : AvaloniaObject
    {
        public static StyledProperty<string> RowAddressProperty = AvaloniaProperty.Register<ZXMemoryRow, string>("RowAddress");
        public static StyledProperty<string> Column1Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column1");
        public static StyledProperty<string> Column2Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column2");
        public static StyledProperty<string> Column3Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column3");
        public static StyledProperty<string> Column4Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column4");
        public static StyledProperty<string> Column5Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column5");
        public static StyledProperty<string> Column6Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column6");
        public static StyledProperty<string> Column7Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column7");
        public static StyledProperty<string> Column8Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column8");
        public static StyledProperty<string> Column9Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column9");
        public static StyledProperty<string> Column10Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column10");
        public static StyledProperty<string> Column11Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column11");
        public static StyledProperty<string> Column12Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column12");
        public static StyledProperty<string> Column13Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column13");
        public static StyledProperty<string> Column14Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column14");
        public static StyledProperty<string> Column15Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column15");
        public static StyledProperty<string> Column16Property = AvaloniaProperty.Register<ZXMemoryRow, string>("Column16");

        public string RowAddress
        {
            get { return GetValue(RowAddressProperty); }
            set { SetValue(RowAddressProperty, value); }
        }
        public string Column1
        {
            get { return GetValue(Column1Property); }
            set { SetValue(Column1Property, value); }
        }
        public string Column2
        {
            get { return GetValue(Column2Property); }
            set { SetValue(Column2Property, value); }
        }
        public string Column3
        {
            get { return GetValue(Column3Property); }
            set { SetValue(Column3Property, value); }
        }
        public string Column4
        {
            get { return GetValue(Column4Property); }
            set { SetValue(Column4Property, value); }
        }
        public string Column5
        {
            get { return GetValue(Column5Property); }
            set { SetValue(Column5Property, value); }
        }
        public string Column6
        {
            get { return GetValue(Column6Property); }
            set { SetValue(Column6Property, value); }
        }
        public string Column7
        {
            get { return GetValue(Column7Property); }
            set { SetValue(Column7Property, value); }
        }
        public string Column8
        {
            get { return GetValue(Column8Property); }
            set { SetValue(Column8Property, value); }
        }
        public string Column9
        {
            get { return GetValue(Column9Property); }
            set { SetValue(Column9Property, value); }
        }
        public string Column10
        {
            get { return GetValue(Column10Property); }
            set { SetValue(Column10Property, value); }
        }
        public string Column11
        {
            get { return GetValue(Column11Property); }
            set { SetValue(Column11Property, value); }
        }
        public string Column12
        {
            get { return GetValue(Column12Property); }
            set { SetValue(Column12Property, value); }
        }
        public string Column13
        {
            get { return GetValue(Column13Property); }
            set { SetValue(Column13Property, value); }
        }
        public string Column14
        {
            get { return GetValue(Column14Property); }
            set { SetValue(Column14Property, value); }
        }
        public string Column15
        {
            get { return GetValue(Column15Property); }
            set { SetValue(Column15Property, value); }
        }
        public string Column16
        {
            get { return GetValue(Column16Property); }
            set { SetValue(Column16Property, value); }
        }

        public void Update(ushort Address, int RowNumber, byte[] Block)
        {
            RowAddress = Address.ToString("X4");
            Column1 = Block[RowNumber * 16 + 0].ToString("X2");
            Column2 = Block[RowNumber * 16 + 1].ToString("X2");
            Column3 = Block[RowNumber * 16 + 2].ToString("X2");
            Column4 = Block[RowNumber * 16 + 3].ToString("X2");
            Column5 = Block[RowNumber * 16 + 4].ToString("X2");
            Column6 = Block[RowNumber * 16 + 5].ToString("X2");
            Column7 = Block[RowNumber * 16 + 6].ToString("X2");
            Column8 = Block[RowNumber * 16 + 7].ToString("X2");
            Column9 = Block[RowNumber * 16 + 8].ToString("X2");
            Column10 = Block[RowNumber * 16 + 9].ToString("X2");
            Column11 = Block[RowNumber * 16 + 10].ToString("X2");
            Column12 = Block[RowNumber * 16 + 11].ToString("X2");
            Column13 = Block[RowNumber * 16 + 12].ToString("X2");
            Column14 = Block[RowNumber * 16 + 13].ToString("X2");
            Column15 = Block[RowNumber * 16 + 14].ToString("X2");
            Column16 = Block[RowNumber * 16 + 15].ToString("X2");
        }
    }
}
