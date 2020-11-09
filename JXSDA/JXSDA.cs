using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TrickyUnits {
    static class JXSDA {

        static public bool Verbose = false;
        static void P(string m) {
            if (Verbose) {
                QCol.Cyan("Verbose: ");
                Console.ResetColor();
                Console.WriteLine(m);
            }
        }

        static private byte[] cpy(byte[] i) {
            P($"cpy called for {i.Length} bytes");
            var o = new List<byte>();
            for (long p = 0; p < i.Length; p++) o.Add(i[p]);
            return o.ToArray();
        }

        static int no8min(int m1, int m2) {
            if (m1 == 0) return m2;
            if (m2 == 0) return m1;
            return Math.Max(m1, m2);
        }

        static byte needbit(int s) {
            if (s <= 255) return 8;
            if (s<= 65535) return 16;
            //if (s <= 4294967295) return 32;
            //return 64;
            return 32;
        }

        static int dicsize(int dent) {
            var nb = needbit(dent);
            var tot = dent * nb;
            return tot;
        }

        static double Ratio(int deel,int geheel) {
            return (double)deel / (double)geheel;
        }

        static public byte[] Pack(byte[] ibuf) {
            P($"Request to pack {ibuf.Length}");
            if (ibuf.Length < 1024) return cpy(ibuf);
            var d64 = new List<long>();
            var d32 = new List<int>();
            var d16 = new List<short>();
            var bank = new BlitzBank(ibuf, BlitzEndian.Little, true);
            byte mod64;
            byte mod32;
            byte mod16;

            // 64 bit
            P("Creating Dictionary for 64 bit");
            for (int i = 0; i < ibuf.Length - 8; i += 8) {
                var p = bank.PeekLong(i);
                if (!d64.Contains(p)) d64.Add(p);
            }
            mod64 = (byte)(ibuf.Length % 8);
            P($"64 bit result (Entries {d64.Count}; Full: {ibuf.Length} Div: {(int)(ibuf.Length / 8)}; Mod: {mod64}; Pb: {needbit(d64.Count)} ");

            // 32 bit
            P("Creating Dictionary for 32 bit");
            for (int i = 0; i < ibuf.Length - 4; i += 4) {
                var p = bank.PeekInt(i);
                if (!d32.Contains(p)) d32.Add(p);
            }
            mod32 = (byte)(ibuf.Length % 4);
            P($"32 bit result (Entries {d32.Count}; Full: {ibuf.Length} Div: {(int)(ibuf.Length / 4)}; Mod: {mod32}; Pb: {needbit(d32.Count)} ");


            // 16 bit
            P("Creating Dictionary for 16 bit");
            for (int i = 0; i < ibuf.Length - 2; i += 2) {
                var p = bank.PeekShort(i);
                if (!d16.Contains(p)) d16.Add(p);
            }
            mod16 = (byte)(ibuf.Length % 2);
            P($"16 bit result (Entries {d16.Count}; Full: {ibuf.Length} Div: {(int)(ibuf.Length / 2)}; Mod: {mod16}; Pb: {needbit(d16.Count)} ");

            byte ubit = 64;
            byte pbit = 64;
            byte uinc = 8;
            byte pinc = 8;
            int dsiz = 0;            
            var rat64 = Ratio(d64.Count, ibuf.Length / 8);
            var rat32 = Ratio(d32.Count, ibuf.Length / 4);
            var rat16 = Ratio(d16.Count, ibuf.Length / 2);
            byte rest = 0;
            P($"Ratio   64:{rat64}; 32:{rat32}; 16:{rat16}");

            if (rat16 < rat64 && rat16 < rat32 && needbit(d16.Count) < 16) { ubit = 16; pbit = needbit(d16.Count); dsiz = (int)d16.Count; rest = mod16; uinc = 2; } else if (rat32 < rat64 && needbit(d32.Count) < 32) { ubit = 32; pbit = needbit(d32.Count); dsiz = (int)d32.Count; rest = mod32; uinc = 4; } 
            P($"Chosen: {ubit} => {pbit}");
            if (pbit == 64) {
                P("Packing not possible, so let's get outta here!");
                return cpy(ibuf);
            }
            var Dict = new Dictionary<long, long>();
            var PureMemStream = new MemoryStream();
            var MS = new QuickStream(PureMemStream);
            MS.WriteString("JXSDA\x1a", true);
            MS.WriteByte(ubit);
            MS.WriteByte(pbit);
            MS.WriteByte(rest);
            for (int i = 0; i < dsiz; i++) {
                switch (ubit) {
                    case 32:
                        Dict[d32[i]] = i;
                        MS.WriteInt(d32[i]);
                        break;
                    case 64:
                        Dict[d64[i]] = i;
                        MS.WriteLong(d64[i]);
                        break;
                    case 16:
                        P($"Dict create {i}/{dsiz}/{d16.Count}");
                        Dict[d16[i]] = i;
                        MS.WriteLong(d16[i]);
                        break;
                    default:
                        throw new Exception("Unknown bit setting in creating dictionary (bug?)");
                }
            }
            for(int p = 0; p < ibuf.Length-uinc; p += uinc) {
                long value;
                switch (ubit) {
                    case 64:
                        value = bank.PeekLong(p);
                        break;
                    case 32:
                        value = bank.PeekInt(p);
                        break;
                    case 16:
                        value = bank.PeekShort(p);
                        break;
                    default:
                        throw new Exception("Unknown bit setting in reading bytes to pack (bug?)");
                }
                switch (pbit) {
                    case 64:
                        throw new Exception("64 bit output invalid! (bug?)");
                    case 32:
                        MS.WriteInt((int)Dict[value]);
                        break;
                    case 16:
                        MS.WriteShort((short)Dict[value]);
                        break;
                    default:
                        throw new Exception("Unknown bit setting in writing bytes to pack (bug?)");
                }
            }
            for (int p = ibuf.Length - rest; p < ibuf.Length; p++) MS.WriteByte(ibuf[p]);
            var ret = PureMemStream.ToArray();
            MS.Close();
            return ret; // temp
        }
    }
}
