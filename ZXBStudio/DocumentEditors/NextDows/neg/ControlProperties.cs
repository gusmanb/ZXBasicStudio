using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.NextDows.neg
{
    public enum ControlProperties
    {
        // Header -------------------------------------------------------------
        Id,
        NextControlAddress,
        ControlDefinitionSize,

        // Runtime ------------------------------------------------------------
        Runtime_Left,
        Runtime_Top,
        Runtime_Width,
        Runtime_Height,
        Runtime_HScroll,
        Runtime_VScroll,
        Runtime_CursorPosition,

        // Common fields ------------------------------------------------------
        ControlType,
        Visible,
        ContainerId,
        Left,
        Top,
        Width,
        Height,
        Ink,
        Paper,


        // Other, and mixed properties ----------------------------------------
        TextAlign,
        Text,

    }
}
