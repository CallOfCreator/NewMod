using System;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Networking;
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
        public bool isActive = false;

        [HideFromIl2Cpp]

        // Inspired by: https://github.com/NuclearPowered/Reactor/blob/e27a79249ea706318f3c06f3dc56a5c42d65b1cf/Reactor.Debugger/Window/Tabs/GameTab.cs#L70
        public void Initialize(PlayerControl wraith, PlayerControl target)
        {
            Owner = wraith;
            Target = target;

            var prefab = AmongUsClient.Instance.PlayerPrefab;
            npc = Instantiate(prefab);
            npc.PlayerId = (byte)GameData.Instance.GetAvailableId();

            var npcData = GameData.Instance.AddDummy(npc);
            AmongUsClient.Instance.Spawn(npcData);
            AmongUsClient.Instance.Spawn(npc);

            npc.isDummy = false;
            npc.notRealPlayer = true;
            KillAnimation.SetMovement(npc, true);
            npc.NetTransform.RpcSnapTo(Owner.transform.position);

            var color = (byte)(npc.PlayerId % Palette.PlayerColors.Length);
            npc.RpcSetName("Wraith NPC");
            npc.RpcSetColor(color);
            npc.RpcSetHat("");
            npc.RpcSetSkin("");
            npc.RpcSetPet("");
            npc.RpcSetVisor("");

            npc.Collider.enabled = false;

            var noShadow = npc.gameObject.AddComponent<NoShadowBehaviour>();
            if (noShadow != null)
            {
                noShadow.rend = npc.cosmetics.currentBodySprite.BodySprite;
                noShadow.hitOverride = npc.Collider;
            }

            ownerLight = Owner.lightSource;
            ownerLight.transform.SetParent(npc.transform, false);
            ownerLight.transform.localPosition = npc.Collider.offset;
            Camera.main.GetComponent<FollowerCamera>().SetTarget(npc);

            npc.cosmetics.enabled = false;
            npc.enabled = false;

            isActive = true;

            Coroutines.Start(WalkToTarget());

            if (Target.AmOwner)
            {
                SoundManager.Instance.PlaySound(NewModAsset.HeartbeatSound.LoadAsset(), false, 1f);
            }
        }
        public void Update()
        {
            if (MeetingHud.Instance)
                Dispose();
        }
        [HideFromIl2Cpp]
        public System.Collections.IEnumerator WalkToTarget()
        {
            if (Target.Data.IsDead || Target.Data.Disconnected)
            {
                Dispose();
            }
            while (isActive && !MeetingHud.Instance)
            {
                Vector2 npcPos = npc.GetTruePosition();
                Vector2 targetPos = Target.GetTruePosition();
                Vector2 dir = (targetPos - npcPos).normalized;

                npc.MyPhysics.SetNormalizedVelocity(dir);

                float distance = Vector2.Distance(npcPos, targetPos);

                if (distance <= 0.1f)
                {
                    npc.MyPhysics.SetNormalizedVelocity(Vector2.zero);

                    Owner.RpcCustomMurder(Target, true, teleportMurderer: false);

                    if (Target.AmOwner)
                    {
                        CoroutinesHelper.CoNotify("<color=#FF4D4D><b>Oops!</b> The <i>Wraith NPC</i> got you...");
                    }
                    WraithCallerUtilities.AddKillNPC(Owner.PlayerId);

                    Dispose();
                    yield break;
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

            if (npc != null)
            {
                var cam = Camera.main.GetComponent<FollowerCamera>();
                cam.SetTarget(Owner);

                ownerLight.transform.SetParent(Owner.transform, false);
                ownerLight.transform.localPosition = Owner.Collider.offset;

                var info = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(d => d.PlayerId == npc.PlayerId);
                if (info != null) GameData.Instance.RemovePlayer(info.PlayerId);

                Destroy(npc.gameObject);
                npc = null;
            }
            Destroy(gameObject);
        }
    }
}
