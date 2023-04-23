using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class CalibrationTape : Tape
    {
        public CalibrationTape() : base(new TapeBlock[] { new TapeBlock(96863) })
        {
        }
    }
}
