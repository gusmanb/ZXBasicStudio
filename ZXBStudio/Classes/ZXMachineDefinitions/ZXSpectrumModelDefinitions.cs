using CoreSpectrum.Hardware;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Controls;

namespace ZXBasicStudio.Classes.ZXMachineDefinitions
{

    public static class ZXSpectrumModelDefinitions
    {
        public static ZXSpectrumModelDefinition[] Definitions { get; private set; }

        public static ZXSpectrumModelDefinition? GetDefinition(ZXSpectrumModel Model)
        {
            return Definitions.FirstOrDefault(sp => sp.Model == Model);
        }

        static ZXSpectrumModelDefinitions()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager("ZXBasicStudio.Resources.ZXSpectrum", typeof(ZXSpectrumModelDefinitions).Assembly);

            List<ZXSpectrumModelDefinition> defs = new List<ZXSpectrumModelDefinition>();

            {
                var rom = resources.GetObject("48k_rom") as byte[];

                if (rom == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                if (rom.Length != 16384)
                    throw new InvalidProgramException("Invalid ROM resource!");

                var romDis = resources.GetString("48k_asm");

                if(romDis == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                var romMap = resources.GetString("48k_map");

                if (romMap == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                var romMapLines = JsonConvert.DeserializeObject<ZXRomLine[]>(romMap);

                if (romMapLines == null)
                    throw new InvalidProgramException("Invalid ROM resource!");

                ZXSpectrumModelDefinition def48k = new ZXSpectrumModelDefinition
                {
                    Model = ZXSpectrumModel.Spectrum48k,
                    RomSet = new byte[][] { rom },
                    RomDissasembly = romDis,
                    RomDissasemblyMap = romMapLines,
                    ResetAddress = 0,
                    InjectAddress = 0x12ac
                };

                defs.Add(def48k);
            }

            {
                var rom0 = resources.GetObject("128k_0_rom") as byte[];

                if (rom0 == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                if (rom0.Length != 16384)
                    throw new InvalidProgramException("Invalid ROM resource!");

                var rom1 = resources.GetObject("128k_1_rom") as byte[];

                if (rom1 == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                if (rom1.Length != 16384)
                    throw new InvalidProgramException("Invalid ROM resource!");

                var romDis = resources.GetString("128k_1_asm");

                if (romDis == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                var romMap = resources.GetString("128k_1_map");

                if (romMap == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                var romMapLines = JsonConvert.DeserializeObject<ZXRomLine[]>(romMap);

                if (romMapLines == null)
                    throw new InvalidProgramException("Invalid ROM resource!");

                ZXSpectrumModelDefinition def128k = new ZXSpectrumModelDefinition
                {
                    Model = ZXSpectrumModel.Spectrum128k,
                    RomSet = new byte[][] { rom0, rom1 },
                    RomDissasembly = romDis,
                    RomDissasemblyMap = romMapLines,
                    ResetAddress = 0x2656,
                    InjectAddress = 0x12ac
                };

                defs.Add(def128k);
            }

            {
                var rom0 = resources.GetObject("Plus2_0_rom") as byte[];

                if (rom0 == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                if (rom0.Length != 16384)
                    throw new InvalidProgramException("Invalid ROM resource!");

                var rom1 = resources.GetObject("Plus2_1_rom") as byte[];

                if (rom1 == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                if (rom1.Length != 16384)
                    throw new InvalidProgramException("Invalid ROM resource!");

                var romDis = resources.GetString("Plus2_1_asm");

                if (romDis == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                var romMap = resources.GetString("Plus2_1_map");

                if (romMap == null)
                    throw new InvalidProgramException("Missing ROM resource!");

                var romMapLines = JsonConvert.DeserializeObject<ZXRomLine[]>(romMap);

                if (romMapLines == null)
                    throw new InvalidProgramException("Invalid ROM resource!");

                ZXSpectrumModelDefinition defPlus2 = new ZXSpectrumModelDefinition
                {
                    Model = ZXSpectrumModel.SpectrumPlus2,
                    RomSet = new byte[][] { rom0, rom1 },
                    RomDissasembly = romDis,
                    RomDissasemblyMap = romMapLines,
                    ResetAddress = 0x2675,
                    InjectAddress = 0x12ac
                };

                defs.Add(defPlus2);
            }

            Definitions = defs.ToArray();
        }
    }

    public class ZXSpectrumModelDefinition
    {
        public required ZXSpectrumModel Model { get; set; }
        public required byte[][] RomSet { get; set; }
        public required string RomDissasembly { get; set; }
        public required ZXRomLine[] RomDissasemblyMap { get; set; }
        public required ushort ResetAddress { get; set; }
        public required ushort InjectAddress { get; set; }
    }

    public class ZXRomLine
    {
        public required int Line { get; set; }
        public required ushort Address { get; set; }
    }

    public enum ZXSpectrumModel
    {
        Spectrum48k,
        Spectrum128k,
        SpectrumPlus2
    }
}
