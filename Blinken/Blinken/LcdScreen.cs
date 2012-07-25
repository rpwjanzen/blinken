using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Blinken
{
    [DebuggerDisplay("{Text}")]
    public sealed class LcdScreen
    {
        public const int Width = 21;
        public const int Height = 7;

        public readonly byte[,] Data = new byte[Width, Height];

        public void Blit(byte[,] source, Point upperLeft)
        {
            int sc = 0;
            for (int c = 0; c < source.GetLength(0); c++)
            {
                int sr = 0;
                for (int r = 0; r < source.GetLength(1); r++)
                {
                    if (c + upperLeft.X < Data.GetLength(0)
                        && r + upperLeft.Y < Data.GetLength(1))
                    {
                        Data[c + upperLeft.X, r + upperLeft.Y] = source[sc, sr];
                    }
                    sr++;
                }
                sc++;
            }
        }

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int c = 0; c < Data.GetLength(1); c++)
                {
                    for (int r = 0; r < Data.GetLength(0); r++)
                    {
                        byte b = Data[r, c];
                        sb.Append(b);
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        public List<byte[]> GetUsbData()
        {
            List<byte[]> bytes = new List<byte[]>();
            for (int row = 0; row <= 6; row += 2)
            {
                byte[] r0 = new byte[21];
                for (int c = 0; c < 21; c++)
                {
                    r0[c] = Data[c, row];
                }

                var bs0 = BitUtil.GetRowUsbBytes(r0);

                byte[] bs1;
                if (row != 6)
                {
                    byte[] r1 = new byte[21];
                    for (int c = 0; c < 21; c++)
                    {
                        r1[c] = Data[c, row + 1];
                    }

                    bs1 = BitUtil.GetRowUsbBytes(r1);
                }
                else
                {
                    // row 8 is not displayed
                    bs1 = new byte[3];
                }

                bs0[2] |= 0x07;
                bs1[2] |= 0x07;

                var usbData = BitUtil.GetUsbPackets(LedBrightness.Low, (byte)row,
                    bs0[0], bs0[1], bs0[2],
                    bs1[0], bs1[1], bs1[2]);

                bytes.Add(usbData);
            }

            return bytes;
        }
    }
}
