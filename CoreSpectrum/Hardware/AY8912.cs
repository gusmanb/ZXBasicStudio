namespace CoreSpectrum.Hardware
{
    public static class Ay8912
    {
        public enum ayemu_stereo_t
        {
            AYEMU_MONO = 0,
            AYEMU_ABC = 1,
            AYEMU_ACB = 2,
            AYEMU_BAC = 3,
            AYEMU_BCA = 4,
            AYEMU_CAB = 5,
            AYEMU_CBA = 6,
            AYEMU_STEREO_CUSTOM = 255
        }
        public enum ayemu_chip_t
        {
            AYEMU_AY = 0,
            AYEMU_YM = 1,
            AYEMU_AY_LION17 = 2,
            AYEMU_YM_LION17 = 3,
            AYEMU_AY_KAY = 4,
            AYEMU_YM_KAY = 5,
            AYEMU_AY_LOG = 6,
            AYEMU_YM_LOG = 7,
            AYEMU_AY_CUSTOM = 8,
            AYEMU_YM_CUSTOM = 9,
        }

        public const int AYEMU_MAX_AMP = 24575;
        public const int AYEMU_DEFAULT_CHIP_FREQ = 1773400;
        public class ayemu_regdata_t
        {
            public int tone_a;
            public int tone_b;
            public int tone_c;
            public int noise;
            public bool R7_tone_a;
            public bool R7_tone_b;
            public bool R7_tone_c;
            public bool R7_noise_a;
            public bool R7_noise_b;
            public bool R7_noise_c;
            public byte vol_a;
            public byte vol_b;
            public byte vol_c;
            public bool env_a;
            public bool env_b;
            public bool env_c;
            public int env_freq;
            public int env_style;
            public byte[] regValues = new byte[18];
        }

        public struct ayemu_sndfmt_t
        {
            public int freq;
            public int channels;
            public int bpc;
        }

        public class ayemu_ay_t
        {
            public int[] table = new int[32];
            public ayemu_chip_t type;
            public int ChipFreq;
            public int[] eq = new int[6];
            public ayemu_regdata_t regs = new();
            public ayemu_sndfmt_t sndfmt = new();
            public int magic;
            public bool default_chip_flag;
            public bool default_stereo_flag;
            public int default_sound_format_flag;
            public bool dirty;
            public int verbose;
            public bool bit_a;
            public bool bit_b;
            public bool bit_c;
            public bool bit_n;
            public int cnt_a;
            public int cnt_b;
            public int cnt_c;
            public int cnt_n;
            public int cnt_e;
            public int ChipTacts_per_outcount;
            public int Amp_Global;
            public int[][] vols = new int[][]
            {
                new int[32],
                new int[32],
                new int[32],
                new int[32],
                new int[32],
                new int[32]
            };
            public int EnvNum;
            public int env_pos;
            public int Cur_Seed;
        }

        public static string? ayemu_err;
        public static sbyte[] VERSION = new sbyte[13];
        public static int MAGIC1 = 0xcdef;
        public static bool bEnvGenInit = false;
        public static int[][] Envelope = new int[][]
        {
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
            new int[128],
        };
        public static int[] Lion17_AY_table = new int[]
        {
            0,
            513,
            828,
            1239,
            1923,
            3238,
            4926,
            9110,
            10344,
            17876,
            24682,
            30442,
            38844,
            47270,
            56402,
            65535
        };
        public static int[] Lion17_YM_table = new int[]
        {
            0,
            0,
            190,
            286,
            375,
            470,
            560,
            664,
            866,
            1130,
            1515,
            1803,
            2253,
            2848,
            3351,
            3862,
            4844,
            6058,
            7290,
            8559,
            10474,
            12878,
            15297,
            17787,
            21500,
            26172,
            30866,
            35676,
            42664,
            50986,
            58842,
            65535
        };
        public static int[] KAY_AY_table = new int[]
        {
            0,
            836,
            1212,
            1773,
            2619,
            3875,
            5397,
            8823,
            10392,
            16706,
            23339,
            29292,
            36969,
            46421,
            55195,
            65535
        };
        public static int[] KAY_YM_table = new int[]
        {
            0,
            0,
            248,
            450,
            670,
            826,
            1010,
            1239,
            1552,
            1919,
            2314,
            2626,
            3131,
            3778,
            4407,
            5031,
            5968,
            7161,
            8415,
            9622,
            11421,
            13689,
            15957,
            18280,
            21759,
            26148,
            30523,
            34879,
            41434,
            49404,
            57492,
            65535
        };
        public static int[][][] default_layout = new int[][][]
        {
            new int[][]
            {
                new int[] { 100, 100, 100, 100, 100, 100 },
                new int[] { 100, 33, 70, 70, 33, 100 },
                new int[] { 100, 33, 33, 100, 70, 70 },
                new int[] { 70, 70, 100, 33, 33, 100 },
                new int[] { 33, 100, 100, 33, 70, 70 },
                new int[] { 70, 70, 33, 100, 100, 33 },
                new int[] { 33, 100, 70, 70, 100, 33 }
            },
            new int[][]
            {
                new int[] { 100, 100, 100, 100, 100, 100 },
                new int[] { 100, 5, 70, 70, 5, 100 },
                new int[] { 100, 5, 5, 100, 70, 70 },
                new int[] { 70, 70, 100, 5, 5, 100 },
                new int[] { 5, 100, 100, 5, 70, 70 },
                new int[] { 70, 70, 5, 100, 100, 5 },
                new int[] { 5, 100, 70, 70, 100, 5 }
            }
        };

        public static void ayemu_init(ayemu_ay_t ay)
        {
            ay.default_chip_flag = true;
            ay.ChipFreq = AYEMU_DEFAULT_CHIP_FREQ;
            ay.default_stereo_flag = true;
            ay.default_sound_format_flag = 1;
            ay.dirty = true;
            ay.verbose = 0;
            ay.magic = MAGIC1;
            ayemu_reset(ay);
        }

        public static void ayemu_reset(ayemu_ay_t ay)
        {
            if (!check_magic(ay))
            {
                return;
            }

            ay.cnt_a = ay.cnt_b = ay.cnt_c = ay.cnt_n = ay.cnt_e = 0;
            ay.bit_a = ay.bit_b = ay.bit_c = ay.bit_n = false;
            ay.env_pos = ay.EnvNum = 0;
            ay.Cur_Seed = 0xffff;
        }

        public static bool ayemu_set_chip_type(ayemu_ay_t ay, ayemu_chip_t type, int[]? custom_table)
        {
            if (!check_magic(ay))
            {
                return false;
            }

            if (type != ayemu_chip_t.AYEMU_AY_CUSTOM && type != ayemu_chip_t.AYEMU_YM_CUSTOM && custom_table != null)
            {
                ayemu_err = "For non-custom chip type 'custom_table' param must be NULL";
                return false;
            }

            if ((type == ayemu_chip_t.AYEMU_AY_CUSTOM || type == ayemu_chip_t.AYEMU_YM_CUSTOM) && custom_table == null)
            {
                ayemu_err = "For custom chip type 'custom_table' param must be not null";
                return false;
            }

            switch (type)
            {
                case ayemu_chip_t.AYEMU_AY:
                case ayemu_chip_t.AYEMU_AY_LION17:
                    set_table_ay(ay, Lion17_AY_table);
                    break;
                case ayemu_chip_t.AYEMU_YM:
                case ayemu_chip_t.AYEMU_YM_LION17:
                    set_table_ym(ay, Lion17_YM_table);
                    break;
                case ayemu_chip_t.AYEMU_AY_KAY:
                    set_table_ay(ay, KAY_AY_table);
                    break;
                case ayemu_chip_t.AYEMU_YM_KAY:
                    set_table_ym(ay, KAY_YM_table);
                    break;
                case ayemu_chip_t.AYEMU_AY_CUSTOM:
                    set_table_ay(ay, custom_table);
                    break;
                case ayemu_chip_t.AYEMU_YM_CUSTOM:
                    set_table_ym(ay, custom_table);
                    break;
                default:
                    ayemu_err = "Incorrect chip type";
                    return false;
            }

            ay.default_chip_flag = false;
            ay.dirty = true;
            return true;
        }

        public static void ayemu_set_chip_freq(ayemu_ay_t ay, int chipfreq)
        {
            if (!check_magic(ay))
            {
                return;
            }

            ay.ChipFreq = chipfreq;
            ay.dirty = true;
        }

        public static bool ayemu_set_stereo(ayemu_ay_t ay, ayemu_stereo_t stereo_type, int[]? custom_eq)
        {
            if (!check_magic(ay))
            {
                return false;
            }

            if (stereo_type != ayemu_stereo_t.AYEMU_STEREO_CUSTOM && custom_eq != null)
            {
                ayemu_err = "Stereo type not custom, 'custom_eq' parametr must be NULL";
                return false;
            }

            int chip = ay.type == ayemu_chip_t.AYEMU_AY ? 0 : 1;
            int i;
            switch (stereo_type)
            {
                case ayemu_stereo_t.AYEMU_MONO:
                case ayemu_stereo_t.AYEMU_ABC:
                case ayemu_stereo_t.AYEMU_ACB:
                case ayemu_stereo_t.AYEMU_BAC:
                case ayemu_stereo_t.AYEMU_BCA:
                case ayemu_stereo_t.AYEMU_CAB:
                case ayemu_stereo_t.AYEMU_CBA:
                    for (i = 0; i < 6; i++)
                    {
                        ay.eq[i] = default_layout[chip][(int)stereo_type][i];
                    }

                    break;
                case ayemu_stereo_t.AYEMU_STEREO_CUSTOM:
                    for (i = 0; i < 6; i++)
                    {
                        ay.eq[i] = custom_eq[i];
                    }

                    break;
                default:
                    ayemu_err = "Incorrect stereo type";
                    return false;
            }

            ay.default_stereo_flag = false;
            ay.dirty = true;
            return true;
        }

        public static bool ayemu_set_sound_format(ayemu_ay_t ay, int freq, int chans, int bits)
        {
            if (!check_magic(ay))
            {
                return false;
            }

            if (bits is not 8 and not 16)
            {
                ayemu_err = "Incorrect bits value";
                return false;
            }
            else if (chans is not 1 and not 2)
            {
                ayemu_err = "Incorrect number of channels";
                return false;
            }
            else if (freq < 50)
            {
                ayemu_err = "Incorrect output sound freq";
                return false;
            }
            else
            {
                ay.sndfmt.freq = freq;
                ay.sndfmt.channels = chans;
                ay.sndfmt.bpc = bits;
            }

            ay.default_sound_format_flag = 0;
            ay.dirty = true;
            return true;
        }

        public static void ayemu_set_reg(ayemu_ay_t ay, int reg, byte value)
        {
            if (!check_magic(ay))
            {
                return;
            }

            if (reg > 15)
            {
                return;
            }

            ay.regs.regValues[reg] = value;

            switch (reg)
            {
                case 0:
                    ay.regs.tone_a = ay.regs.tone_a & 0x0f00 | value;
                    break;

                case 1:
                    ay.regs.tone_a = ay.regs.tone_a & 0x00ff | (value & 0x0f) << 8;
                    break;

                case 2:
                    ay.regs.tone_b = ay.regs.tone_b & 0x0f00 | value;
                    break;

                case 3:
                    ay.regs.tone_b = ay.regs.tone_b & 0x00ff | (value & 0x0f) << 8;
                    break;

                case 4:
                    ay.regs.tone_c = ay.regs.tone_c & 0x0f00 | value;
                    break;

                case 5:
                    ay.regs.tone_c = ay.regs.tone_c & 0x00ff | (value & 0x0f) << 8;
                    break;

                case 6:
                    ay.regs.noise = value & 0x1f;
                    break;

                case 7:
                    ay.regs.R7_tone_a = !((value & 0x01) != 0);
                    ay.regs.R7_tone_b = !((value & 0x02) != 0);
                    ay.regs.R7_tone_c = !((value & 0x04) != 0);

                    ay.regs.R7_noise_a = !((value & 0x08) != 0);
                    ay.regs.R7_noise_b = !((value & 0x10) != 0);
                    ay.regs.R7_noise_c = !((value & 0x20) != 0);

                    break;

                case 8:
                    ay.regs.vol_a = (byte)(value & 0x0f);
                    ay.regs.env_a = false;
                    if ((value & 0x10) != 0)
                    {
                        ay.regs.env_a = true;
                    }

                    break;

                case 9:
                    ay.regs.vol_b = (byte)(value & 0x0f);
                    ay.regs.env_b = false;
                    if ((value & 0x10) != 0)
                    {
                        ay.regs.env_b = true;
                    }

                    break;

                case 10:
                    ay.regs.vol_c = (byte)(value & 0x0f);
                    ay.regs.env_c = false;
                    if ((value & 0x10) != 0)
                    {
                        ay.regs.env_c = true;
                    }

                    break;

                case 11:
                    ay.regs.env_freq = ay.regs.env_freq & 0xff00 | value;
                    break;

                case 12:
                    ay.regs.env_freq = ay.regs.env_freq & 0x00ff | value << 8;
                    break;

                case 13:
                    ay.regs.env_style = value & 0x0f;
                    ay.env_pos = ay.cnt_e = 0;
                    break;
            }
        }

        public static byte ayemu_get_reg(ayemu_ay_t ay, int reg) => ay.regs.regValues[reg];

        public static bool ayemu_gen_sound(ayemu_ay_t ay, byte[] buff)
        {
            if (!check_magic(ay))
            {
                return false;
            }

            prepare_generation(ay);
            int snd_numcount = buff.Length / (ay.sndfmt.channels * (ay.sndfmt.bpc >> 3));

            int pos = 0;

            while (snd_numcount-- > 0)
            {
                int mix_r;
                int mix_l = mix_r = 0;
                int m;
                for (m = 0; m < ay.ChipTacts_per_outcount; m++)
                {
                    if (++ay.cnt_a >= ay.regs.tone_a)
                    {
                        ay.cnt_a = 0;
                        ay.bit_a = !ay.bit_a;
                    }

                    if (++ay.cnt_b >= ay.regs.tone_b)
                    {
                        ay.cnt_b = 0;
                        ay.bit_b = !ay.bit_b;
                    }

                    if (++ay.cnt_c >= ay.regs.tone_c)
                    {
                        ay.cnt_c = 0;
                        ay.bit_c = !ay.bit_c;
                    }

                    if (++ay.cnt_n >= ay.regs.noise * 2)
                    {
                        ay.cnt_n = 0;
                        ay.Cur_Seed = ay.Cur_Seed * 2 + 1 ^ (ay.Cur_Seed >> 16 ^ ay.Cur_Seed >> 13) & 1;
                        ay.bit_n = (ay.Cur_Seed >> 16 & 1) != 0;
                    }

                    if (++ay.cnt_e >= ay.regs.env_freq)
                    {
                        ay.cnt_e = 0;
                        if (++ay.env_pos > 127)
                        {
                            ay.env_pos = 64;
                        }
                    }

                    int tmpvol;
                    if ((ay.bit_a | !ay.regs.R7_tone_a) & (ay.bit_n | !ay.regs.R7_noise_a))
                    {
                        tmpvol = ay.regs.env_a ? Envelope[ay.regs.env_style][ay.env_pos] : ay.regs.vol_a * 2 + 1;
                        mix_l += ay.vols[0][tmpvol];
                        mix_r += ay.vols[1][tmpvol];
                    }

                    if ((ay.bit_b | !ay.regs.R7_tone_b) & (ay.bit_n | !ay.regs.R7_noise_b))
                    {
                        tmpvol = ay.regs.env_b ? Envelope[ay.regs.env_style][ay.env_pos] : ay.regs.vol_b * 2 + 1;
                        mix_l += ay.vols[2][tmpvol];
                        mix_r += ay.vols[3][tmpvol];
                    }

                    if ((ay.bit_c | !ay.regs.R7_tone_c) & (ay.bit_n | !ay.regs.R7_noise_c))
                    {
                        tmpvol = ay.regs.env_c ? Envelope[ay.regs.env_style][ay.env_pos] : ay.regs.vol_c * 2 + 1;
                        mix_l += ay.vols[4][tmpvol];
                        mix_r += ay.vols[5][tmpvol];
                    }
                }

                mix_l /= ay.Amp_Global;
                mix_r /= ay.Amp_Global;
                if (ay.sndfmt.bpc == 8)
                {
                    mix_l = mix_l >> 8 | 128;
                    mix_r = mix_r >> 8 | 128;
                    buff[pos++] = (byte)mix_l;
                    if (ay.sndfmt.channels != 1)
                    {
                        buff[pos++] = (byte)mix_r;
                    }
                }
                else
                {
                    buff[pos++] = (byte)(mix_l & 0x00FF);
                    buff[pos++] = (byte)(mix_l >> 8);
                    if (ay.sndfmt.channels != 1)
                    {
                        buff[pos++] = (byte)(mix_r & 0x00FF);
                        buff[pos++] = (byte)(mix_r >> 8);
                    }
                }
            }

            return true;
        }

        private static bool check_magic(ayemu_ay_t ay)
        {
            return ay.magic == MAGIC1;
        }

        private static void gen_env()
        {
            int env;
            for (env = 0; env < 16; env++)
            {
                int hold = 0;
                int dir = (env & 4) != 0 ? 1 : -1;
                int vol = (env & 4) != 0 ? -1 : 32;
                int pos;
                for (pos = 0; pos < 128; pos++)
                {
                    if (hold == 0)
                    {
                        vol += dir;
                        if (vol is < 0 or >= 32)
                        {
                            if ((env & 8) != 0)
                            {
                                if ((env & 2) != 0)
                                {
                                    dir = -dir;
                                }

                                vol = dir > 0 ? 0 : 31;
                                if ((env & 1) != 0)
                                {
                                    hold = 1;
                                    vol = dir > 0 ? 31 : 0;
                                }
                            }
                            else
                            {
                                vol = 0;
                                hold = 1;
                            }
                        }
                    }

                    Envelope[env][pos] = vol;
                }
            }

            bEnvGenInit = true;
        }

        private static void set_table_ay(ayemu_ay_t ay, int[] tbl)
        {
            int n;
            for (n = 0; n < 32; n++)
            {
                ay.table[n] = tbl[n / 2];
            }

            ay.type = ayemu_chip_t.AYEMU_AY;
        }

        private static void set_table_ym(ayemu_ay_t ay, int[] tbl)
        {
            int n;
            for (n = 0; n < 32; n++)
            {
                ay.table[n] = tbl[n];
            }

            ay.type = ayemu_chip_t.AYEMU_YM;
        }

        private static void prepare_generation(ayemu_ay_t ay)
        {
            if (!ay.dirty)
            {
                return;
            }

            if (!bEnvGenInit)
            {
                gen_env();
            }

            if (ay.default_chip_flag)
            {
                _ = ayemu_set_chip_type(ay, ayemu_chip_t.AYEMU_AY, null);
            }

            if (ay.default_stereo_flag)
            {
                _ = ayemu_set_stereo(ay, ayemu_stereo_t.AYEMU_ABC, null);
            }

            if (ay.default_sound_format_flag != 0)
            {
                _ = ayemu_set_sound_format(ay, 44100, 2, 16);
            }

            ay.ChipTacts_per_outcount = ay.ChipFreq / ay.sndfmt.freq / 8;

            {
                int n;
                for (n = 0; n < 32; n++)
                {
                    int nvol = ay.table[n];
                    int m;
                    for (m = 0; m < 6; m++)
                    {
                        ay.vols[m][n] = (int)(nvol * ay.eq[m] / 100.0);
                    }
                }
            }

            int max_l = ay.vols[0][31] + ay.vols[2][31] + ay.vols[3][31];
            int max_r = ay.vols[1][31] + ay.vols[3][31] + ay.vols[5][31];
            int vol = max_l > max_r ? max_l : max_r;
            ay.Amp_Global = ay.ChipTacts_per_outcount * vol / AYEMU_MAX_AMP;
            ay.dirty = false;
        }

    }
}