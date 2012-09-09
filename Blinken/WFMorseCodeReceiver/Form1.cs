using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video;
using AForge.Imaging.Filters;
using AForge;
using AForge.Math.Geometry;
using AForge.Imaging;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WFMorseCodeReceiver
{
    public partial class Form1 : Form
    {
        VideoCaptureDevice m_videoSource;
        const int frames_per_second = 15;
        const int frameSkipCount = 9;
        bool isTesting = false;

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            DoCamera();
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_videoSource.SignalToStop();
            m_videoSource.WaitForStop();
        }

        void DoCamera()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            string videoDeviceMonkierText = videoDevices[0].MonikerString;

            m_videoSource = new VideoCaptureDevice(videoDeviceMonkierText);
            m_videoSource.DesiredFrameRate = frames_per_second;

            m_videoSource.Start();
            m_videoSource.NewFrame += new AForge.Video.NewFrameEventHandler(videoSource_NewFrame);
        }

        string rawtext = string.Empty;
        string decodedMessage = string.Empty;
        int previousDecodedMessageLength = 0;

        void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var frame = eventArgs.Frame;

            if (isTesting)
            {
                var r = new ColorFiltering(new IntRange(250, 255), new IntRange(250, 255), new IntRange(250, 255));
                var r1 = r.Apply(frame);

                ImageStatistics is0 = new ImageStatistics(r1);
                int no = is0.PixelsCountWithoutBlack;
                Console.WriteLine(no);

                m_pictureBox.Image = r1;
                return;
            }

            var cf1 = new ColorFiltering(new IntRange(250, 255), new IntRange(250, 255), new IntRange(250, 255));
            var fi1 = cf1.Apply(frame);

            ImageStatistics imageStatistics = new ImageStatistics(fi1);
            int numOn = imageStatistics.PixelsCountWithoutBlack;
            if (numOn > 10)
            {
                rawtext += "O";
            }
            else
            {
                rawtext += "-";
            }

            var ar = GetLetters(GuessMorseCode(rawtext)).ToArray();
            if (ar.Length != 0)
            {
                decodedMessage += ar[0];
                if (previousDecodedMessageLength != decodedMessage.Length)
                {
                    foreach (var c in decodedMessage)
                    {
                        Console.Write(c);
                    }
                    Console.WriteLine();
                    previousDecodedMessageLength = decodedMessage.Length;
                }
                rawtext = string.Empty;
            }
        }

        private IEnumerable<MorseCodeType> GuessMorseCode(IEnumerable<char> morseCodeInput)
        {
            foreach (var s in GetLightStates(morseCodeInput))
            {
                int codeLength = s.Item2;
                if (codeLength == 0)
                    continue;

                if (s.Item1 == LightState.On)
                {
                    if (codeLength < frames_per_second - frameSkipCount)
                    {
                        Console.WriteLine("Skipped too short ON  sequence: " + codeLength);
                        continue;
                    }

                    MorseCodeType morseCodeType;
                    if (codeLength > (frames_per_second - frameSkipCount) * 3)
                        morseCodeType = MorseCodeType.Dah;
                    else
                        morseCodeType = MorseCodeType.Dit;

                    yield return morseCodeType;
                }
                else
                {
                    if (codeLength < frames_per_second - frameSkipCount)
                    {
                        Console.WriteLine("Skipped too short OFF sequence: " + codeLength);
                        continue;
                    }

                    MorseCodeType morseCodeType;
                    if (codeLength > (frames_per_second - frameSkipCount) * 7)
                        morseCodeType = MorseCodeType.WordSpace;
                    else if (codeLength > (frames_per_second - frameSkipCount) * 3)
                        morseCodeType = MorseCodeType.TwoLetterSpace;
                    else
                        morseCodeType = MorseCodeType.SameLetterSpace;

                    yield return morseCodeType;
                }
            }
        }

        private IEnumerable<char> GetLetters(IEnumerable<MorseCodeType> morseCode)
        {
            List<MorseCodeType> letterParts = new List<MorseCodeType>();
            foreach (var morseCodeType in morseCode)
            {
                switch (morseCodeType)
                {
                    case MorseCodeType.Dit:
                    case MorseCodeType.Dah:
                        letterParts.Add(morseCodeType);
                        break;

                    case MorseCodeType.SameLetterSpace:
                        continue;

                    case MorseCodeType.TwoLetterSpace:
                        yield return GetSpecificLetter(letterParts);
                        letterParts.Clear();
                        break;

                    case MorseCodeType.WordSpace:
                        yield return GetSpecificLetter(letterParts);
                        letterParts.Clear();

                        yield return ' ';
                        break;
                }
            }
        }

        private char GetSpecificLetter(List<MorseCodeType> letterParts)
        {
            string text = new string(letterParts.Select(m => m == MorseCodeType.Dit ? '.' : '-').ToArray());
            var kvp0 = m_morseCode.FirstOrDefault(kvp => kvp.Value == text);
            if (kvp0.Equals(default(KeyValuePair<char, string>)))
            {
                Console.WriteLine("Could not recognize " + text);
                return '#';
            }
            else
                return char.ToUpper(kvp0.Key);
        }

        private IEnumerable<Tuple<LightState, int>> GetLightStates(IEnumerable<char> input)
        {
            int sequenceLength = 0;

            bool isOn = false;
            foreach (var c in input)
            {
                if (c == '-')
                {
                    if (isOn)
                    {
                        isOn = false;
                        yield return Tuple.Create(LightState.On, sequenceLength);
                        sequenceLength = 0;
                    }
                    else
                        sequenceLength++;
                }
                else if (c == 'O')
                {
                    if (!isOn)
                    {
                        isOn = true;
                        yield return Tuple.Create(LightState.Off, sequenceLength);
                        sequenceLength = 0;
                    }
                    else
                    {
                        sequenceLength++;
                    }
                }
                else
                    throw new InvalidOperationException();
            }
        }

        enum LightState { Off, On };
        enum MorseCodeType { Dit, Dah, SameLetterSpace, TwoLetterSpace, WordSpace };

        readonly Dictionary<char, string> m_morseCode = new Dictionary<char, string>
        {
            { 'a', ".-" },
            { 'b', "-..." },
            { 'c', "-.-." },
            { 'd', "-.." },
            { 'e', "." },
            { 'f', "..-." },
            { 'g', "--." },
            { 'h', "...." },
            { 'i', ".." },
            { 'j', ".---" },
            { 'k', "-.-" },
            { 'l', ".-.." },
            { 'm', "--" },
            { 'n', "-." },
            { 'o', "---" },
            { 'p', ".--." },
            { 'q', "--.-" },
            { 'r', ".-." },
            { 's', "..." },
            { 't', "-" },
            { 'u', "..-" },
            { 'v', "...-" },
            { 'w', ".--" },
            { 'x', "-..-" },
            { 'y', "-.--" },
            { 'z', "--.." },
            { '1', ".----" },
            { '2', "..---" },
            { '3', "...--" },
            { '4', "....-" },
            { '5', "....." },
            { '6', "-...." },
            { '7', "--..." },
            { '8', "---.." },
            { '9', "----." },
            { '0', "-----" },
            { '.', ".-.-.-" },
            { ',', "..---" },
            { '?', "..--.." },
            { '\'', ".----." },
            { '/', "-..-." },
            { '(', "-.--." },
            { ')', "-.--.-" },
            { '&', ".-..." },
            { ':', "---..." },
            { ';', "-.-.-." },
            { '=', "-...-" },
            { '+', ".-.-." },
            { '-', "-....-" },
            { '_', "..--.-" },
            { '"', ".-..-." },
            { '$', "...-..-" },
            { '@', ".--.-." },
        };
    }
}
