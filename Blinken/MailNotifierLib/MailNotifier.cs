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

        public void SetColor(Color color)
        {
            byte[] usbData = color.ToNotifierColor().ToUsbData();
            WriteData(m_device, usbData);
        }

        public void DoFadeTo(Color color)
        {
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;

            r /= 4;
            g /= 4;
            b /= 4;

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

    public class NotifierColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public byte [] ToUsbData()
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
