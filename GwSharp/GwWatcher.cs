using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GwSharp
{
    /// <summary>
    /// Flags that are used to determine enabled features of a GwWatcher instance.
    /// </summary>
    [Flags]
    public enum GwNotifyFilter
    {
        /// <summary>
        /// Watch for event state changes.
        /// </summary>
        EventState = 1,

        /// <summary>
        /// Watch for WvW score changes.
        /// </summary>
        WvWScore = 2,

        /// <summary>
        /// Watch for WvW score changes, per map.
        /// </summary>
        WvWMapScore = 4,

        /// <summary>
        /// Watch for WvW objective changes.
        /// </summary>
        WvWObjective = 8,

        /// <summary>
        /// Watch for WvW match changes. Should occur weekly.
        /// </summary>
        WvWMatchChange = 16,

        /// <summary>
        /// Watch for all possible PvE changes.
        /// </summary>
        PvE = EventState,

        /// <summary>
        /// Watch for all possible WvW changes.
        /// </summary>
        WvW = WvWScore | WvWMapScore | WvWObjective | WvWMatchChange,

        /// <summary>
        /// Watch for all possible changes.
        /// </summary>
        All = int.MaxValue
    }

    /// <summary>
    /// Watches for changes in data from the Guild Wars 2 API.
    /// </summary>
    public class GwWatcher
    {
        public delegate void DataReadyCallback(GwWatcher sender);
        public delegate void DataChangedCallback<in T>(GwWatcher sender, T previous, T current);

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
            get
            {
                if ((notifyFilter & GwNotifyFilter.WvW) == 0)
                    throw new Exception("WvW data is not enabled");
                lock (dataLock)
                    return matches.Values.ToList();
            }
        }

        /// <summary>
        /// Dictionary of current events.
        /// </summary>
        public ReadOnlyDictionary<string, GwEvent> Events
        {
            get
            {
                if (!notifyFilter.HasFlag(GwNotifyFilter.EventState))
                    throw new Exception("Event data is not enabled");
                return new ReadOnlyDictionary<string, GwEvent>(events);
            }
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
        /// Raised after new information is polled for.
        /// </summary>
        public event DataReadyCallback DataReady;

        /// <summary>
        /// Raised when an event status has changed.
        /// </summary>
        public event DataChangedCallback<GwEvent> EventStateChanged;

        /// <summary>
        /// Raised when the WvW matches change weekly.
        /// </summary>
        public event DataReadyCallback WvWMatchesChanged;

        /// <summary>
        /// Raised when the score for a WvW map changes.
        /// </summary>
        public DataChangedCallback<GwMatchMap> WvWMapScoreChanged;

        /// <summary>
        /// Raised when the score for the WvW matchup changes.
        /// </summary>
        public DataChangedCallback<GwMatchDetails> WvWScoreChanged;

        /// <summary>
        /// Raised when an objective in WvW changes.
        /// </summary>
        public DataChangedCallback<GwMatchObjective> WvWObjectiveChanged;

        private Api api;
        private bool enableRaisingEvents = false;
        private GwNotifyFilter notifyFilter;
        private GwWorld world;
        private Thread pollThread;

        private readonly object dataLock = new object();
        private ConcurrentDictionary<string, GwEvent> events;
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
                lock (dataLock)
                {
                    if (NotifyFilter.HasFlag(GwNotifyFilter.EventState))
                        CheckEventChanges();

                    // any WvW
                    if ((notifyFilter & GwNotifyFilter.WvW) != 0)
                    {
                        CheckWvWMatchChanges();
                        CheckWvWDetailChanges();
                    }
                }

                if (DataReady != null)
                    DataReady(this);

                Thread.Sleep(PollFrequency);
            }
        }

        private void CheckEventChanges()
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
                            EventStateChanged(this, ev.Value, newEv);
                    }
                }
            }

            events = new ConcurrentDictionary<string, GwEvent>(newEvents);
        }

        private void CheckWvWMatchChanges()
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
        }

        private void CheckWvWDetailChanges()
        {
            var newDetails = World.Match.FetchDetails();

            if (details != null)
            {
                if (notifyFilter.HasFlag(GwNotifyFilter.WvWScore))
                {
                    if (details.Score != newDetails.Score)
                    {
                        if (WvWScoreChanged != null)
                            WvWScoreChanged(this, details, newDetails);
                    }
                }

                if (notifyFilter.HasFlag(GwNotifyFilter.WvWMapScore))
                {
                    for (var i = 0; i < details.Maps.Count; i++)
                    {
                        var o = details.Maps[i];
                        var n = newDetails.Maps[i];

                        if (o.Score != n.Score)
                        {
                            if (WvWMapScoreChanged != null)
                                WvWMapScoreChanged(this, o, n);
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
                                    WvWObjectiveChanged(this, o.Objectives[j], n.Objectives[j]);
                            }
                        }
                    }
                }
            }

            details = newDetails;
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
