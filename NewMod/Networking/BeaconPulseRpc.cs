using Hazel;
using NewMod.Components.ScreenEffects;
using NewMod.Components.ScreenEffects.Effects;
using NewMod.Roles.CrewmateRoles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace NewMod.Networking;

[RegisterCustomRpc((uint)CustomRPC.BeaconPulse)]
public class BeaconPulseRpc : PlayerCustomRpc<NewMod, BeaconPulseRpc.Data>
{
    public BeaconPulseRpc(NewMod plugin, uint id) : base(plugin, id)
    {
    }

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write(data.Duration);
    }

    public override Data Read(MessageReader reader)
    {
        return new Data(reader.ReadSingle());
    }

    public override void Handle(PlayerControl source, Data data)
    {
        if (source.Data.Role is not Beacon || source.Data.IsDead)
            return;

        var cam = Camera.main;
        if (!cam)
            return;

        var effect = cam.GetScreenEffect<DistorationWaveEffect>() ?? cam.AddScreenEffect<DistorationWaveEffect>();
        effect.expiresAt = Time.time + data.Duration;

        Instance.LogMessage($"Beacon pulse triggered by {source.Data.PlayerName} for {data.Duration}s");
    }

    public readonly record struct Data(float Duration);
}
