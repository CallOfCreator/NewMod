using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.CrewmateRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1;

public class WardenClueButton : CustomActionButton
{
    public override string Name => WardenRole.ClueType == 0 ? "Entry" : WardenRole.ClueType == 1 ? "Ability" : "Presence";
    public override float Cooldown => 0f;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.TertiaryAbility;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.WardenInvestigate;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is WardenRole;
    }

    protected override void OnClick()
    {
        WardenRole.ClueType = (WardenRole.ClueType + 1) % 3;
        OverrideName(Name);
    }
}