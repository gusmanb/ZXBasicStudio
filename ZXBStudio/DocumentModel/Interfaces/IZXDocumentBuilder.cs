using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.BuildSystem;
using ZXBasicStudio.DocumentModel.Enums;

namespace ZXBasicStudio.DocumentModel.Interfaces
{
    /// <summary>
    /// Interface for document builders.
    /// </summary>
    public interface IZXDocumentBuilder
    {
        /// <summary>
        /// Id of the builder
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// List of dependencies (only in the same stage)
        /// </summary>
        public Guid[]? DependsOn { get; }

        /// <summary>
        /// Builds all the documents of the handled type belonging to the project.
        /// </summary>
        /// <param name="BuildPath">Path of the project. The document builder must handle all the documents inside the path.</param>
        /// <param name="Stage">Stage of the build</param>
        /// <param name="BuildType">Type of build, to discriminate debug and release</param>
        /// <param name="CompiledProgram">Program compiled in this build, only provided for post-build builders</param>
        /// <param name="OutputLogWriter">TextWriter used to show logs to the user</param>
        /// <returns>True if the build was successful, false in other case.</returns>
        bool Build(string BuildPath, ZXBuildStage Stage, ZXBuildType BuildType, ZXProgram? CompiledProgram, TextWriter OutputLog);
    }
}
