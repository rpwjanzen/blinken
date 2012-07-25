using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Collections;
using System.Drawing;

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
                Console.WriteLine("Woot");

                device.OpenDevice();
                m_device = device;
                break;
            }
        }

        private readonly List<string> m_lines = new List<string>();
        private string m_text;
        public string Text
        {
            get { return m_text; }
            set
            {
                m_text = value ?? string.Empty;

                m_lines.Clear();

                string [] words = Text.Split(' ');
                foreach (var word in words)
                {
                    string w = word;

                    while (w.Length > 5)
                    {
                        string part = w.Substring(0, 5);
                        //w = part;
                        m_lines.Add(part);

                        w = '-' + w.Substring(5);
                    }
                    m_lines.Add(w);
                }
            }
        }

        public void DrawText()
        {
            foreach(var line in m_lines)
            {
                DoText(m_device, line);
                System.Threading.Thread.Sleep(400);
                DoText(m_device, line);
                System.Threading.Thread.Sleep(400);
            }
        }

        private static void DoText(HidDevice device, string text)
        {
            LcdScreen lcdScreen = new LcdScreen();
            var characters = text.Select(c => Alphabet.Letters[c]).Select(c =>
                {
                    // need to rotate 90 degrees to the right
                    var c0 = new byte[c.Data.GetLength(1), c.Data.GetLength(0)];
                    for (int col = 0; col < c.Data.GetLength(0); col++)
                    {
                        for (int r = 0; r < c.Data.GetLength(1); r++)
                        {
                            c0[r, col] = c.Data[col, r];
                        }
                    }

                    return c0;
                }).ToList();

            Point upperLeft = Point.Empty;
            foreach(var c in characters)
            {
                lcdScreen.Blit(c, upperLeft);
                upperLeft.X = upperLeft.X + c.GetLength(0) + 1;
            }

            var usbData = lcdScreen.GetUsbData();

            for (int i = 0; i < usbData.Count; i++)
            {
                device.Write(usbData[i]);
            }
        }
    }

    enum LedBrightness : byte { Low = 0, Med = 1, High = 2 }
}
