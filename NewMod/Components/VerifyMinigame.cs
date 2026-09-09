using System;
using Il2CppInterop.Runtime.Attributes;
using NewMod.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NewMod.Components;

[RegisterInIl2Cpp]
public class VerifyChoiceButton(IntPtr ptr) : MonoBehaviour(ptr)
{
    public VerifyMinigame Minigame;
    public VerifierClaimType Claim;
    public bool Expected;
    public SpriteRenderer Renderer;
    public Sprite NormalSprite;
    public Sprite HoverSprite;

    public void Select()
    {
        Minigame.Verify(Claim, Expected);
    }

    public void ShowHover()
    {
        Renderer.sprite = HoverSprite;
    }

    public void ShowNormal()
    {
        Renderer.sprite = NormalSprite;
    }
}

[RegisterInIl2Cpp]
public class VerifyMinigame(IntPtr ptr) : Minigame(ptr)
{
    public PlayerControl Target;
    public TextMeshPro TaskText;
    public TextMeshPro KillText;
    public TextMeshPro VentText;
    public TextMeshPro AbilityText;
    public PassiveButton HelpIcon;
    public PassiveButton CloseButton;

    public void Awake()
    {
        TaskText = transform.Find("UI/TaskText").GetComponent<TextMeshPro>();
        KillText = transform.Find("UI/KillText").GetComponent<TextMeshPro>();
        VentText = transform.Find("UI/VentText").GetComponent<TextMeshPro>();
        AbilityText = transform.Find("UI/AbilityText").GetComponent<TextMeshPro>();
        HelpIcon = transform.Find("UI/HelpIcon").GetComponent<PassiveButton>();
        CloseButton = transform.Find("UI/CloseButton").GetComponent<PassiveButton>();

        HelpIcon.OnClick.RemoveAllListeners();
        HelpIcon.OnClick.AddListener((UnityAction)ShowPopup);

        CloseButton.OnClick.RemoveAllListeners();
        CloseButton.OnClick.AddListener((UnityAction)(() => Close()));

        Bind(TaskText, VerifierClaimType.DidTask);
        Bind(KillText, VerifierClaimType.NearBody);
        Bind(VentText, VerifierClaimType.EnteredVent);
        Bind(AbilityText, VerifierClaimType.UsedAbility);
    }

    public static VerifyMinigame CreateMinigame(PlayerControl target)
    {
        var gameObject = Instantiate(NewModAsset.VerifyMinigame.LoadAsset(), HudManager.Instance.transform);

        gameObject.transform.localPosition = new Vector3(0f, 0f, Depth);
        gameObject.transform.localScale = Vector3.one * 0.5f;
        gameObject.transform.SetAsLastSibling();

        var uiLayer = LayerMask.NameToLayer("UI");
        foreach (var child in gameObject.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = uiLayer;

        var minigame = gameObject.AddComponent<VerifyMinigame>();
        minigame.Open(target);
        return minigame;
    }

    public void Open(PlayerControl target)
    {
        Target = target;

        TaskText.text = "The player did a task";
        KillText.text = "The player was near a body";
        VentText.text = "The player entered a vent";
        AbilityText.text = "The player used an ability";

        Begin(null);
        SetupInput(false, true);
    }

    public void ShowPopup()
    {
        var dialogue = HudManager.Instance.Dialogue;
        var position = dialogue.transform.localPosition;

        dialogue.transform.localPosition = new Vector3(position.x, position.y, Depth - 10f);
        dialogue.transform.SetAsLastSibling();
        dialogue.target.text = $"Verify one statement about {Target.Data.PlayerName}.\n" + "Green confirms the statement. Red denies it.\n" + "The answer can be Confirmed, Denied, or Unknown.";
        dialogue.gameObject.SetActive(true);
    }

    [HideFromIl2Cpp]
    private void Bind(TextMeshPro text, VerifierClaimType claim)
    {
        Bind(text.transform.Find("ConfirmIcon").GetComponent<PassiveButton>(), claim, true, NewModAsset.ConfirmIconHover.LoadAsset());
        Bind(text.transform.Find("DenyIcon").GetComponent<PassiveButton>(), claim, false, NewModAsset.DenyIconHover.LoadAsset());
    }

    [HideFromIl2Cpp]
    private void Bind(PassiveButton button, VerifierClaimType claim, bool expected, Sprite hoverSprite)
    {
        var renderer = button.GetComponent<SpriteRenderer>();
        var handler = button.gameObject.AddComponent<VerifyChoiceButton>();

        handler.Minigame = this;
        handler.Claim = claim;
        handler.Expected = expected;
        handler.Renderer = renderer;
        handler.NormalSprite = renderer.sprite;
        handler.HoverSprite = hoverSprite;

        button.OnClick.RemoveAllListeners();
        button.OnMouseOver.RemoveAllListeners();
        button.OnMouseOut.RemoveAllListeners();

        button.OnClick.AddListener((UnityAction)handler.Select);
        button.OnMouseOver.AddListener((UnityAction)handler.ShowHover);
        button.OnMouseOut.AddListener((UnityAction)handler.ShowNormal);
    }

    [HideFromIl2Cpp]
    public void Verify(VerifierClaimType claim, bool expected)
    {
        if (VerifierUtilities.UsedThisMeeting)
            return;

        var targetName = Target.Data.PlayerName;
        var result = VerifierUtilities.GetVerificationResult(Target, claim, expected);

        VerifierUtilities.UsedThisMeeting = true;
        VerifierUtilities.SelectingPlayer = false;
        VerifierUtilities.UpdateMeetingButton();

        Close();

        Coroutines.Start(VerifierUtilities.CoNotifyAfterDelay(0.3f, $"<color=#58E8BE>Verifier result</color>\n{targetName}: {result}"));
    }
}