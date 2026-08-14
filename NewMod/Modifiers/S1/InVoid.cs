using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using NewMod.Components.ScreenEffects;
using UnityEngine;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class InVoid : BaseModifier
{
    public override string ModifierName => "InVoid";

    public override string GetDescription()
    {
        return "In Void";
    }

    public override bool ShowInFreeplay => true;
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            HudManager.Instance.KillButton.Hide();
            Player.killTimer = 240f;
            if (Camera.main != null && !Camera.main.gameObject.GetComponent<NegativeRealityEffect>())
            {
                Camera.main.gameObject.AddComponent<NegativeRealityEffect>();
            }
            else if (Camera.main != null && !Camera.main.gameObject.GetComponent<NegativeRealityEffect>().enabled)
            {
                Camera.main.gameObject.GetComponent<NegativeRealityEffect>().enabled = true;
            }

            foreach (var door in ShipStatus.Instance.AllDoors)
            {
                door.gameObject.SetActive(false);
            }
        }

        if (!Player.AmOwner)
        {
            Player.Visible = false;
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            if (Camera.main != null && Camera.main.gameObject.GetComponent<NegativeRealityEffect>())
            {
                Camera.main.gameObject.GetComponent<NegativeRealityEffect>().enabled = false;
            }

            HudManager.Instance.KillButton.Show();

            foreach (var door in ShipStatus.Instance.AllDoors)
            {
                door.gameObject.SetActive(true);
            }

            Player.RpcAddModifier<JustLeftVoid>();
        }

        if (!Player.AmOwner)
        {
            Player.Visible = true;
        }
    }

    [RegisterEvent]
    public static void BeforeMurderEventThing(BeforeMurderEvent @event)
    {
        if (@event.Source.HasModifier<InVoid>())
        {
            @event.Cancel();
        }
    }
}