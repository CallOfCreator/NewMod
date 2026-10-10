using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class InspectBodyButton : CustomActionButton<DeadBody>
{
    public DeadBody Inspecting;
    public override string Name => "Inspect Body";
    public override float Cooldown => 3f;
    public override float Distance => OptionGroupSingleton<PranksterOptions>.Instance.InspectRange;
    public override float EffectDuration => OptionGroupSingleton<PranksterOptions>.Instance.InspectDuration;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.DeadBodySprite;

    public override bool Enabled(RoleBehaviour role)
    {
        return Button && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.AllPlayerControls.ToArray().Any(player => player.Data && player.Data.Role is Prankster);
    }

    public override DeadBody GetTarget()
    {
        var position = PlayerControl.LocalPlayer.GetTruePosition();
        return Helpers.GetNearestDeadBodies(position, Distance, Helpers.CreateFilter(Constants.NotShipMask)).Where(body => !body.Reported && !PhysicsHelpers.AnythingBetween(position, body.TruePosition, Constants.ShipAndObjectsMask, false)).OrderBy(body => Vector2.Distance(position, body.TruePosition)).FirstOrDefault();
    }

    public override void SetOutline(bool active)
    {
    }

    protected override void OnClick()
    {
        Inspecting = Target;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (EffectActive && (!Inspecting || Vector2.Distance(playerControl.GetTruePosition(), Inspecting.TruePosition) > Distance || MeetingHud.Instance))
        {
            EffectActive = false;
            Timer = Cooldown;
        }
    }

    public override void OnEffectEnd()
    {
        if (!Inspecting || Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), Inspecting.TruePosition) > Distance || MeetingHud.Instance) return;
        var position = Inspecting.TruePosition;
        PranksterUtilities.RpcInspectBody(PlayerControl.LocalPlayer, Inspecting.ParentId, position.x, position.y);
        Inspecting = null;
    }
}
