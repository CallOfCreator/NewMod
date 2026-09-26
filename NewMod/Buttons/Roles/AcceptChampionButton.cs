using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.ImpostorRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class AcceptChampionButton : CustomActionButton
{
    public override string Name => "Accept Alliance";
    public override float Cooldown => 0f;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.TyrantAlliance;

    public override bool Enabled(RoleBehaviour role)
    {
        return Button && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data && PlayerControl.LocalPlayer.PlayerId == Tyrant.ChampionId && !Tyrant.OfferAnswered && !PlayerControl.LocalPlayer.Data.IsDead;
    }

    protected override void OnClick()
    {
        Tyrant.RpcAnswerOffer(PlayerControl.LocalPlayer, true);
    }
}