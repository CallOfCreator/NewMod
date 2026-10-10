using System;
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
    private static IEnumerator _announcementRoutine;

    [HarmonyPatch(nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void StartPostfix()
    {
        StopAnnouncementRoutine();
        _announcementRoutine = Coroutines.Start(CoCheckAnnouncement());
    }

    [HarmonyPatch(nameof(LobbyBehaviour.OnDestroy))]
    [HarmonyPostfix]
    public static void OnDestroyPostfix()
    {
        StopAnnouncementRoutine();
    }

    private static void StopAnnouncementRoutine()
    {
        if (_announcementRoutine == null)
            return;

        Coroutines.Stop(_announcementRoutine);
        ((IDisposable)_announcementRoutine).Dispose();
        _announcementRoutine = null;
    }

    private static IEnumerator CoCheckAnnouncement()
    {
        var url = NewMod.NewModBackendAPI + "/api/v1/get-announcement";
        while (LobbyBehaviour.Instance)
        {
            var req = UnityWebRequest.Get(url);
            try
            {
                yield return req.SendWebRequest();

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
                            lastContent = content;
                            ShowPopup("New Lobby Message!", content);
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

        popup.transform.Find("HeaderText").GetComponent<TextTranslatorTMP>().Destroy();
        popup.transform.Find("BodyText").GetComponent<TextTranslatorTMP>().Destroy();

        var headerText = popup.transform.Find("HeaderText").GetComponent<TextMeshPro>();
        headerText.text = title;

        var bodyText = popup.transform.Find("BodyText").GetComponent<TextMeshPro>();
        bodyText.text = content;

        popup.gameObject.SetActive(true);

        var nav = popup.GetComponent<ControllerNavMenu>();
        nav.OpenMenu(true);
    }
}

public class AnnouncementResponse
{
    public string content { get; set; }
}
