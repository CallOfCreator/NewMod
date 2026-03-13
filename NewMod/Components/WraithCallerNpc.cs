using System;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class WraithCallerNpc(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public PlayerControl Owner { get; set; }
        public PlayerControl Target { get; set; }
        public PlayerControl npc;
        public LightSource ownerLight;
        public bool isActive;

        [HideFromIl2Cpp]
        // Inspired by: https://github.com/NuclearPowered/Reactor/blob/e27a79249ea706318f3c06f3dc56a5c42d65b1cf/Reactor.Debugger/Window/Tabs/GameTab.cs#L70
        public void Initialize(PlayerControl wraith, PlayerControl target, PlayerControl spawned)
        {
            Owner = wraith;
            Target = target;
            npc = spawned;

            isActive = true;

            KillAnimation.SetMovement(npc, true);

            npc.Collider.enabled = false;
            npc.MyPhysics.Speed = OptionGroupSingleton<WraithCallerOptions>.Instance.NPCSpeed;

            if (!npc.TryGetComponent<ModifierComponent>(out _))
                npc.gameObject.AddComponent<ModifierComponent>();

            if (AmongUsClient.Instance.AmHost)
            {
                NewMod.Instance.Log.LogMessage($"Host is setting cosmetics for NPC (ID: {npc.PlayerId}");
                npc.NetTransform.RpcSnapTo(Owner.transform.position);

                var color = (byte)(npc.PlayerId % Palette.PlayerColors.Length);
                npc.RpcSetName("Wraith NPC");
                npc.RpcSetColor(color);

                var noShadow = npc.gameObject.AddComponent<NoShadowBehaviour>();
                if (noShadow != null)
                {
                    noShadow.rend = npc.cosmetics.currentBodySprite.BodySprite;
                    noShadow.hitOverride = npc.Collider;
                }
            }

            Coroutines.Start(WalkToTarget());

            if (OptionGroupSingleton<WraithCallerOptions>.Instance.ShouldSwitchCamToNPC)
            {
                Camera.main.GetComponent<FollowerCamera>().SetTarget(npc);
                ownerLight = Owner.lightSource;
                ownerLight.transform.SetParent(npc.transform, false);
                ownerLight.transform.localPosition = npc.Collider.offset;
            }

            npc.cosmetics.enabled = true;
            npc.enabled = false;

            if (Target.AmOwner)
                SoundManager.Instance.PlaySound(NewModAsset.HeartbeatSound.LoadAsset(), false, 1f);
        }

        [HideFromIl2Cpp]
        private System.Collections.IEnumerator WalkToTarget()
        {
            //yield return null;

            if (!AmongUsClient.Instance.AmHost) yield break;

            while (isActive && !MeetingHud.Instance)
            {
                if (!Target || !Target.Data || Target.Data.IsDead)
                    break;

                Vector2 npcPos = npc.GetTruePosition();
                Vector2 targetPos = Target.GetTruePosition();
                Vector2 dir = (targetPos - npcPos).normalized;

                npc.MyPhysics.SetNormalizedVelocity(dir);

                if (Vector2.Distance(npcPos, targetPos) <= 0.1f)
                {
                    npc.MyPhysics.SetNormalizedVelocity(Vector2.zero);

                    Owner.RpcCustomMurder(Target, true, teleportMurderer: false);

                    if (Target.AmOwner)
                        CoroutinesHelper.CoNotify("<color=#FF4D4D><b>Oops!</b> The <i>Wraith NPC</i> got you...");

                    WraithCallerUtilities.AddKillNPC(Owner.PlayerId);
                    break;
                }
                yield return new WaitForFixedUpdate();
            }

            npc.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            Dispose();
        }

        [HideFromIl2Cpp]
        public void Dispose()
        {
            if (!isActive) return;
            isActive = false;

            if (OptionGroupSingleton<WraithCallerOptions>.Instance.ShouldSwitchCamToNPC)
            {
                Camera.main.GetComponent<FollowerCamera>().SetTarget(Owner);
                ownerLight.transform.SetParent(Owner.transform, false);
                ownerLight.transform.localPosition = Owner.Collider.offset;
            }

            if (AmongUsClient.Instance.AmHost)
            {
                var info = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(d => d.PlayerId == npc.PlayerId);
                GameData.Instance.RemovePlayer(info.PlayerId);
                PlayerControl.AllPlayerControls.Remove(npc);

                npc.Despawn();
                Destroy(npc.gameObject);
                npc = null;
            }

            Destroy(gameObject);
        }
    }
}