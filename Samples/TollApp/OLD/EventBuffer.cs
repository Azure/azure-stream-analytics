//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace TollApp
{
    internal class EventBuffer
    {
        private long eventId;
        private SortedList<TollEventKey, TollEvent> events;

        internal EventBuffer()
        {
            eventId = 0;
            events = new SortedList<TollEventKey, TollEvent>(new TollEventKeyComparer());
        }

        public IEnumerable<TollEvent> GetEvents(DateTime time)
        {
            while (events.Count > 0)
            {
                var e = events.First();
                if (e.Key.Timestamp <= time)
                {
                    events.RemoveAt(0);
                    yield return e.Value;
                }
                else
                {
                    break;
                }
            }
        }

        internal void Add(DateTime timeStamp, TollEvent e)
        {
            events.Add(new TollEventKey { Timestamp = timeStamp, EventId = eventId++ }, e);
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
    }
}
