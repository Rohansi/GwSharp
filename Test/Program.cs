using System;
using System.Collections.Generic;
using System.Linq;
using GuildWars2;

namespace Test
{
    static class Program
    {
        static void Main()
        {
            var watcher = new GwWatcher();
            watcher.World = GwWatcher.Worlds.First(w => w.Name == "Blackgate");
            watcher.NotifyFilter = GwNotifyFilter.All;
            watcher.EnableRaisingEvents = true;

            watcher.EventStatusChanged += (sender, ev) => Console.WriteLine("'{0}': {1}", ev.Name, ev.Status);
            watcher.WvWScoreChanged += (sender, map) => Console.WriteLine("{0}: R={1}, B={2}, G={3}", map.Type, map.Score[GwMatchTeam.Red], map.Score[GwMatchTeam.Blue], map.Score[GwMatchTeam.Green]);
            watcher.WvWObjectiveChanged += (sender, objective) => Console.WriteLine("{0}: {1} now belongs to {2}", objective.Map.Type, objective.Name, objective.Owner);
        }
    }
}
