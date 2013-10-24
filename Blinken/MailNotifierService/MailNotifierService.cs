using System;
using System.Collections.Generic;
using System.Drawing;
using System.ServiceModel;
using System.Threading;
using MailNotifierLib;

namespace MailNotifierService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public sealed class NotifierService : IMailNotifierService
    {
        private readonly MailNotifier m_mailNotifier;

        public NotifierService()
        {
            m_mailNotifier = new MailNotifier();
            m_mailNotifier.Color = Color.White;
        }

        #region IMailNotifierService Members

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public void SetColorRgb(byte red, byte green, byte blue)
        {
            m_mailNotifier.Color = Color.FromArgb(red, green, blue);
        }

        public void FadeToRgb(byte red, byte green, byte blue)
        {
            var currentColor = m_mailNotifier.Color;

            int maxR = Math.Min(red, NotifierColor.MaxColorValue);
            int maxG = Math.Min(green, NotifierColor.MaxColorValue);
            int maxB = Math.Min(blue, NotifierColor.MaxColorValue);

            while (true)
            {
                for (int i = 0; i < NotifierColor.MaxColorValue; i++)
                {
                    byte mappedRed = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                    byte mappedGreen = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                    byte mappedBlue = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);

                    m_mailNotifier.Color = Color.FromArgb(mappedRed, mappedGreen, mappedBlue);

                    System.Threading.Thread.Sleep(25);
                }

                System.Threading.Thread.Sleep(100);

                for (int i = NotifierColor.MaxColorValue; i >= 0; i--)
                {
                    byte mappedRed = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                    byte mappedGreen = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                    byte mappedBlue = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);

                    m_mailNotifier.Color = Color.FromArgb(mappedRed, mappedGreen, mappedBlue);

                    System.Threading.Thread.Sleep(25);
                }
            }
        }

        public void FadeToMultiRgb(byte[] colorBytes)
        {
            Color[] baseColors = new Color[colorBytes.Length / 3];
            for (int i = 0; i < baseColors.Length; i++)
            {
                int red = colorBytes[i * 3];
                int green = colorBytes[(i * 3) + 1];
                int blue = colorBytes[(i * 3) + 2];

                int maxR = Math.Min(red, NotifierColor.MaxColorValue);
                int maxG = Math.Min(green, NotifierColor.MaxColorValue);
                int maxB = Math.Min(blue, NotifierColor.MaxColorValue);

                baseColors[i] = Color.FromArgb(maxR, maxG, maxB);
            }

            while (true)
            {
                for (int i = 0; i < baseColors.Length; i++)
                {
                    //int maxR = baseColors[i].R;
                    //int maxG = baseColors[i].G;
                    //int maxB = baseColors[i].B;

                    //FadeInRgb(maxR, maxG, maxB);
                    //System.Threading.Thread.Sleep(100);
                    //FadeOutRgb(maxR, maxG, maxB);

                    var start = Color.FromArgb(baseColors[i].R, baseColors[i].G, baseColors[i].B);
                    var end = Color.FromArgb(baseColors[(i + 1) % baseColors.Length].R, baseColors[(i + 1) % baseColors.Length].G, baseColors[(i + 1) % baseColors.Length].B);

                    FadeFromTo(start, end);
                }
            }
        }

        private void FadeInRgb(int maxR, int maxG, int maxB)
        {
            for (int i = 0; i < NotifierColor.MaxColorValue; i++)
            {
                byte r = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                byte g = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                byte b = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);

                m_mailNotifier.Color = Color.FromArgb(r, g, b);

                System.Threading.Thread.Sleep(25);
            }
        }

        private void FadeOutRgb(int maxR, int maxG, int maxB)
        {
            for (int i = NotifierColor.MaxColorValue; i >= 0; i--)
            {
                byte r = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                byte g = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                byte b = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);

                m_mailNotifier.Color = Color.FromArgb(r, g, b);

                System.Threading.Thread.Sleep(25);
            }
        }

        private void FadeFromTo(Color start, Color end)
        {
            var startHsl = ColorUtil.RgbToHsl(start.R, start.G, start.B);
            var endHsl = ColorUtil.RgbToHsl(end.R, end.G, end.B);

            var hueRange = endHsl[0] - startHsl[0];
            var saturationRange = endHsl[1] - startHsl[1];
            var lightnessRange = endHsl[2] - startHsl[2];

            int steps = 64;
            // build table?
            for (int step = 0; step < steps; step++)
            {
                var hue = Map(step, 0, steps, startHsl[0], endHsl[0]);
                var saturation = Map(step, 0, steps, startHsl[1], endHsl[1]);
                var lightness = Map(step, 0, steps, startHsl[2], endHsl[2]);

                var colorBytes = ColorUtil.HslToRgb(hue, saturation, lightness);

                m_mailNotifier.Color = Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2]);

                System.Threading.Thread.Sleep(25);
            }
        }

        /// <param name="value">[0.0 - 1.0]</param>
        private double Lerp(double value, double min, double max)
        {
            double diff = max - min;
            double scaledValue = diff * value + min;
            return scaledValue;
        }

        /// <param name="value">[sourceMin - sourceMax]</param>
        private double Map(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            double sourceProgress = (value - sourceMin) / (sourceMax - sourceMin);
            double targetProgress = Lerp(sourceProgress, targetMin, targetMax);
            return targetProgress;
        }

        #endregion
    }
}
