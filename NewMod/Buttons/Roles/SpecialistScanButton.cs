using MiraAPI.Hud;
using NewMod.Modifiers.S1;
using NewMod.Utilities;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class SpecialistScanButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;
    public override string Name => "Scan";
    public Vector2 ScanOrigin;
    public bool Interrupted;
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, 8f);
    public override float EffectDuration => 1.5f;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.RadarIcon;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is Specialist;
    }

    public override bool CanUse()
    {
        return base.CanUse() && Specialist.ScanStates.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var state) && state.Charges > 0 && !Utils.IsActive(SystemTypes.Comms);
    }

    protected override void OnClick()
    {
        Interrupted = false;
        ScanOrigin = PlayerControl.LocalPlayer.GetTruePosition();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (EffectActive && (Vector2.Distance(ScanOrigin, playerControl.GetTruePosition()) >= 0.15f || Utils.IsActive(SystemTypes.Comms)))
        {
            Interrupted = true;
            ResetCooldownAndOrEffect();
        }
    }

    public override void OnEffectEnd()
    {
        if (!Interrupted && PlayerControl.LocalPlayer.Data.Role is Specialist && !PlayerControl.LocalPlayer.Data.IsDead && !MeetingHud.Instance && Vector2.Distance(ScanOrigin, PlayerControl.LocalPlayer.GetTruePosition()) < 0.15f && !Utils.IsActive(SystemTypes.Comms))
            Specialist.Scan();
    }
}