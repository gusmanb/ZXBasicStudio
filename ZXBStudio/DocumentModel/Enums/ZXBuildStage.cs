using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentModel.Enums
{
    /// <summary>
    /// Build stage of documents
    /// </summary>
    [Flags]
    public enum ZXBuildStage
    {
        /// <summary>
        /// Documents must be built before the project is compiled
        /// </summary>
        PreBuild = 1,
        /// <summary>
        /// Documents must be built after the project is compiled
        /// </summary>
        PostBuild = 2
    }
}
