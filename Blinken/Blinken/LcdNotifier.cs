using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidLibrary;

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

                DoEye(device);
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
    }
}
