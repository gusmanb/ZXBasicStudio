using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Collections.Generic;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_r_r_tests : InstructionsExecutionTestsBase
    {
        static LD_r_r_tests()
        {
            var combinations = new List<object[]>();

            var registers = new[] {"B", "C", "D", "E", "H", "L", null, "A"};
            for(var src = 0; src<=7; src++)
            {
                for(var dest = 0; dest <= 7; dest++)
                {
                    if(src == 6 || dest == 6) continue;
                    var opcode = (byte)(src | (dest << 3) | 0x40);
                    combinations.Add(new object[] { registers[dest], registers[src], opcode});
                }
            }

            LD_r_r_Source = combinations.ToArray();
        }

        public static object[] LD_r_r_Source;

        [Test]
        [TestCaseSource("LD_r_r_Source")]
        public void LD_r_r_loads_register_with_value(string dest, string src, byte opcode)
        {
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();

            SetReg(dest, oldValue);
            SetReg(src, newValue);

            Execute(opcode);

            Assert.AreEqual(newValue, GetReg<byte>(dest));
        }

        [Test]
        [TestCaseSource("LD_r_r_Source")]
        public void LD_r_r_do_not_modify_flags(string dest, string src, byte opcode)
        {
            AssertNoFlagsAreModified(opcode);
        }

        [Test]
        [TestCaseSource("LD_r_r_Source")]
        public void LD_r_r_returns_proper_T_states(string dest, string src, byte opcode)
        {
            var states = Execute(opcode);
            Assert.AreEqual(4, states);
        }
    }
}