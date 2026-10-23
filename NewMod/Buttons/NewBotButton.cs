using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using NewBotController = NewMod.NewBot.NewBot;

namespace NewMod.Buttons;
// TODO: This should be replaced with a custom console
public class NewBotButton : CustomActionButton
{
    public override string Name => "NewBot";
    public override float Cooldown => 0f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

    public override bool Enabled(RoleBehaviour role)
    {
        return AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    }

    public override bool CanUse()
    {
        return !MeetingHud.Instance && !ExileController.Instance;
    }

    protected override void OnClick()
    {
        NewBotController.Open();
    }
}
