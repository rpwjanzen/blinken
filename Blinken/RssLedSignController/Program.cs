using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Argotic.Syndication;
using Blinken;
using Blinken.Font;

namespace RssLedSignController
{
    class Program
    {
        private static RssFeed m_forecastFeed;
        private static RssFeed m_warningFeed;

        private static List<string> m_forecastFeedLines = new List<string>();
        private static List<string> m_warningFeedLines = new List<string>();
        private static List<string> m_allFeedLines = new List<string>();

        static void Main(string[] args)
        {
            var lcdNotifier = new LcdNotifier();
            LedFont somyFont = LedFont.LoadFromFile(@"Font\somybmp04_7.txt");

            m_forecastFeed = new RssFeed();
            m_forecastFeed.Loaded += forecastFeed_Loaded;
            m_forecastFeed.LoadAsync(new Uri("http://www.weatheroffice.gc.ca/rss/city/ab-52_e.xml"), null);

            //m_warningFeed = new RssFeed();
            //m_warningFeed.Loaded += warningFeed_Loaded;
            //m_warningFeed.LoadAsync(new Uri("http://www.weatheroffice.gc.ca/rss/warning/ab-52_e.xml"), null);


            while (true)
            {
                lock (m_allFeedLines)
                {
                    for (int i = 0; i < m_allFeedLines.Count; i++)
                    {
                        lcdNotifier.Text = m_allFeedLines[i].ToUpper();
                        lcdNotifier.DrawText(somyFont);
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

        private static void warningFeed_Loaded(object sender, Argotic.Common.SyndicationResourceLoadedEventArgs e)
        {
            foreach (var rssItem in m_forecastFeed.Channel.Items)
            {
                string[] lines = rssItem.Description.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    line = RemoveHtml(line);

                    lines[i] = line.Trim();
                }

                lock (m_warningFeedLines)
                {
                    m_warningFeedLines.Clear();
                    m_warningFeedLines.AddRange(lines);
                }

                UpdateAllFeedLines();
            }
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
                    lock (m_warningFeedLines)
                    {
                        m_allFeedLines.Clear();

                        m_allFeedLines.Add("Calgary Weather Forecast...");
                        m_allFeedLines.AddRange(m_forecastFeedLines);

                        //m_allFeedLines.Add("Calgary Weather Warnings...");
                        //m_allFeedLines.AddRange(m_warningFeedLines);
                    }
                }
            }
        }
    }
}
