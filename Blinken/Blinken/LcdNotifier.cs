using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Blinken.Font;
using HidLibrary;

namespace Blinken
{
    public sealed class LcdNotifier
    {
        private readonly HidDevice m_device;

        public LcdNotifier()
        {
            var devices = HidLibrary.HidDevices.Enumerate(0x1D34, 0x0013);
            foreach (var device in devices)
            {
                Console.WriteLine("Found USB LED sign.");

                device.OpenDevice();
                m_device = device;
                break;
            }
        }

        public string Text;

        public void DrawText(LedFont font)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            DoText(m_device, Text ?? string.Empty, font);
            System.Threading.Thread.Sleep(400);
        }

        private static void DoText(HidDevice device, string text, LedFont font)
        {
            VirtualLcdScreen lcdScreen = new VirtualLcdScreen();
            List<Letter> characters;
            characters = text
                .Select(c => font[c])
                .ToList();

            int totalCharacterWidths = characters
                .Aggregate(0, (acc, l) => l.Data.GetLength(0) + 1 + acc);
            int totalWidth = Math.Max(21, totalCharacterWidths);

            lcdScreen.Width = totalWidth;

            Point upperLeft = Point.Empty;
            foreach (var c in characters)
            {
                lcdScreen.Blit(c.Data, upperLeft);
                upperLeft.X = upperLeft.X + c.Data.GetLength(0) + 1;
            }

            {
                var usbData = lcdScreen.GetUsbData(0);

                for (int n = 0; n < 2; n++)
                {
                    for (int i = 0; i < usbData.Count; i++)
                    {
                        device.Write(usbData[i]);
                    }
                    System.Threading.Thread.Sleep(400);
                }
            }

            for (int c = 0; c < lcdScreen.Width; c++)
            {
                var usbData = lcdScreen.GetUsbData(c);

                for (int i = 0; i < usbData.Count; i++)
                {
                    device.Write(usbData[i]);
                }

                System.Threading.Thread.Sleep(40);
            }
        }
    }

    public enum LedBrightness : byte { Low = 0, Med = 1, High = 2 }
}
