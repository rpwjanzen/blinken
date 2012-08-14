using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Blinken
{
    public sealed class LcdScreen
    {
        public const int Width = 21;
        public const int Height = 7;

        public readonly bool[,] Data = new bool[Width, Height];

        /// <summary>
        /// Blits to the source to the LED screen position specified by the upper left point.
        /// </summary>
        public void Blit(bool[,] source, Point upperLeft)
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

        public List<byte[]> GetUsbData()
        {
            List<byte[]> bytes = new List<byte[]>();

            for (int row = 0; row <= 6; row += 2)
            {
                bool[] r0 = new bool[21];
                for (int c = 0; c < 21; c++)
                {
                    r0[c] = Data[c, row];
                }

                var bs0 = BitUtil.GetRowUsbBytes(r0);

                byte[] bs1;
                if (row != 6)
                {
                    bool[] r1 = new bool[21];
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
