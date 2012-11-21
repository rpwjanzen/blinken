using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TfsConsoleApplication
{
    class Program
    {
        static Timer m_timer;
        static List<Tuple<string, List<int>>> m_allCounts;

        static void Main(string[] args)
        {
            m_timer = new Timer();
            m_timer.Elapsed += m_timer_Elapsed;
            m_timer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            m_timer.Start();

            m_allCounts = GetTfsUserCommitCounts();
            foreach (var t in m_allCounts)
                RenderToConsole(t.Item1, t.Item2);

            while (true)
            {
                foreach (var t in m_allCounts)
                {
                    string[] parts = t.Item1.Split('\\');
                    string initials = parts[1].Substring(0, 2).ToUpperInvariant();
                    string rest = parts[1].Substring(2);
                    string renderText = initials + rest;
                    RenderText(renderText);
                    var image = GetImage(t.Item2.ToList());
                    RenderImage(image);
                }
            }
        }

        static void m_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_allCounts = GetTfsUserCommitCounts();
            foreach(var t in m_allCounts)
                RenderToConsole(t.Item1, t.Item2);
        }

        private static List<Tuple<string, List<int>>> GetTfsUserCommitCounts()
        {
            List<Tuple<string, List<int>>> allCounts = new List<Tuple<string, List<int>>>();
            Uri tfsUri = new Uri("http://mentortfs:8080/tfs");

            TfsConfigurationServer configurationServer =
                TfsConfigurationServerFactory.GetConfigurationServer(tfsUri);

            // Get the catalog of team project collections
            ReadOnlyCollection<CatalogNode> collectionNodes = configurationServer.CatalogNode.QueryChildren(
                new[] { CatalogResourceTypes.ProjectCollection },
                false, CatalogQueryOptions.None);

            // List the team project collections
            foreach (CatalogNode collectionNode in collectionNodes)
            {
                // Use the InstanceId property to get the team project collection
                Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
                TfsTeamProjectCollection teamProjectCollection = configurationServer.GetTeamProjectCollection(collectionId);

                // Print the name of the team project collection
                Console.WriteLine("Collection: " + teamProjectCollection.Name);

                // Get a catalog of team projects for the collection
                ReadOnlyCollection<CatalogNode> projectNodes = collectionNode.QueryChildren(
                    new[] { CatalogResourceTypes.TeamProject },
                    false, CatalogQueryOptions.None);

                // List the team projects in the collection
                foreach (CatalogNode projectNode in projectNodes)
                {
                    Console.WriteLine(" Team Project: " + projectNode.Resource.DisplayName);
                }

                VersionControlServer versionControlServer = (VersionControlServer)teamProjectCollection.GetService(typeof(VersionControlServer));
                foreach (var username in new string[]
                {
                    @"MEI\pdewis",
                    @"MEI\ptrinh",
                    @"MEI\dkoleszar", 

                    @"MEI\kmiller",
                    @"MEI\rjanzen",
                    @"MEI\tnguyen",
                    @"MEI\kimam",
                    @"MEI\jpeoples",
                    
                    @"MEI\dsum",
                    @"MEI\espielman",
                    @"MEI\mkwan",
                    @"MEI\tmcivor",
                    @"MEI\akwok",
                })
                {
                    var dailyChangeCounts = GetDailyChangeCounts(versionControlServer, username);
                    allCounts.Add(Tuple.Create(username, dailyChangeCounts.ToList()));
                }
            }

            return allCounts;
        }

        private static bool[] GetImage(IEnumerable<int> counts)
        {
            int width = 21;
            int height = 7;
            // LCD is 21x7
            bool[,] image = new bool[width, height];

            for (int c = 0; c < width; c++)
            {
                var count = counts.ElementAt(c);
                for (int r = 0; r < height; r++)
                {
                    bool isOn = count > r;
                    image[c, r] = isOn;
                }
            }

            bool[] image2 = new bool[width * height];
            for (int r = 0; r < image.GetLength(1); r++)
            {
                for (int c = 0; c < image.GetLength(0); c++)
                {
                    image2[width * r + c] = image[c, r];
                }
            }

            return image2;
        }

        private static void RenderText(string text)
        {
            const string uriText = "net.pipe://localhost/ledsign/sign";

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            EndpointAddress endpointAddress = new EndpointAddress(uriText);
            SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress);
            client.SetText(text);
            client.Close();
        }

        private static void RenderImage(bool[] image)
        {
            const string uriText = "net.pipe://localhost/ledsign/sign";

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            EndpointAddress endpointAddress = new EndpointAddress(uriText);
            SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress);
            client.SetImage(image);
            client.Close();
        }

        private static IEnumerable<int> GetDailyChangeCounts(VersionControlServer versionControlServer, string username)
        {
            var now = DateTime.Now.Date;
            // get up to last 5 weeks of of data (so we can ignore the weekends later)
            var counts = Enumerable.Range(0, 7 * 5)
                .Select(i =>
                {
                    var startInterval = now - TimeSpan.FromDays(i);
                    var endInterval = now - TimeSpan.FromDays(i - 1);

                    return Tuple.Create(startInterval, endInterval);
                })
                // skip weekends
                .Where(t => t.Item2.DayOfWeek != DayOfWeek.Monday && t.Item2.DayOfWeek != DayOfWeek.Sunday)
                // ensure we have 21 days of data
                .Take(21)
                .Select(t => ChangeCountForDay(t.Item1, t.Item2, username, versionControlServer))
                .ToArray();

            return counts;
        }

        private static void RenderToConsole(string username, IEnumerable<int> counts)
        {
            Console.WriteLine(username);
            foreach (int count in counts)
            {
                for (int col = 0; col < count; col++)
                    Console.Write('*');
                Console.WriteLine();
            }
        }

        private static int ChangeCountForDay(DateTime startInterval, DateTime endInterval, string username, VersionControlServer versionControlServer)
        {
            //var now = DateTime.Now.Date;

            //var startInterval = now - TimeSpan.FromDays(daysAgo);
            //var endInterval = now - TimeSpan.FromDays(daysAgo - 1);

            DateVersionSpec versionFrom = new DateVersionSpec(startInterval);
            VersionSpec versionTo = new DateVersionSpec(endInterval);

            var userHistory = versionControlServer.QueryHistory(
                "$/",
                VersionSpec.Latest,
                0,
                RecursionType.Full,
                username,
                versionFrom,
                versionTo,
                Int32.MaxValue,
                false,
                false);

            var count = userHistory.OfType<Changeset>().Count();
            return count;
        }

        /// <param name="value">[0.0 - 1.0]</param>
        private static double Lerp(double value, double min, double max)
        {
            double diff = max - min;
            double scaledValue = diff * value + min;
            return scaledValue;
        }

        /// <param name="value">[sourceMin - sourceMax]</param>
        private static double Map(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            double sourceProgress = (value - sourceMin) / (sourceMax - sourceMin);
            double targetProgress = Lerp(sourceProgress, targetMin, targetMax);
            return targetProgress;
        }
    }
}
