using System.Drawing;
using System.Collections.Generic;

namespace Blinken.Extensions
{
    public static class ColorEx
    {
        public static List<NotifierColor> AllColors = new List<NotifierColor>();

        static ColorEx()
        {
            List<NotifierColor> colors = new List<NotifierColor>();
            for (int r = 0; r < 96; r += 32)
            {
                for (int g = 0; g < 96; g += 32)
                {
                    for (int b = 0; b < 96; b += 32)
                    {
                        colors.Add(Color.FromArgb(r,g,b).ToNotifierColor());
                    }
                }
            }

            AllColors = colors;
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

        private static LightLevel GetBucketValue(byte b)
        {
            if (b <= 0)
                return LightLevel.Off;
            if (b <= 32)
                return LightLevel.Low;
            if (b <= 64)
                return LightLevel.Mid;
            
            return LightLevel.High;
        }
    }
}
