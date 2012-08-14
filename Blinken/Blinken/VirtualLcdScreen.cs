using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Blinken
{
    public sealed class VirtualLcdScreen
    {
        public int Width
        {
            get { return Data.GetLength(0); }
            set
            {
                if (Width == value)
                    return;

                if (value < Width)
                    m_data = GetShrunkLcdScreen(value, Height);
                else
                    m_data = GetEnlargedLcdScreen(value, Height);
            }
        }

        public int Height
        {
            get { return Data.GetLength(1); }
        }

        private bool[,] m_data;
        public bool[,] Data
        {
            get { return m_data; }
        }

        public VirtualLcdScreen()
        {
            m_data = new bool[21, 7];
        }

        private bool[,] GetEnlargedLcdScreen(int newWidth, int newHeight)
        {
            bool[,] newData = new bool[newWidth, newHeight];

            for (int c = 0; c < Data.GetLength(0); c++)
            {
                for (int r = 0; r < Data.GetLength(1); r++)
                {
                    newData[c, r] = Data[c, r];
                }
            }

            return newData;
        }

        private bool[,] GetShrunkLcdScreen(int newWidth, int newHeight)
        {
            bool[,] newData = new bool[newWidth, newHeight];

            for (int c = 0; c < newWidth; c++)
            {
                for (int r = 0; r < newHeight; r++)
                {
                    newData[c, r] = Data[c, r];
                }
            }

            return newData;
        }

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

        public List<byte[]> GetUsbData(int horizontalOffset)
        {
            List<byte[]> bytes = new List<byte[]>();
            for (int row = 0; row <= 6; row += 2)
            {
                bool[] r0 = new bool[21];
                for (int c = 0; c < 21; c++)
                {
                    if (c + horizontalOffset < Data.GetLength(0))
                        r0[c] = Data[c + horizontalOffset, row];
                    else
                        r0[c] = false;
                }

                var bs0 = BitUtil.GetRowUsbBytes(r0);

                byte[] bs1;
                if (row != 6)
                {
                    bool[] r1 = new bool[21];
                    for (int c = 0; c < 21; c++)
                    {
                        if (c + horizontalOffset < Data.GetLength(0))
                            r1[c] = Data[c + horizontalOffset, row + 1];
                        else
                            r1[c] = false;
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
