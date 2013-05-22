using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    public class GwWorld
    {
        private readonly Api api;
        public string Id { get; internal set; }
        public string Name { get; internal set; }

        internal GwWorld(Api api, string id, string name)
        {
            this.api = api;
            Id = id;
            Name = name;
        }

        public GwMatch Match
        {
            get { return api.FindMatch(this); }
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
