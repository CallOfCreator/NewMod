using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.NecromancerOptions;
using NewMod.Roles.ImpostorRoles;
using UnityEngine;
using NewMod.Utilities;

namespace NewMod.Buttons;

[RegisterButton]
public class NecromancerButton : CustomActionButton
{
    public override string Name => ""; // It's currently empty since the button has already a name on it
    public override float Cooldown => OptionGroupSingleton<NecromancerOption>.Instance.ButtonCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<NecromancerOption>.Instance.AbilityUses;
    public override float EffectDuration => 0f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => NewModAsset.NecromancerButton;
    protected override void OnClick()
    {
       NewMod.Instance.Log.LogMessage("Button Clicked!");

       var closestBody = Utils.GetClosestBody();
       if (closestBody != null)
       {
         Utils.RpcRevive(closestBody);
       }
    }
    public override bool Enabled(RoleBehaviour role)
    {
       return role is NecromancerRole;
    }
    public override bool CanUse()
    {
            bool isTimerDone = Timer <= 0;
            bool hasUsesLeft = UsesLeft > 0;
            var closestBody = Utils.GetClosestBody();
            bool isNearDeadBody = closestBody != null;

            if (closestBody == null)
            {
                return false;
            }

            bool wasNotKilledByNecromancer = true;
            var deadBody = closestBody.GetComponent<DeadBody>();
            if (deadBody != null)
            {
                var killedPlayer = GameData.Instance.GetPlayerById(deadBody.ParentId)?.Object;
                if (killedPlayer != null)
                {
                    var killer = Utils.GetKiller(killedPlayer);
                    if (killer != null && killer.Data.Role is NecromancerRole)
                    {
                        wasNotKilledByNecromancer = false;
                    }
                }
            }
            return isTimerDone && hasUsesLeft && isNearDeadBody && wasNotKilledByNecromancer;
        }
}
    