﻿using System;
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
                Console.Error.WriteLine("Cannot find USB LED sign.");
                return;
            }
        }

        public string Text;

        public void ScrollText(LedFont font)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            ScrollText(m_device, Text ?? string.Empty, font);
        }

        private static void ScrollText(HidDevice device, string text, LedFont font)
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
            //int tw = 0;
            for (int ci = 0; ci < characters.Count; ci++)
            {
                var c = characters[ci];
                
                //var segment = new bool[c.Data.GetLength(0) + 1, 1];
                //for(int i = 0; i < segment.GetLength(0); i++)
                //{
                //    segment[i, 0] = line[tw + i];
                //}
                
                //// blit upper border
                //lcdScreen.Blit(segment, upperLeft);
                //upperLeft.Y = upperLeft.Y + 2;

                // blit letter contents
                //lcdScreen.Blit(c.Data, new Rectangle(Point.Empty, new Size(c.Data.GetLength(0), c.Data.GetLength(1))), new Point(upperLeft.X, upperLeft.Y));
                lcdScreen.Blit(c.Data, upperLeft);

                //// blit lower border
                //upperLeft.Y = upperLeft.Y + 4;

                //lcdScreen.Blit(segment, upperLeft);

                //var letterwidth = c.Data.GetLength(0);
                upperLeft.X = upperLeft.X + c.Data.GetLength(0) + 1;
                //upperLeft.Y = 0;
                //tw += letterwidth + 1;
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
