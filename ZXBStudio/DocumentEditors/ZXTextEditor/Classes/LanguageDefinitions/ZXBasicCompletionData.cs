using Avalonia.Media;
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
        public IImage? Image { get; set; }

        public required string? Text { get; set; }

        public object? Content { get { return new internalIcompletion {  Content = Text }; } }

        public object? Description { get; set; }

        public double Priority { get; set; }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        class internalIcompletion : ICompletionData
        {
            public IImage Image { get { return null; } }

            public string Text { get { return null; } }

            public object Content { get; set; }

            public object Description { get { return null; } }

            public double Priority { get { return 0; } }

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                throw new NotImplementedException();
            }
        }
    }
}
