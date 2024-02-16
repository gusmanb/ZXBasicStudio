using CoreSpectrum.SupportClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.BuildSystem;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.DocumentEditors.NextDows.log
{
    /// <summary>
    /// Class to export on prebuild operation
    /// </summary>
    public class ExportManager : IZXDocumentBuilder
    {

        public Guid Id => Guid.Parse("2372bad1-0176-4881-8c2e-3b8c506cbaaf");

        public Guid[]? DependsOn => null;

        private FileTypes fileType = FileTypes.Undefined;

        public bool Initialize(FileTypes fileType)
        {
            this.fileType = fileType;
            return true;
        }


        public bool Build(string BuildPath, ZXBuildStage Stage, ZXBuildType BuildType, ZXProgram? program, TextWriter OutputLog)
        {
            if (!ServiceLayer.Initialized)
            {
                ServiceLayer.Initialize();
            }

            // Get all config filenames
            var files = ServiceLayer.Files_GetAllConfigFiles(BuildPath, ".zxf.zbs");    // ZXForms files
            if (files == null)
            {
                return true;
            }

            foreach (var file in files)
            {
                // TODO: Export file...                
            }
            return true;
        }
    }
}
