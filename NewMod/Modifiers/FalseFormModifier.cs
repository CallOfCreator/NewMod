using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using NewMod.Options.Modifiers;
using UnityEngine;

namespace NewMod.Modifiers
{
    public class FalseFormModifier : TimedModifier
    {
        public override string ModifierName => "FalseForm";
        public override bool AutoStart =>
            OptionGroupSingleton<FalseFormModifierOptions>.Instance.EnableModifier;
        public override float Duration =>
            (int)OptionGroupSingleton<FalseFormModifierOptions>.Instance.FalseFormDuration;
        public override bool ShowInFreeplay => true;
        public override bool HideOnUi => false;
        public override bool RemoveOnComplete => true;
        private float timer;
        private AppearanceBackup oldAppearance;
        public override void OnActivate()
        {
            oldAppearance = new AppearanceBackup
            {
                PlayerName = Player.Data.PlayerName,
                HatId = Player.Data.DefaultOutfit.HatId,
                SkinId = Player.Data.DefaultOutfit.SkinId,
                PetId = Player.Data.DefaultOutfit.PetId,
                ColorId = Player.Data.DefaultOutfit.ColorId
            };
        }
        public override bool? CanVent()
        {
            return Player.Data.Role.CanVent;
        }
        public override string GetDescription()
        {
            return ModifierName
                + $"\nYour appearance changes every {OptionGroupSingleton<FalseFormModifierOptions>.Instance.FalseFormAppearanceTimer.Value} seconds.";
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timer += Time.fixedDeltaTime;

            if (timer >= OptionGroupSingleton<FalseFormModifierOptions>.Instance.FalseFormAppearanceTimer.Value)
            {
                Player.RpcSetName(Helpers.RandomString(5));
                Player.RpcSetColor((byte)Random.Range(0, Palette.PlayerColors.Count));
                Player.RpcSetHat(HatManager.Instance.AllHats[Random.Range(0, HatManager.Instance.allHats.Count)].ProductId);
                Player.RpcSetSkin(HatManager.Instance.AllSkins[Random.Range(0, HatManager.Instance.allSkins.Count)].ProductId);
                Player.RpcSetPet(HatManager.Instance.AllPets[Random.Range(0, HatManager.Instance.allPets.Count)].ProductId);
            }
        }
        public override void OnDeactivate()
        {
            if (OptionGroupSingleton<FalseFormModifierOptions>.Instance.RevertAppearance)
            {
                Player.RpcSetName(oldAppearance.PlayerName);
                Player.RpcSetColor((byte)oldAppearance.ColorId);
                Player.RpcSetHat(oldAppearance.HatId);
                Player.RpcSetSkin(oldAppearance.SkinId);
                Player.RpcSetPet(oldAppearance.PetId);
            }
        }
    }
    class AppearanceBackup
    {
        public string PlayerName;
        public string HatId, SkinId, PetId;
        public int ColorId;
    }
}
