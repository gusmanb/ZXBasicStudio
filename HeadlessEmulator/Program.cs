using CommandLine;
using CoreSpectrum.Hardware;
using CoreSpectrum.SupportClasses;
using HeadlessEmulator;
using System.Text;

const ushort reset128k = 0x2656;
const ushort resetPlus2 = 0x2675;

int bpCounter = 0;

Options? mainOptions = null;

CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed(RunOptions);

if (mainOptions == null)
    return;

SpectrumBase emulator;

switch (mainOptions.Model)
{
    case SpectrumModel.M128k:
        Load128k();
        break;
    case SpectrumModel.MPlus2:
        LoadPlus2();
        break;
    default:
        Load48k();
        break;
}

emulator.BreakpointHit += Emulator_BreakpointHit;
emulator.ProgramReady += Emulator_ProgramReady;

emulator.Start(true);
emulator.Pause();
emulator.Turbo(true, false);

ushort org = mainOptions.Org == null ? mainOptions.Address : mainOptions.Org.Value;

Console.WriteLine($"Injecting binary image {mainOptions.Binary} at address {mainOptions.Address} org {org}");

byte[] image = File.ReadAllBytes(mainOptions.Binary);

ProgramImage program = new ProgramImage { InitialBank = 0, Chunks = new ImageChunk[] { new ImageChunk { Address = mainOptions.Address, Bank = 0, Data = image } }, Org = org };

emulator.InjectProgram(program);
emulator.Start();

Console.WriteLine("Running emulator, press a key to abort...");
Console.ReadKey();

Console.WriteLine("Terminating...");

emulator.Stop();
emulator.Dispose();
return;

void Emulator_ProgramReady(object? sender, EventArgs e)
{
    emulator.Pause();

    Console.WriteLine("Program ready, creating breakpoints...");
    /*
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST00h", Address = 0x00, Temporary = false });
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST08h", Address = 0x08, Temporary = false });
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST10h", Address = 0x10, Temporary = false });
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST18h", Address = 0x18, Temporary = false });
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST20h", Address = 0x20, Temporary = false });
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST28h", Address = 0x28, Temporary = false });
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST30h", Address = 0x30, Temporary = false });
    emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = "RST38h", Address = 0x38, Temporary = false });*/

    foreach (var addr in mainOptions.Breakpoints)
    {
        Console.WriteLine($"Creating breakpoint at 0x{addr.ToString("X4")}...");
        emulator.AddBreakpoint(new CoreSpectrum.Debug.Breakpoint { Id = $"0x{addr.ToString("X4")}", Address = addr, Temporary = false });
    }

    Console.WriteLine("Running program...");
    emulator.Resume();
}

void Emulator_BreakpointHit(object? sender, BreakPointEventArgs e)
{
    Console.WriteLine($"Breakpoint hit: {e.Breakpoint.Id}.");

    Console.WriteLine("Dumping memory and registers...");

    byte[] mem = emulator.Memory.GetContents(0, 65536);
    File.WriteAllBytes($"{e.Breakpoint.Id}_{bpCounter++}.mem", mem);

    StringBuilder sb = new StringBuilder();

    sb.AppendLine($"PC: 0x{((ushort)emulator.Z80.Registers.PC).ToString("X4")}");
    sb.AppendLine($"SP: 0x{((ushort)emulator.Z80.Registers.SP).ToString("X4")}");
    sb.AppendLine($"IX: 0x{((ushort)emulator.Z80.Registers.IX).ToString("X4")}");
    sb.AppendLine($"IY: 0x{((ushort)emulator.Z80.Registers.IY).ToString("X4")}");

    sb.AppendLine($"AF: 0x{((ushort)emulator.Z80.Registers.AF).ToString("X4")}");
    sb.AppendLine($"BC: 0x{((ushort)emulator.Z80.Registers.BC).ToString("X4")}");
    sb.AppendLine($"DE: 0x{((ushort)emulator.Z80.Registers.DE).ToString("X4")}");
    sb.AppendLine($"HL: 0x{((ushort)emulator.Z80.Registers.HL).ToString("X4")}");

    sb.AppendLine($"AF': 0x{((ushort)emulator.Z80.Registers.Alternate.AF).ToString("X4")}");
    sb.AppendLine($"BC': 0x{((ushort)emulator.Z80.Registers.Alternate.BC).ToString("X4")}");
    sb.AppendLine($"DE': 0x{((ushort)emulator.Z80.Registers.Alternate.DE).ToString("X4")}");
    sb.AppendLine($"HL': 0x{((ushort)emulator.Z80.Registers.Alternate.HL).ToString("X4")}");

    File.WriteAllText($"{e.Breakpoint.Id}_{bpCounter++}.reg", sb.ToString());

    Console.WriteLine("Resuming emulation...");

    emulator.Resume();
}

void ProcessRST08()
{
    ushort value = emulator.Memory.GetUshort(0x5C5D);
    Console.WriteLine($"CH_ADD: {value}");

    ushort stack = (ushort)emulator.Z80.Registers.SP;
    value = emulator.Memory.GetUshort(stack);

    byte errCode = emulator.Memory.GetByte(value);
    Console.WriteLine($"ERR_NR: {errCode}");

    value = emulator.Memory.GetUshort(0x5C3C);
    Console.WriteLine($"ERR_SP: {value}");
}

void Load48k()
{
    Console.WriteLine("Creating 48k emulator...");
    byte[] rom0 = MainResources._48k_rom;

    var emu = new Spectrum48k(new[] { rom0 });
    emulator = emu;
}

void LoadPlus2()
{
    Console.WriteLine("Creating +2 emulator...");
    byte[] rom0 = MainResources.Plus2_0_rom;
    byte[] rom1 = MainResources.Plus2_1_rom;

    var emu = new Spectrum128k(new[] { rom0, rom1 }, resetPlus2);
    emulator = emu;
}

void Load128k()
{
    Console.WriteLine("Creating 128k emulator...");
    byte[] rom0 = MainResources._128k_0_rom;
    byte[] rom1 = MainResources._128k_1_rom;

    var emu = new Spectrum128k(new[] { rom0, rom1 }, reset128k);
    emulator = emu;
}

void RunOptions(Options opts)
{
    mainOptions = opts;    
}
public class Options
{ 
    [Option('b', "binary", Required = true, HelpText = "Binary file to load")]
    public string Binary { get; set; }
    [Option('a', "address", Required = true, HelpText = "Address of the binary")]
    public ushort Address { get; set; }
    [Option('o', "org", Required = false, Default = null, HelpText = "Startup address, if omited the binary address will be used")]
    public ushort? Org { get; set; }
    [Option('m', "model", Required = false, Default = SpectrumModel.M48k, HelpText = "Spectrum model. Possible options: M48k, M128k and MPlus2")]
    public SpectrumModel Model { get; set; }
    [Option('p', "breakpoints", Required = true, HelpText = "List of breakpoint addresses")]
    public IEnumerable<ushort> Breakpoints { get; set; }
}

public enum SpectrumModel
{
    M48k,
    M128k,
    MPlus2
}