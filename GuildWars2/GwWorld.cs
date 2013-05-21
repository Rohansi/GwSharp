using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    public class GwWorld
    {
        public readonly string Id;
        public readonly string Name;

        public GwMatch Match
        {
            get { return GwMatch.Find(this); }
        }

        internal GwWorld(string id)
        {
            Id = id;
            Name = NameCache.GetWorld(id);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwWorld;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(GwWorld a, GwWorld b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Id == b.Id;
        }

        public static bool operator !=(GwWorld a, GwWorld b)
        {
            return !(a == b);
        }
    }
}
