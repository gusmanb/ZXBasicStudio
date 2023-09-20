using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXBuildSettings
    {
        public string? MainFile { get; set; }
        public int? OptimizationLevel { get; set; }
        public ushort? Origin { get; set; }
        public int? ArrayBase { get; set; }
        public int? StringBase { get; set; }
        public bool SinclairMode { get; set; }
        public int? HeapSize { get; set; }
        public bool StrictBool { get; set; }
        public bool EnableBreak { get; set; }
        public bool Explicit { get; set; }
        public string[]? Defines { get; set; }
        public bool IgnoreCase { get; set; }
        public bool Strict { get; set; }
        public bool Headerless { get; set; }
        public bool NextMode { get; set; }
        public string? NextCmd { get; set; }
        public string GetSettings()
        {
            List<string> settings = new List<string>();

            if (OptimizationLevel != null)
                settings.Add($"-O {OptimizationLevel}");

            if (Origin != null)
                settings.Add($"-S {Origin}");

            if (SinclairMode)
                settings.Add("-Z");
            else
            {
                if (StrictBool)
                    settings.Add("--strict-bool");

                if (IgnoreCase)
                    settings.Add("-i");

                if (ArrayBase != null)
                    settings.Add($"--array-base {ArrayBase}");

                if (StringBase != null)
                    settings.Add($"--string-base {StringBase}");
            }

            if (HeapSize != null)
                settings.Add($"-H {HeapSize}");

            if (EnableBreak)
                settings.Add("--enable-break");

            if (Explicit)
                settings.Add("--explicit");

            if (Defines != null)
            {
                foreach (var define in Defines)
                    settings.Add($"-D {define}");
            }

            if (Strict)
                settings.Add("--strict");

            return string.Join(' ', settings);
        }

        public string GetDebugSettings()
        {
            /*
            List<string> settings = new List<string>();

            if (OptimizationLevel != null)
                settings.Add($"-O {OptimizationLevel}");

            if (Origin != null)
                settings.Add($"-S {Origin}");

            if (ArrayBase != null)
                settings.Add($"--array-base {ArrayBase}");

            if (StringBase != null)
                settings.Add($"--string-base {StringBase}");

            if (SinclairMode)
                settings.Add("-Z");

            if (HeapSize != null)
                settings.Add($"-H {HeapSize}");

            if (StrictBool)
                settings.Add("--strict-bool");

            if (EnableBreak)
                settings.Add("--enable-break");

            if (Explicit)
                settings.Add("--explicit");

            if (Defines != null)
            {
                foreach (var define in Defines)
                    settings.Add($"-D {define}");
            }

            if (IgnoreCase)
                settings.Add("-i");

            if (Strict)
                settings.Add("--strict");

            settings.Add("+W150");

            return string.Join(' ', settings);*/
            List<string> settings = new List<string>();

            if (OptimizationLevel != null)
                settings.Add($"-O {OptimizationLevel}");

            if (Origin != null)
                settings.Add($"-S {Origin}");

            if (SinclairMode)
                settings.Add("-Z");
            else
            {
                if (StrictBool)
                    settings.Add("--strict-bool");

                if (IgnoreCase)
                    settings.Add("-i");

                if (ArrayBase != null)
                    settings.Add($"--array-base {ArrayBase}");

                if (StringBase != null)
                    settings.Add($"--string-base {StringBase}");
            }

            if (HeapSize != null)
                settings.Add($"-H {HeapSize}");

            if (EnableBreak)
                settings.Add("--enable-break");

            if (Explicit)
                settings.Add("--explicit");

            if (Defines != null)
            {
                foreach (var define in Defines)
                    settings.Add($"-D {define}");
            }

            if (Strict)
                settings.Add("--strict");

            settings.Add("+W150");

            return string.Join(' ', settings);
        }

        public ZXBuildSettings Clone()
        {
            return (ZXBuildSettings)this.MemberwiseClone();
        }
    }
}
