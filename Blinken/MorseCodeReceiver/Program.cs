using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video.DirectShow;
using AForge.Video;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge;

namespace MorseCodeReceiver
{
    class Program
    {
        enum LightState { Off, On };
        enum MorseCodeType { Dit, Dah, SameLetterSpace, TwoLetterSpace, WordSpace };
        readonly static Dictionary<char, string> m_morseCode = new Dictionary<char, string>
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


        static VideoCaptureDevice m_videoSource;
        const int m_framesPerSecond = 30;
        const int m_framesPerDit = 5;
        const int m_maxDroppedFramesCount = 1;
        static bool m_isTesting = false;

        static string m_rawtext = string.Empty;
        static string m_decodedMessage = string.Empty;
        static int m_previousDecodedMessageLength = 0;

        static void Main(string[] args)
        {
            DoCamera();
            Console.ReadLine();

            m_videoSource.SignalToStop();
            m_videoSource.WaitForStop();
        }

        static void DoCamera()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            string videoDeviceMonkierText = videoDevices[0].MonikerString;

            m_videoSource = new VideoCaptureDevice(videoDeviceMonkierText);
            m_videoSource.DesiredFrameRate = m_framesPerSecond;

            m_videoSource.Start();
            m_videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
        }

        static void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var frame = eventArgs.Frame;

            if (m_isTesting)
            {
                var r = new ColorFiltering(new IntRange(250, 255), new IntRange(250, 255), new IntRange(250, 255));
                var r1 = r.Apply(frame);

                ImageStatistics is0 = new ImageStatistics(r1);
                int no = is0.PixelsCountWithoutBlack;
                Console.WriteLine(no);

                return;
            }

            var cf1 = new ColorFiltering(new IntRange(250, 255), new IntRange(250, 255), new IntRange(250, 255));
            var fi1 = cf1.Apply(frame);

            ImageStatistics imageStatistics = new ImageStatistics(fi1);
            int numOn = imageStatistics.PixelsCountWithoutBlack;
            if (numOn > 15)
            {
                m_rawtext += "O";
            }
            else
            {
                m_rawtext += "-";
            }

            Console.Clear();
            Console.SetCursorPosition(0, 1);
            Console.Write(m_rawtext);

            var ar = GetLetters(GuessMorseCode(m_rawtext)).ToArray();
            if (ar.Length != 0)
            {
                foreach(var v in ar)
                    m_decodedMessage += v;
                m_rawtext = string.Empty;
            }

            string message = "'";
            foreach (var c in m_decodedMessage)
            {
                message += c;
            }
            message += "'";

            Console.SetCursorPosition(0, 4);
            Console.Write(message);
        }

        private static IEnumerable<MorseCodeType> GuessMorseCode(IEnumerable<char> morseCodeInput)
        {
            foreach (var s in GetLightStates(morseCodeInput))
            {
                int codeLength = s.Item2;
                if (codeLength == 0)
                    continue;

                MorseCodeType morseCodeType;
                if (s.Item1 == LightState.On)
                {
                    if (codeLength < m_framesPerDit - m_maxDroppedFramesCount)
                    {
                        //Console.WriteLine("Ignored short ON  sequence: " + codeLength);
                        continue;
                    }

                    if (codeLength > (m_framesPerDit - m_maxDroppedFramesCount) * 3)
                        morseCodeType = MorseCodeType.Dah;
                    else
                        morseCodeType = MorseCodeType.Dit;
                }
                else
                {
                    if (codeLength < m_framesPerDit - m_maxDroppedFramesCount)
                    {
                        //Console.WriteLine("Ignored short Off sequence: " + codeLength);
                        continue;
                    }

                    if (codeLength > (m_framesPerDit - m_maxDroppedFramesCount) * 7)
                        morseCodeType = MorseCodeType.WordSpace;
                    else if (codeLength > (m_framesPerDit - m_maxDroppedFramesCount) * 3)
                        morseCodeType = MorseCodeType.TwoLetterSpace;
                    else
                        morseCodeType = MorseCodeType.SameLetterSpace;
                }

                //Console.WriteLine("Decoded " + morseCodeType);
                yield return morseCodeType;
            }
        }

        private static IEnumerable<char> GetLetters(IEnumerable<MorseCodeType> morseCode)
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
                        if (letterParts.Any())
                            yield return GetSpecificLetter(letterParts);

                        letterParts.Clear();
                        break;

                    case MorseCodeType.WordSpace:
                        if (letterParts.Any())
                            yield return GetSpecificLetter(letterParts);

                        letterParts.Clear();

                        yield return ' ';
                        break;
                }
            }
        }

        private static char GetSpecificLetter(List<MorseCodeType> letterParts)
        {
            string text = new string(letterParts.Select(m => m == MorseCodeType.Dit ? '.' : '-').ToArray());
            var kvp0 = m_morseCode.FirstOrDefault(kvp => kvp.Value == text);
            if (kvp0.Equals(default(KeyValuePair<char, string>)))
            {
                //Console.WriteLine("Could not recognize '" + text + "'");
                return '#';
            }
            else
            {
                //Console.WriteLine("Decoded: " + kvp0.Key);
                return char.ToUpper(kvp0.Key);
            }
        }

        private static IEnumerable<Tuple<LightState, int>> GetLightStates(IEnumerable<char> input)
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



    }
}
