using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Drawing;
using Blinken.Extensions;

namespace Blinken
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
                Console.WriteLine("Woot");

                device.OpenDevice();

                byte[] init1 = { 0x00, 0x1f, 0x02, 0x00, 0x2e, 0x00, 0x00, 0x2b, 0x03 };
                byte[] init2 = { 0x00, 0x00, 0x02, 0x00, 0x2e, 0x00, 0x00, 0x2b, 0x04 };
                // not required?
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

        public void DoFade(Color start)
        {
            
        }

        public void DoRainbow()
        {
            //byte[] red = Color.Red.ToNotifierColor();
            //byte[] orange = Color.Orange.ToNotifierColor();
            //byte[] yellow = Color.Yellow.ToNotifierColor();
            //byte[] green = Color.Green.ToNotifierColor();
            //byte[] blue = Color.Blue.ToNotifierColor();
            //byte[] indigo = Color.Indigo.ToNotifierColor();
            //byte[] purple = Color.Purple.ToNotifierColor();
            //byte[] white = Color.White.ToNotifierColor();
            //byte[] black = Color.Black.ToNotifierColor();

            while (true)
            {
                for (int i = 0; i < ColorEx.AllColors.Count; i++)
                {
                    WriteData(m_device, ColorEx.AllColors[i].ToUsbData());
                    Delay();
                }
            }
        }

        private void Delay()
        {
            System.Threading.Thread.Sleep(500);
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
        public LightLevel Red;
        public LightLevel Green;
        public LightLevel Blue;

        public byte [] ToUsbData()
        {
            byte red = GetLightLevelData(Red);
            byte green = GetLightLevelData(Green);
            byte blue = GetLightLevelData(Blue);
            byte[] usbData = { 0x00, red, green, blue, 0x00, 0x00, 0x00, 0x00, 0x05 };

            return usbData;
        }

        public static byte[] GetUsbData(Color color)
        {
            byte[] usbData = { 0x00, color.R, color.G, color.B, 0x00, 0x00, 0x00, 0x00, 0x05 };

            return usbData;
        }

        private byte GetLightLevelData(LightLevel lightLevel)
        {
            switch (lightLevel)
            {
                case LightLevel.Off:
                    return 0;
                case LightLevel.Low:
                    return 32;
                case LightLevel.Mid:
                    return 64;
                case LightLevel.High:
                    return 196;

                default:
                    return 0;
            }
        }
    }

    public enum LightLevel { Off, Low, Mid, High };
}
