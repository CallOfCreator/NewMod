using System;
using System.Collections;
using System.Linq;
using AmongUs.Data;
using CorsacCosmetics.Cosmetics;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.Utilities.Assets;
using NewMod.Achievements;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewMod.Patches;

[HarmonyPatch]
public static class NewModCosmeticTabsPatch
{
    private static int HatPage;
    private static int VisorPage;
    private static int NameplatePage;

    [HarmonyPrepare]
    public static bool Prepare()
    {
        return NewMod.CorsacCosmeticsEnabled;
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
    [HarmonyPrefix]
    public static bool HatsOnEnable(HatsTab __instance)
    {
        __instance.PlayerPreview.gameObject.SetActive(true);

        if (PlayerControl.LocalPlayer)
            __instance.PlayerPreview.UpdateFromLocalPlayer(PlayerMaterial.MaskType.None);
        else
            __instance.PlayerPreview.UpdateFromDataManager(PlayerMaterial.MaskType.None);

        SetupButtons(__instance, () =>
        {
            HatPage--;
            if (HatPage < 0) HatPage = CosmeticsLoader.Instance.HatGroups.Count;
            GenerateHats(__instance);
        }, () =>
        {
            HatPage++;
            if (HatPage > CosmeticsLoader.Instance.HatGroups.Count) HatPage = 0;
            GenerateHats(__instance);
        });

        GenerateHats(__instance);
        return false;
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.ClickEquip))]
    [HarmonyPrefix]
    public static bool HatsClickEquip(HatsTab __instance)
    {
        return __instance.GetCurrentProdID() != Names.Normalize("og_newmod", "hat", "newmod") || PreseasonAchievementsTab.ThreeInARow.Unlocked;
    }

    [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
    [HarmonyPrefix]
    public static bool VisorsOnEnable(VisorsTab __instance)
    {
        __instance.PlayerPreview.gameObject.SetActive(true);

        if (PlayerControl.LocalPlayer)
            __instance.PlayerPreview.UpdateFromLocalPlayer(PlayerMaterial.MaskType.None);
        else
            __instance.PlayerPreview.UpdateFromDataManager(PlayerMaterial.MaskType.None);

        SetupButtons(__instance, () =>
        {
            VisorPage--;
            if (VisorPage < 0) VisorPage = CosmeticsLoader.Instance.VisorGroups.Count;
            GenerateVisors(__instance);
        }, () =>
        {
            VisorPage++;
            if (VisorPage > CosmeticsLoader.Instance.VisorGroups.Count) VisorPage = 0;
            GenerateVisors(__instance);
        });

        GenerateVisors(__instance);
        return false;
    }

    [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.OnEnable))]
    [HarmonyPrefix]
    public static bool NameplatesOnEnable(NameplatesTab __instance)
    {
        __instance.PlayerPreview.gameObject.SetActive(false);
        Coroutines.Start(CoLoadNameplatePreview(__instance));

        SetupButtons(__instance, () =>
        {
            NameplatePage--;
            if (NameplatePage < 0) NameplatePage = CosmeticsLoader.Instance.NameplateGroups.Count;
            GenerateNameplates(__instance);
        }, () =>
        {
            NameplatePage++;
            if (NameplatePage > CosmeticsLoader.Instance.NameplateGroups.Count) NameplatePage = 0;
            GenerateNameplates(__instance);
        });

        GenerateNameplates(__instance);
        return false;
    }

    [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.Update))]
    [HarmonyPrefix]
    public static void NameplatesUpdate(NameplatesTab __instance)
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NameplatePage--;
            if (NameplatePage < 0) NameplatePage = CosmeticsLoader.Instance.NameplateGroups.Count;
            GenerateNameplates(__instance);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NameplatePage++;
            if (NameplatePage > CosmeticsLoader.Instance.NameplateGroups.Count) NameplatePage = 0;
            GenerateNameplates(__instance);
        }
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
    [HarmonyPrefix]
    public static void HatsUpdate(HatsTab __instance)
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            HatPage--;
            if (HatPage < 0) HatPage = CosmeticsLoader.Instance.HatGroups.Count;
            GenerateHats(__instance);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            HatPage++;
            if (HatPage > CosmeticsLoader.Instance.HatGroups.Count) HatPage = 0;
            GenerateHats(__instance);
        }
    }

    [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.Update))]
    [HarmonyPrefix]
    public static void VisorsUpdate(VisorsTab __instance)
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            VisorPage--;
            if (VisorPage < 0) VisorPage = CosmeticsLoader.Instance.VisorGroups.Count;
            GenerateVisors(__instance);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            VisorPage++;
            if (VisorPage > CosmeticsLoader.Instance.VisorGroups.Count) VisorPage = 0;
            GenerateVisors(__instance);
        }
    }

    public static void GenerateHats(HatsTab tab)
    {
        foreach (var chip in tab.ColorChips)
            Object.Destroy(chip.gameObject);

        tab.ColorChips.Clear();
        tab.scroller.ScrollToTop();

        var title = tab.transform.FindChild("Text").GetComponent<TextMeshPro>();
        title.GetComponent<TextTranslatorTMP>()?.DestroyImmediate();

        var pageCount = CosmeticsLoader.Instance.HatGroups.Count + 1;
        title.text = HatPage == 0 ? $"Hats ({HatPage + 1}/{pageCount})" : $"{CosmeticsLoader.Instance.HatGroups.GetGroupNameByIndex(HatPage - 1)} ({HatPage + 1}/{pageCount})";
        var ogNewModHatId = Names.Normalize("og_newmod", "hat", "newmod");

        var hats = HatManager.Instance.GetUnlockedHats().Where(h =>
        {
            if (HatPage == 0) return !h.ProductId.StartsWith("corsac");
            if (!h.ProductId.StartsWith("corsac")) return false;

            var group = Names.GetGroup(h.ProductId);
            return group == CosmeticsLoader.Instance.HatGroups.GetGroupIdByIndex(HatPage - 1);
        }).ToArray();

        tab.currentHat = HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat);

        for (var i = 0; i < hats.Length; i++)
        {
            var hat = hats[i];
            var x = tab.XRange.Lerp(i % tab.NumPerRow / (tab.NumPerRow - 1f));
            var y = tab.YStart - i / tab.NumPerRow * tab.YOffset;

            var chip = Object.Instantiate(tab.ColorTabPrefab, tab.scroller.Inner);
            chip.gameObject.name = hat.ProductId;
            chip.transform.localPosition = new Vector3(x, y, -1f);

            if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
            {
                chip.Button.OnMouseOver.AddListener((UnityAction)(() => tab.SelectHat(hat)));
                chip.Button.OnMouseOut.AddListener((UnityAction)(() => tab.SelectHat(HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat))));
                chip.Button.OnClick.AddListener((UnityAction)tab.ClickEquip);
            }
            else
            {
                chip.Button.OnClick.AddListener((UnityAction)(() => tab.SelectHat(hat)));
            }

            chip.Button.ClickMask = tab.scroller.Hitbox;
            chip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
            tab.UpdateMaterials(chip.Inner.FrontLayer, hat);
            hat.SetPreview(chip.Inner.FrontLayer, tab.CurrentColorId());
            chip.Tag = hat;
            chip.SelectionHighlight.gameObject.SetActive(false);
            tab.ColorChips.Add(chip);

            if (!HatManager.Instance.CheckLongModeValidCosmetic(hat.ProdId, tab.PlayerPreview.GetIgnoreLongMode()) || (hat.ProductId == ogNewModHatId && !PreseasonAchievementsTab.ThreeInARow.Unlocked)) chip.SetUnavailable();
            if (!HatManager.Instance.CheckLongModeValidCosmetic(hat.ProdId, tab.PlayerPreview.GetIgnoreLongMode()) || (hat.ProductId == ogNewModHatId && !PreseasonAchievementsTab.ThreeInARow.Unlocked)) chip.SetUnavailable();
        }

        tab.currentHatIsEquipped = true;
        tab.SetScrollerBounds();
    }

    public static void GenerateVisors(VisorsTab tab)
    {
        foreach (var chip in tab.ColorChips)
            Object.Destroy(chip.gameObject);

        tab.ColorChips.Clear();
        tab.scroller.ScrollToTop();

        var title = tab.transform.FindChild("Text").GetComponent<TextMeshPro>();
        title.GetComponent<TextTranslatorTMP>()?.DestroyImmediate();

        var pageCount = CosmeticsLoader.Instance.VisorGroups.Count + 1;
        title.text = VisorPage == 0 ? $"Visors ({VisorPage + 1}/{pageCount})" : $"{CosmeticsLoader.Instance.VisorGroups.GetGroupNameByIndex(VisorPage - 1)} ({VisorPage + 1}/{pageCount})";

        var visors = HatManager.Instance.GetUnlockedVisors().Where(v =>
        {
            if (VisorPage == 0) return !v.ProductId.StartsWith("corsac");
            if (!v.ProductId.StartsWith("corsac")) return false;

            var group = Names.GetGroup(v.ProductId);
            return group == CosmeticsLoader.Instance.VisorGroups.GetGroupIdByIndex(VisorPage - 1);
        }).ToArray();

        tab.visorId = DataManager.Player.Customization.Visor;

        for (var i = 0; i < visors.Length; i++)
        {
            var visor = visors[i];
            var x = tab.XRange.Lerp(i % tab.NumPerRow / (tab.NumPerRow - 1f));
            var y = tab.YStart - i / tab.NumPerRow * tab.YOffset;

            var chip = Object.Instantiate(tab.ColorTabPrefab, tab.scroller.Inner);
            chip.gameObject.name = visor.ProductId;
            chip.transform.localPosition = new Vector3(x, y, -1f);

            if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
            {
                chip.Button.OnMouseOver.AddListener((UnityAction)(() => tab.SelectVisor(visor)));
                chip.Button.OnMouseOut.AddListener((UnityAction)(() => tab.SelectVisor(HatManager.Instance.GetVisorById(DataManager.Player.Customization.Visor))));
                chip.Button.OnClick.AddListener((UnityAction)tab.ClickEquip);
            }
            else
            {
                chip.Button.OnClick.AddListener((UnityAction)(() => tab.SelectVisor(visor)));
            }

            chip.Button.ClickMask = tab.scroller.Hitbox;
            chip.ProductId = visor.ProductId;
            tab.UpdateMaterials(chip.Inner.FrontLayer, visor);
            visor.SetPreview(chip.Inner.FrontLayer, tab.CurrentColorId());
            chip.Tag = visor.ProdId;
            chip.SelectionHighlight.gameObject.SetActive(false);
            tab.ColorChips.Add(chip);

            if (!HatManager.Instance.CheckLongModeValidCosmetic(visor.ProdId, tab.PlayerPreview.GetIgnoreLongMode()))
                chip.SetUnavailable();
        }

        tab.currentVisorIsEquipped = true;
        tab.SetScrollerBounds();
    }

    public static IEnumerator CoLoadNameplatePreview(NameplatesTab tab)
    {
        yield return new WaitForEndOfFrame();
        tab.previewArea.PreviewNameplate(DataManager.Player.Customization.NamePlate);
    }

    public static void GenerateNameplates(NameplatesTab tab)
    {
        foreach (var chip in tab.ColorChips)
            Object.Destroy(chip.gameObject);

        tab.ColorChips.Clear();
        tab.scroller.ScrollToTop();

        var title = tab.transform.FindChild("Text").GetComponent<TextMeshPro>();
        title.GetComponent<TextTranslatorTMP>()?.DestroyImmediate();

        var pageCount = CosmeticsLoader.Instance.NameplateGroups.Count + 1;
        title.text = NameplatePage == 0 ? $"Nameplates ({NameplatePage + 1}/{pageCount})" : $"{CosmeticsLoader.Instance.NameplateGroups.GetGroupNameByIndex(NameplatePage - 1)} ({NameplatePage + 1}/{pageCount})";

        var plates = HatManager.Instance.GetUnlockedNamePlates().Where(p =>
        {
            if (NameplatePage == 0) return !p.ProductId.StartsWith("corsac");
            if (!p.ProductId.StartsWith("corsac")) return false;

            var group = Names.GetGroup(p.ProductId);
            return group == CosmeticsLoader.Instance.NameplateGroups.GetGroupIdByIndex(NameplatePage - 1);
        }).ToArray();

        tab.plateId = DataManager.Player.Customization.NamePlate;

        for (var i = 0; i < plates.Length; i++)
        {
            var plate = plates[i];
            var x = tab.XRange.Lerp(i % tab.NumPerRow / (tab.NumPerRow - 1f));
            var y = tab.YStart - i / tab.NumPerRow * tab.YOffset;

            var chip = Object.Instantiate(tab.ColorTabPrefab, tab.scroller.Inner);
            chip.gameObject.name = plate.ProductId;
            chip.transform.localPosition = new Vector3(x, y, -1f);

            if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
            {
                chip.Button.OnMouseOver.AddListener((UnityAction)(() => tab.SelectNameplate(plate)));
                chip.Button.OnMouseOut.AddListener((UnityAction)(() => tab.SelectNameplate(HatManager.Instance.GetNamePlateById(DataManager.Player.Customization.NamePlate))));
                chip.Button.OnClick.AddListener((UnityAction)tab.ClickEquip);
            }
            else
            {
                chip.Button.OnClick.AddListener((UnityAction)(() => tab.SelectNameplate(plate)));
            }

            chip.Button.ClickMask = tab.scroller.Hitbox;
            chip.ProductId = plate.ProdId;

            var image = chip.transform.GetChild(1).GetComponent<SpriteRenderer>();

            tab.StartCoroutine(tab.CoLoadAssetAsync<NamePlateViewData>(plate.GetAssetReference(), (Action<NamePlateViewData>)(viewData => image.sprite = viewData?.Image)));

            chip.Tag = plate;
            chip.SelectionHighlight.gameObject.SetActive(false);
            tab.ColorChips.Add(chip);
        }

        tab.currentNameplateIsEquipped = true;
        tab.SetScrollerBounds();
    }

    public static void SetupButtons(InventoryTab tab, Action previous, Action next)
    {
        var title = tab.transform.FindChild("Text").GetComponent<TextMeshPro>();
        title.GetComponent<TextTranslatorTMP>()?.DestroyImmediate();
        title.alignment = TextAlignmentOptions.Center;
        title.transform.localPosition = new Vector3(0.86f, -0.23f, -55f);

        var left = tab.transform.FindChild("NewModPageLeft");
        GameObject leftObj;

        if (!left)
        {
            leftObj = Object.Instantiate(PlayerCustomizationMenu.Instance.BackButton.gameObject, tab.transform);
            leftObj.name = "NewModPageLeft";
            leftObj.GetComponent<AspectPosition>()?.DestroyImmediate();
            leftObj.GetComponent<CloseButtonConsoleBehaviour>()?.DestroyImmediate();
        }
        else
        {
            leftObj = left.gameObject;
        }

        leftObj.SetActive(true);
        leftObj.transform.localPosition = new Vector3(-1.19f, -0.23f, -55f);
        leftObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        var leftButton = leftObj.GetComponent<PassiveButton>();
        var leftSprite = leftObj.GetComponent<SpriteRenderer>();

        leftSprite.sprite = MiraAssets.NextButton.LoadAsset();
        leftSprite.flipX = true;

        leftButton.OnClick = new Button.ButtonClickedEvent();
        leftButton.OnClick.AddListener((UnityAction)(() => previous()));

        leftButton.OnMouseOver = new UnityEvent();
        leftButton.OnMouseOver.AddListener((UnityAction)(() => leftSprite.sprite = MiraAssets.NextButtonActive.LoadAsset()));

        leftButton.OnMouseOut = new UnityEvent();
        leftButton.OnMouseOut.AddListener((UnityAction)(() => leftSprite.sprite = MiraAssets.NextButton.LoadAsset()));

        var right = tab.transform.FindChild("NewModPageRight");
        GameObject rightObj;

        if (!right)
        {
            rightObj = Object.Instantiate(PlayerCustomizationMenu.Instance.BackButton.gameObject, tab.transform);
            rightObj.name = "NewModPageRight";
            rightObj.GetComponent<AspectPosition>()?.DestroyImmediate();
            rightObj.GetComponent<CloseButtonConsoleBehaviour>()?.DestroyImmediate();
        }
        else
        {
            rightObj = right.gameObject;
        }

        rightObj.SetActive(true);
        rightObj.transform.localPosition = new Vector3(2.91f, -0.23f, -55f);
        rightObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        var rightButton = rightObj.GetComponent<PassiveButton>();
        var rightSprite = rightObj.GetComponent<SpriteRenderer>();

        rightSprite.sprite = MiraAssets.NextButton.LoadAsset();
        rightSprite.flipX = false;

        rightButton.OnClick = new Button.ButtonClickedEvent();
        rightButton.OnClick.AddListener((UnityAction)(() => next()));

        rightButton.OnMouseOver = new UnityEvent();
        rightButton.OnMouseOver.AddListener((UnityAction)(() => rightSprite.sprite = MiraAssets.NextButtonActive.LoadAsset()));

        rightButton.OnMouseOut = new UnityEvent();
        rightButton.OnMouseOut.AddListener((UnityAction)(() => rightSprite.sprite = MiraAssets.NextButton.LoadAsset()));
    }
}