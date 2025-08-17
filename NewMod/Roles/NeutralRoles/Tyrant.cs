using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using NewMod.Colors;
using NewMod.Components;
using NewMod.Options.Roles.TyrantOptions;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles
{
    public sealed class Tyrant : ImpostorRole, INewModRole
    {
        public string RoleName => "Tyrant";
        public string RoleDescription => "Slow them. Bind them. End them";
        public string RoleLongDescription =>
            "You are the Tyrant. Each kill strengthens your control over the ship:\n";
        public Color RoleColor => new(0.78f, 0.10f, 0.16f, 1f);
        public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
        public RoleOptionsGroup RoleOptionsGroup { get; } = RoleOptionsGroup.Neutral;
        public NewModFaction Faction = NewModFaction.Apex;
        public CustomRoleConfiguration Configuration => new(this)
        {
            MaxRoleCount = 1,
            OptionsScreenshot = MiraAssets.Empty,
            Icon = NewModAsset.CrownIcon,
            CanGetKilled = true,
            UseVanillaKillButton = true,
            CanUseVent = true,
            TasksCountForProgress = false,
            CanUseSabotage = true,
            DefaultChance = 25,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            GhostRole = AmongUs.GameOptions.RoleTypes.Crewmate,
            RoleHintType = RoleHintType.RoleTab
        };
        public TeamIntroConfiguration TeamConfiguration => new()
        {
            IntroTeamDescription = RoleDescription,
            IntroTeamColor = RoleColor
        };

        [HideFromIl2Cpp]
        public StringBuilder SetTabText()
        {
            var tabText = INewModRole.GetRoleTabText(this);
            var green = Palette.AcceptedGreen.ToHtmlStringRGBA();
            int kills = GetKillCount();

            string firstKill = "* 1st Kill — Fear Pulse: nearby foes suffer reduced vision and speed for a short time.\n";
            string secondKill = "* 2nd Kill — Zone of Suppression: a dome that disables buttons for those inside.\n";
            string thirdKill = "* 3rd Kill — Intimidation Protocol: the next witness is frozen briefly.\n";
            string fourthKill = "* 4th Kill — Apex Throne: designate a Champion who cannot oppose you.\n";

            void AppendAbilityLine(int index, string text)
            {
                if (kills > index)
                {
                    tabText.AppendLine($"<size=70%><color=#{green}><b><s>{text}</s></b></color></size>");
                }
                else if (kills == index)
                {
                    tabText.AppendLine($"<size=70%><color=#{green}><b><s>{text}</s></b></color></size>");
                    tabText.AppendLine($"<size=64%><color=#{green}>✓ Unlocked</color></size>");
                }
                else if (index == kills + 1)
                {
                    tabText.AppendLine($"<size=72%><b><color=#FFD166>{text}</color></b></size>");
                }
                else
                {
                    tabText.AppendLine($"<size=70%><color=#B7B7B7>{text}</size></color>");
                }
            }
            AppendAbilityLine(1, firstKill);
            AppendAbilityLine(2, secondKill);
            AppendAbilityLine(3, thirdKill);
            AppendAbilityLine(4, fourthKill);

            return tabText;
        }
        public int _kills;
        public static Material _circleMat;
        public static byte _championId;
        public static bool ApexThroneReady;
        public static bool ApexThroneOutcomeSet;
        public enum ThroneOutcome { None, ChampionSideWin }
        public static ThroneOutcome Outcome = ThroneOutcome.None;
        public static readonly HashSet<byte> PendingBetrayals = new();
        public int GetKillCount() => _kills;
        public byte GetChampion() => _championId;
        public void SetChampion(byte playerId) => _championId = playerId;
        public static void ClearChampion() => _championId = byte.MaxValue;
        public static Material GetCircleMat()
        {
            if (_circleMat) return _circleMat;
            _circleMat = new(Shader.Find("Sprites/Default"))
            {
                renderQueue = 3000
            };
            return _circleMat;
        }
        public static GameObject CreateCircle(Vector3 pos, float radius, Color color, float duration, int segments = 64)
        {
            var go = new GameObject("Tyrant_Circle");
            go.transform.position = pos;

            HudManager.Instance.StartCoroutine(Effects.ScaleIn(go.transform, 0f, 1f, 0.5f));

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();

            var mat = new Material(GetCircleMat()) { color = color };
            mr.sharedMaterial = mat;

            float visualRadius = radius;

            segments = Mathf.Max(12, segments);
            var verts = new Vector3[segments + 1];
            var tris = new int[segments * 3];

            verts[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                verts[i + 1] = new Vector3(Mathf.Cos(a) * visualRadius, Mathf.Sin(a) * visualRadius, 0f);
                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = (i == segments - 1) ? 1 : (i + 2);
            }

            var mesh = new Mesh { name = "FearPulseFill" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0, true);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mf.sharedMesh = mesh;

            Coroutines.Start(CoroutinesHelper.DespawnCircle(go, duration));
            return go;
        }

        [RegisterEvent]
        public static void OnAfterMurderEvent(AfterMurderEvent evt)
        {
            var tyrant = evt.Source.Data.Role as Tyrant;

            tyrant._kills++;

            if (tyrant.GetKillCount() == 1)
            {
                RpcSpawnFearPulse(evt.Source, evt.Source.GetTruePosition().x, evt.Source.GetTruePosition().y);
            }
            else if (tyrant.GetKillCount() == 2)
            {
                RpcSpawnSuppressionDome(evt.Source, evt.Source.GetTruePosition().x, evt.Source.GetTruePosition().y);
            }
            else if (tyrant.GetKillCount() == 3)
            {
                RpcArmWitnessTrap(evt.Source, evt.Source.GetTruePosition().x, evt.Source.GetTruePosition().y);
            }
            else
            {
                ApexThroneReady = true;
                ApexThroneOutcomeSet = false;

                var menu = CustomPlayerMenu.Create();
                menu.Begin(
                    player => !player.Data.IsDead &&
                              !player.Data.Disconnected &&
                               player.PlayerId != PlayerControl.LocalPlayer.PlayerId,
                    player =>
                    {
                        tyrant.SetChampion(player.PlayerId);
                        menu.Close();

                        if (tyrant.Player.AmOwner)
                            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#9CCC65>Apex Throne is armed. You have chosen a Champion.</color>"));

                        RpcNotifyChampion(tyrant.Player, player);

                    });
            }
        }
        [RegisterEvent]
        public static void OnMeetingStart(StartMeetingEvent evt)
        {
            if (_championId == byte.MaxValue) return;
            if (PlayerControl.LocalPlayer.PlayerId != _championId) return;

            var tyrantPlayer = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.Data.Role is Tyrant);
            if (!tyrantPlayer) return;

            foreach (var ps in evt.MeetingHud.playerStates)
            {
                if (ps.TargetPlayerId == tyrantPlayer.PlayerId)
                {
                    ps.NameText.text += "\n<color=#C62828><size=60%>Tyrant</size></color>";
                    break;
                }
            }
        }
        [RegisterEvent]
        public static void OnHandleVote(HandleVoteEvent evt)
        {
            var voter = evt.VoteData.Owner;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role is not Tyrant tyrant) continue;
                if (voter.PlayerId != _championId) continue;

                bool betrays = evt.TargetId == tyrant.Player.PlayerId;

                if (betrays)
                {
                    if (evt.VoteData.VotedFor(evt.TargetId))
                        evt.VoteData.RemovePlayerVote(evt.TargetId);

                    evt.VoteData.VoteForPlayer(evt.VoteData.Owner.PlayerId);
                    evt.VoteData.SetRemainingVotes(0);

                    PendingBetrayals.Add(voter.PlayerId);
                    ApexThroneOutcomeSet = true;
                    Outcome = ThroneOutcome.None;
                }
                else
                {
                    ApexThroneOutcomeSet = true;
                    Outcome = ThroneOutcome.ChampionSideWin;
                }
                if (voter.AmOwner)
                {
                    var msg = (Outcome == ThroneOutcome.ChampionSideWin)
                        ? "<color=#64B5F6>You submitted to the Tyrant’s will.</color>"
                        : "<color=red>Betrayal detected. You will be punished.</color>";
                    Coroutines.Start(CoroutinesHelper.CoNotify(msg));
                }
                break;
            }
        }
        [RegisterEvent]
        public static void OnProcessVotes(ProcessVotesEvent evt)
        {
            if (PendingBetrayals.Count == 0) return;

            var first = default(byte);
            foreach (var id in PendingBetrayals) { first = id; break; }
            PendingBetrayals.Clear();

            var info = GameData.Instance.GetPlayerById(first);

            if (info != null)
            {
                evt.ExiledPlayer = info;
            }
        }
        [RegisterEvent]
        public static void OnGameEnd(GameEndEvent evt)
        {
            ApexThroneReady = false;
            ApexThroneOutcomeSet = false;
            Outcome = ThroneOutcome.None;
            ClearChampion();
        }
        public void SpawnSuppressionDome(Vector3 pos)
        {
            var go = new GameObject("Supression_Dome");
            go.transform.position = pos;

            var area = go.AddComponent<SuppressionDomeArea>();
            area.Init(Player.PlayerId, radius: OptionGroupSingleton<TyrantOptions>.Instance.DomeRadius, OptionGroupSingleton<TyrantOptions>.Instance.DomeDuration);

            if (Player.AmOwner)
                CreateCircle(Player.GetTruePosition(), OptionGroupSingleton<TyrantOptions>.Instance.DomeRadius, Palette.AcceptedGreen, OptionGroupSingleton<TyrantOptions>.Instance.DomeDuration);
        }
        public void ArmWitnessTrap(Vector3 pos)
        {
            var go = new GameObject("WitnessTrap");
            go.transform.position = pos;

            var trap = go.AddComponent<WitnessTrapArea>();
            trap.Init(
                ownerId: Player.PlayerId,
                radius: OptionGroupSingleton<TyrantOptions>.Instance.WitnessRange,
                freeze: OptionGroupSingleton<TyrantOptions>.Instance.WitnessFreezeDuration,
                duration: OptionGroupSingleton<TyrantOptions>.Instance.WitnessArmWindow
            );

            if (Player.AmOwner)
                CreateCircle(Player.GetTruePosition(), OptionGroupSingleton<TyrantOptions>.Instance.WitnessRange, Color.cyan, OptionGroupSingleton<TyrantOptions>.Instance.WitnessArmWindow);
        }
        public void SpawnFearPulse(Vector3 pos)
        {
            var go = new GameObject("FearPulseArea");
            go.transform.position = pos;

            var area = go.AddComponent<FearPulseArea>();
            area.Init(
                ownerId: Player.PlayerId,
                radius: OptionGroupSingleton<TyrantOptions>.Instance.FearPulseRadius,
                duration: OptionGroupSingleton<TyrantOptions>.Instance.FearPulseDuration,
                speedMul: OptionGroupSingleton<TyrantOptions>.Instance.FearPulseSpeed
            );

            if (Player.AmOwner)
                CreateCircle(Player.GetTruePosition(), OptionGroupSingleton<TyrantOptions>.Instance.FearPulseRadius, new Color(1f, 0.35f, 0.2f, 0.6f), OptionGroupSingleton<TyrantOptions>.Instance.FearPulseDuration);
        }
        [MethodRpc((uint)CustomRPC.NotifyChampion)]
        public static void RpcNotifyChampion(PlayerControl source, PlayerControl target)
        {
            if (target.AmOwner)
            {
                Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FFD54F>{source.Data.PlayerName}</color> is your <color=#C62828>Tyrant</color>. Obey or be exiled."));
            }
        }
        [MethodRpc((uint)CustomRPC.FearPulse)]
        public static void RpcSpawnFearPulse(PlayerControl source, float x, float y)
        {
            var tyrant = source.Data.Role as Tyrant;

            tyrant.SpawnFearPulse(new Vector2(x, y));
        }
        [MethodRpc((uint)CustomRPC.SuppressionDome)]
        public static void RpcSpawnSuppressionDome(PlayerControl source, float x, float y)
        {
            var tyrant = source.Data.Role as Tyrant;
            tyrant.SpawnSuppressionDome(new Vector2(x, y));
        }
        [MethodRpc((uint)CustomRPC.WitnessTrap)]
        public static void RpcArmWitnessTrap(PlayerControl source, float x, float y)
        {
            var tyrant = source.Data.Role as Tyrant;
            tyrant.ArmWitnessTrap(new Vector2(x, y));
        }
    }
}
