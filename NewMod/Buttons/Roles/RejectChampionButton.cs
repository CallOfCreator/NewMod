using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.ImpostorRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class RejectChampionButton : CustomActionButton
{
    public override string Name => "Reject Alliance";
    public override float Cooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.CrownIcon;
    public override bool Enabled(RoleBehaviour role) =>
        Button && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data &&
        PlayerControl.LocalPlayer.PlayerId == Tyrant.ChampionId && !Tyrant.OfferAnswered && !PlayerControl.LocalPlayer.Data.IsDead;
    protected override void OnClick()
    {
        Tyrant.RpcAnswerOffer(PlayerControl.LocalPlayer, false);
    }
}
