using System;
using System.Linq;
using GwSharp;

namespace Test
{
    static class Program
    {
        static void Main()
        {
            var watcher = new GwWatcher();

            // choose a world to watch
            watcher.World = watcher.Worlds.First(w => w.Name == "Blackgate");

            // what to watch for
            watcher.NotifyFilter = GwNotifyFilter.All;

            // how often to poll for changes
            watcher.PollFrequency = new TimeSpan(0, 0, 10);

            // start watching
            watcher.EnableRaisingEvents = true;

            // called when an event's status changes
            watcher.EventStateChanged += (sender, ev) =>
                Console.WriteLine("{0}: {1}", ev.Name, ev.State);

            // called when the WvW score changes
            watcher.WvWScoreChanged += (sender, map) =>
            {
                var redScore = map.Score[GwMatchTeam.Red];
                var blueScore = map.Score[GwMatchTeam.Blue];
                var greenScore = map.Score[GwMatchTeam.Green];
                Console.WriteLine("Score changed: R={0}, B={1}, G={2}", redScore, blueScore, greenScore);
            };

            // called when a WvW objective changes
            watcher.WvWObjectiveChanged += (sender, objective) =>
                Console.WriteLine("{0}: {1} now belongs to {2}", objective.Map.Type, objective.Name, objective.Owner);
        }
    }
}
