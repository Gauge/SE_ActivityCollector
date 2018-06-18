using System.Collections.Generic;
using System.Diagnostics;

namespace ActivityCollectorPlugin
{
    public static class Analytics
    {
        private static Dictionary<string, Stopwatch> timers = new Dictionary<string, Stopwatch>();

        public static void Start(string name)
        {
            if (ActivityCollectorPlugin.DebugMode)
            {
                if (!timers.ContainsKey(name))
                {
                    timers.Add(name, new Stopwatch());
                }
                timers[name].Restart();
            }
        }

        public static void Stop(string name)
        {
            if (timers.ContainsKey(name) && ActivityCollectorPlugin.DebugMode)
            {
                timers[name].Stop();
                ActivityCollectorPlugin.log.Info($"[{name}] {(timers[name].ElapsedTicks * 1000d) / Stopwatch.Frequency}ms");
            }
        }
    }
}
