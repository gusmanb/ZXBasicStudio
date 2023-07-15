using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.NextDows.neg
{
    /// <summary>
    /// Types of ZXForms controls
    /// </summary>
    public enum ControlsTypes
    {
        /// <summary>
        /// Free to use
        /// </summary>
        None=0,
        
        /// <summary>
        /// A panel can contain other elements, every ZXForms window has a main panel with id=0
        /// </summary>
        Panel=1,

        /// <summary>
        /// Print some text
        /// </summary>
        Label=2,

        // Not implemented ----------------------------------------------
        
        Box=101,
        Button=102,
        Check=103,
        Circle=104,
        Image=105,
        Line=107,
        List=108,
        Modal=109,
        Radio=111,
        Select=112,
        Table=113,
        TextBox=114
    }
}
