using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuildWars2
{
    internal class NameCache
    {
        private Api api;
        private string language;

        private object nameLock = new object();
        private Dictionary<string, string> worldNames;
        private Dictionary<string, string> mapNames;
        private Dictionary<string, string> eventNames;
        private Dictionary<string, string> objectiveNames;
         
        public NameCache(Api api, string language)
        {
            this.api = api;
            this.language = language;
        }

        public string GetWorld(string id)
        {
            lock (nameLock)
            {
                string value;
                worldNames.TryGetValue(id, out value);
                return value;
            }
        }

        public string GetMap(string id)
        {
            lock (nameLock)
                return mapNames[id];
        }

        public string GetEvent(string id)
        {
            lock (nameLock)
                return eventNames[id];
        }

        public string GetObjective(string id)
        {
            lock (nameLock)
                return objectiveNames[id];
        }

        public List<string> GetWorlds()
        {
            lock (nameLock)
                return worldNames.Keys.ToList();
        }

        public List<string> GetMaps()
        {
            lock (nameLock)
                return mapNames.Keys.ToList();
        }

        public List<string> GetEvents()
        {
            lock (nameLock)
                return eventNames.Keys.ToList();
        }

        public void Refresh()
        {
            lock (nameLock)
            {
                worldNames = api.GetNames("WorldNames", language);
                mapNames = api.GetNames("MapNames", language);
                eventNames = api.GetNames("EventNames", language);
                objectiveNames = api.GetNames("ObjectiveNames", language);
            }
        }
    }
}
