using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GwSharp
{
    public class GwMatch
    {
        private readonly Api api;

        /// <summary>
        /// Unique identifier for the match. It is in this format:
        /// "R-T" where R is the region (1=US, 2=EU) and T is the tier
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// The world that is playing as red.
        /// </summary>
        public readonly GwWorld Red;

        /// <summary>
        /// The world that is playing as blue.
        /// </summary>
        public readonly GwWorld Blue;

        /// <summary>
        /// The world that is playing as green.
        /// </summary>
        public readonly GwWorld Green;

        internal GwMatch(Api api, string id, GwWorld red, GwWorld blue, GwWorld green)
        {
            this.api = api;

            Id = id;
            Red = red;
            Blue = blue;
            Green = green;
        }

        /// <summary>
        /// Returns the most up to date match details for this world.
        /// </summary>
        public GwMatchDetails FetchDetails()
        {
            return api.GetMatchDetails(Id);
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
    }
}
