using System.Collections.Generic;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Usables;
using NewMod.Patches;
using NewMod.Patches.Roles.Visionary;
using NewMod.Roles.ImpostorRoles;

namespace NewMod
{
    public static class NewModEventHandler
    {
        public static void RegisterEventsLogs()
        {
            var registrations = new List<string>
            {
                $"{nameof(GameEndEvent)}: {nameof(EndGamePatch.OnGameEnd)}",
                $"{nameof(EnterVentEvent)}: {nameof(VisionaryVentPatch.OnEnterVent)}",
                $"{nameof(BeforeMurderEvent)}: {nameof(VisionaryMurderPatch.OnBeforeMurder)}",
                $"{nameof(AfterMurderEvent)}: {nameof(NewMod.OnAfterMurder)}",
                $"{nameof(HandleVoteEvent)}: {nameof(Tyrant.OnHandleVote)}",
                $"{nameof(StartMeetingEvent)}: {nameof(Tyrant.OnMeetingStart)}",
                $"{nameof(ProcessVotesEvent)}: {nameof(Tyrant.OnProcessVotes)}"
            };
            NewMod.Instance.Log.LogInfo("Registered events: " + "\n" + string.Join(", ", registrations));
        }
    }
}
