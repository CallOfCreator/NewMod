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
public class VerifyMinigame(IntPtr ptr) : Minigame(ptr)
{
    public PlayerControl Target;
    public TextMeshPro TaskText;
    public TextMeshPro KillText;
    public TextMeshPro VentText;
    public TextMeshPro AbilityText;
    public PassiveButton HelpIcon;

    public void Awake()
    {
        TaskText = transform.Find("UI/TaskText").GetComponent<TextMeshPro>();
        KillText = transform.Find("UI/KillText").GetComponent<TextMeshPro>();
        VentText = transform.Find("UI/VentText").GetComponent<TextMeshPro>();
        AbilityText = transform.Find("UI/AbilityText").GetComponent<TextMeshPro>();
        HelpIcon = transform.Find("UI/HelpIcon").GetComponent<PassiveButton>();

        HelpIcon.OnClick.AddListener((UnityAction)ShowPopup);
    }

    public void Start()
    {
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
        var confirm = text.transform.Find("ConfirmIcon").GetComponent<PassiveButton>();
        var deny = text.transform.Find("DenyIcon").GetComponent<PassiveButton>();

        var confirmRenderer = confirm.GetComponent<SpriteRenderer>();
        var denyRenderer = deny.GetComponent<SpriteRenderer>();

        var confirmSprite = confirmRenderer.sprite;
        var denySprite = denyRenderer.sprite;
        var confirmHover = NewModAsset.ConfirmIconHover.LoadAsset();
        var denyHover = NewModAsset.DenyIconHover.LoadAsset();

        confirm.activeSprites = null;
        confirm.inactiveSprites = null;
        deny.activeSprites = null;
        deny.inactiveSprites = null;

        confirmRenderer.enabled = true;
        denyRenderer.enabled = true;

        confirm.OnMouseOver.AddListener((UnityAction)(() => confirmRenderer.sprite = confirmHover));

        confirm.OnMouseOut.AddListener((UnityAction)(() => confirmRenderer.sprite = confirmSprite));

        deny.OnMouseOver.AddListener((UnityAction)(() => denyRenderer.sprite = denyHover));

        deny.OnMouseOut.AddListener((UnityAction)(() => denyRenderer.sprite = denySprite));

        confirm.OnClick.AddListener((UnityAction)(() => Verify(claim, true)));

        deny.OnClick.AddListener((UnityAction)(() => Verify(claim, false)));
    }

    [HideFromIl2Cpp]
    private void Verify(VerifierClaimType claim, bool expected)
    {
        if (VerifierUtilities.UsedThisMeeting)
            return;

        var result = VerifierUtilities.GetVerificationResult(Target, claim, expected);

        VerifierUtilities.UsedThisMeeting = true;
        VerifierUtilities.SelectingPlayer = false;
        VerifierUtilities.UpdateMeetingButton();

        Close();

        Coroutines.Start(VerifierUtilities.CoNotifyAfterDelay(0.3f, $"<color=#58E8BE>Verifier result</color>\n{Target.Data.PlayerName}: {result}"));
    }
}