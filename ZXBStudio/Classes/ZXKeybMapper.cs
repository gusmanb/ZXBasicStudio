using Avalonia.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentModel.Classes;

namespace ZXBasicStudio.Classes
{
    public static class ZXKeybMapper
    {
        static Dictionary<Guid, ZXKeybCommand[]> mappings;
        static Dictionary<Guid, string> sourceNames;

        static ZXKeybMapper()
        {
            mappings = new Dictionary<Guid, ZXKeybCommand[]>();
            sourceNames = new Dictionary<Guid, string>();

            LoadDefaults();

            try
            {
                if (ZXApplicationFileProvider.Exists(ZXConstants.APPKEYS_FILE))
                {
                    var remappings = JsonConvert.DeserializeObject<Dictionary<Guid, ZXKeybCommand[]>>(ZXApplicationFileProvider.ReadAllText(ZXConstants.APPKEYS_FILE)) ?? new Dictionary<Guid, ZXKeybCommand[]>();

                    foreach (var cmds in remappings)
                    {
                        if (mappings.ContainsKey(cmds.Key))
                            mappings[cmds.Key] = cmds.Value;
                    }
                }
            }
            catch { }


        }

        static void LoadDefaults() 
        {
            mappings.Clear();
            sourceNames.Clear();

            mappings.Add(MainWindow.KeybSourceId, MainWindow.KeybCommands);
            sourceNames.Add(MainWindow.KeybSourceId, "Global keys");

            var docs = ZXDocumentProvider.DocumentTypes;

            foreach (var doc in docs)
            {
                if (doc.EditorCommands != null && !mappings.ContainsKey(doc.DocumentTypeId))
                {
                    mappings.Add(doc.DocumentTypeId, doc.EditorCommands);
                    sourceNames.Add(doc.DocumentTypeId, $"{doc.DocumentName} editor");
                }
            }
        }

        public static ZXKeybMapperSource[] GetKeybCommands()
        {
            return mappings.Select(k => new ZXKeybMapperSource { SourceId = k.Key, SourceName = sourceNames[k.Key], KeybCommands = k.Value.Select(i => new ZXKeybCommand { CommandId = i.CommandId, CommandName = i.CommandName, Key = i.Key, Modifiers = i.Modifiers }).ToArray() }).ToArray();

        }
        public static Guid? GetCommandId(Guid SourceId, Key Key, KeyModifiers Modifiers)
        {
            if (!mappings.ContainsKey(SourceId))
                return null;

            return mappings[SourceId].FirstOrDefault(m => m.Key != Key.None && m.Key == Key && m.Modifiers == Modifiers)?.CommandId;
        }
        public static bool UpdateCommands(Guid SourceId, ZXKeybCommand[] Commands)
        {
            try
            {

                if (!mappings.ContainsKey(SourceId))
                    return false;

                mappings[SourceId] = Commands;
                ZXApplicationFileProvider.WriteAllText(ZXConstants.APPKEYS_FILE, JsonConvert.SerializeObject(mappings));
                return true;
            }
            catch { return false; }
        }

        public static void RestoreDefaults()
        {
            LoadDefaults();
            try 
            {
                if(ZXApplicationFileProvider.Exists(ZXConstants.APPKEYS_FILE))
                    ZXApplicationFileProvider.Delete(ZXConstants.APPKEYS_FILE);

            } catch { }
        }
    }

    public class ZXKeybMapperSource
    {
        public Guid SourceId { get; set; }
        public required string SourceName { get; set; }
        public required ZXKeybCommand[] KeybCommands { get; set; }
    }
}
