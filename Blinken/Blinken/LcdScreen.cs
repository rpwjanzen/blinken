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

                var bs0 = GetUsbBytes(r0);

                byte[] bs1;
                if (row != 6)
                {
                    byte[] r1 = new byte[21];
                    for (int c = 0; c < 21; c++)
                    {
                        r1[c] = Data[c, row + 1];
                    }

                    bs1 = GetUsbBytes(r1);
                }
                else
                {
                    // row 8 is not displayed
                    bs1 = new byte[3];
                }

                bs0[2] |= 0x07;
                bs1[2] |= 0x07;

                var usbData = GetUsbData(LedBrightness.Low, (byte)row,
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
            for (int i = 0; i < 8; i++)
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

            return new byte[] { b0, b1, b2 };
        }

        // 21x7 LEDs in board, 2 rows at a time
        private byte[] GetUsbData(LedBrightness ledBrightness, byte startingRow, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
        {
            byte brightness = (byte)ledBrightness;

            // need to reverse the order of the bits in each byte
            b0 = Reverse(b0);
            b1 = Reverse(b1);
            b2 = Reverse(b2);
            b3 = Reverse(b3);
            b4 = Reverse(b4);
            b5 = Reverse(b5);

            byte[] data = new byte[] {
                0x00, // padding?
                brightness, startingRow,
                b2, b1, b0,
                b5, b4, b3,
            };

            return data;
        }

        private byte Reverse(byte b)
        {
            // input bits to be reversed
            byte r = b; // r will be reversed bits of v; first get LSB of v
            int s = 8 * 1 - 1; // extra shift needed at end

            for (b >>= 1; b > 0; b >>= 1)
            {   
              r <<= 1;
              r |= (byte)(b & (byte)1);
              s--;
            }

            r <<= s; // shift when v's highest bits are zero

            return r;
        }
    }
}
