using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions
{
    public class ZXBasicCompletionData : ICompletionData
    {

        static IImage keywordIcon;
        static IImage typeIcon;
        static IImage directiveIcon;

        static ZXBasicCompletionData()
        {
            keywordIcon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/KeywordIcon.png")));
            typeIcon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/TypeIcon.png")));
            directiveIcon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/DirectiveIcon.png")));
        }

        public ZXBasicCompletionData(ZXBasicCompletionType DataType, string Text, string Description)
        {
            this.Text = Text;
            this.Description = Description;

            switch(DataType)
            {
                case ZXBasicCompletionType.Keyword:
                    Image = keywordIcon;
                    Priority = 3;
                    break;
                case ZXBasicCompletionType.Type:
                    Image = typeIcon;
                    Priority = 2;
                    break;
                case ZXBasicCompletionType.Directive:
                    Image = directiveIcon;
                    Priority = 1;
                    break;
                default:
                    throw new InvalidCastException("Invalid ZXBasicCompletionType");
            }
        }

        public IImage Image { get; }

        public string Text { get; }

        public object Content => Text;

        public object Description { get; }

        public double Priority { get; } = 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }

    public enum ZXBasicCompletionType
    {
        Keyword,
        //Function,
        //Variable,
        //Constant,
        Type,
        //Label,
        //Macro,
        //Operator,
        Directive,
        //Comment,
        //String,
        //Number,
        //Other
    }

}
