using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class CollectWraithTraceButton : CustomActionButton<DeadBody>
{
    public override string Name => "Collect Trace";
    public override float Cooldown => 0f;
    public override float Distance => OptionGroupSingleton<WraithCallerOptions>.Instance.TraceRange;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.WraithIcon;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is WraithCaller;
    }

    public override DeadBody GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (WraithCallerUtilities.Traces.Contains(player.PlayerId) || WraithCallerUtilities.ActiveNpcs.Values.Any(npc => npc && npc.isActive && npc.Owner == player))
            return null;

        var position = player.GetTruePosition();
        return Helpers.GetNearestDeadBodies(position, Distance, Helpers.CreateFilter(Constants.NotShipMask)).Where(body => !body.Reported && !PranksterUtilities.IsPranksterBody(body) && !WraithCallerUtilities.CollectedTraces.Contains((player.PlayerId, body.ParentId)) && !PhysicsHelpers.AnythingBetween(position, body.TruePosition, Constants.ShipAndObjectsMask, false)).OrderBy(body => Vector2.Distance(position, body.TruePosition)).FirstOrDefault();
    }

    public override void SetOutline(bool active)
    {
        if (!Target) return;
        foreach (var renderer in Target.bodyRenderers)
            renderer.material.SetFloat("_Outline", active ? 1f : 0f);
    }

    protected override void OnClick()
    {
        WraithCallerUtilities.RpcCollectTrace(PlayerControl.LocalPlayer, Target.ParentId);
    }
}
