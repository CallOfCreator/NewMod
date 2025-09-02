using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace NewMod
{
    public static class NewModEventHandler
    {
        public static void RegisterEventsLogs()
        {
            var type = typeof(MiraEventManager);
            var fld = type.GetField("EventWrappers", BindingFlags.NonPublic | BindingFlags.Static);
            var dictObj = fld.GetValue(null);
            if (dictObj is not IDictionary dict || dict.Count == 0)
            {
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Registered NewMod Events ===");

            foreach (DictionaryEntry entry in dict)
            {
                var eventType = entry.Key as Type;
                var listObj = entry.Value;
                int count = 0;
                var lines = new List<string>();

                if (listObj is IEnumerable wrappers)
                {
                    foreach (var wrapper in wrappers)
                    {
                        if (wrapper == null) continue;
                        var wType = wrapper.GetType();

                        var ehProp = wType.GetProperty("EventHandler", BindingFlags.Public | BindingFlags.Instance);
                        var prProp = wType.GetProperty("Priority", BindingFlags.Public | BindingFlags.Instance);

                        var del = ehProp.GetValue(wrapper) as Delegate;
                        var prio = prProp.GetValue(wrapper) as int? ?? 0;

                        var method = del.Method;
                        var declType = method.DeclaringType.FullName;
                        var methodName = method.Name;

                        lines.Add($" [{prio}] {declType}.{methodName}()");
                        count++;
                    }
                }

                sb.AppendLine($"{eventType.FullName}  (handlers: {count})");
                foreach (var l in lines) sb.AppendLine(l);
            }
            NewMod.Instance.Log.LogInfo(sb.ToString());
        }

        // General events
        [RegisterEvent]
        public static void OnRoundStart(RoundStartEvent evt)
        {
            if (!evt.TriggeredByIntro) return;

            HudManager.Instance.Chat.enabled = false;
        }
    }
}
