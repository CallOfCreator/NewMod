using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using NewMod.Utilities;
using NewMod.Options;
using MiraAPI.Networking;
using UnityEngine;

namespace NewMod.Modifiers;

[RegisterModifier]
public class ExplosiveModifier : TimedModifier
{
    public override string ModifierName => "Explosive";
    public override bool HideOnUi => false;
    public override bool AutoStart => true;
    public override float Duration => OptionGroupSingleton<GeneralOption>.Instance.Duration;
    public override bool RemoveOnComplete => true;
    private bool isFlashing = false;
    public override bool CanVent()
    {
        return Player.Data.Role.CanVent;
    }
    public override string GetHudString()
    {
        return ModifierName + "\nif you die, all nearby players are killed";
    }
    public override void OnActivate()
    {
       NewMod.Instance.Log.LogInfo("Activated!");
    }
    public override void OnDeactivate()
    {
        NewMod.Instance.Log.LogInfo("Deactivated!");
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player.AmOwner)
        {
            if (Duration <= 5f)
            {
                isFlashing = !isFlashing;
                var color = isFlashing ? new Color(1f, 0f, 0f) : new Color(0.5f, 0.5f, 0.5f);
                Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, color);
            }
            else if (Duration <= 10f)
            {
                Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, new Color(0f, 0.8f, 1f));
            }
            else if (Duration <= 30f)
            {
                Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, new Color(0f, 1.5f, 0f));
            }
        }
    }
    public override void OnTimerComplete()
    {
       
    }
    public override void OnDeath(DeathReason deathReason)
    {
        var murderer = Utils.GetKiller(Player);
        if (murderer == null) return;

        var closestPlayers = Helpers.GetClosestPlayers(Player.GetTruePosition(), OptionGroupSingleton<GeneralOption>.Instance.KillDistance, true);

        foreach (var player in closestPlayers)
        {
            if (player.Data.IsDead || player.Data.Disconnected) continue;

            murderer.RpcCustomMurder(
            player,
            createDeadBody: true,
            didSucceed: true,
            showKillAnim: false,
            playKillSound: true,
            teleportMurderer: false
          );
          NewMod.Instance.Log.LogInfo($"{player.Data.PlayerName} has been killed by the explosion.");
        }
    }
}