using CoreSpectrum.Enums;
using CoreSpectrum.Hardware;
using CoreSpectrum.Renderers;
using CoreSpectrum.SupportClasses;
using NAudio.Wave;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestCore
{
    public partial class Form1 : Form
    {

        private static readonly int BRIGHT = 0xff, NORM = 0xd7;
        private static readonly Rectangle videoScreenRect = new Rectangle(0, 0, 416, 312);
        //private static readonly Rectangle videoScreenRect = new Rectangle(0, 0, 256, 192);

        Bitmap renderBitmap;
        Machine machine;
        ULAWaveStream ulaAudio;
        WaveOutEvent waveOut;
        PaletizedVideoRenderer<int> renderer;

        public Form1()
        {
            InitializeComponent();

            byte[] rom = File.ReadAllBytes(Application.StartupPath + "\\48k.rom");

            int[] palette = new int[]
            {
                Color.FromArgb(0, 0, 0).ToArgb(),
                Color.FromArgb(0, 0, NORM).ToArgb(),
                Color.FromArgb(NORM, 0, 0).ToArgb(),
                Color.FromArgb(NORM, 0, NORM).ToArgb(),
                Color.FromArgb(0, NORM, 0).ToArgb(),
                Color.FromArgb(0, NORM, NORM).ToArgb(),
                Color.FromArgb(NORM, NORM, 0).ToArgb(),
                Color.FromArgb(NORM, NORM, NORM).ToArgb(),
                Color.FromArgb(0, 0, 0).ToArgb(),
                Color.FromArgb(0, 0, BRIGHT).ToArgb(),
                Color.FromArgb(BRIGHT, 0, 0).ToArgb(),
                Color.FromArgb(BRIGHT, 0, BRIGHT).ToArgb(),
                Color.FromArgb(0, BRIGHT, 0).ToArgb(),
                Color.FromArgb(0, BRIGHT, BRIGHT).ToArgb(),
                Color.FromArgb(BRIGHT, BRIGHT, 0).ToArgb(),
                Color.FromArgb(BRIGHT, BRIGHT, BRIGHT).ToArgb(),

            };

            ulaAudio = new ULAWaveStream();
            ImageList lst = new ImageList();
            renderer = new PaletizedVideoRenderer<int>(palette, false);
            machine = new Machine(rom, renderer, ulaAudio);
            machine.FrameRendered += Machine_FrameRendered;
            renderBitmap = new Bitmap(416, 312, PixelFormat.Format32bppArgb);
            machine.Start();

            
        }

        private void Machine_FrameRendered(object? sender, Machine.SpectrumFrameArgs e)
        {
            try
            {
                BitmapData bmd = renderBitmap.LockBits(
                            videoScreenRect,
                            ImageLockMode.WriteOnly,
                            PixelFormat.Format32bppArgb);

                for (int buc = 0; buc < videoScreenRect.Height; buc++)
                    Marshal.Copy(renderer.VideoBuffer, buc * videoScreenRect.Width, bmd.Scan0 + (bmd.Stride * buc), videoScreenRect.Width);

                renderBitmap.UnlockBits(bmd);

                BeginInvoke(() =>
                {
                    Graphics g = this.CreateGraphics();
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    g.DrawImage(renderBitmap, new Rectangle(0, 0, this.Width, this.Height));
                    g.Dispose();
                });
            }
            catch { }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            machine.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var arka = Z80File.Load(Application.StartupPath + "\\Arkanoid.z80");
            machine.LoadZ80Program(arka);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var mm = Z80File.Load(Application.StartupPath + "\\ManicMiner.z80");
            machine.LoadZ80Program(mm);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    machine.PressKey(CoreSpectrum.Enums.SpectrumKeys.Caps);
                    break;
                case Keys.ControlKey:
                    machine.PressKey(CoreSpectrum.Enums.SpectrumKeys.Sym);
                    break;
                case Keys.Enter:
                    machine.PressKey(CoreSpectrum.Enums.SpectrumKeys.Enter);
                    break;
                default:
                    if (Enum.TryParse(typeof(SpectrumKeys), e.KeyCode.ToString(), true, out var key))
                        machine.PressKey((SpectrumKeys)key);
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    machine.ReleaseKey(CoreSpectrum.Enums.SpectrumKeys.Caps);
                    break;
                case Keys.ControlKey:
                    machine.ReleaseKey(CoreSpectrum.Enums.SpectrumKeys.Sym);
                    break;
                case Keys.Enter:
                    machine.ReleaseKey(CoreSpectrum.Enums.SpectrumKeys.Enter);
                    break;
                default:
                    if (Enum.TryParse(typeof(SpectrumKeys), e.KeyCode.ToString(), true, out var key))
                        machine.ReleaseKey((SpectrumKeys)key);
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //var mm = Z80File.Load(Application.StartupPath + "\\ManicMiner.z80");
            //machine.LoadZ80Program(mm);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
        }
        bool loaded = false;

        private void Form1_Click(object sender, EventArgs e)
        {
            machine.Pause();
            var abu = TZXFile.Load(Application.StartupPath + "\\cc.tzx");
            machine.DataCorder.InsertTape(abu);
            machine.Resume();
            machine.DataCorder.Play();
            
        }

    }
}