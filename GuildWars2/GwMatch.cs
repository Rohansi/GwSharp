using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    public class GwMatch
    {
        private readonly Api api;

        public readonly string Id;
        public readonly GwWorld Red;
        public readonly GwWorld Blue;
        public readonly GwWorld Green;

        internal GwMatch(Api api, string id, GwWorld red, GwWorld blue, GwWorld green)
        {
            this.api = api;

            Id = id;
            Red = red;
            Blue = blue;
            Green = green;
        }

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
