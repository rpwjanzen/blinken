using System.Drawing;
using System.Collections.Generic;

namespace MailNotifierLib.Extensions
{
    public static class ColorEx
    {
        public static List<NotifierColor> AllColors = new List<NotifierColor>();
        private const byte High = 64;
        private const byte Mid = 48;
        private const byte Low = 32;
        private const byte Off = 0;

        static ColorEx()
        {
            List<NotifierColor> colors = new List<NotifierColor>();
            // reds
            colors.Add(MakeColor(High, Off, Off));
            colors.Add(MakeColor(High, Low, Low));

            // orange
            colors.Add(MakeColor(High, Low, Off));

            // yellow
            colors.Add(MakeColor(High, High, Off));

            // greens
            colors.Add(MakeColor(Low, High, Off));
            colors.Add(MakeColor(Off, High, Off));
            colors.Add(MakeColor(Off, Low, Low));

            // blues
            colors.Add(MakeColor(Off, Off, High));
            colors.Add(MakeColor(Low, Low, High));
            colors.Add(MakeColor(Low, Off, High));
            colors.Add(MakeColor(Low, Off, Low));

            AllColors = colors;
        }

        private static NotifierColor MakeColor(byte r, byte g, byte b)
        {
            return Color.FromArgb(r, g, b).ToNotifierColor();
        }

        public static NotifierColor ToNotifierColor(this Color color)
        {
            NotifierColor notifierColor = new NotifierColor()
            {
                Red = GetBucketValue(color.R),
                Green = GetBucketValue(color.G),
                Blue = GetBucketValue(color.B),
            };

            return notifierColor;
        }

        private static byte GetBucketValue(byte b)
        {
            if (b <= 0)
                return 0;
            if (b < NotifierColor.MaxColorValue)
                return b;

            return NotifierColor.MaxColorValue;
        }
    }
}
