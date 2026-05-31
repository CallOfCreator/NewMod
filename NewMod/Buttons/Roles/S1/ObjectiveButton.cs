using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Buttons.Roles.S1
{
    [MiraIgnore]
    public class ObjectiveButton : CustomActionButton
    {
        public override string Name => "Terminate";
        public override float Cooldown => 0f;
        public override int MaxUses => 1;
        public override MiraKeybind Keybind => MiraGlobalKeybinds.PrimaryAbility;
        public override ButtonLocation Location => ButtonLocation.BottomRight;
        public override LoadableAsset<Sprite> Sprite => MiraAssets.Empty;
        public override bool Enabled(RoleBehaviour role) => role is TerminatorRole;
        public override bool CanUse()
        {
            if (!base.CanUse() || !TerminatorRole.ObjectiveSpawned)
                return false;

            float radius = OptionGroupSingleton<TerminatorOptions>.Instance.FinalObjectiveRadius;
            return Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), TerminatorRole.ObjectivePosition) <= radius;
        }

        protected override void OnClick()
        {
            GameManager.Instance.RpcEndGame((GameOverReason)NewModEndReasons.TerminatorWin, false);
        }
    }
}