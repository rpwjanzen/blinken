using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MorseCodeGenerator.MailNotifierService;

namespace MorseCodeGenerator
{
    class Program
    {
        static readonly TimeSpan m_dotDuration = TimeSpan.FromMilliseconds(250);

        static void Main(string[] args)
        {
            try
            {
                const string uriText = "net.pipe://localhost/mailnotifier/sign";

                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

                EndpointAddress endpointAddress = new EndpointAddress(uriText);

                MailNotifierServiceClient client = new MailNotifierServiceClient(binding, endpointAddress);
                string text = "the quick brown fox jumped over the lazy dogs. ";
                Console.WriteLine(text);
                //client.SetColor(255, 0, 0);
                for (int i = 0; i < 5000; i++)
                {
                    var actions = ToBlinkAction(text).ToList();
                    foreach (var a in actions)
                        a(client);

                    client.SetColor(0, 0, 0);
                }

                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("She broke. :(" + e);
                Console.ReadLine();
            }
        }

        static IEnumerable<Action<MailNotifierServiceClient>> ToBlinkAction(string text)
        {
            text = text.ToLower();
            string[] words = text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (i != 0)
                    yield return DisplayWordSpace;

                string word = words[i];
                List<char> characters = word
                    .Where(c => m_morseCode.ContainsKey(c))
                    .ToList();

                foreach (var a in ToBlinkAction(characters))
                    yield return a;
            }
        }

        static IEnumerable<Action<MailNotifierServiceClient>> ToBlinkAction(List<char> word)
        {
            for (int i = 0; i < word.Count; i++)
            {
                char c = word[i];
                if (i != 0)
                    yield return DisplayLetterSpace;
                
                foreach(var a in ToBlinkAction(c))
                    yield return a;
            }
        }

        static IEnumerable<Action<MailNotifierServiceClient>> ToBlinkAction(char c)
        {
            string text = m_morseCode[c];
            for (int i = 0; i < text.Length; i++ )
            {
                char t = text[i];
                if (i != 0)
                    yield return DisplayLetterPause;

                if (t == '-')
                {
                    yield return DisplayDash;
                }
                else if (t == '.')
                {
                    yield return DisplayDot;
                }
                else
                {
                    throw new InvalidOperationException();
                }

            }
        }

        

        static void DisplayDot(MailNotifierServiceClient client)
        {
            client.SetColor(255, 0, 0);
            System.Threading.Thread.Sleep(m_dotDuration);
        }

        static void DisplayDash(MailNotifierServiceClient client)
        {
            // a dash is equal to 2 dots
            client.SetColor(255, 0, 0);
            System.Threading.Thread.Sleep((int)(m_dotDuration.TotalMilliseconds * 3));
        }

        static void DisplayLetterPause(MailNotifierServiceClient client)
        {
            client.SetColor(0, 0, 0);
            System.Threading.Thread.Sleep(m_dotDuration);
        }

        static void DisplayLetterSpace(MailNotifierServiceClient client)
        {
            client.SetColor(0, 0, 0);
            System.Threading.Thread.Sleep((int)(m_dotDuration.TotalMilliseconds * 3));
        }

        static void DisplayWordSpace(MailNotifierServiceClient client)
        {
            client.SetColor(0, 0, 0);
            System.Threading.Thread.Sleep((int)(m_dotDuration.TotalMilliseconds * 7));
        }

        static readonly Dictionary<char, string> m_morseCode = new Dictionary<char, string>
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
