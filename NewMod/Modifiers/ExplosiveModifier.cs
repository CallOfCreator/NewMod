using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using NewMod.Options.Modifiers;
using UnityEngine;

namespace NewMod.Modifiers;

public class ExplosiveModifier : TimedModifier
{
    public override string ModifierName => "Explosive";
    public override bool HideOnUi => false;
    public override bool AutoStart => true;
    public override bool ShowInFreeplay => true;
    public override float Duration => OptionGroupSingleton<ExplosiveModifierOptions>.Instance.Duration;
    public override bool RemoveOnComplete => true;

    public override bool? CanVent()
    {
        return Player.Data.Role.CanVent;
    }

    public override string GetDescription()
    {
        return "If you are killed, all nearby players are killed.";
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player.AmOwner)
            return;

        var material = Player.cosmetics.currentBodySprite.BodySprite.material;

        if (TimeRemaining <= 5f)
        {
            var flash = Mathf.FloorToInt(TimeRemaining * 4f) % 2 == 0;
            material.SetColor(ShaderID.VisorColor, flash ? Color.red : Palette.VisorColor);
        }
        else if (TimeRemaining <= 10f)
        {
            material.SetColor(ShaderID.VisorColor, new Color(0f, 0.8f, 1f));
        }
        else if (TimeRemaining <= 30f)
        {
            material.SetColor(ShaderID.VisorColor, Color.green);
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
            Player.SetPlayerMaterialColors(Player.cosmetics.currentBodySprite.BodySprite);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || !evt.Target.HasModifier<ExplosiveModifier>() || !evt.Target.Data.IsDead)
            return;

        var closestPlayers = Helpers.GetClosestPlayers(evt.Target.GetTruePosition(), OptionGroupSingleton<ExplosiveModifierOptions>.Instance.KillDistance);

        foreach (var player in closestPlayers)
        {
            if (player.Data.IsDead || player.Data.Disconnected)
                continue;

            evt.Source.RpcCustomMurder(player, true, createDeadBody: true, teleportMurderer: false, showKillAnim: false, playKillSound: true);

            Info($"{player.Data.PlayerName} has been killed by the explosion.");
        }
    }
}
