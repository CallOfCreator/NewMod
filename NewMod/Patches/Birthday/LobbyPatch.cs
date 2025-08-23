using UnityEngine;
using HarmonyLib;
using System;
using Object = UnityEngine.Object;
using Reactor.Utilities.Extensions;
using System.IO;

namespace NewMod.Patches.Birthday
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    public static class LobbyPatch
    {
        public static GameObject CustomLobby;
        public static readonly Vector2[] BirthdaySpawns =
        [
           new Vector2(-0.6738f, -2.5016f),
           new Vector2(-0.7068f, -2.4353f),
           new Vector2(-0.4568f, -2.4353f),
           new Vector2( 0.8968f, -2.2000f),
           new Vector2( 1.6468f, -1.9000f),
           new Vector2( 1.5218f, -1.9139f),
           new Vector2( 2.5000f, -1.5155f),
           new Vector2( 3.0000f, -1.5000f),
           new Vector2( 3.0000f, -1.1000f)
        ];

        [HarmonyPatch(nameof(LobbyBehaviour.Start))]
        [HarmonyPrefix]
        public static bool StartPrefix(LobbyBehaviour __instance)
        {
            if (!NewModDateTime.IsNewModBirthdayWeek) return true;

            CustomLobby = Object.Instantiate(NewModAsset.CustomLobby.LoadAsset());
            CustomLobby.transform.SetParent(__instance.transform, false);
            CustomLobby.transform.localPosition = Vector3.zero;
            return true;
        }

        [HarmonyPatch(nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]
        public static void Postfix(LobbyBehaviour __instance)
        {
            var toast = Toast.CreateToast();
            toast.transform.localPosition = new Vector3(-4.4217f, 2.2098f, 0f);

            if (DateTime.Now < NewModDateTime.NewModBirthday)
            {
                TimeSpan countdown = NewModDateTime.NewModBirthday - DateTime.Now;
                toast.StartCountdown(countdown);
            }
            else
            {
                toast.gameObject.SetActive(false);
            }

            if (!NewModDateTime.IsNewModBirthdayWeek) return;

            var originalLobby = "Lobby(Clone)";
            GameObject.Find(originalLobby).GetComponent<EdgeCollider2D>().Destroy();
            GameObject.Find(originalLobby + "/Background").SetActive(false);
            GameObject.Find(originalLobby + "/ShipRoom").SetActive(false);
            GameObject.Find(originalLobby + "/RightEngine").SetActive(false);
            GameObject.Find(originalLobby + "/LeftEngine").SetActive(false);
            GameObject.Find(originalLobby + "/SmallBox").SetActive(false);
            GameObject.Find(originalLobby + "/Leftbox").SetActive(false);
            GameObject.Find(originalLobby + "/RightBox").SetActive(false);

            var wardrobe = GameObject.Find(originalLobby + "/panel_Wardrobe");
            if (wardrobe != null)
            {
                wardrobe.transform.localPosition = new Vector3(4.6701f, -0.0529f, 0f);
                wardrobe.transform.localScale = new Vector3(0.7301f, 0.7f, 1f);
            }
            Debug.Log(__instance.SpawnPositions.Length);

            __instance.SpawnPositions = new Vector2[BirthdaySpawns.Length];
            for (int i = 0; i < BirthdaySpawns.Length; i++)
            {
                __instance.SpawnPositions[i] = BirthdaySpawns[i];
            }
        }
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        public static void Prefix(ShipStatus __instance)
        {
            CustomLobby.DestroyImmediate();
        }
    }
}
