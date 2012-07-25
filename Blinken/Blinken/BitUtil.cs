namespace Blinken
{
    public static class BitUtil
    {
        public static byte[] GetRowUsbBytes(byte[] rowData)
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
        public static byte[] GetUsbPackets(LedBrightness ledBrightness, byte startingRow, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
        {
            byte brightness = (byte)ledBrightness;

            // need to reverse the order of the bits in each byte
            b0 = BitUtil.Reverse(b0);
            b1 = BitUtil.Reverse(b1);
            b2 = BitUtil.Reverse(b2);
            b3 = BitUtil.Reverse(b3);
            b4 = BitUtil.Reverse(b4);
            b5 = BitUtil.Reverse(b5);

            byte[] data = new byte[] {
                0x00, // padding?
                brightness, startingRow,
                b2, b1, b0,
                b5, b4, b3,
            };

            return data;
        }

        public static byte Reverse(byte b)
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
