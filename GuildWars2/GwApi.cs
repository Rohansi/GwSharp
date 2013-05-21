using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace GuildWars2
{
    internal static class GwApi
    {
        private static readonly Dictionary<string, string> RequestUris = new Dictionary<string, string>()
        {
            // PvE
            { "WorldNames", "https://api.guildwars2.com/v1/world_names.json" },
            { "MapNames", "https://api.guildwars2.com/v1/map_names.json" },
            { "EventNames", "https://api.guildwars2.com/v1/event_names.json" },
            { "Events", "https://api.guildwars2.com/v1/events.json?world_id={0}" },

            // WvW
            { "ObjectiveNames", "https://api.guildwars2.com/v1/wvw/objective_names.json" },
            { "Matches", "https://api.guildwars2.com/v1/wvw/matches.json" },
            { "MatchDetails", "https://api.guildwars2.com/v1/wvw/match_details.json?match_id={0}" }
        };

        public static dynamic Request(string request, string id = "")
        {
            var uri = RequestUris[request];
            var json = ReadPage(string.Format(uri, id));
            return JsonConvert.DeserializeObject(json);
        }

        private static string ReadPage(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.KeepAlive = true;
            request.Timeout = 5000;

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }
    }
}
