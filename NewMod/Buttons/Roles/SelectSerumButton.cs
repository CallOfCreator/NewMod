using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class SelectSerumButton : CustomActionButton
{
    public override string Name => InjectorUtilities.SelectedSerum.ToString();
    public override float Cooldown => 0f;
    public override MiraKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.InjectIcon;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is InjectorRole;
    }

    protected override void OnClick()
    {
        InjectorUtilities.SelectedSerum = InjectorUtilities.SelectedSerum == InjectorRole.SerumType.Adrenaline ? InjectorRole.SerumType.Sedative : InjectorRole.SerumType.Adrenaline;
        OverrideName(Name);
    }
}
