using System;
using System.Collections.Generic;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewMod.UI;
// TODO: Change this menu into a custom minigame
public class NewBotMenu : CustomMultiSelectMenu<NewBotMenu.Choice>
{
    public Choice Selected;

    public sealed record Choice(string Text, Color Color, RoleBehaviour Role = null);

    public static NewBotMenu Open(string title, IEnumerable<Choice> choices)
    {
        var menu = Create<NewBotMenu>(
            hoverSelectSprite: MiraAssets.Empty,
            onMouseOver: (highlight, icon, _) =>
            {
                highlight.color = new Color(0.75f, 0.55f, 1f);
                icon.gameObject.SetActive(false);
            },
            onMouseOut: (highlight, icon, _) =>
            {
                highlight.color = Color.clear;
                icon.gameObject.SetActive(false);
            });
        menu.Begin(
            choices,
            (Choice choice) =>
            {
                menu.Selected = choice;
                menu.ForceClose();
            },
            false);
        var heading = Helpers.CreateTextLabel("NewBotHeading", menu.transform, AspectPosition.EdgeAlignments.Center, Vector3.zero, 2.2f);
        Object.Destroy(heading.GetComponent<AspectPosition>());
        heading.transform.localPosition = new Vector3(0f, 2.15f, -2f);
        heading.rectTransform.sizeDelta = new Vector2(6f, 0.5f);
        heading.alignment = TextAlignmentOptions.Center;
        heading.enableAutoSizing = true;
        heading.fontSizeMin = 1.4f;
        heading.fontSizeMax = 2.2f;
        heading.text = title;
        return menu;
    }

    protected override void SetupPanelEntry(ShapeshifterPanel panel, int i, Choice entry, Action onClick)
    {
        panel.Background.sprite = MiraAssets.RoundedBox.LoadAsset();
        panel.Background.sharedMaterial = HudManager.Instance.FullScreen.sharedMaterial;
        panel.Background.drawMode = SpriteDrawMode.Sliced;
        panel.Background.size = new Vector2(1.8f, 0.48f);
        panel.Background.transform.localScale = Vector3.one;
        panel.Background.color = new Color(0.17f, 0.19f, 0.23f);
        panel.Background.enabled = true;
        var highlight = panel.transform.Find("Nameplate/Highlight").GetComponent<SpriteRenderer>();
        highlight.transform.GetChild(0).gameObject.SetActive(false);
        foreach (var renderer in panel.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer != panel.Background && renderer != highlight)
                renderer.gameObject.SetActive(false);
        }

        foreach (var label in panel.GetComponentsInChildren<TextMeshPro>(true))
        {
            if (label != panel.NameText)
                label.gameObject.SetActive(false);
        }

        panel.PlayerIcon.gameObject.SetActive(false);
        panel.NameText.text = entry.Text;
        panel.NameText.color = entry.Color;
        panel.NameText.alignment = TextAlignmentOptions.Center;
        panel.NameText.enableAutoSizing = true;
        panel.NameText.fontSizeMin = 1.2f;
        panel.NameText.fontSizeMax = 2.2f;
        panel.NameText.transform.localPosition = new Vector3(0f, 0f, -1f);
        panel.NameText.rectTransform.sizeDelta = new Vector2(1.65f, 0.5f);
        panel.Button.OnClick = new Button.ButtonClickedEvent();
        panel.Button.OnClick.AddListener((UnityAction)onClick);
    }
}
