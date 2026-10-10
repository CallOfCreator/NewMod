using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Modifiers.S1;
using NewMod.Options.Roles;
using NewMod.Roles.ImpostorRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

/// <summary>
/// Revives a nearby player as Necromancer.
/// </summary>
public class ReviveButton : CustomActionButton, IEnergyAbility
{
    public EnergyCategory Category => EnergyCategory.Protection;

    /// <summary>
    /// Gets an empty label because the sprite includes the name.
    /// </summary>
    public override string Name => string.Empty;

    /// <summary>
    /// Gets the revive cooldown with the Overclocked modifier applied.
    /// </summary>
    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<NecromancerOption>.Instance.ButtonCooldown);

    /// <summary>
    /// Gets the number of revives allowed by the role options.
    /// </summary>
    public override int MaxUses => (int)OptionGroupSingleton<NecromancerOption>.Instance.AbilityUses;

    /// <summary>
    /// Gets zero duration since reviving is immediate.
    /// </summary>
    public override float EffectDuration => 0f;

    /// <summary>
    /// Gets the revive keybind.
    /// </summary>
    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

    /// <summary>
    /// Gets the button location.
    /// </summary>
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    /// <summary>
    /// Gets the revive button sprite.
    /// </summary>
    public override LoadableAsset<Sprite> Sprite => NewModAsset.NecromancerButton;

    public static DeadBody GetReviveTarget()
    {
        var local = PlayerControl.LocalPlayer;
        var localPos = local.GetTruePosition();

        return Helpers.GetNearestDeadBodies(localPos, ShipStatus.Instance.MaxLightRadius, Helpers.CreateFilter(Constants.NotShipMask)).Where(body => body != null && IsValidReviveTarget(local, body)).OrderBy(body => Vector2.Distance(localPos, body.TruePosition)).FirstOrDefault();
    }

    public static bool IsValidReviveTarget(PlayerControl local, DeadBody body)
    {
        if (PranksterUtilities.IsPranksterBody(body))
            return false;

        var killedPlayer = GameData.Instance.GetPlayerById(body.ParentId)?.Object;
        if (!killedPlayer || !killedPlayer.Data.IsDead || killedPlayer.Data.Disconnected)
            return false;

        var killer = Utils.GetKiller(killedPlayer);

        if (killer != null && killer.PlayerId == local.PlayerId)
            return false;

        return true;
    }

    /// <summary>
    /// Checks whether the button is ready and a valid body is nearby.
    /// </summary>
    /// <returns>True if the player can revive a nearby body.</returns>
    public override bool CanUse()
    {
        return base.CanUse() && GetReviveTarget() != null;
    }

    /// <summary>
    /// Plays the revive sound and revives the nearest valid body.
    /// </summary>
    protected override void OnClick()
    {
        var local = PlayerControl.LocalPlayer;
        var body = GetReviveTarget();

        SoundManager.Instance.PlaySound(NewModAsset.ReviveSound?.LoadAsset(), false, 2f);

        Utils.HandleRevive(local, body.ParentId, RoleTypes.Crewmate, body.transform.position.x, body.transform.position.y);
    }

    /// <summary>
    /// Enables the button for Necromancer.
    /// </summary>
    /// <param name="role">The current player's role.</param>
    /// <returns>True for Necromancer.</returns>
    public override bool Enabled(RoleBehaviour role)
    {
        return role is NecromancerRole;
    }
}
