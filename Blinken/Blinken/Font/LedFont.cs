using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Blinken.Font
{
    public class LedFont
    {
        public string Name;

        private readonly Dictionary<char, Letter> m_letters = new Dictionary<char, Letter>();

        public Letter this[char c]
        {
            get
            {
                Letter letter;
                if (m_letters.TryGetValue(c, out letter))
                    return letter;

                return Letter.Empty;
            }
            set
            {
                m_letters[c] = value;
            }
        }

        public static LedFont LoadFromFile(string path)
        {
            var lines = File.ReadAllLines(path);

            LedFont font = new LedFont()
            {
                Name = Path.GetFileName(path),
            };

            char ch = ' ';
            List<string> letterLines = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith(":"))
                {
                    MakeLetter(font, ch, letterLines);

                    // start new letter
                    ch = line[1];
                    letterLines = new List<string>();
                }
                else
                {
                    letterLines.Add(line);
                }
            }

            MakeLetter(font, ch, letterLines);
            return font;
        }

        private static void MakeLetter(LedFont font, char ch, List<string> letterLines)
        {
            // save previous letter
            int width = letterLines.Any() ? letterLines.Max(l => l.Length) : 0;
            // ignore seperator line
            int height = Math.Max(0, letterLines.Count - 1);

            byte[,] data = new byte[width, height];
            // ignore seperator line
            for (int r = 0; r < Math.Max(0, letterLines.Count - 1); r++)
            {
                var cl = letterLines[r];
                for (int c = 0; c < cl.Length; c++)
                {
                    data[c, r] = cl[c] == 'x' ? (byte)1 : (byte)0;
                }
            }

            if (font.m_letters.ContainsKey(ch))
                Console.WriteLine("Replacing '{0}'", ch);
            font[ch] = new Letter(data);
        }
    }
}
