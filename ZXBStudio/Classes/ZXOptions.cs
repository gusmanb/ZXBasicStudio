using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXOptions
    {
        public static ZXOptions Current { get; set; }

        static ZXOptions()
        {
            if (ZXApplicationFileProvider.Exists(ZXConstants.APPOPTIONS_FILE))
            {
                try
                {
                    Current = JsonConvert.DeserializeObject<ZXOptions>(ZXApplicationFileProvider.ReadAllText(ZXConstants.APPOPTIONS_FILE)) ?? new ZXOptions();
                }
                catch { Current = new ZXOptions(); }
            }
            else
            { Current = new ZXOptions(); }
        }
        public static void SaveCurrentSettings()
        {
            var settings = JsonConvert.SerializeObject(Current);
            ZXApplicationFileProvider.WriteAllText(ZXConstants.APPOPTIONS_FILE, settings);
        }
        public string? ZxbasmPath { get; set; }
        public string? ZxbcPath { get; set; }
        public double EditorFontSize { get; set; } = 16.0;
        public bool WordWrap { get; set; } = true;
        public bool AudioDisabled { get; set; }
        public bool Cls { get; set; }
        public bool Borderless { get; set; }
        public bool AntiAlias { get; set; }
        public ZXBuildSettings? DefaultBuildSettings { get; set; }
        public string? NextEmulatorPath { get; set; }
    }
}
