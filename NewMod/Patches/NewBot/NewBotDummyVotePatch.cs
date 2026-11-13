using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using NewBotController = NewMod.NewBot.NewBot;

namespace NewMod.Patches.NewBot;

public static class NewBotDummyVotePatch
{
    [RegisterEvent]
    public static void OnDummyVote(DummyVoteEvent evt)
    {
        var bot = NewBotController.Instance;
        if (!bot || !bot.Practicing || AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay || !bot.Map.Dummies.Contains(evt.Dummy.Player))
            return;

        var target = evt.Dummy.PlayerIdToVoteFor;
        evt.Dummy.PlayerIdToVoteFor = 253;
        var suspect = target < 0 ? null : GameData.Instance.GetPlayerById((byte)target);
        evt.CanSkip = !suspect || suspect.IsDead || suspect.Disconnected || suspect.PlayerId == evt.Dummy.Player.PlayerId;
        evt.PlayerIsValid = player => !evt.CanSkip && player.PlayerId == target;
    }
}
