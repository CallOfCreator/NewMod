using System.Collections;
using System.Text.Json;
using HarmonyLib;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace NewMod.Patches;

[HarmonyPatch(typeof(LobbyBehaviour))]
public static class LobbyPatch
{
    public static string lastContent;

    [HarmonyPatch(nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(LobbyBehaviour __instance)
    {
        Coroutines.Start(CoCheckAnnouncement(__instance));
    }

    private static IEnumerator CoCheckAnnouncement(LobbyBehaviour lobby)
    {
        while (lobby && !HudManager.Instance)
            yield return null;

        var url = NewMod.NewModBackendAPI + "/api/v1/get-announcement";
        while (lobby)
        {
            var req = UnityWebRequest.Get(url);
            try
            {
                req.SendWebRequest();
                while (lobby && !req.isDone)
                    yield return null;

                if (!lobby || !HudManager.Instance)
                {
                    req.Abort();
                    yield break;
                }

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var body = req.downloadHandler.text;

                    if (!string.IsNullOrEmpty(body))
                    {
                        AnnouncementResponse res = null;
                        try
                        {
                            res = JsonSerializer.Deserialize<AnnouncementResponse>(body);
                        }
                        catch (JsonException exception)
                        {
                            Warning($"Invalid lobby announcement: {exception.Message}");
                        }

                        var content = res?.content;
                        if (!string.IsNullOrEmpty(content) && content != lastContent)
                        {
                            ShowPopup("New Lobby Message!", content);
                            lastContent = content;
                        }
                    }
                }
            }
            finally
            {
                req.Dispose();
            }

            yield return new WaitForSeconds(8f);
        }
    }

    public static void ShowPopup(string title, string content)
    {
        var banMenu = HudManager.Instance.GetComponentInChildren<BanMenu>(true);
        var template = banMenu.ReportReason.ConfirmScreen;
        var popup = Object.Instantiate(template, HudManager.Instance.transform);

        var header = popup.transform.Find("HeaderText");
        var body = popup.transform.Find("BodyText");
        header.GetComponent<TextTranslatorTMP>().Destroy();
        body.GetComponent<TextTranslatorTMP>().Destroy();
        header.GetComponent<TextMeshPro>().text = title;
        body.GetComponent<TextMeshPro>().text = content;

        popup.gameObject.SetActive(true);

        var nav = popup.GetComponent<ControllerNavMenu>();
        nav.OpenMenu(true);
    }
}

public class AnnouncementResponse
{
    public string content { get; set; }
}
