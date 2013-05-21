using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    public enum GwMatchTeam
    {
        Red = 0, Blue = 1, Green = 2, Unknown = -1
    }

    public class GwMatchDetails
    {
        public readonly GwMatch Match;
        public readonly GwMatchScore Score; 
        public readonly ReadOnlyCollection<GwMatchMap> Maps;

        internal GwMatchDetails(dynamic matchObj)
        {
            Match = GwMatch.Get((string)matchObj.match_id);

            var score = new List<int>();
            for (var i = 0; i < matchObj.scores.Count; i++)
            {
                score.Add((int)matchObj.scores[i]);
            }
            Score = new GwMatchScore(score);

            var maps = new List<GwMatchMap>();
            for (var i = 0; i < matchObj.maps.Count; i++)
            {
                maps.Add(new GwMatchMap(this, matchObj.maps[i]));
            }
            Maps = new ReadOnlyCollection<GwMatchMap>(maps);
        }

        public static GwMatchDetails Get(string matchId)
        {
            var matchObj = GwApi.Request("MatchDetails", matchId);
            return new GwMatchDetails(matchObj);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatchDetails;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Match.GetHashCode() ^ Score.GetHashCode() ^ Maps.GetHashCode();
        }

        public static bool operator ==(GwMatchDetails a, GwMatchDetails b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Match.Id == b.Match.Id && a.Score == b.Score && a.Maps.SequenceEqual(b.Maps);
        }

        public static bool operator !=(GwMatchDetails a, GwMatchDetails b)
        {
            return !(a == b);
        }
    }

    public class GwMatchMap
    {
        public readonly GwMatch Match;
        public readonly GwMatchDetails Details;
        public readonly string Type;
        public readonly GwMatchScore Score;
        public readonly ReadOnlyCollection<GwMatchObjective> Objectives;
         
        internal GwMatchMap(GwMatchDetails details, dynamic matchMapObj)
        {
            Match = details.Match;
            Details = details;

            Type = matchMapObj.type;

            var score = new List<int>();
            for (var i = 0; i < matchMapObj.scores.Count; i++)
            {
                score.Add((int)matchMapObj.scores[i]);
            }
            Score = new GwMatchScore(score);

            var objectives = new List<GwMatchObjective>();
            for (var i = 0; i < matchMapObj.objectives.Count; i++)
            {
                objectives.Add(new GwMatchObjective(this, matchMapObj.objectives[i]));
            }
            Objectives = new ReadOnlyCollection<GwMatchObjective>(objectives);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatchMap;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Match.GetHashCode() ^ Type.GetHashCode() ^ Score.GetHashCode() ^ Objectives.GetHashCode();
        }

        public static bool operator ==(GwMatchMap a, GwMatchMap b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Match.Id == b.Match.Id && a.Type == b.Type && a.Score == b.Score && a.Objectives.SequenceEqual(b.Objectives);
        }

        public static bool operator !=(GwMatchMap a, GwMatchMap b)
        {
            return !(a == b);
        }
    }

    public class GwMatchObjective
    {
        public readonly GwMatch Match;
        public readonly GwMatchDetails Details;
        public readonly GwMatchMap Map;
        public readonly string Id;
        public readonly string Name;
        public readonly GwMatchTeam Owner;

        public GwMatchObjective(GwMatchMap map, dynamic matchObjectiveObj)
        {
            Match = map.Match;
            Details = map.Details;
            Map = map;

            Id = matchObjectiveObj.id;
            Name = NameCache.GetObjective(Id);

            switch ((string)matchObjectiveObj.owner)
            {
                case "Red":
                    Owner = GwMatchTeam.Red;
                    break;
                case "Blue":
                    Owner = GwMatchTeam.Blue;
                    break;
                case "Green":
                    Owner = GwMatchTeam.Green;
                    break;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatchObjective;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Match.GetHashCode() ^ Id.GetHashCode() ^ Owner.GetHashCode();
        }

        public static bool operator ==(GwMatchObjective a, GwMatchObjective b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Match.Id == b.Match.Id && a.Id == b.Id && a.Owner == b.Owner;
        }

        public static bool operator !=(GwMatchObjective a, GwMatchObjective b)
        {
            return !(a == b);
        }
    }

    public class GwMatchScore
    {
        private readonly List<int> scores;
         
        internal GwMatchScore(List<int> scores)
        {
            this.scores = scores;
        }

        public int this[int i]
        {
            get { return scores[i]; }
        }

        public int this[GwMatchTeam i]
        {
            get { return scores[(int)i]; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatchScore;
            return this == other;
        }

        public override int GetHashCode()
        {
            return scores.GetHashCode();
        }

        public static bool operator==(GwMatchScore a, GwMatchScore b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.scores.SequenceEqual(b.scores);
        }

        public static bool operator !=(GwMatchScore a, GwMatchScore b)
        {
            return !(a == b);
        }
    }
}
