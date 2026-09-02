using System.Linq;
using MiraAPI.Utilities.Assets;
using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class IdentityCrisisGE : IGeneralEvent
{
    public string Title => "Identity Crisis";
    public string Description => "IDENTITIES HAVE BEEN REASSIGNED!";
    public LoadableAsset<Sprite> Icon => NewModAsset.IdentityCrisisIcon;
    public Color AccentColor => new(0.18f, 0.78f, 1f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<global::NewMod.Options.GEOptions>.Instance.IdentityCrisisWeight;
    public float Duration => 25f;

    public bool CanOccur()
    {
        var players = PlayerControl.AllPlayerControls.ToArray().Where(player => !player.Data.IsDead && !player.Data.Disconnected && !player.HasModifier<InVoid>()).ToArray();

        return players.Length > 1 && players.All(player => player.CurrentOutfitType == PlayerOutfitType.Default);
    }

    public void OnEventStart()
    {
        var players = PlayerControl.AllPlayerControls.ToArray().Where(player => !player.Data.IsDead && !player.Data.Disconnected && !player.HasModifier<InVoid>()).OrderBy(player => player.PlayerId).ToArray();

        if (players.Length < 2)
            return;

        var outfits = players.Select(player => player.Data.DefaultOutfit).ToArray();

        for (var i = 0; i < players.Length; i++)
            players[i].RawSetOutfit(outfits[(i + 1) % outfits.Length], PlayerOutfitType.Shapeshifted);
    }

    public void OnEventEnd()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Disconnected || player.CurrentOutfitType != PlayerOutfitType.Shapeshifted)
                continue;

            player.RawSetOutfit(player.Data.DefaultOutfit, PlayerOutfitType.Default);
        }
    }
}
