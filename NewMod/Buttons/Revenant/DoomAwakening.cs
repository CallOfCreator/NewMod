using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using NewMod.Options.Roles.RevenantOptions;
using RV = NewMod.Roles.ImpostorRoles.Revenant;
using NewMod.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Buttons.Revenant
{
    /// <summary>
    /// Defines a custom action button for the role.
    /// </summary>
    public class DoomAwakening : CustomActionButton
    {
        /// <summary>
        /// The name displayed on the button.
        /// </summary>
        public override string Name => "";

        /// <summary>
        /// Cooldown time for this ability, as configured in <see cref="RevenantOptions"/>.
        /// </summary>
        public override float Cooldown => OptionGroupSingleton<RevenantOptions>.Instance.DoomAwakeningCooldown;

        /// <summary>
        /// Maximum uses allowed for this ability, as configured in <see cref="RevenantOptions"/>.
        /// </summary>
        public override int MaxUses => (int)OptionGroupSingleton<RevenantOptions>.Instance.DoomAwakeningMaxUses;

        /// <summary>
        /// Defines the on-screen position of this button.
        /// </summary>
        public override ButtonLocation Location => ButtonLocation.BottomLeft;

        /// <summary>
        /// Determines how long the effect lasts. Configured in <see cref="RevenantOptions"/>.
        /// </summary>
        public override float EffectDuration => OptionGroupSingleton<RevenantOptions>.Instance.DoomAwakeningDuration;

        /// <summary>
        /// The icon or sprite representing this button.
        /// </summary>
        public override LoadableAsset<Sprite> Sprite => NewModAsset.DoomAwakeningButton;

        /// <summary>
        /// Specifies whether this button is enabled for the specified role.
        /// </summary>
        /// <param name="role">The role under consideration.</param>
        /// <returns>True if the role is <see cref="RV"/>, otherwise false.</returns>
        public override bool Enabled(RoleBehaviour role)
        {
            return role is RV;
        }

        /// <summary>
        /// Checks if this button can be used, ensuring that Feign Death has already been used.
        /// </summary>
        /// <returns>True if base conditions are met and the player has used Feign Death, otherwise false.</returns>
        public override bool CanUse()
        {
            return base.CanUse() && RV.HasUsedFeignDeath;
        }

        /// <summary>
        /// Invoked when the button is clicked. Starts the Doom Awakening sequence.
        /// </summary>
        protected override void OnClick()
        {
            var player = PlayerControl.LocalPlayer;
            Coroutines.Start(StartDoomAwakening(player));
        }
        public static List<PlayerControl> killedPlayers = new();

        /// <summary>
        /// Executes the Doom Awakening effect, increasing speed, fading the screen, and killing nearby players.
        /// </summary>
        /// <param name="player">The PlayerControl instance of the local player.</param>
        /// <returns>An IEnumerator for coroutine control.</returns>
        public System.Collections.IEnumerator StartDoomAwakening(PlayerControl player)
        {
            float originalSpeed = player.MyPhysics.Speed;
            player.MyPhysics.Speed *= 2f;

            var clip = NewModAsset.DoomAwakeningSound.LoadAsset();
            SoundManager.Instance.PlaySound(clip, true, 1f, null);

            var fullScreen = HudManager.Instance.FullScreen;
            fullScreen.color = new Color(1f, 0f, 0f, 0f);
            fullScreen.gameObject.SetActive(true);

            float fadeInTime = 0.5f;

            // Fade in to a red overlay
            for (float t = 0; t < fadeInTime; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(0f, 0.3f, t / fadeInTime);
                fullScreen.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }
            fullScreen.color = new Color(1f, 0f, 0f, 0.3f);

            float duration = EffectDuration;
            float timer = 0f;
            int killCount = 0;

            float ghostInterval = 0.2f;
            float ghostTimer = 0f;

            Queue<GameObject> ghosts = new Queue<GameObject>();
            SpriteRenderer playerRenderer = player.cosmetics.normalBodySprite.BodySprite;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                ghostTimer += Time.deltaTime;

                if (ghostTimer >= ghostInterval && player.MyPhysics.Speed > 0.01f)
                {
                    ghostTimer = 0f;
                    GameObject ghost = new("Revenant-Ghost");
                    var ghostRenderer = ghost.AddComponent<SpriteRenderer>();
                    ghostRenderer.sprite = playerRenderer.sprite;
                    ghostRenderer.flipX = playerRenderer.flipX;
                    ghostRenderer.flipY = playerRenderer.flipY;
                    ghostRenderer.sharedMaterial = playerRenderer.sharedMaterial;
                    PlayerMaterial.SetColors(player.Data.DefaultOutfit.ColorId, ghostRenderer);
                    ghostRenderer.sortingLayerID = playerRenderer.sortingLayerID;
                    ghostRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
                    ghost.transform.position = player.transform.position;
                    ghost.transform.rotation = player.transform.rotation;
                    ghost.transform.localScale = player.transform.lossyScale;

                    Coroutines.Start(Utils.FadeAndDestroy(ghost, 1f));
                    ghosts.Enqueue(ghost);

                    // Limit the number of ghost sprites
                    if (ghosts.Count > 5)
                    {
                        var oldGhost = ghosts.Dequeue();
                        if (oldGhost != null)
                            Object.Destroy(oldGhost);
                    }
                }
                // Kill any nearby players
                foreach (var target in PlayerControl.AllPlayerControls)
                {
                    if (target == player || target.Data.IsDead || target.Data.Disconnected || target.inVent || target.Data.Role.IsImpostor)
                        continue;

                    if (Vector2.Distance(player.GetTruePosition(), target.GetTruePosition()) < 1f)
                    {
                        player.RpcCustomMurder(target,
                            didSucceed: true,
                            resetKillTimer: false,
                            createDeadBody: true,
                            teleportMurderer: false,
                            showKillAnim: false,
                            playKillSound: true);
                        killCount++;
                        killedPlayers.Add(target);
                    }
                    if (target.AmOwner)
                    {
                        SoundManager.Instance.PlaySound(NewModAsset.DoomAwakeningEndSound.LoadAsset(), false, 1f, null);
                    }
                }
                yield return null;
            }
            if (killedPlayers.Count >= 3)
            {
                SoundManager.Instance.PlaySound(NewModAsset.DoomAwakeningEndSound.LoadAsset(), false, 1f, null);
            }

            // Fade out the red overlay
            float fadeOutTime = 0.5f;
            for (float t = 0f; t < fadeOutTime; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(0.3f, 0f, t / fadeOutTime);
                fullScreen.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }
            fullScreen.gameObject.SetActive(false);

            // Restore original speed and conclude
            player.MyPhysics.Speed = originalSpeed;
            SoundManager.Instance.StopSound(clip);
            RV.StalkingStates.Remove(player.PlayerId);
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=green>Doom Awakening ended.</color>"));
            Helpers.CreateAndShowNotification($"Doom Awakening killed {killCount} players", Color.red, null, null);
            killedPlayers.Clear();
        }
    }
}
