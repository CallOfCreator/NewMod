using System;
using System.Collections.Generic;
using Hazel;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace NewMod.Networking;

[RegisterCustomRpc((uint)CustomRPC.VisionaryPhoto)]
public class VisionaryPhotoRpc : PlayerCustomRpc<NewMod, VisionaryPhotoRpc.Data>
{
    public const int MaxPhotoBytes = 2 * 1024 * 1024;
    public const int ChunkSize = 700;
    public const int ChunksPerFrame = 4;
    public static readonly Dictionary<(int Id, int Meeting), (byte[] Image, int Received)> Transfers = [];

    public readonly record struct Data(int Id, int Meeting, int Total, int Offset, byte[] Bytes);

    public VisionaryPhotoRpc(NewMod plugin, uint id) : base(plugin, id)
    {
    }

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write(data.Id);
        writer.Write(data.Meeting);
        writer.Write(data.Total);
        writer.Write(data.Offset);
        writer.WriteBytesAndSize(data.Bytes);
    }

    public override Data Read(MessageReader reader)
    {
        return new Data(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBytesAndSize());
    }

    public override void Handle(PlayerControl source, Data data)
    {
        if (!source.IsHost() || data.Total is < 1 or > MaxPhotoBytes || data.Offset < 0 || data.Bytes.Length is < 1 or > ChunkSize || data.Offset > data.Total || data.Bytes.Length > data.Total - data.Offset || !VisionaryUtilities.Cameras.TryGetValue(data.Id, out var record))
            return;
        if (data.Meeting >= 0 && (!MeetingHud.Instance || data.Meeting != VisionaryUtilities.MeetingNumber || record.BroadcastMeeting == data.Meeting))
            return;
        if (data.Meeting < 0 && record.Owner != PlayerControl.LocalPlayer.PlayerId)
            return;
        var key = (data.Id, data.Meeting);
        if (data.Offset == 0)
            Transfers[key] = (new byte[data.Total], 0);
        if (!Transfers.TryGetValue(key, out var transfer) || transfer.Image.Length != data.Total || transfer.Received != data.Offset)
            return;
        Array.Copy(data.Bytes, 0, transfer.Image, data.Offset, data.Bytes.Length);
        transfer.Received += data.Bytes.Length;
        if (transfer.Received != data.Total)
        {
            Transfers[key] = transfer;
            return;
        }

        Transfers.Remove(key);
        record.Image = transfer.Image;
        if (data.Meeting >= 0)
        {
            record.BroadcastMeeting = data.Meeting;
            Coroutines.Start(VisionaryUtilities.ShowPhoto(data.Id, OptionGroupSingleton<VisionaryOptions>.Instance.MaxDisplayDuration));
        }
    }
}
