using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Argotic.Common;
using Argotic.Syndication;
using Blinken;
using Blinken.Font;

namespace RssLedSignController
{
    class Program
    {
        private static List<string> m_forecastFeedLines = new List<string>();
        private static List<string> m_allFeedLines = new List<string>();

        private static Timer m_reloadTimer;

        //private readonly static string FeedUrl = "http://rss.cbc.ca/lineup/offbeat.xml";
        //private readonly static string FeedUrl = "http://feeds.huffingtonpost.com/HP/MostPopular";
        
        // Calgary weather
        //private readonly static string FeedUrl = "http://weather.gc.ca/rss/city/ab-52_e.xml";

        // Calgary weather alerts
        private readonly static string FeedUrl = "http://weather.gc.ca/rss/warning/ab-52_e.xml";

        private static readonly bool m_includeDescription = false;

        static void Main(string[] args)
        {
            var lcdNotifier = new LcdNotifier();
            LedFont font = LedFont.LoadFromFile(@"Font\somybmp01_7.txt");
            //LedFont font = LedFont.LoadFromFile(@"Font\tinier.txt");

            m_reloadTimer = new Timer();
            m_reloadTimer.Elapsed += (s, e) =>
            {
                Console.WriteLine("Reloading RSS Feed....");
                StartLoadingFeed();
                
            };

            m_reloadTimer.Interval = TimeSpan.FromDays(1).TotalMilliseconds;
            m_reloadTimer.Start();
            Console.WriteLine("Reloading RSS Feed....({0})", FeedUrl);
            StartLoadingFeed();

            int i = 0;
            while (true)
            {
                lock (m_allFeedLines)
                {
                    if (m_allFeedLines.Any())
                    {
                        if (i >= m_allFeedLines.Count)
                        {
                            i = 0;
                        }

                        lcdNotifier.Text = m_allFeedLines[i];
                    }
                }

                lcdNotifier.ScrollText(font, TimeSpan.FromMilliseconds(40));
                i++;
            }
        }

        private static void StartLoadingFeed()
        {
            RssFeed feed = new RssFeed();
            feed.Loaded += feed_Loaded;
            feed.LoadAsync(new Uri(FeedUrl), null);
        }

        private static void feed_Loaded(object sender, SyndicationResourceLoadedEventArgs e)
        {
            var feed = (RssFeed)sender;
            feed.Loaded -= feed_Loaded;

            Console.WriteLine("RSS Feed loaded at " + DateTime.UtcNow);
            List<string[]> allLines = new List<string[]>();
            foreach (var rssItem in feed.Channel.Items)
            {
                string title = rssItem.Title;
                title = RemoveHtml(title);
                if (m_includeDescription)
                {
                    string description = rssItem.Description;
                    description = RemoveHtml(description);
                    allLines.Add(new string[] { title + ": " + description });
                }
                else
                {
                    allLines.Add(new string[] { title });
                }
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
            foreach (var s in new[] { "<b>", "</b>", "<br/>", "<p>", "</p>" })
            {
                line = line.Replace(s, "");
            }

            // strip "<img* />"
            line = Regex.Replace(line, @"<img\s[^>]*>(?:\s*?</img>)?", "", RegexOptions.CultureInvariant);

            // CBC
            line = line.Replace("*", "");

            line = line.Replace("&deg;", "*");
            line = line.Replace("&lt;", "<");
            line = line.Replace("&gt;", ">");
            line = line.Replace("&amp;", "&");

            // huffington post feed
            line = line.Replace("![CDATA[", "");
            line = line.Replace("]]", "");

            line = line.Replace("<&>", "&");
            line = line.Replace("PHOTOS", "");
            line = line.Replace("PHOTO", "");
            line = line.Replace("UPDATE", "");
            line = line.Replace("AUDIO", "");
            line = line.Replace("LIVE UPDATES", "");
            line = line.Replace("VIDEO", "");
            line = line.Replace("POLL", "");
            line = line.Replace("(/)", "");
            line = line.Replace("()", "");
            line = line.Replace("(, )", "");
            line = line.Replace("(, , )", "");

            line = line.Trim();

            return line;
        }

        private static void UpdateAllFeedLines()
        {
            List<string> linesCopy;
            lock (m_allFeedLines)
            {
                lock (m_forecastFeedLines)
                {
                    m_allFeedLines.Clear();
                    m_allFeedLines.AddRange(m_forecastFeedLines);
                    linesCopy = m_allFeedLines.ToList();
                }
            }

            Console.WriteLine("Begin updated RSS feed...");
            foreach (var s in linesCopy)
                Console.WriteLine(s);
            Console.WriteLine("End updated RSS feed.");
        }
    }
}
