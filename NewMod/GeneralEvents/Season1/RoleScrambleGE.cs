using System.Linq;
using MiraAPI.Utilities;
using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class RoleScrambleGE : IGeneralEvent
{
    public string Title => "Role Scramble";
    public string Description => " ROLE ASSIGNMENTS HAVE BEEN CORRUPTED!";
    public LoadableAsset<Sprite> Icon => NewModAsset.RoleScrambleIcon;
    public Color AccentColor => new(0.95f, 0.62f, 0.12f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<Options.GEOptions>.Instance.RoleScrambleFrequency.Value;
    public float Duration => 5f;

    public bool CanOccur()
    {
        return Helpers.GetAlivePlayers().Count(player => !player.HasModifier<InVoid>()) >= 3;
    }

    public void OnEventStart()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        var players = Helpers.GetAlivePlayers().Where(player => !player.HasModifier<InVoid>()).ToArray();

        if (players.Length < 3)
            return;

        var roles = players.Select(player => player.Data.Role.Role).ToArray();

        for (var i = roles.Length - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (roles[i], roles[j]) = (roles[j], roles[i]);
        }

        if (players.Select((player, index) => player.Data.Role.Role == roles[index]).All(same => same))
        {
            var first = roles[0];
            for (var i = 0; i < roles.Length - 1; i++)
                roles[i] = roles[i + 1];
            roles[^1] = first;
        }

        for (var i = 0; i < players.Length; i++)
            players[i].RpcSetRole(roles[i], true);
    }

    public void OnEventEnd()
    {
    }
}