using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Blinken;
using Blinken.Font;

namespace TfsConsoleApplication
{
    public sealed class Render
    {
        private static LedFont m_font = LedFont.LoadFromFile("MyTiny.txt");

        public static void AppendText(string text, VirtualLcdScreen virtualLcdScreen)
        {
            List<Letter> characters = text
                .Select(c => m_font[c])
                .ToList();

            int totalTextCharacterWidth = characters
                .Aggregate(0, (acc, l) => l.Data.GetLength(0) + 1 + acc);

            // min width of 21?
            int totalTextWidth = Math.Max(21, totalTextCharacterWidth);
            int originalWidth = virtualLcdScreen.Width;
            virtualLcdScreen.Width += totalTextWidth;

            var line = new bool[totalTextWidth];
            for (int i = 0; i < line.GetLength(0); i++)
            {
                line[i] = i % 5 != 0;
            }

            // blit each character to the virtual lcd screen
            Point upperLeft = new Point(originalWidth, 0);
            for (int ci = 0; ci < characters.Count; ci++)
            {
                var c = characters[ci];
                virtualLcdScreen.Blit(c.Data, upperLeft);
                upperLeft.X = upperLeft.X +
                    // character width
                    c.Data.GetLength(0)
                    // 1 LED spacer between characters
                    + 1;
            }
        }

        public static void AppendImage(bool[,] imageData, VirtualLcdScreen virtualLcdScreen)
        {
            int originalWidth = virtualLcdScreen.Width;
            virtualLcdScreen.Width += imageData.GetLength(0);
            virtualLcdScreen.Blit(imageData, new Point(originalWidth, 0));
        }

        public static bool[,] GetGraph(IEnumerable<int> counts)
        {
            int width = 21;
            int height = 7;
            // LCD is 21x7
            bool[,] graph = new bool[width, height];

            for (int c = 0; c < width; c++)
            {
                var count = counts.ElementAt(c);
                for (int r = 0; r < height; r++)
                {
                    bool isOn = count > r;
                    // render bottom to top
                    var height2 = ((height - 1) - r);
                    graph[c, height2] = isOn;
                }
            }

            return graph;
        }

        public static bool[] SerializeImage(bool[,] image)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);

            bool[] serializeImage = new bool[width * height];
            for (int r = 0; r < image.GetLength(1); r++)
            {
                for (int c = 0; c < image.GetLength(0); c++)
                {
                    // render bottom to top
                    int rowIndex = (6 - r);
                    serializeImage[width * rowIndex + c] = image[c, r];
                }
            }

            return serializeImage;
        }

        public static void RenderToConsole(string username, IEnumerable<int> counts)
        {
            Console.WriteLine(username);
            foreach (int count in counts)
            {
                for (int col = 0; col < count; col++)
                    Console.Write('*');
                Console.WriteLine();
            }
        }
    }
}
