using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.SpecialAgentOptions;
using NewMod.Roles.NeutralRoles;
using UnityEngine;
using AmongUs.GameOptions;
using Object = UnityEngine.Object;
using NewMod.Utilities;
using Reactor.Utilities;

namespace NewMod.Buttons
{
    [RegisterButton]
    public class AssignButton : CustomActionButton
    {
        public override string Name => "ASSIGN MISSION";
        public override float Cooldown => OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignCooldown;
        public override int MaxUses => (int)OptionGroupSingleton<SpecialAgentOptions>.Instance.AssignMaxUses;
        public override ButtonLocation Location => ButtonLocation.BottomRight;
        public override float EffectDuration => 0f;
        public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;

        private NetworkedPlayerInfo targetPlayer;

        public override bool Enabled(RoleBehaviour role)
        {
            return role is SpecialAgent;
        }
        public override bool CanUse()
        {
            return SpecialAgent.AssignedPlayer == null;
        }
        protected override void OnClick()
        {
            ShapeshifterRole shapeshifterRole = Object.Instantiate(
                RoleManager.Instance.AllRoles.First(r => r.Role == RoleTypes.Shapeshifter)
            ).TryCast<ShapeshifterRole>();
            
            ShapeshifterMinigame minigame = Object.Instantiate(shapeshifterRole.ShapeshifterMenu);
            Object.Destroy(shapeshifterRole.gameObject);
            minigame.name = "SpecialAgent Mission";
            minigame.transform.SetParent(Camera.main.transform, false);
            minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            Minigame.Instance = minigame;
            minigame.MyTask = null;
            minigame.MyNormTask = null;

            if (PlayerControl.LocalPlayer)
            {
                if (MapBehaviour.Instance)
                {
                    MapBehaviour.Instance.Close();
                }
                PlayerControl.LocalPlayer.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            }

            minigame.StartCoroutine(minigame.CoAnimateOpen());
            DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MinigameOpened(PlayerControl.LocalPlayer.Data, minigame.TaskType);
            minigame.potentialVictims = new Il2CppSystem.Collections.Generic.List<ShapeshifterPanel>();
            
            List<PlayerControl> playerList = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => !p.Data.IsDead && !p.Data.Disconnected && p.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                .ToList();

            Il2CppSystem.Collections.Generic.List<UiElement> uiElements = new();

            for (int i = 0; i < playerList.Count; i++)
            {
                var player = playerList[i];
                int num = i % 3;
                int num2 = i / 3;
                bool flag = PlayerControl.LocalPlayer.Data.Role.NameColor == player.Data.Role.NameColor;

                ShapeshifterPanel panel = Object.Instantiate(minigame.PanelPrefab, minigame.transform);
                panel.transform.localPosition = new Vector3(
                    minigame.XStart + num * minigame.XOffset,
                    minigame.YStart + num2 * minigame.YOffset,
                    -1f
                );

                NetworkedPlayerInfo playerInfo = player.Data;

                panel.SetPlayer(i, playerInfo, (Il2CppSystem.Action)(() =>
                {
                    SpecialAgent.AssignedPlayer = playerInfo.Object;

                    Utils.AssignMission(SpecialAgent.AssignedPlayer);

                    if (OptionGroupSingleton<SpecialAgentOptions>.Instance.TargetCameraTracking)
                    {
                        var cam = Camera.main.GetComponent<FollowerCamera>();
                        if (cam != null)
                        {
                            cam.SetTarget(targetPlayer.Object);
                            Coroutines.Start(CoResetCamera(cam, OptionGroupSingleton<SpecialAgentOptions>.Instance.CameraTrackingDuration));
                        }
                    }
                    minigame.Close();
                }));

                panel.NameText.color = flag ? player.Data.Role.NameColor : Color.white;
                minigame.potentialVictims.Add(panel);
                uiElements.Add(panel.Button);
            }

            ControllerManager.Instance.OpenOverlayMenu(
                minigame.name,
                minigame.BackButton,
                minigame.DefaultButtonSelected,
                uiElements
            );
        }
        public static IEnumerator CoResetCamera(FollowerCamera cam, float duration)
        {
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (cam != null)
            {
                cam.SetTarget(PlayerControl.LocalPlayer);
            }
        }
    }
}
