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
        private Thread m_thread;
        private readonly object m_threadLock = new object();

        private bool m_stopRequested;

        public NotifierService()
        {
            m_mailNotifier = new MailNotifier();
            m_mailNotifier.SetColor(Color.White);

            m_thread = new Thread(new ThreadStart(() => { }));
            m_thread.Start();
        }

        public void Stop()
        {
            lock (m_threadLock)
            {
                m_stopRequested = true;
            }
        }

        #region IMailNotifierService Members

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public void SetColor(byte red, byte green, byte blue)
        {
            lock (m_threadLock)
            {
                m_stopRequested = true;
                m_thread.Join();

                lock(m_mailNotifier)
                    m_mailNotifier.SetColor(Color.FromArgb(red, green, blue));
            }
        }

        public void DoFadeTo(byte red, byte green, byte blue)
        {
            lock (m_threadLock)
            {
                m_stopRequested = true;
                m_thread.Join();
                m_stopRequested = false;

                m_thread = new Thread(new ThreadStart(() =>
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    
                    int maxR = Math.Min(red, NotifierColor.MaxColorValue);
                    int maxG = Math.Min(green, NotifierColor.MaxColorValue);
                    int maxB = Math.Min(blue, NotifierColor.MaxColorValue);

                    while (!m_stopRequested)
                    {
                        for (int i = 0; i < NotifierColor.MaxColorValue; i ++)
                        {
                            r = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                            g = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                            b = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);

                            lock (m_mailNotifier)
                                m_mailNotifier.SetColor(Color.FromArgb(r,g,b));

                            System.Threading.Thread.Sleep(25);
                        }

                        System.Threading.Thread.Sleep(100);
                        
                        for (int i = NotifierColor.MaxColorValue; i >= 0; i--)
                        {
                            r = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                            g = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                            b = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);
                            
                            lock (m_mailNotifier)
                                m_mailNotifier.SetColor(Color.FromArgb(r,g,b));

                            System.Threading.Thread.Sleep(25);
                        }
                    }
                }));
                m_thread.IsBackground = true;
                m_thread.Start();
            }
        }

        public void DoFadeToMulti(byte[] colorBytes)
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

            lock (m_threadLock)
            {
                m_stopRequested = true;
                m_thread.Join();
                m_stopRequested = false;

                m_thread = new Thread(new ThreadStart(() =>
                {
                    while (!m_stopRequested)
                    {
                        for (int i = 0; i < baseColors.Length; i++)
                        {
                            int maxR = baseColors[i].R;
                            int maxG = baseColors[i].G;
                            int maxB = baseColors[i].B;

                            FadeIn(maxR, maxG, maxB);
                            System.Threading.Thread.Sleep(100);
                            FadeOut(maxR, maxG, maxB);
                        }
                    }
                }));
                m_thread.IsBackground = true;
                m_thread.Start();
            }
        }

        private void FadeIn(int maxR, int maxG, int maxB)
        {
            for (int i = 0; i < NotifierColor.MaxColorValue; i++)
            {
                byte r = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                byte g = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                byte b = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);

                lock (m_mailNotifier)
                    m_mailNotifier.SetColor(Color.FromArgb(r, g, b));

                System.Threading.Thread.Sleep(25);
            }
        }

        private void FadeOut(int maxR, int maxG, int maxB)
        {
            for (int i = NotifierColor.MaxColorValue; i >= 0; i--)
            {
                byte r = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxR);
                byte g = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxG);
                byte b = (byte)Map(i, 0, NotifierColor.MaxColorValue, 0, maxB);

                lock (m_mailNotifier)
                    m_mailNotifier.SetColor(Color.FromArgb(r, g, b));

                System.Threading.Thread.Sleep(25);
            }
        }

        /// <param name="value">[0.0 - 1.0]</param>
        private double Lerp(double value, double min, double max)
        {
            double diff = max - min;
            double scaledValue =  diff * value + min;
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
