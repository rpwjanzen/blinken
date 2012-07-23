using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Collections;

namespace Blinken
{
    public sealed class LcdNotifier
    {
        public LcdNotifier()
        {
            var devices = HidLibrary.HidDevices.Enumerate(0x1D34, 0x0013);
            foreach (var device in devices)
            {
                Console.WriteLine("Woot");

                device.OpenDevice();
                System.Threading.Thread.Sleep(2500);
                while (true)
                {
                    DoIt(device);
                }
                //DoEye(device);
            }
        }

        private static void DoIt(HidDevice device)
        {
            int i = 0;
            while(true)
            {
                i++;
                i %= 3;
                LedBrightness b = (LedBrightness)i;
                byte[] p0 = GetUsbData(b, StartingRow.Zero, 0x00);
                byte[] p1 = GetUsbData(b, StartingRow.Second, 0x00);
                byte[] p2 = GetUsbData(b, StartingRow.Fourth, 0x00);
                byte[] p3 = GetUsbData(b, StartingRow.Sixth, 0x00);

                device.Write(p0);
                device.Write(p1);
                device.Write(p2);
                device.Write(p3);

                System.Threading.Thread.Sleep(400);
            }
        }

        private void DoEye(HidDevice device)
        {
            while (true)
            {
                byte[] Packet0 = new byte[] { 0x00, 0x00, 0x00, 0xFF, 0xFC, 0x7F, 0xFF, 0xF8, 0x3F };
                byte[] Packet1 = new byte[] { 0x00, 0x00, 0x02, 0xFF, 0xF0, 0x1F, 0xFF, 0xE0, 0x0F };
                byte[] Packet2 = new byte[] { 0x00, 0x00, 0x04, 0xFF, 0xF0, 0x1F, 0xFF, 0xF8, 0x3F };
                byte[] Packet3 = new byte[] { 0x00, 0x00, 0x06, 0xFF, 0xFC, 0x7F,
                    0xFF, 0xFF, 0xFF };

                device.Write(Packet0);
                device.Write(Packet1);
                device.Write(Packet2);
                device.Write(Packet3);

                System.Threading.Thread.Sleep(400);

                byte[] Packet4 = new byte[] { 0x00, 0x00, 0x00, 0xFF, 0xFE, 0xFF, 0xFF, 0xFD, 0x7F };
                byte[] Packet5 = new byte[] { 0x00, 0x00, 0x02, 0xFF, 0xFB, 0xBF, 0xFF, 0xF7, 0xDF };
                byte[] Packet6 = new byte[] { 0x00, 0x00, 0x04, 0xFF, 0xFB, 0xBF, 0xFF, 0xFD, 0x7F };
                byte[] Packet7 = new byte[] { 0x00, 0x00, 0x06, 0xFF, 0xFE, 0xFF,
                    0xFF, 0xFF, 0xFF };

                device.Write(Packet4);
                device.Write(Packet5);
                device.Write(Packet6);
                device.Write(Packet7);

                System.Threading.Thread.Sleep(400);
            }
        }

        // 21x7 LEDs in board
        private static byte [] GetUsbData(LedBrightness ledBrightness, StartingRow startingRow, byte fill)
        {
            byte brightness = (byte)ledBrightness;
            byte row0 = (byte)startingRow;
            byte row1 = (byte)(((int)row0) + 1);

            byte[] data = new byte [] {
                0x00, // padding?
                brightness, row0,
                fill, fill, fill,
                fill, fill, fill,
            };

            return data;
        }
    }

    enum LedBrightness : byte { Low = 0, Med = 1, High = 2 }

    enum StartingRow : byte { Zero = 0, Second = 2, Fourth = 4, Sixth = 6 }
}
