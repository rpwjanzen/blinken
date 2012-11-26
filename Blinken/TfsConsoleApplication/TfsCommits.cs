using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TfsConsoleApplication
{
    public sealed class TfsCommits
    {
        public static List<Tuple<string, List<int>>> GetTfsUserCommitCounts()
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
                .Select(t => GetIntervalChangeSetCount(t.Item1, t.Item2, username, versionControlServer))
                .ToArray();

            return counts;
        }

        private static int GetIntervalChangeSetCount(DateTime startInterval, DateTime endInterval, string username, VersionControlServer versionControlServer)
        {
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
    }
}
