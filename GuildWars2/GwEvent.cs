using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    /// <summary>
    /// Represents the status of an event.
    /// </summary>
    public enum EventStatus
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
        public readonly string Id;
        public readonly string Name;
        public readonly GwWorld World;
        public readonly GwMap Map;
        public EventStatus Status;

        private GwEvent(string id, string worldId, string mapId, string status)
        {
            Id = id;
            Name = NameCache.GetEvent(id);
            World = new GwWorld(worldId);
            Map = new GwMap(mapId);

            switch (status)
            {
                case "Active":
                    Status = EventStatus.Active;
                    break;
                case "Success":
                    Status = EventStatus.Success;
                    break;
                case "Fail":
                    Status = EventStatus.Fail;
                    break;
                case "Warmup":
                    Status = EventStatus.Warmup;
                    break;
                case "Preparation":
                    Status = EventStatus.Preparation;
                    break;
                case "Inactive":
                    Status = EventStatus.Inactive;
                    break;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as GwEvent;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(GwEvent a, GwEvent b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Id == b.Id;
        }

        public static bool operator !=(GwEvent a, GwEvent b)
        {
            return !(a == b);
        }

        private static object eventsLock = new object();
        private static Dictionary<string, Dictionary<string, GwEvent>> events;

        static GwEvent()
        {
            events = new Dictionary<string, Dictionary<string, GwEvent>>();
        }

        internal static GwEvent Get(string worldId, string eventId)
        {
            if (NameCache.GetEvent(eventId) == null)
            {
                throw new Exception("Unknown eventId");
            }

            lock (eventsLock)
            {
                Dictionary<string, GwEvent> states;
                if (!events.TryGetValue(worldId, out states))
                {
                    throw new Exception("Unknown worldId, call Refresh first");
                }

                GwEvent ev;
                if (!states.TryGetValue(eventId, out ev))
                {
                    ev = new GwEvent(eventId, worldId, null, "Inactive");
                }

                return ev;
            }
        }

        internal static Dictionary<string, GwEvent> GetAll(string worldId)
        {
            List<string> eventIds = NameCache.GetEvents();
            return eventIds.Select(id => Get(worldId, id)).ToDictionary(e => e.Id, e => e);
        }

        internal static void Refresh(string worldId)
        {
            lock (eventsLock)
            {
                Dictionary<string, GwEvent> states;
                if (!events.TryGetValue(worldId, out states))
                {
                    states = new Dictionary<string, GwEvent>();
                }

                states.Clear();

                var eventsObj = GwApi.Request("Events", worldId);
                foreach (var ev in eventsObj.events)
                {
                    states.Add((string)ev.event_id, new GwEvent((string)ev.event_id, (string)ev.world_id, (string)ev.map_id, (string)ev.state));
                }

                events[worldId] = states;
            }
        }
    }
}
