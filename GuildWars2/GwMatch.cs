using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    public class GwMatch
    {
        public readonly string Id;
        public readonly GwWorld Red;
        public readonly GwWorld Blue;
        public readonly GwWorld Green;

        internal GwMatch(string id, string redId, string blueId, string greenId)
        {
            Id = id;
            Red = new GwWorld(redId);
            Blue = new GwWorld(blueId);
            Green = new GwWorld(greenId);
        }

        public GwMatchDetails FetchDetails()
        {
            return GwMatchDetails.Get(Id);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatch;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Red.GetHashCode() ^ Blue.GetHashCode() ^ Green.GetHashCode();
        }

        public static bool operator ==(GwMatch a, GwMatch b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Id == b.Id && a.Red == b.Red && a.Blue == b.Blue && a.Green == b.Green;
        }

        public static bool operator !=(GwMatch a, GwMatch b)
        {
            return !(a == b);
        }

        private static object matchesLock = new object();
        private static Dictionary<string, GwMatch> matches;

        static GwMatch()
        {
            Refresh();
        }

        internal static GwMatch Find(GwWorld world)
        {
            lock (matchesLock)
                return matches.Values.FirstOrDefault(m => m.Red.Id == world.Id || m.Blue.Id == world.Id || m.Green.Id == world.Id);
        }

        internal static GwMatch Get(string id)
        {
            lock (matchesLock)
            {
                GwMatch value;
                matches.TryGetValue(id, out value);
                return value;
            }
        }

        internal static Dictionary<string, GwMatch> GetAll()
        {
            lock (matchesLock)
                return matches.ToDictionary(kv => kv.Key, kv => kv.Value);
        } 

        internal static void Refresh()
        {
            lock (matchesLock)
            {
                var matchesObj = GwApi.Request("Matches");
                matches = new Dictionary<string, GwMatch>();
                foreach (var match in matchesObj.wvw_matches)
                {
                    var id = (string)match.wvw_match_id;
                    matches.Add(id, new GwMatch(id, (string)match.red_world_id, (string)match.blue_world_id, (string)match.green_world_id));
                }
            }
        }
    }
}
