using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXExportOptions
    {
        public ZXExportOptions(string OutputPath, bool AddBasic, bool Autorun) 
        {
            this.OutputPath = OutputPath;
            this.AddBasic = AddBasic;
            this.Autorun = Autorun;
        }
        public string OutputPath { get; set; }
        public bool AddBasic { get; set; }
        public bool Autorun { get; set; }

        public string GetExportOptions()
        {
            string opts = $"-o \"{OutputPath}\"";

            string ext = Path.GetExtension(OutputPath).ToLower();

            switch (ext)
            {
                case ".tap":
                    opts += " --tap";
                    break;
                case ".tzx":
                    opts += " --tzx";
                    break;
            }

            if (AddBasic)
                opts += " --BASIC";

            if (Autorun)
                opts += " --autorun";

            return opts;
        }
    }
}
