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

            while (true)
            {
                foreach (var t in m_allCounts)
                {
                    RenderText(t.Item1);
                    var image = GetImage(null, t.Item2.ToList());
                    RenderImage(image);
                }
            }

            Console.ReadLine();
        }

        static void m_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_allCounts = GetTfsUserCommitCounts();
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
                foreach (var username in new string[] { @"MEI\ptrinh", @"MEI\dkoleszar", @"MEI\pdewis", @"MEI\rjanzen", @"MEI\tnguyen", @"MEI\kimam", @"MEI\jpeoples", @"MEI\kmiller" })
                {
                    var dailyChangeCounts = GetDailyChangeCounts(versionControlServer, username);
                    allCounts.Add(Tuple.Create(username, dailyChangeCounts.ToList()));
                }
            }

            return allCounts;
        }

        private static bool [] GetImage(string username, IEnumerable<int> counts)
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
            try
            {
                const string uriText = "net.pipe://localhost/ledsign/sign";

                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

                EndpointAddress endpointAddress = new EndpointAddress(uriText);
                SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress);
                client.SetText(text);
                client.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("She broke. :(");
            }
        }

        private static void RenderImage(bool[] image)
        {
            try
            {
                const string uriText = "net.pipe://localhost/ledsign/sign";

                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

                EndpointAddress endpointAddress = new EndpointAddress(uriText);
                SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress);
                client.SetImage(image);
                client.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("She broke. :(");
            }
        }

        private static IEnumerable<int> GetDailyChangeCounts(VersionControlServer versionControlServer, string username)
        {
            var counts = Enumerable.Range(0, 21)
                .Select(i => ChangeCountForDay(i, username, versionControlServer))
                .ToArray();

            Console.WriteLine(username);
            foreach (var c in counts)
                Console.WriteLine(c);

            // map to 0 - 7 height
            int min = counts.Min();
            int max = counts.Max();

            if (min > 0)
                min = 0;
            if (max < 7)
                max = 7;

            int[] mapped = new int[counts.Length];
            for (int i = 0; i < counts.Length; i++)
            {
                mapped[i] = counts[i];//(int)Math.Round(Map(counts[i], min, max, 0, 7));
            }

            return mapped;
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

        private static int ChangeCountForDay(int daysAgo, string username, VersionControlServer versionControlServer)
        {
            var now = DateTime.Now.Date;

            var startInterval = now - TimeSpan.FromDays(daysAgo);
            var endInterval = now - TimeSpan.FromDays(daysAgo - 1);

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
