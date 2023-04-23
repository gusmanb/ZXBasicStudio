using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Konamiman.Z80dotNet.ZexallTest
{
    //ZEXALL and ZEXDOC tests executor
    //Usage:
    //ZexallTest - run all ZEXALL tests
    //ZexallTest zexdoc.com - run all ZEXDOC tests
    //ZexallTests zexall.com|zexdoc.com n - run all tests after skipping the first n

    class Program
    {
        private static byte DollarCode;

        static void Main(string[] args)
        {
            (new Program()).Run(args);
        }

        private void Run(string[] args)
        {
            var fileName = args.Length == 0 ? "zexall.com" : args[0];
            var testsToSkip = args.Length >= 2 ? int.Parse(args[1]) : 0;

            DollarCode = Encoding.ASCII.GetBytes(new[] {'$'})[0];

            var z80 = new Z80Processor();
            z80.ClockSynchronizer = null;
            z80.AutoStopOnRetWithStackEmpty = true;
            
            var program = File.ReadAllBytes(fileName);
            z80.Memory.SetContents(0x100, program);

            z80.Memory[6] = 0xFF;
            z80.Memory[7] = 0xFF;

            z80.BeforeInstructionFetch += Z80OnBeforeInstructionFetch;

            SkipTests(z80, testsToSkip);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            z80.Reset();
            z80.Registers.PC = 0x100;
            z80.Continue();

            sw.Stop();
            Console.WriteLine("\r\nElapsed time: " + sw.Elapsed);
        }

        private static void SkipTests(Z80Processor z80, int testsToSkipCount)
        {
            ushort loadTestsAddress = 0x120;
            ushort originalAddress = 0x13A;
            ushort newTestAddress = (ushort) (originalAddress + testsToSkipCount*2);
            z80.Memory[loadTestsAddress] = newTestAddress.GetLowByte();
            z80.Memory[loadTestsAddress + 1] = newTestAddress.GetHighByte();
        }

        private void Z80OnBeforeInstructionFetch(object sender, BeforeInstructionFetchEventArgs args)
        {
            //Absolutely minimum implementation of CP/M for ZEXALL and ZEXDOC to work

            var z80 = (IZ80Processor)sender;

            if (z80.Registers.PC == 0) {
                args.ExecutionStopper.Stop();
                return;
            }
            else if (z80.Registers.PC != 5)
                return;

            var function = z80.Registers.C;

            if(function == 9) {
                var messageAddress = z80.Registers.DE;
                var bytesToPrint = new List<byte>();
                byte byteToPrint;
                while((byteToPrint = z80.Memory[messageAddress]) != DollarCode) {
                    bytesToPrint.Add(byteToPrint);
                    messageAddress++;
                }

                var stringToPrint = Encoding.ASCII.GetString(bytesToPrint.ToArray());
                Console.Write(stringToPrint);
            }
            else if(function == 2) {
                var byteToPrint = z80.Registers.E;
                var charToPrint = Encoding.ASCII.GetString(new[] {byteToPrint})[0];
                Console.Write(charToPrint);
            }

            z80.ExecuteRet();
        }
    }
}
