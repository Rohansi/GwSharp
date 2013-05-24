using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace GwSharp
{
    internal class Api
    {
        private static readonly Dictionary<string, string> RequestUrls = new Dictionary<string, string>
        {
            // PvE
            { "WorldNames", "world_names.json" },
            { "MapNames", "map_names.json" },
            { "EventNames", "event_names.json" },
            { "Events", "events.json" }, // world_id

            // WvW
            { "ObjectiveNames", "wvw/objective_names.json" },
            { "Matches", "wvw/matches.json" },
            { "MatchDetails", "wvw/match_details.json" } // match_id
        };

        private readonly RestClient client;
        private readonly NameCache nameCache;

        private readonly object matchesLock = new object();
        private Dictionary<string, GwMatch> matches;

        public Api(string language = "en")
        {
            client = new RestClient("https://api.guildwars2.com/v1");
            nameCache = new NameCache(this, language);

            Refresh();
        }

        public void Refresh()
        {
            nameCache.Refresh();
        }

        public Dictionary<string, string> GetNames(string url, string lang)
        {
            var request = new RestRequest(RequestUrls[url], Method.GET);
            request.AddParameter("lang", lang);
            var response = client.Execute<List<IdName>>(request);
            return response.Data.ToDictionary(i => i.id, i => i.name);
        }

        public List<GwWorld> GetWorlds()
        {
            return nameCache.GetWorlds().Select(id => new GwWorld(this, id, nameCache.GetWorld(id))).ToList();
        }

        public List<GwMap> GetMaps()
        {
            return nameCache.GetMaps().Select(id => new GwMap(id, nameCache.GetMap(id))).ToList();
        }

        public Dictionary<string, GwEvent> GetEvents(string worldId)
        {
            var request = new RestRequest(RequestUrls["Events"], Method.GET);
            request.AddParameter("world_id", worldId);
            var response = client.Execute<EventsResult>(request);

            var result = new Dictionary<string, GwEvent>();
            foreach (var i in response.Data.events)
            {
                var world = new GwWorld(this, i.world_id, nameCache.GetWorld(i.world_id));
                var map = new GwMap(i.map_id, nameCache.GetMap(i.map_id));
                var ev = new GwEvent(i.event_id, nameCache.GetEvent(i.event_id), world, map, i.state);
                result.Add(ev.Id, ev);
            }
            return result;
        }

        public Dictionary<string, GwMatch> GetMatches()
        {
            lock (matchesLock)
            {
                matches = FetchMatches();
                return matches;
            }
        }

        private Dictionary<string, GwMatch> FetchMatches()
        {
            var request = new RestRequest(RequestUrls["Matches"], Method.GET);
            var response = client.Execute<MatchesResult>(request);

            var result = new Dictionary<string, GwMatch>();
            foreach (var i in response.Data.wvw_matches)
            {
                var red = new GwWorld(this, i.red_world_id, nameCache.GetWorld(i.red_world_id));
                var blue = new GwWorld(this, i.blue_world_id, nameCache.GetWorld(i.blue_world_id));
                var green = new GwWorld(this, i.green_world_id, nameCache.GetWorld(i.green_world_id));
                var match = new GwMatch(this, i.wvw_match_id, red, blue, green);
                result.Add(match.Id, match);
            }

            return result;
        }

        public GwMatch FindMatch(GwWorld world)
        {
            lock (matchesLock)
            {
                if (matches == null)
                    matches = FetchMatches();

                return matches.Values.First(m => m.Red == world || m.Blue == world || m.Green == world);
            }
        }

        public GwMatchDetails GetMatchDetails(string matchId)
        {
            var request = new RestRequest(RequestUrls["MatchDetails"], Method.GET);
            request.AddParameter("match_id", matchId);
            var response = client.Execute<MatchDetailsResult>(request);

            var details = new GwMatchDetails(response.Data.match_id);
            var maps = new List<GwMatchMap>();
            foreach (var m in response.Data.maps)
            {
                var map = new GwMatchMap(details, m.type, m.scores);
                var objectives = new List<GwMatchObjective>();
                foreach (var o in m.objectives)
                {
                    objectives.Add(new GwMatchObjective(details, map, o.id, nameCache.GetObjective(o.id), o.owner));
                }
                map.Objectives = objectives.AsReadOnly();
                maps.Add(map);
            }
            details.Score = new GwMatchScore(response.Data.scores);
            details.Maps = maps.AsReadOnly();
            return details;
        }
    }

    internal class IdName
    {
        public string id { get; internal set; }
        public string name { get; internal set; }
    }

    internal class EventsResult
    {
        public List<Event> events { get; internal set; }
    }

    internal class Event
    {
        public string event_id { get; internal set; }
        public string world_id { get; internal set; }
        public string map_id { get; internal set; }
        public GwEventState state { get; internal set; }
    }

    internal class MatchesResult
    {
        public List<Match> wvw_matches { get; internal set; }
    }

    internal class Match
    {
        public string wvw_match_id { get; internal set; }
        public string red_world_id { get; internal set; }
        public string blue_world_id { get; internal set; }
        public string green_world_id { get; internal set; }
    }

    internal class MatchDetailsResult
    {
        public string match_id { get; internal set; }
        public List<int> scores { get; internal set; }
        public List<MatchMap> maps { get; internal set; }
    }

    internal class MatchMap
    {
        public string type { get; internal set; }
        public List<int> scores { get; internal set; }
        public List<MatchObjective> objectives { get; internal set; }
    }

    internal class MatchObjective
    {
        public string id { get; internal set; }
        public GwMatchTeam owner { get; internal set; }
        public string owner_guild { get; internal set; }
    }
}
