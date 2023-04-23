using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public static class BreakpointManager
    {
        static Dictionary<string, List<ZXBreakPoint>> _breakpoints = new Dictionary<string, List<ZXBreakPoint>>();

        public static event EventHandler? BreakPointAdded;
        public static event EventHandler? BreakPointRemoved;

        public static IEnumerable<ZXBreakPoint> AllBreakPoints
        {
            get { return _breakpoints.SelectMany(k => k.Value); }
        }

        public static IEnumerable<ZXBreakPoint> BreakPoints(string File)
        {
            if(!_breakpoints.ContainsKey(File))
                return Enumerable.Empty<ZXBreakPoint>();

            return _breakpoints[File];
        }

        public static ZXBreakPoint BreakPoint(string File, int Index) 
        {
            return _breakpoints[File][Index];
        }

        public static bool AddBreakpoint(ZXBreakPoint breakPoint) 
        {
            if(!_breakpoints.ContainsKey(breakPoint.File))
                _breakpoints[breakPoint.File] = new List<ZXBreakPoint>();

            var bps = _breakpoints[breakPoint.File];

            if (bps.Any(b => b.Line == breakPoint.Line))
                return true;

            _breakpoints[breakPoint.File].Add(new ZXBreakPoint (breakPoint.File, breakPoint.Line ));

            if (BreakPointAdded != null)
                BreakPointAdded(null, EventArgs.Empty);

            return true;
        }

        public static bool RemoveBreakpoint(string File, ZXBreakPoint breakPoint) 
        {
            if (!_breakpoints.ContainsKey(File))
                return false;

            var bps = _breakpoints[File];

            var bp = bps.FirstOrDefault(b => b.Line == breakPoint.Line);

            if(bp == null) 
                return false;

            bps.Remove(bp);

            if (BreakPointRemoved != null)
                BreakPointRemoved(null, EventArgs.Empty);

            return true;
        }

        public static void UpdateFileName(string OldFile, string NewFile)
        {
            if (!_breakpoints.ContainsKey(OldFile))
                return;

            var bps = _breakpoints[OldFile];
            _breakpoints.Remove(OldFile);
            _breakpoints[NewFile] = bps;
        }

        public static void ClearBreakpoints()
        {
            _breakpoints.Clear();
        }
    }
}
