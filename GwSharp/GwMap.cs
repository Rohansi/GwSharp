using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GwSharp
{
    public class GwMap
    {
        /// <summary>
        /// Unique identifier for the map.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Name of the map.
        /// </summary>
        public readonly string Name;

        internal GwMap(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwMap;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(GwMap a, GwMap b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Id == b.Id;
        }

        public static bool operator !=(GwMap a, GwMap b)
        {
            return !(a == b);
        }
    }
}
