using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles.S1;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NewMod.Utilities
{
    public enum VerifierClaimType : byte
    {
        None,
        DidTask,
        EnteredVent,
        NearBody,
        UsedAbility
    }

    [Flags]
    public enum VerifierFactFlags : byte
    {
        None = 0,
        DidTask = 1,
        EnteredVent = 2,
        NearBody = 4,
        UsedAbility = 8
    }

    public static class VerifierUtilities
    {
        public static readonly Dictionary<byte, VerifierFactFlags> RoundFacts = new();

        public static CustomPlayerMenu ClaimMenu;
        public static VerifierClaimType PendingClaim;
        public static bool UsedThisMeeting;

        [RegisterEvent]
        public static void OnRoundStart(RoundStartEvent evt)
        {
            if (!evt.TriggeredByIntro)
                return;

            Reset(clearFacts: true);
        }

        [RegisterEvent]
        public static void OnGameEnd(GameEndEvent evt)
        {
            Reset(clearFacts: true);
        }

        [RegisterEvent]
        public static void OnCompleteTask(CompleteTaskEvent evt)
        {
            if (evt.Player && evt.Player.AmOwner)
                RpcRegisterFact(evt.Player, (byte)VerifierClaimType.DidTask);
        }

        [RegisterEvent]
        public static void OnEnterVent(EnterVentEvent evt)
        {
            if (evt.Player && evt.Player.AmOwner)
                RpcRegisterFact(evt.Player, (byte)VerifierClaimType.EnteredVent);
        }

        [RegisterEvent]
        public static void OnMiraButtonClick(MiraButtonClickEvent evt)
        {
            var player = PlayerControl.LocalPlayer;

            if (!player || MeetingHud.Instance || ExileController.Instance)
                return;

            if (!evt.Button.CanClick())
                return;

            RpcRegisterFact(player, (byte)VerifierClaimType.UsedAbility);
        }

        [RegisterEvent]
        public static void OnVanillaButtonClick(VanillaButtonClickEvent evt)
        {
            var player = PlayerControl.LocalPlayer;

            if (!player || MeetingHud.Instance || ExileController.Instance)
                return;

            RpcRegisterFact(player, (byte)VerifierClaimType.UsedAbility);
        }

        [RegisterEvent]
        public static void OnReportBody(ReportBodyEvent evt)
        {
            if (!AmongUsClient.Instance.AmHost)
                return;

            RegisterNearBody();
        }

        [RegisterEvent]
        public static void OnMeetingStart(StartMeetingEvent evt)
        {
            Reset(hideMeetingButton: false);

            var player = PlayerControl.LocalPlayer;
            if (!player || player.Data.IsDead || player.Data.Disconnected || player.Data.Role is not VerifierRole)
                return;

            Coroutines.Start(CoSetupMeetingButton(evt.MeetingHud));
        }

        [RegisterEvent]
        public static void OnEndMeeting(EndMeetingEvent evt)
        {
            Reset();
        }

        [RegisterEvent]
        public static void OnMeetingSelect(MeetingSelectEvent evt)
        {
            if (PendingClaim == VerifierClaimType.None || UsedThisMeeting)
                return;

            var player = PlayerControl.LocalPlayer;
            if (!player || player.Data.Role is not VerifierRole)
                return;

            var target = Utils.PlayerById((byte)evt.TargetId);
            if (!target || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
                return;

            evt.AllowSelect = false;

            var result = GetVerificationResult(target, PendingClaim);

            UsedThisMeeting = true;
            PendingClaim = VerifierClaimType.None;

            CloseClaimMenu();
            UpdateMeetingButton();

            Coroutines.Start(CoNotifyAfterDelay(
                0.15f,
                $"<color=#58E8BE>Verifier result</color>\n{target.Data.PlayerName}: {result}"));
        }

        [MethodRpc((uint)CustomRPC.VerifierRegisterFact)]
        public static void RpcRegisterFact(PlayerControl source, byte claimType)
        {
            RegisterFact(source.PlayerId, (VerifierClaimType)claimType);
        }

        public static void RegisterFact(byte playerId, VerifierClaimType claimType)
        {
            var flag = GetFlag(claimType);

            if (flag == VerifierFactFlags.None)
                return;

            RoundFacts.TryGetValue(playerId, out var current);
            RoundFacts[playerId] = current | flag;
        }

        public static void RegisterNearBody()
        {
            float radius = OptionGroupSingleton<VerifierOptions>.Instance.NearBodyRadius;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player || player.Data == null || player.Data.IsDead || player.Data.Disconnected)
                    continue;

                var bodies = Helpers.GetNearestDeadBodies(
                    player.GetTruePosition(),
                    radius,
                    Helpers.CreateFilter(Constants.NotShipMask)
                );

                if (bodies != null && bodies.Count > 0)
                    RpcRegisterFact(player, (byte)VerifierClaimType.NearBody);
            }
        }

        public static IEnumerator CoSetupMeetingButton(MeetingHud hud)
        {
            while (hud && hud.CurrentState == MeetingHud.VoteStates.Animating)
                yield return null;

            yield return new WaitForSeconds(0.25f);

            if (!hud || PlayerControl.LocalPlayer.Data.Role is not VerifierRole)
                yield break;

            UpdateMeetingButton(hud);
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#58E8BE>Verifier:</color> use the meeting ability button to verify a claim."));
        }

        public static void UpdateMeetingButton(MeetingHud hud = null)
        {
            hud = MeetingHud.Instance;

            if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.Role is not VerifierRole)
                return;

            var button = hud.MeetingAbilityButton;

            button.Show();
            button.SetInfiniteUses();
            button.SetCoolDown(0f, 1f);

            button.graphic.sprite = NewModAsset.VerifyButton.LoadAsset();
            button.graphic.SetCooldownNormalizedUvs();

            button.OverrideText(UsedThisMeeting ? "USED" : PendingClaim != VerifierClaimType.None ? "READY" : "VERIFY");
            button.OverrideColor(UsedThisMeeting
                ? new Color32(90, 90, 90, 255)
                : PendingClaim != VerifierClaimType.None
                    ? new Color32(255, 209, 102, 255)
                    : new Color32(88, 232, 190, 255));
        }

        public static void OnMeetingAbilityClicked()
        {
            if (UsedThisMeeting)
            {
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#B7B7B7>Verifier already used this meeting.</color>"));
                return;
            }

            if (PendingClaim != VerifierClaimType.None)
            {
                PendingClaim = VerifierClaimType.None;
                UpdateMeetingButton();
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#B7B7B7>Verifier claim cancelled.</color>"));
                return;
            }

            if (ClaimMenu)
            {
                CloseClaimMenu();
                return;
            }

            OpenClaimMenu();
        }

        public static void OpenClaimMenu()
        {
            CloseClaimMenu();

            PendingClaim = VerifierClaimType.None;
            UpdateMeetingButton();

            ClaimMenu = CustomPlayerMenu.Create();
            ClaimMenu.transform.localPosition = new Vector3(0f, 0f, -50f);
            ClaimMenu.Begin(_ => false, _ =>
            {
                CloseClaimMenu();
                PendingClaim = VerifierClaimType.None;
                UpdateMeetingButton();
            });

            CreateText("VerifierTitle", "<color=#58E8BE><b>VERIFY CLAIM</b></color>", new Vector3(0f, 1.65f, -1f), 1.55f);

            CreateOptionButton(VerifierClaimType.DidTask, "Did task", new Vector3(-1.35f, 0.8f, -1f));
            CreateOptionButton(VerifierClaimType.EnteredVent, "Entered vent", new Vector3(1.35f, 0.8f, -1f));
            CreateOptionButton(VerifierClaimType.NearBody, "Near body", new Vector3(-1.35f, 0.15f, -1f));
            CreateOptionButton(VerifierClaimType.UsedAbility, "Used ability", new Vector3(1.35f, 0.15f, -1f));

            CreateToggleButton("VerifierCancel", "Cancel", new Vector3(0f, -0.65f, -1f), new Vector3(0.75f, 0.65f, 1f), (UnityAction)(() =>
            {
                CloseClaimMenu();
                PendingClaim = VerifierClaimType.None;
                UpdateMeetingButton();
            }));

            CreateText("VerifierStatus", "<color=#D8D8D8>Select a claim type.</color>", new Vector3(0f, -1.15f, -1f), 0.9f);
        }

        // TODO: Replace this system once the Verifier minigame is ready.
        public static void CreateOptionButton(VerifierClaimType claim, string label, Vector3 position)
        {
            var capturedClaim = claim;

            CreateToggleButton(
                "VerifierOption_" + claim,
                label,
                position,
                new Vector3(1.05f, 0.72f, 1f),
                (UnityAction)(() => SelectClaim(capturedClaim)));
        }

        public static void CreateToggleButton(string name, string label, Vector3 position, Vector3 scale, UnityAction onClick)
        {
            var toggle = Object.Instantiate(HudManager.Instance.GameMenu.CensorChatButton, ClaimMenu.transform);
            var button = toggle.GetComponent<PassiveButton>();
            var translator = toggle.Text.GetComponent<TextTranslatorTMP>();
            var highlight = toggle.transform.FindChild("ButtonHighlight");

            toggle.name = name;
            toggle.gameObject.SetActive(true);
            toggle.transform.localPosition = position;
            toggle.transform.localScale = scale;

            foreach (var child in toggle.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = 5;

            if (translator)
                Object.DestroyImmediate(translator);

            if (highlight)
                Object.DestroyImmediate(highlight.gameObject);

            toggle.Background.color = new Color32(12, 12, 12, 245);

            toggle.Text.text = label;
            toggle.Text.color = Color.white;
            toggle.Text.fontSize = 1.1f;
            toggle.Text.alignment = TextAlignmentOptions.Center;

            if (toggle.Rollover)
            {
                toggle.Rollover.Target = toggle.Background;
                toggle.Rollover.TargetText = toggle.Text;
                toggle.Rollover.OutColor = new Color32(12, 12, 12, 245);
                toggle.Rollover.OverColor = new Color32(38, 38, 38, 255);
                toggle.Rollover.UnselectedColor = new Color32(12, 12, 12, 245);
            }

            button.ClickMask = null;
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(onClick);

            Object.DestroyImmediate(toggle);
        }

        public static TextMeshPro CreateText(string name, string content, Vector3 position, float size)
        {
            var obj = new GameObject(name);
            obj.layer = 5;
            obj.transform.SetParent(ClaimMenu.transform, false);
            obj.transform.localPosition = position;

            var text = obj.AddComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = size;
            text.text = content;

            return text;
        }

        public static void SelectClaim(VerifierClaimType claim)
        {
            PendingClaim = claim;
            CloseClaimMenu();
            UpdateMeetingButton();

            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#58E8BE>Verifier:</color> click a meeting player."));
        }

        public static string GetVerificationResult(PlayerControl target, VerifierClaimType claim)
        {
            float unknownChance = OptionGroupSingleton<VerifierOptions>.Instance.UnknownChance;

            if (Random.Range(0f, 100f) < unknownChance)
                return "<color=#B7B7B7>Unknown</color>";

            RoundFacts.TryGetValue(target.PlayerId, out var facts);

            return (facts & GetFlag(claim)) != 0
                ? "<color=#58E8BE>Confirmed</color>"
                : "<color=#FF4D4D>Denied</color>";
        }

        public static VerifierFactFlags GetFlag(VerifierClaimType claimType)
        {
            return claimType switch
            {
                VerifierClaimType.DidTask => VerifierFactFlags.DidTask,
                VerifierClaimType.EnteredVent => VerifierFactFlags.EnteredVent,
                VerifierClaimType.NearBody => VerifierFactFlags.NearBody,
                VerifierClaimType.UsedAbility => VerifierFactFlags.UsedAbility,
                _ => VerifierFactFlags.None
            };
        }

        public static IEnumerator CoNotifyAfterDelay(float delay, string msg)
        {
            yield return new WaitForSeconds(delay);
            Coroutines.Start(CoroutinesHelper.CoNotify(msg));
        }

        public static void CloseClaimMenu()
        {
            if (ClaimMenu)
                ClaimMenu.Close();

            ClaimMenu = null;
        }

        public static void Reset(bool clearFacts = false, bool hideMeetingButton = true)
        {
            if (clearFacts)
                RoundFacts.Clear();

            UsedThisMeeting = false;
            PendingClaim = VerifierClaimType.None;
            CloseClaimMenu();

            if (hideMeetingButton && MeetingHud.Instance && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data.Role is VerifierRole)
                MeetingHud.Instance.MeetingAbilityButton.Hide();
        }
    }
}