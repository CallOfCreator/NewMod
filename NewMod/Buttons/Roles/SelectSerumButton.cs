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
    public override LoadableAsset<Sprite> Sprite => InjectorUtilities.SelectedSerum == InjectorRole.SerumType.Adrenaline ? NewModAsset.InjectAdrenaline : NewModAsset.InjectSedative;

    public override bool Enabled(RoleBehaviour role)
    {
        return role is InjectorRole;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Button.graphic.SetCooldownNormalizedUvs();
    }

    protected override void OnClick()
    {
        InjectorUtilities.SelectedSerum = InjectorUtilities.SelectedSerum == InjectorRole.SerumType.Adrenaline ? InjectorRole.SerumType.Sedative : InjectorRole.SerumType.Adrenaline;
        OverrideName(Name);
        OverrideSprite(Sprite.LoadAsset());
        Button.graphic.SetCooldownNormalizedUvs();
    }
}
