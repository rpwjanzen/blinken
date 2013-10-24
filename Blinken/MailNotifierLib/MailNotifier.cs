using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Drawing;
using MailNotifierLib.Extensions;

namespace MailNotifierLib
{
    public sealed class MailNotifier
    {
        private readonly HidDevice m_device;

        // Device cannot accurately display 255 different levels (hardware limitation?)
        // Yellow and Orange look the same

        public MailNotifier()
        {
            var devices = HidLibrary.HidDevices.Enumerate(0x1D34, 0x0004);
            foreach (var device in devices)
            {
                System.Diagnostics.Debug.WriteLine("Found Dream Cheeky Mail Notifier Device");

                device.OpenDevice();

                byte[] init1 = { 0x00, 0x1f, 0x02, 0x00, 0x2e, 0x00, 0x00, 0x2b, 0x03 };
                byte[] init2 = { 0x00, 0x00, 0x02, 0x00, 0x2e, 0x00, 0x00, 0x2b, 0x04 };
                byte[] init3 = { 0x00, 0x00, 0x02, 0x00, 0x2e, 0x00, 0x00, 0x2b, 0x05 };

                WriteData(device, init1);
                WriteData(device, init2);

                m_device = device;
            }
        }

        private Color m_color;
        public Color Color
        {
            get { return m_color; }
            set
            {
                byte[] usbData = value.ToNotifierColor().ToUsbData();
                WriteData(m_device, usbData);
                m_color = value;
            }
        }

        public void FadeTo(Color color)
        {
            byte r = (byte)(color.R / 4);
            byte g = (byte)(color.G / 4);
            byte b = (byte)(color.B / 4);

            for (int i = 0; i < NotifierColor.MaxColorValue; i += 1)
            {
                WriteData(m_device, Color.FromArgb(
                    (byte)((i / (double)NotifierColor.MaxColorValue) * r),
                    (byte)((i / (double)NotifierColor.MaxColorValue) * g),
                    (byte)((i / (double)NotifierColor.MaxColorValue) * b)
                    ).ToNotifierColor().ToUsbData());
                System.Threading.Thread.Sleep(50);
            }
            for (int i = NotifierColor.MaxColorValue; i >= 0; i -= 1)
            {
                WriteData(m_device, Color.FromArgb(
                    (byte)((i / (double)NotifierColor.MaxColorValue) * r),
                    (byte)((i / (double)NotifierColor.MaxColorValue) * g),
                    (byte)((i / (double)NotifierColor.MaxColorValue) * b)
                    ).ToNotifierColor().ToUsbData());
                System.Threading.Thread.Sleep(50);
            }
        }

        private static bool WriteData(HidDevice device, byte[] data)
        {
            bool wroteData = device.Write(data);
            if (!wroteData)
                Console.WriteLine("Failed to write data");
            return wroteData;
        }
    }

    // http://stackoverflow.com/questions/2353211/hsl-to-rgb-color-conversion
    public static class ColorUtil
    {
        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0.0) t += 1;
            if (t > 1.0) t -= 1;
            if (t < (1.0 / 6.0))
            {
                return p + (q - p) * 6 * t;
            }

            if (t < (1.0 / 2.0))
            {
                return q;
            }

            if (t < (2.0 / 3.0))
            {
                return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
            }

            return p;
        }

        /**
         * Converts an HSL color value to RGB. Conversion formula
         * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
         * Assumes h, s, and l are contained in the set [0, 1] and
         * returns r, g, and b in the set [0, 255].
         *
         * @param   Number  h       The hue
         * @param   Number  s       The saturation
         * @param   Number  l       The lightness
         * @return  Array           The RGB representation
         */
        public static byte[] HslToRgb(double hue, double saturation, double lightness)
        {
            double red;
            double green;
            double blue;

            if (saturation == 0)
            {
                red = green = blue = lightness; // achromatic
            }
            else
            {

                var q = lightness < 0.5 ? lightness * (1.0 + saturation) : lightness + saturation - lightness * saturation;
                var p = 2 * lightness - q;
                red = HueToRgb(p, q, hue + 1.0 / 3.0);
                green = HueToRgb(p, q, hue);
                blue = HueToRgb(p, q, hue - 1.0 / 3.0);
            }

            return new byte[] { (byte)(red * 255.0), (byte)(green * 255.0), (byte)(blue * 255.0) };
        }

        /**
         * Converts an RGB color value to HSL. Conversion formula
         * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
         * Assumes r, g, and b are contained in the set [0, 255] and
         * returns h, s, and l in the set [0, 1].
         *
         * @param   Number  r       The red color value
         * @param   Number  g       The green color value
         * @param   Number  b       The blue color value
         * @return  Array           The HSL representation
         */
        public static double[] RgbToHsl(byte red, byte green, byte blue)
        {
            var scaledRed = red / 255.0;
            var scaledGreen = green / 255.0;
            var scaledBlue = blue / 255.0;

            var max = Math.Max(Math.Max(scaledRed, scaledGreen), scaledBlue);
            var min = Math.Min(Math.Min(scaledRed, scaledGreen), scaledBlue);

            var hue = (max + min) / 2.0;
            var saturation = hue;
            var lightness = saturation;

            if(max == min)
            {
                hue = saturation = 0.0; // achromatic
            }
            else
            {
                var delta = max - min;
                if (lightness > 0.5)
                {
                    saturation = delta / (2.0 - max - min);
                }
                else
                {
                    saturation = delta / (max + min);
                }

                if (max == scaledRed)
                {
                    hue = (scaledGreen - scaledBlue) / delta + (scaledGreen < scaledBlue ? 6.0 : 0.0);
                }
                else if (max == scaledGreen)
                {
                    hue = (scaledBlue - scaledRed) / delta + 2.0;
                }
                else if (max == scaledBlue)
                {
                    hue = (scaledRed - scaledGreen) / delta + 4.0;
                }
                else
                {
                    throw new NotSupportedException();
                }

                hue /= 6.0;
            }

            return new double [] { hue, saturation, lightness };
        }
    }

    public class NotifierColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public byte[] ToUsbData()
        {
            byte[] usbData = { 0x00, Red, Green, Blue, 0x00, 0x00, 0x00, 0x00, 0x05 };

            return usbData;
        }

        public static byte[] GetUsbData(Color color)
        {
            byte[] usbData = { 0x00, color.R, color.G, color.B, 0x00, 0x00, 0x00, 0x00, 0x05 };

            return usbData;
        }

        public static byte MaxColorValue { get { return 64; } }
    }
}
