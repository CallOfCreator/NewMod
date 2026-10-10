using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

/// <summary>
/// Previews the Visionary's photos.
/// </summary>
public class ShowScreenshotButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Intelligence;

    public override string Name => "Preview";

    public override float Cooldown => 1f;

    public override float EffectDuration => 0;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite => NewModAsset.ShowScreenshotButton;

    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;

    public override bool CanUse()
    {
        return base.CanUse() && VisionaryUtilities.HasScreenshots && !VisionaryUtilities.IsShowing;
    }

    protected override void OnClick()
    {
        Coroutines.Start(VisionaryUtilities.ShowScreenshots(OptionGroupSingleton<VisionaryOptions>.Instance.MaxDisplayDuration));
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is TheVisionary;
    }
}
