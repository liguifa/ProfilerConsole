using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProfilerConsole
{
    public static class Profiler
    {
        private static Stopwatch watch;
        private static DateTime mStartTime;
        private static DateTime mEndTime;

        public static void Start()
        {
            watch = new Stopwatch();
            watch.Start();
            mStartTime = DateTime.Now;
        }

        public static void Stop()
        {
            watch.Stop();
            mEndTime = DateTime.Now;
            ProfilerEvent @event = new ProfilerEvent();
            @event.Name = HttpContext.Current.Request.RawUrl;
            @event.StartTime = mStartTime;
            @event.EndTime = mEndTime;
            @event.Duration = watch.Elapsed.Milliseconds;
            ProfilerEventSet.Add(@event);
        }

        public static HtmlString Console()
        {
            string template = ProfilerConsole.View.Console.GetConsole();
            var result = Engine.Razor.RunCompile(template, $"templateKey_{Guid.NewGuid()}", null, new { Events = ProfilerEventSet.GetEvents() });
            return new HtmlString(result);
        }
    }
}
