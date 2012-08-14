using System;
using System.Collections.Generic;
using System.Linq;
using Argotic.Syndication;
using Blinken;
using Blinken.Font;

namespace RssLedSignController
{
    class Program
    {
        private static RssFeed m_forecastFeed;

        private static List<string> m_forecastFeedLines = new List<string>();
        private static List<string> m_allFeedLines = new List<string>();

        static void Main(string[] args)
        {
            var lcdNotifier = new LcdNotifier();
            LedFont font = LedFont.LoadFromFile(@"Font\Tiny.txt");

            m_forecastFeed = new RssFeed();
            m_forecastFeed.Loaded += forecastFeed_Loaded;
            m_forecastFeed.LoadAsync(new Uri("http://www.weatheroffice.gc.ca/rss/city/ab-52_e.xml"), null);

            while (true)
            {
                lock (m_allFeedLines)
                {
                    for (int i = 0; i < m_allFeedLines.Count; i++)
                    {
                        lcdNotifier.Text = m_allFeedLines[i].ToUpper();
                        lcdNotifier.ScrollText(font);
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
        }

        private static void forecastFeed_Loaded(object sender, Argotic.Common.SyndicationResourceLoadedEventArgs e)
        {
            List<string[]> allLines = new List<string[]>();
            foreach (var rssItem in m_forecastFeed.Channel.Items)
            {
                string[] itemLines = rssItem.Description.Split('\n');
                for (int i = 0; i < itemLines.Length; i++)
                {
                    string line = itemLines[i];
                    line = RemoveHtml(line);

                    itemLines[i] = line.Trim();
                }

                allLines.Add(itemLines);
            }

            lock (m_forecastFeedLines)
            {
                m_forecastFeedLines.Clear();
                m_forecastFeedLines.AddRange(allLines.SelectMany(s => s));
            }

            UpdateAllFeedLines();
        }

        private static string RemoveHtml(string line)
        {
            foreach (var s in new[] { "<b>", "</b>", "<br/>" })
            {
                line = line.Replace(s, "");
            }

            line = line.Replace("&deg;", "*");
            return line;
        }

        private static void UpdateAllFeedLines()
        {
            lock (m_allFeedLines)
            {
                lock (m_forecastFeedLines)
                {
                    m_allFeedLines.Clear();
                    m_allFeedLines.AddRange(m_forecastFeedLines);
                }
            }
        }
    }
}
