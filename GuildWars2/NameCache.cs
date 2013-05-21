using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    internal static class NameCache
    {
        private static object nameLock = new object();
        private static Dictionary<string, string> worldNames;
        private static Dictionary<string, string> mapNames;
        private static Dictionary<string, string> eventNames;
        private static Dictionary<string, string> objectiveNames;
         
        static NameCache()
        {
            Refresh();
        }

        public static string GetWorld(string id)
        {
            lock (nameLock)
            {
                string value;
                worldNames.TryGetValue(id, out value);
                return value;
            }
        }

        public static string GetMap(string id)
        {
            lock (nameLock)
            {
                string value;
                mapNames.TryGetValue(id ?? "", out value);
                return value;
            }
        }

        public static string GetEvent(string id)
        {
            lock (nameLock)
            {
                string value;
                eventNames.TryGetValue(id, out value);
                return value;
            }
        }

        public static string GetObjective(string id)
        {
            lock (nameLock)
            {
                string value;
                objectiveNames.TryGetValue(id, out value);
                return value;
            }
        }

        internal static List<string> GetWorlds()
        {
            lock (nameLock)
                return worldNames.Keys.ToList();
        }

        internal static List<string> GetMaps()
        {
            lock (nameLock)
                return mapNames.Keys.ToList();
        }

        internal static List<string> GetEvents()
        {
            lock (nameLock)
                return eventNames.Keys.ToList();
        }

        internal static void Refresh()
        {
            lock (nameLock)
            {
                var worlds = GwApi.Request("WorldNames");
                worldNames = new Dictionary<string, string>();
                foreach (var kv in worlds)
                {
                    worldNames.Add((string)kv.id, (string)kv.name);
                }

                var maps = GwApi.Request("MapNames");
                mapNames = new Dictionary<string, string>();
                foreach (var kv in maps)
                {
                    mapNames.Add((string)kv.id, (string)kv.name);
                }

                var events = GwApi.Request("EventNames");
                eventNames = new Dictionary<string, string>();
                foreach (var kv in events)
                {
                    eventNames.Add((string)kv.id, (string)kv.name);
                }

                var objectives = GwApi.Request("ObjectiveNames");
                objectiveNames = new Dictionary<string, string>();
                foreach (var kv in objectives)
                {
                    objectiveNames.Add((string)kv.id, (string)kv.name);
                }

                GwMatch.Refresh();
            }
        }
    }
}
