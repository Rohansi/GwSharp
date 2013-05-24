using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GwSharp
{
    /// <summary>
    /// Represents the status of an event.
    /// </summary>
    public enum GwEventState
    {
        /// <summary>
        /// Event is in progress.
        /// </summary>
        Active,

        /// <summary>
        /// Event succeeded.
        /// </summary>
        Success,

        /// <summary>
        /// Event failed.
        /// </summary>
        Fail,

        /// <summary>
        /// Event is able to be activated.
        /// </summary>
        Warmup,

        /// <summary>
        /// Event is beginning.
        /// </summary>
        Preparation,

        /// <summary>
        /// Event is not active.
        /// </summary>
        Inactive
    }

    public class GwEvent
    {
        /// <summary>
        /// Unique identifier of the event.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Name of the event.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// World that the event is taking place in.
        /// </summary>
        public readonly GwWorld World;

        /// <summary>
        /// Map that the event is taking place in. 
        /// </summary>
        public readonly GwMap Map;

        /// <summary>
        /// Current state of the event.
        /// </summary>
        public readonly GwEventState State;

        internal GwEvent(string id, string name, GwWorld world, GwMap map, GwEventState state)
        {
            Id = id;
            Name = name;
            World = world;
            Map = map;
            State = state;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwEvent;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ State.GetHashCode();
        }

        public static bool operator ==(GwEvent a, GwEvent b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Id == b.Id && a.State == b.State;
        }

        public static bool operator !=(GwEvent a, GwEvent b)
        {
            return !(a == b);
        }
    }
}
