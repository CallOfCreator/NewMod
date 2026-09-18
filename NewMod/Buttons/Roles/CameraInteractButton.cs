using System.Linq;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

public class CameraInteractButton : CustomActionButton
{
    public int TargetId = -1;
    public int PendingId = -1;
    public Vector2 Origin;
    public override string Name => "Camera";
    public override float Cooldown => 1f;
    public override float EffectDuration => 1.5f;
    public override MiraKeybind Keybind => null;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.Camera;

    public override bool Enabled(RoleBehaviour role)
    {
        return true;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        var canInteract = playerControl.CanMove && !MeetingHud.Instance && !playerControl.Data.IsDead && !playerControl.inVent;
        if (EffectActive && (!canInteract || Vector2.Distance(Origin, playerControl.GetTruePosition()) >= 0.15f || !VisionaryUtilities.Cameras.TryGetValue(PendingId, out var pending) || !pending.Object))
        {
            PendingId = -1;
            ResetCooldownAndOrEffect();
        }

        TargetId = -1;
        if (canInteract)
        {
            var position = playerControl.GetTruePosition();
            var nearby = VisionaryUtilities.Cameras.Where(pair =>
            {
                if (!pair.Value.Object || (pair.Value.Owner == playerControl.PlayerId && !pair.Value.Ready))
                    return false;
                var offset = (Vector2)pair.Value.Object.transform.position - position;
                return offset.magnitude <= 1.25f && !PhysicsHelpers.AnythingBetween(position, pair.Value.Object.transform.position, Constants.ShipAndObjectsMask, false, pair.Value.Object.GetComponent<Collider2D>(), pair.Value.Object.transform);
            }).OrderBy(pair => Vector2.Distance(pair.Value.Object.transform.position, position)).FirstOrDefault();
            TargetId = EffectActive ? PendingId : nearby.Value != null ? nearby.Key : -1;
        }

        foreach (var (id, record) in VisionaryUtilities.Cameras)
        {
            if (!record.Object)
                continue;
            var outlined = id == TargetId && record.Owner == playerControl.PlayerId;
            var asset = !record.Ready ? NewModAsset.CameraOff : record.Failed ? outlined ? NewModAsset.CameraDisabledOutline : NewModAsset.CameraDisabled : outlined ? NewModAsset.CameraEnabledOutline : NewModAsset.CameraEnabled;
            record.Object.GetComponentInChildren<SpriteRenderer>().sprite = asset.LoadAsset();
        }

        if (Button)
        {
            Button.gameObject.SetActive(TargetId >= 0);
            if (TargetId >= 0)
                OverrideName(VisionaryUtilities.Cameras[TargetId].Owner == playerControl.PlayerId ? "Retrieve" : "Disable Camera");
        }
    }

    public override bool CanUse()
    {
        return base.CanUse() && TargetId >= 0;
    }

    protected override void OnClick()
    {
        PendingId = TargetId;
        Origin = PlayerControl.LocalPlayer.GetTruePosition();
    }

    public override void OnEffectEnd()
    {
        if (PendingId >= 0 && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead && Vector2.Distance(Origin, PlayerControl.LocalPlayer.GetTruePosition()) < 0.15f)
            VisionaryUtilities.RpcInteractCamera(PlayerControl.LocalPlayer, PendingId);
        PendingId = -1;
    }
}