using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfilerConsole
{
    internal static class ProfilerEventSet
    {
        private static List<ProfilerEvent> mEvents = new List<ProfilerEvent>();
        public static event Action<ProfilerEvent> AddEvent;

        public static void Add(ProfilerEvent @event)
        {
            mEvents.Add(@event);
            if (AddEvent != null)
            {
                AddEvent(@event);
            }
        }

        public static List<ProfilerEvent> GetEvents()
        {
            return mEvents;
        }
    }
}
