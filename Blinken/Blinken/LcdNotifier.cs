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
            if (m_device == null)
            {
                Console.Error.WriteLine("!!!!!!!!!!!!!!!!!Cannot find USB LED sign!!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }
        }

        public string Text = "Hello";
        public bool[,] Image;

        public void ScrollImage(TimeSpan scrollDelay)
        {
            if (Image == null)
                return;

            VirtualLcdScreen virtualLcdScreen = new VirtualLcdScreen();
            
            int totalWidth = Math.Max(21, Image.GetLength(0));

            virtualLcdScreen.Width = totalWidth;

            Point upperLeft = Point.Empty;
            virtualLcdScreen.Blit(Image, upperLeft);

            // initial delay before we start scrolling because text is hard to read if it is scrolling from the start
            {
                var usbData = virtualLcdScreen.GetUsbData(0);

                for (int n = 0; n < 2; n++)
                {
                    for (int i = 0; i < usbData.Count; i++)
                    {
                        m_device.Write(usbData[i]);
                    }

                    // initial delay before horizontal scrolling starts
                    System.Threading.Thread.Sleep(400);
                }
            }

            for (int horizontalOffset = 0; horizontalOffset < virtualLcdScreen.Width; horizontalOffset++)
            {
                var usbData = virtualLcdScreen.GetUsbData(horizontalOffset);

                for (int i = 0; i < usbData.Count; i++)
                {
                    var usbBytes = usbData[i];
                    m_device.Write(usbBytes);
                }

                // horizontal scrolling delay
                System.Threading.Thread.Sleep(scrollDelay);
            }
        }

        public void ShowText(LedFont font)
        {
            VirtualLcdScreen lcdScreen = new VirtualLcdScreen();
            List<Letter> characters;
            characters = Text
                .Select(c => font[c])
                .ToList();

            int totalCharacterWidths = characters
                .Aggregate(0, (acc, l) => l.Data.GetLength(0) + 1 + acc);
            int totalWidth = Math.Max(21, totalCharacterWidths);

            lcdScreen.Width = totalWidth;

            var line = new bool[totalWidth];
            for (int i = 0; i < line.GetLength(0); i++)
            {
                line[i] = i % 5 != 0;
            }

            Point upperLeft = Point.Empty;
            for (int ci = 0; ci < characters.Count; ci++)
            {
                var c = characters[ci];
                lcdScreen.Blit(c.Data, upperLeft);
                upperLeft.X = upperLeft.X + c.Data.GetLength(0) + 1;
            }

            // display text
            var usbData = lcdScreen.GetUsbData(0);
            for (int i = 0; i < usbData.Count; i++)
            {
                m_device.Write(usbData[i]);
            }
            //System.Threading.Thread.Sleep(400);
        }

        public void ScrollText(LedFont font, TimeSpan scrollDelay)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            ScrollText(m_device, Text ?? string.Empty, font, scrollDelay);
        }

        private static void ScrollText(HidDevice device, string text, LedFont font, TimeSpan scrollDelay)
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

            var line = new bool[totalWidth];
            for (int i = 0; i < line.GetLength(0); i++)
            {
                line[i] = i % 5 != 0;
            }

            Point upperLeft = Point.Empty;
            for (int ci = 0; ci < characters.Count; ci++)
            {
                var c = characters[ci];
                lcdScreen.Blit(c.Data, upperLeft);
                upperLeft.X = upperLeft.X + c.Data.GetLength(0) + 1;
            }

            // initial delay before we start scrolling because text is hard to read if it is scrolling from the start
            {
                var usbData = lcdScreen.GetUsbData(0);

                for (int n = 0; n < 2; n++)
                {
                    for (int i = 0; i < usbData.Count; i++)
                    {
                        device.Write(usbData[i]);
                    }

                    // initial delay before horizontal scrolling starts
                    System.Threading.Thread.Sleep(400);
                }
            }

            for (int horizontalOffset = 0; horizontalOffset < lcdScreen.Width; horizontalOffset++)
            {
                var usbData = lcdScreen.GetUsbData(horizontalOffset);

                for (int i = 0; i < usbData.Count; i++)
                {
                    var usbBytes = usbData[i];
                    device.Write(usbBytes);
                }

                // horizontal scrolling delay
                System.Threading.Thread.Sleep(scrollDelay);
            }
        }
    }

    public enum LedBrightness : byte { Low = 0, Med = 1, High = 2 }
}
