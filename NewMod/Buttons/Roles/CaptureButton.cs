using NewMod.Modifiers.S1;
using System.Collections;
using System.Linq;
using MiraAPI.Utilities;
using Reactor.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.NeutralRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Roles;

/// <summary>
///     Defines a custom action button for the role.
/// </summary>
public class CaptureButton : CustomActionButton, IEnergyAbility
{
    public GameObject PlacementPreview;
    public Vector2 PlacementPosition;
    public float PlacementAngle;

    public EnergyCategory Category => EnergyCategory.Intelligence;

    public override string Name => "Place Camera";

    public override float Cooldown => OverclockedModifier.GetCooldown(PlayerControl.LocalPlayer, OptionGroupSingleton<VisionaryOptions>.Instance.ScreenshotCooldown);

    public override float EffectDuration => 0;

    public override int MaxUses => (int)OptionGroupSingleton<VisionaryOptions>.Instance.MaxScreenshots;

    public override LoadableAsset<Sprite> Sprite => NewModAsset.Camera;

    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;

    protected override void OnClick()
    {
        VisionaryUtilities.RpcRequestCamera(PlayerControl.LocalPlayer, PlacementPosition.x, PlacementPosition.y, PlacementAngle);
    }

    public override bool CanUse()
    {
        return base.CanUse() && !PlacementPreview;
    }

    public override void ClickHandler()
    {
        if (CanClick())
            Coroutines.Start(PlaceCamera());
    }

    public IEnumerator PlaceCamera()
    {
        var player = PlayerControl.LocalPlayer;
        var preview = new GameObject("CameraPlacement");
        PlacementPreview = preview;
        preview.transform.SetParent(ShipStatus.Instance.transform, false);
        preview.layer = LayerMask.NameToLayer("UI");
        preview.transform.localScale = Vector3.one * 0.05f;
        var sprite = new GameObject("CameraSprite").AddComponent<SpriteRenderer>();
        sprite.gameObject.layer = preview.layer;
        sprite.transform.SetParent(preview.transform, false);
        sprite.sprite = NewModAsset.CameraOff.LoadAsset();
        sprite.transform.localPosition = Vector3.down * sprite.sprite.vertices.Min(vertex => vertex.y);
        sprite.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        var arrow = preview.AddComponent<LineRenderer>();
        arrow.sharedMaterial = Utils.GetCircleMat();
        arrow.useWorldSpace = true;
        arrow.positionCount = 5;
        arrow.startWidth = arrow.endWidth = 0.04f;
        var aiming = false;
        var confirmed = false;
        PlacementPosition = player.GetTruePosition();
        preview.transform.position = new Vector3(PlacementPosition.x, PlacementPosition.y, PlacementPosition.y / 1000f);
        PlacementAngle = player.cosmetics.FlipX ? 180f : 0f;
        try
        {
            while (preview && player && !MeetingHud.Instance && (Input.GetMouseButton(0) || Input.touchCount > 0))
                yield return null;
            while (preview && player && player.Data.Role is TheVisionary && !player.Data.IsDead && player.CanMove && !player.inVent && !MeetingHud.Instance && ShipStatus.Instance)
            {
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                    break;
                if (Input.touchCount > 1 || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Canceled))
                    break;
                var touching = Input.touchCount == 1;
                var pointer = touching ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                var point = (Vector2)Camera.main.ScreenToWorldPoint(pointer);
                var pressed = touching ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
                var released = touching ? Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);
                if (!aiming)
                    PlacementPosition = point;
                var offset = PlacementPosition - player.GetTruePosition();
                var valid = offset.magnitude <= OptionGroupSingleton<VisionaryOptions>.Instance.PlacementRange && !PhysicsHelpers.AnyNonTriggersBetween(player.GetTruePosition(), offset.normalized, offset.magnitude, Constants.ShipAndObjectsMask);
                if (pressed && valid)
                    aiming = true;
                var aim = point - PlacementPosition;
                if (aiming && aim.sqrMagnitude > 0.01f)
                    PlacementAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                preview.transform.position = new Vector3(PlacementPosition.x, PlacementPosition.y, PlacementPosition.y / 1000f);
                var radians = PlacementAngle * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var tip = PlacementPosition + direction;
                var side = new Vector2(-direction.y, direction.x) * 0.15f;
                arrow.SetPositions(new Vector3[] { PlacementPosition, tip, tip - direction * 0.25f + side, tip, tip - direction * 0.25f - side });
                var color = valid ? new Color(0.6f, 0.6f, 0.6f, 0.6f) : new Color(1f, 0.3f, 0.3f, 0.6f);
                sprite.color = color;
                arrow.startColor = arrow.endColor = color;
                if (aiming && released)
                {
                    confirmed = valid;
                    break;
                }

                yield return null;
            }
        }
        finally
        {
            Object.Destroy(preview);
            if (PlacementPreview == preview)
                PlacementPreview = null;
        }

        if (confirmed)
            base.ClickHandler();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is TheVisionary;
    }
}