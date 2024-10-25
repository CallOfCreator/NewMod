using System;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using NewMod.Options;
using NewMod.Roles;

namespace NewMod.Modifiers;

[RegisterModifier]
public class ExplosiveModifier : GameModifier
{
     public override string ModifierName => "Explosive";
     public override bool HideOnUi => false;
     public override bool CanVent()
    {
        return Player.Data.Role.CanVent;
    }
    public override string GetHudString()
    {
        return ModifierName + "\nif you die, all nearby players are killed";
    }
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role is not NecromancerRole;
    }
    public override void OnActivate()
    {
       NewMod.Instance.Log.LogInfo("Activated!");
    }
    public override void OnDeactivate()
    {
        NewMod.Instance.Log.LogInfo("Deactivated!");
    }
    
    public override int GetAmountPerGame()
    {
        return 3;
    }
    public override int GetAssignmentChance()
    {
      return 100;
    }
    public override void OnDeath(DeathReason deathReason)
    {
        var closestPlayer = Helpers.GetClosestPlayers(Player.GetTruePosition(), OptionGroupSingleton<GeneralOption>.Instance.KillDistance, true);

        foreach (var player in closestPlayer)
        {
            if (player == null) return;
            if (player.Data.IsDead || player.Data.Disconnected) return;

            Player.RpcCustomMurder(player, createDeadBody:true, teleportMurderer:false, showKillAnim:false, playKillSound:true);
        }
    }
}