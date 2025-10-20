using Hazel;
using NewMod.Components.ScreenEffects;
using NewMod.Roles.CrewmateRoles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Networking
{
    [RegisterCustomRpc((uint)CustomRPC.BeaconPulse)]
    public class BeaconPulseRpc : PlayerCustomRpc<NewMod, BeaconPulseRpc.Data>
    {
        public BeaconPulseRpc(NewMod plugin, uint id) : base(plugin, id) { }

        public readonly record struct Data(float Duration);

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Duration);
        }

        public override Data Read(MessageReader reader)
        {
            return new Data(reader.ReadSingle());
        }

        public override void Handle(PlayerControl sender, Data data)
        {
            var cam = Camera.main;
            if (cam && !cam.GetComponent<DistorationWaveEffect>())
            {
                cam.gameObject.AddComponent<DistorationWaveEffect>();
            }
            Beacon.pulseUntil = Time.time + data.Duration;

            Logger<NewMod>.Instance.LogMessage($"Beacon pulse triggered by {sender.Data.PlayerName} for {data.Duration}s");
        }
    }
}
