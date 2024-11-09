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
using Innersloth.Assets;

namespace NewMod.Buttons
{
    [RegisterButton]
    public class AssignMissionButton : CustomActionButton
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

        protected override void OnClick()
        {
            ShapeshifterRole shapeshifterRole = Object.Instantiate(
                RoleManager.Instance.AllRoles.First(r => r.Role == RoleTypes.Shapeshifter)
            ).Cast<ShapeshifterRole>();

            ShapeshifterMinigame minigame = Object.Instantiate(shapeshifterRole.ShapeshifterMenu);
            Object.Destroy(shapeshifterRole.gameObject);
            minigame.name = "SpecialAgent Mission";
            minigame.transform.SetParent(Camera.main.transform, false);
            minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            minigame.StartCoroutine(minigame.CoAnimateOpen());
            minigame.potentialVictims = new Il2CppSystem.Collections.Generic.List<ShapeshifterPanel>();

            List<PlayerControl> playerList = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => !p.Data.IsDead && !p.Data.Disconnected)
                .ToList();

            Il2CppSystem.Collections.Generic.List<UiElement> uiElements = new();

            for (int i = 0; i < playerList.Count; i++)
            {
                int num = i % 3;
                int num2 = i / 3;

                ShapeshifterPanel panel = Object.Instantiate(minigame.PanelPrefab, minigame.transform);
                panel.transform.localPosition = new Vector3(
                    minigame.XStart + num2 * minigame.XOffset,
                    minigame.YStart + num * minigame.YOffset,
                    -1f
                );

                NetworkedPlayerInfo playerInfo = playerList[i].Data;

                panel.SetPlayer(i, playerInfo, (System.Action) (() => {
                targetPlayer = playerInfo;

                Utils.AssignMission(targetPlayer.Object);

                minigame.Close();
                }));

                uiElements.Add(panel.GetComponent<UiElement>());
                minigame.potentialVictims.Add(panel);
            }

            ControllerManager.Instance.OpenOverlayMenu(
                minigame.name,
                minigame.BackButton,
                minigame.DefaultButtonSelected,
                uiElements,
                false
            );

            if (OptionGroupSingleton<SpecialAgentOptions>.Instance.TargetCameraTracking)
            {
                FollowerCamera cam = Camera.main!.GetComponent<FollowerCamera>();
            }
        }
    }
}
