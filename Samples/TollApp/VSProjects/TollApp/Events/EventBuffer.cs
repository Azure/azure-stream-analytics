using System;
using System.Collections.Generic;
using System.Linq;

namespace TollApp.Events
{
    internal class EventBuffer
    {
        #region Private variables

        private long _eventId;
        private readonly SortedList<TollEventKey, TollEvent> _events;

        #endregion

        #region Constructor

        internal EventBuffer()
        {
            _eventId = 0;
            _events = new SortedList<TollEventKey, TollEvent>(new TollEventKeyComparer());
        }

        #endregion

        #region Public Methods

        internal IEnumerable<TollEvent> GetEvents(DateTime startTime)
        {
            while (_events.Count > 0)
            {
                var e = _events.First();
                if (e.Key.Timestamp <= startTime)
                {
                    _events.RemoveAt(0);
                    yield return e.Value;
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region Private Methods

        internal void Add(DateTime timeStamp, TollEvent e)
        {
            _events.Add(new TollEventKey { Timestamp = timeStamp, EventId = _eventId++ }, e);
        }

        // Sorted List does not allow duplicates. Add event id to the key and use custom comparer.
        private struct TollEventKey
        {
            public DateTime Timestamp;
            public long EventId;
        }

        private class TollEventKeyComparer : IComparer<TollEventKey>
        {
            public int Compare(TollEventKey x, TollEventKey y)
            {
                int compare = x.Timestamp.CompareTo(y.Timestamp);

                return compare != 0
                    ? compare
                    : x.EventId.CompareTo(y.EventId);
            }
        }

        #endregion
    }
}
