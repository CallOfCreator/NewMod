using UnityEngine;
using HarmonyLib;
using Object = UnityEngine.Object;
using Reactor.Utilities.Extensions;
using MiraAPI.Utilities;
using NewMod.Components.ScreenEffects;
using Reactor.Utilities;
using NewMod.Utilities;

namespace NewMod.Patches.Birthday
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    public static class LobbyPatch
    {
        public static GameObject CustomLobby;
        public static BirthdayToast ToastObj;
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
            if (NewModDateTime.IsNewModBirthdayWeek)
            {
                __instance.SpawnPositions = new Vector2[BirthdaySpawns.Length];

                for (int i = 0; i < BirthdaySpawns.Length; i++)
                {
                    __instance.SpawnPositions[i] = BirthdaySpawns[i % BirthdaySpawns.Length];
                }
                CustomLobby = Object.Instantiate(NewModAsset.HalloweenLobby.LoadAsset());
                CustomLobby.layer = LayerMask.NameToLayer("Ship");
                CustomLobby.transform.SetParent(__instance.transform, false);
                CustomLobby.transform.localPosition = Vector3.zero;
                return false;
            }
            else if (NewModDateTime.IsHalloweenSeason)
            {
                __instance.SpawnPositions = new Vector2[BirthdaySpawns.Length];

                for (int i = 0; i < BirthdaySpawns.Length; i++)
                {
                    __instance.SpawnPositions[i] = BirthdaySpawns[i % BirthdaySpawns.Length];
                }
                CustomLobby = Object.Instantiate(NewModAsset.HalloweenLobby.LoadAsset());
                CustomLobby.layer = LayerMask.NameToLayer("Ship");
                CustomLobby.transform.SetParent(__instance.transform, false);
                CustomLobby.transform.localPosition = Vector3.zero;

                if (Helpers.CheckChance(30))
                {
                    int effectIndex = Random.Range(0, 3);
                    switch (effectIndex)
                    {
                        case 0:
                            Camera.main.gameObject.AddComponent<GlitchEffect>();
                            break;
                        case 1:
                            Camera.main.gameObject.AddComponent<EarthquakeEffect>();
                            break;
                        case 2:
                            Camera.main.gameObject.AddComponent<SlowPulseHueEffect>();
                            break;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch(nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]
        public static void Postfix(LobbyBehaviour __instance)
        {
            if (!NewModDateTime.IsNewModBirthdayWeek || !NewModDateTime.IsHalloweenSeason) return;

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
                wardrobe.transform.localPosition = new Vector3(-4.368f, -0.0027f, 0f);
                wardrobe.transform.localScale = new Vector3(0.6475f, 0.7f, 1f);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        public static void Prefix(ShipStatus __instance)
        {
            CustomLobby.DestroyImmediate();
            Coroutines.Start(CoroutinesHelper.RemoveCameraEffect(Camera.main, 0f));
        }
    }
}
