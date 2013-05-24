using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GwSharp
{
    public class GwWorld
    {
        private readonly Api api;

        /// <summary>
        /// Unique identifier of the world. It is in this format:
        /// "RLXX" where R is region (1=US, 2=EU), L is language (0=EN, 1=FR, 2=DE, 3=SP) and XX is for uniqueness
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Name of the world.
        /// </summary>
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
