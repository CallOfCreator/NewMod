/*using System;
using System.Collections;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using NewMod.Features;
using NewMod.Options;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components
{
    [RegisterInIl2Cpp]
    public class GeneralNPC(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public PlayerControl Owner { get; set; }
        public PlayerControl npc;
        public bool isActive = false;

        [HideFromIl2Cpp]
        public void Initialize(PlayerControl owner)
        {
            Owner = owner;

            var prefab = AmongUsClient.Instance.PlayerPrefab;
            npc = Instantiate(prefab);
            npc.PlayerId = (byte)GameData.Instance.GetAvailableId();

            var npcData = GameData.Instance.AddDummy(npc);
            AmongUsClient.Instance.Spawn(npcData);
            AmongUsClient.Instance.Spawn(npc);

            npc.notRealPlayer = true;
            KillAnimation.SetMovement(npc, true);
            npc.NetTransform.RpcSnapTo(Owner.GetTruePosition());
            npc.MyPhysics.Speed = OptionGroupSingleton<GeneralNpcOptions>.Instance.GeneralNPCSpeed.Value;

            npc.RpcSetName("General NPC");
            npc.RpcSetColor((byte)(npc.PlayerId % Palette.PlayerColors.Length));
            npc.RpcSetHat("");
            npc.RpcSetSkin("");
            npc.RpcSetPet("");
            npc.RpcSetVisor("");

            if (!npc.TryGetComponent<ModifierComponent>(out var mc))
            {
                mc = npc.gameObject.AddComponent<ModifierComponent>();
            }

            npc.Collider.enabled = true;
            npc.cosmetics.enabled = false;
            npc.enabled = false;

            isActive = true;
            Coroutines.Start(WalkStopLoop());
        }
    
        [HideFromIl2Cpp]
        public IEnumerator WalkStopLoop()
        {
            var runTime = OptionGroupSingleton<GeneralNpcOptions>.Instance.GeneralNPCRunTime.Value;
            var stopTime = OptionGroupSingleton<GeneralNpcOptions>.Instance.GeneralNPCStopTime.Value;
            while (isActive && !MeetingHud.Instance)
            {
                var pos = npc.GetTruePosition();
                var current = RoomPathfinding.GetCurrentRoom(pos);
                NewMod.Instance.Log.LogMessage($"[NPC] pos={pos} room={(current ? current.name : "none")}");

                var target = RoomPathfinding.PickRandomOtherRoom(current);
                if (!target)
                {
                    NewMod.Instance.Log.LogWarning("[NPC] no target room, waiting");
                    npc.MyPhysics.SetNormalizedVelocity(Vector2.zero);
                    yield return new WaitForSeconds(stopTime);
                    continue;
                }

                var path = RoomPathfinding.FindRoomPath(current, target);
                NewMod.Instance.Log.LogMessage($"[NPC] target={target.name} pathLen={path?.Count ?? 0}");

                if (path == null || path.Count == 0)
                {
                    npc.MyPhysics.SetNormalizedVelocity(Vector2.zero);
                    yield return new WaitForSeconds(stopTime);
                    continue;
                }

                foreach (var room in path)
                {
                    if (!isActive || MeetingHud.Instance) break;

                    if (!RoomPathfinding.TryPickWaypointInside(room.roomArea, out var wp))
                        wp = (Vector2)room.roomArea.bounds.center;

                    NewMod.Instance.Log.LogMessage($"[NPC] moving to room={room.name} wp={wp}");

                    float timer = 0f;
                    while (isActive && !MeetingHud.Instance &&
                           Vector2.Distance(npc.GetTruePosition(), wp) > 0.05f)
                    {
                        var dir = (wp - npc.GetTruePosition()).normalized;
                        npc.MyPhysics.SetNormalizedVelocity(dir);

                        timer += Time.fixedDeltaTime;
                        if (timer >= runTime)
                        {
                            npc.MyPhysics.SetNormalizedVelocity(Vector2.zero);
                            NewMod.Instance.Log.LogMessage($"[NPC] burst stop {stopTime}s");
                            yield return new WaitForSeconds(stopTime);
                            timer = 0f;
                        }

                        yield return new WaitForFixedUpdate();
                    }

                    npc.MyPhysics.SetNormalizedVelocity(Vector2.zero);
                    NewMod.Instance.Log.LogMessage($"[NPC] arrived {room.name}, visiting {stopTime}s");
                    yield return new WaitForSeconds(stopTime);
                }
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
                var info = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(d => d.PlayerId == npc.PlayerId);
                if (info != null) GameData.Instance.RemovePlayer(info.PlayerId);
                Destroy(npc.gameObject);
                npc = null;
            }
            Destroy(gameObject);
        }
    }
}*/
