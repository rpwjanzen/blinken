﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Blinken;
using Blinken.Font;

namespace TfsConsoleApplication
{
    class Program
    {
        static Timer m_timer;
        static List<Tuple<string, List<int>>> m_allCounts;

        static void Main(string[] args)
        {
            m_timer = new Timer();
            m_timer.Elapsed += (s, e) => UpdateTfsUserChangesetCounts();
            m_timer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
            m_timer.Start();

            UpdateTfsUserChangesetCounts();

            while (true)
            {
                VirtualLcdScreen virtualLcdScreen = new VirtualLcdScreen();

                var now = DateTime.Now.ToString("HH:mm");
                Render.AppendText(now, virtualLcdScreen);

                foreach (var t in m_allCounts)
                {
                    string[] parts = t.Item1.Split('\\');
                    string initials = parts[1].Substring(0, 2).ToUpperInvariant();
                    string rest = parts[1].Substring(2);
                    switch (initials)
                    {
                        case "RJ":
                            initials = "RYAN";
                            break;
                        case "KM":
                            initials = "KEVIN";
                            break;
                        case "KB":
                            initials = "KYLE";
                            break;
                        case "BB":
                            initials = "BRAD";
                            break;
                        case "JW":
                            initials = "JAMES";
                            break;
                        case "BS":
                            initials = "BRYAN";
                            break;

                        default:
                            break;
                    }
                    string renderText = " " + initials; //+rest.ToUpperInvariant();
                    Render.AppendText(renderText, virtualLcdScreen);
                    var image = Render.GetGraph(t.Item2.ToList());
                    Render.AppendImage(image, virtualLcdScreen);
                }

                bool[] serializedImageData = Render.SerializeImage(virtualLcdScreen.Data);
                LedSign.RenderImageToLedSign(serializedImageData);
            }
        }

        private static void UpdateTfsUserChangesetCounts()
        {
            m_allCounts = TfsCommits.GetTfsUserCommitCounts();
            foreach (var t in m_allCounts)
                Render.RenderToConsole(t.Item1, t.Item2);
        }
    }
}
