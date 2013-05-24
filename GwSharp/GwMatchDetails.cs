using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GwSharp
{
    public enum GwMatchTeam
    {
        Red = 0, Blue = 1, Green = 2
    }

    public class GwMatchDetails
    {
        /// <summary>
        /// Unique identifier for the match. It is in this format:
        /// "R-T" where R is the region (1 = US, 2 = EU) and T is the tier
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Scores for the match. Access like an array with GwMatchTeam.
        /// </summary>
        public GwMatchScore Score { get; internal set; }

        /// <summary>
        /// List of maps in this matchup.
        /// </summary>
        public ReadOnlyCollection<GwMatchMap> Maps { get; internal set; }

        public GwMatchDetails(string id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatchDetails;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Score.GetHashCode() ^ Maps.GetHashCode();
        }

        public static bool operator ==(GwMatchDetails a, GwMatchDetails b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Score == b.Score && a.Maps.SequenceEqual(b.Maps);
        }

        public static bool operator !=(GwMatchDetails a, GwMatchDetails b)
        {
            return !(a == b);
        }
    }

    public class GwMatchMap
    {
        /// <summary>
        /// Reference to the parent GwMatchDetails instance.
        /// </summary>
        public readonly GwMatchDetails Details;

        /// <summary>
        /// Type of the map.
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Scores for the match. Access like an array with GwMatchTeam.
        /// </summary>
        public readonly GwMatchScore Score;

        /// <summary>
        /// List of objectives in this map.
        /// </summary>
        public ReadOnlyCollection<GwMatchObjective> Objectives { get; internal set; }

        public GwMatchMap(GwMatchDetails details, string type, List<int> score)
        {
            Details = details;
            Type = type;
            Score = new GwMatchScore(score);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatchMap;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Score.GetHashCode() ^ Objectives.GetHashCode();
        }

        public static bool operator ==(GwMatchMap a, GwMatchMap b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Type == b.Type && a.Score == b.Score && a.Objectives.SequenceEqual(b.Objectives);
        }

        public static bool operator !=(GwMatchMap a, GwMatchMap b)
        {
            return !(a == b);
        }
    }

    public class GwMatchObjective
    {
        /// <summary>
        /// Reference to the parent GwMatchDetails instance.
        /// </summary>
        public readonly GwMatchDetails Details;

        /// <summary>
        /// Reference to the parent GwMatchMap instance.
        /// </summary>
        public readonly GwMatchMap Map;

        /// <summary>
        /// Unique identifier of the objective.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Name of the objective.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Owner of the objective.
        /// </summary>
        public readonly GwMatchTeam Owner;

        public GwMatchObjective(GwMatchDetails details, GwMatchMap map, string id, string name, GwMatchTeam owner)
        {
            Details = details;
            Map = map;
            Id = id;
            Name = name;
            Owner = owner;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMatchObjective;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Owner.GetHashCode();
        }

        public static bool operator ==(GwMatchObjective a, GwMatchObjective b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Id == b.Id && a.Owner == b.Owner;
        }

        public static bool operator !=(GwMatchObjective a, GwMatchObjective b)
        {
            return !(a == b);
        }
    }

    /// <summary>
    /// Score container. Access like an array with GwMatchTeam.
    /// </summary>
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
