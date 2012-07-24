using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Blinken
{
    public sealed class LcdScreen
    {
        public const int Width = 21;
        public const int Height = 7;

        public readonly byte[,] Data = new byte [Width,Height];

        public void Blit(byte[,] source, Point upperLeft)
        {
            int sr = 0;
            for (int r = upperLeft.Y; r < source.GetLength(0); r++)
            {
                int sc = 0;
                for (int c = upperLeft.X; c < source.GetLength(1); c++)
                {
                    Data[r, c] = source[sr, sc];
                    sc++;
                }
                sr++;
            }
        }

        public List<byte[]> GetUsbData()
        {
            List<byte[]> bytes = new List<byte[]>();
            for (int row = 0; row <= 6; row += 2)
            {
                byte [] r0 = new byte [21];
                for(int c = 0; c < 21; c++)
                {
                    r0[c] = Data[row,c];
                }

                var bs0 = GetUsbBytes(r0);

                byte[] bs1;
                if (row != 6)
                {
                    byte[] r1 = new byte[21];
                    for (int c = 0; c < 21; c++)
                    {
                        r1[c] = Data[row + 1, c];
                    }

                    bs1 = GetUsbBytes(r1);
                }
                else
                {
                    // row 8 is not displayed
                    bs1 = new byte[3];
                }

                var usbData = GetUsbData(LedBrightness.Low, (byte)(row + 1),
                    bs0[0], bs0[1], bs0[2],
                    bs1[0], bs1[1], bs1[2]);

                bytes.Add(usbData);
            }

            return bytes;
        }

        private byte[] GetUsbBytes(byte[] rowData)
        {
            System.Diagnostics.Debug.Assert(rowData.Length == 21);

            byte b0 = 0;
            byte mask = 0x80;
            for(int i = 0; i < 8; i++)
            {
                byte pixelData = rowData[i];
                if (pixelData == 0)
                    b0 |= mask;

                mask = (byte)(mask >> 1);
            }

            byte b1 = 0;
            mask = 0x80;
            for (int i = 8; i < 16; i++)
            {
                byte pixelData = rowData[i];
                if (pixelData == 0)
                    b1 |= mask;

                mask = (byte)(mask >> 1);
            }

            byte b2 = 0;
            mask = 0x80;
            for (int i = 16; i < 21; i++)
            {
                byte pixelData = rowData[i];
                if (pixelData == 0)
                    b2 |= mask;

                mask = (byte)(mask >> 1);
            }

            return new byte [] { b0, b1, b2 };
        }

        // 21x7 LEDs in board, 2 rows at a time
        private byte[] GetUsbData(LedBrightness ledBrightness, byte startingRow, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
        {
            byte brightness = (byte)ledBrightness;

            byte[] data = new byte[] {
                0x00, // padding?
                brightness, startingRow,
                b0, b1, b2,
                b3, b4, b5,
            };

            return data;
        }
    }
}
