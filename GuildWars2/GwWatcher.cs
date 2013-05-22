using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GuildWars2
{
    [Flags]
    public enum GwNotifyFilter
    {
        None = 0,
        EventState = 1,
        WvWScore = 2,
        WvWObjective = 4,
        WvWMatchChange = 8,
        WvW = WvWScore | WvWObjective | WvWMatchChange,
        All = ushort.MaxValue
    }

    public class GwWatcher
    {
        public delegate void EventStateChangedCallback(GwWatcher sender, GwEvent ev);
        public delegate void WvWMatchesChangedCallback(GwWatcher sender);
        public delegate void WvWScoreChangedCallback(GwWatcher sender, GwMatchMap map);
        public delegate void WvWObjectiveChangedCallback(GwWatcher sender, GwMatchObjective objective);

        /// <summary>
        /// Enumeration of all worlds.
        /// </summary>
        public IEnumerable<GwWorld> Worlds
        {
            get { return api.GetWorlds(); }
        }

        /// <summary>
        /// Enumeration of all maps.
        /// </summary>
        public IEnumerable<GwMap> Maps
        {
            get { return api.GetMaps(); }
        }

        /// <summary>
        /// Enumeration of current WvW matches.
        /// </summary>
        public IEnumerable<GwMatch> Matches
        {
            get { lock (dataLock) return (matches ?? api.GetMatches()).Values.ToList(); }
        }

        /// <summary>
        /// Starts/stops the instance from polling for changes.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get { return enableRaisingEvents; }
            set
            {
                UpdateEnabled(value);
            }
        }

        /// <summary>
        /// Changes to watch for.
        /// </summary>
        public GwNotifyFilter NotifyFilter
        {
            get { return notifyFilter; }
            set
            {
                notifyFilter = value;

                // restart
                var enabled = EnableRaisingEvents;
                EnableRaisingEvents = false;
                EnableRaisingEvents = enabled;
            }
        }

        /// <summary>
        /// The world to watch.
        /// </summary>
        public GwWorld World
        {
            get { return world; }
            set
            {
                world = value;

                // restart
                var enabled = EnableRaisingEvents;
                EnableRaisingEvents = false;
                EnableRaisingEvents = enabled;
            }
        }

        /// <summary>
        /// Changes how often changes should be polled for.
        /// </summary>
        public TimeSpan PollFrequency = new TimeSpan(0, 0, 15);

        /// <summary>
        /// Raised when an event status has changed.
        /// </summary>
        public event EventStateChangedCallback EventStateChanged;

        /// <summary>
        /// Raised when the WvW matches change weekly.
        /// </summary>
        public event WvWMatchesChangedCallback WvWMatchesChanged;

        /// <summary>
        /// Raised when the score for a WvW map changes.
        /// </summary>
        public WvWScoreChangedCallback WvWScoreChanged;

        /// <summary>
        /// Raised when an objective in WvW changes.
        /// </summary>
        public WvWObjectiveChangedCallback WvWObjectiveChanged;

        private Api api;
        private bool enableRaisingEvents = false;
        private GwNotifyFilter notifyFilter;
        private GwWorld world;
        private Thread pollThread;

        private readonly object dataLock = new object();
        private Dictionary<string, GwEvent> events;
        private Dictionary<string, GwMatch> matches;
        private GwMatchDetails details;

        public GwWatcher()
        {
            api = new Api();
        }

        private void PollMethod()
        {
            while (true)
            {
                Thread.Sleep(PollFrequency);

                if (NotifyFilter.HasFlag(GwNotifyFilter.EventState))
                {
                    var newEvents = api.GetEvents(world.Id);

                    if (events != null)
                    {
                        foreach (var ev in events)
                        {
                            var newEv = newEvents[ev.Key];
                            if (ev.Value.State != newEv.State)
                            {
                                if (EventStateChanged != null)
                                    EventStateChanged(this, newEv);
                            }
                        }
                    }

                    events = newEvents;
                }

                // any WvW
                if ((notifyFilter & GwNotifyFilter.WvW) != 0)
                {
                    var newMatches = api.GetMatches();

                    if (notifyFilter.HasFlag(GwNotifyFilter.WvWMatchChange) && matches != null)
                    {
                        if (!matches.SequenceEqual(newMatches))
                        {
                            if (WvWMatchesChanged != null)
                                WvWMatchesChanged(this);
                        }
                    }
                   
                    matches = newMatches;

                    var newDetails = World.Match.FetchDetails();
                    
                    if (details != null)
                    {
                        if (notifyFilter.HasFlag(GwNotifyFilter.WvWScore))
                        {
                            for (var i = 0; i < details.Maps.Count; i++)
                            {
                                var o = details.Maps[i];
                                var n = newDetails.Maps[i];

                                if (o.Score != n.Score)
                                {
                                    if (WvWScoreChanged != null)
                                        WvWScoreChanged(this, n);
                                }
                            }
                        }

                        if (notifyFilter.HasFlag(GwNotifyFilter.WvWObjective))
                        {
                            for (var i = 0; i < details.Maps.Count; i++)
                            {
                                var o = details.Maps[i];
                                var n = newDetails.Maps[i];

                                for (var j = 0; j < o.Objectives.Count; j++)
                                {
                                    if (o.Objectives[j] != n.Objectives[j])
                                    {
                                        if (WvWObjectiveChanged != null)
                                            WvWObjectiveChanged(this, n.Objectives[j]);
                                    }
                                }
                            }
                        }
                    }

                    details = newDetails;
                }
            }
        }

        private void UpdateEnabled(bool enabled)
        {
            if (enableRaisingEvents && !enabled)
            {
                if (pollThread != null)
                {
                    pollThread.Abort();
                    pollThread = null;

                    events = null;
                    matches = null;
                    details = null;
                }
            }

            if (!enableRaisingEvents && enabled)
            {
                if (World == null)
                {
                    throw new Exception("World must be set before enabling GwWatcher");
                }

                pollThread = new Thread(PollMethod);
                pollThread.Start();
            }

            enableRaisingEvents = enabled;
        }
    }
}
