using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Modifiers;
using NewMod.Modifiers.S1;
using NewMod.Components.Minigames;
using NewMod.Networking;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewMod.Utilities;

public static class VisionaryUtilities
{
    public class CameraRecord
    {
        public byte Owner;
        public Vector2 Direction;
        public GameObject Object;
        public byte[] Image;
        public bool Ready;
        public bool Failed;
        public bool Collected;
        public int BroadcastMeeting = -1;
    }

    public static readonly Dictionary<int, CameraRecord> Cameras = new();
    public static readonly HashSet<byte> BroadcastOwners = new();
    public static GameObject PhotoPanel;
    public static int NextCameraId;
    public static int SelectedCamera = -1;
    public static int MeetingNumber;
    public static bool IsShowing => PhotoPanel;
    public static bool HasScreenshots => Cameras.Values.Any(camera => camera.Owner == PlayerControl.LocalPlayer.PlayerId && camera.Collected && camera.Image != null);

    [MethodRpc((uint)CustomRPC.VisionaryRequestCamera, LocalHandling = RpcLocalHandling.After)]
    public static void RpcRequestCamera(PlayerControl source, float x, float y, float angle)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not TheVisionary || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance)
            return;
        if (Cameras.Values.Count(camera => camera.Owner == source.PlayerId) >= OptionGroupSingleton<VisionaryOptions>.Instance.MaxScreenshots)
            return;
        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(angle) || source.inVent || !source.CanMove)
            return;
        var position = new Vector2(x, y);
        var offset = position - source.GetTruePosition();
        if (offset.magnitude > OptionGroupSingleton<VisionaryOptions>.Instance.PlacementRange || PhysicsHelpers.AnyNonTriggersBetween(source.GetTruePosition(), offset.normalized, offset.magnitude, Constants.ShipAndObjectsMask))
            return;
        RpcPlaceCamera(PlayerControl.LocalPlayer, source.PlayerId, NextCameraId++, x, y, angle);
    }

    [MethodRpc((uint)CustomRPC.VisionaryPlaceCamera, LocalHandling = RpcLocalHandling.After)]
    public static void RpcPlaceCamera(PlayerControl source, byte ownerId, int id, float x, float y, float angle)
    {
        if (!source.IsHost() || Cameras.ContainsKey(id))
            return;
        NextCameraId = Math.Max(NextCameraId, id + 1);
        var go = new GameObject("VisionaryCamera") { layer = LayerMask.NameToLayer("Ship") };
        go.transform.SetParent(ShipStatus.Instance.transform, false);
        go.transform.position = new Vector3(x, y, y / 1000f);
        go.transform.localScale = Vector3.one * 0.5f;

        var renderer = new GameObject("CameraSprite").AddComponent<SpriteRenderer>();
        renderer.transform.SetParent(go.transform, false);
        renderer.sprite = NewModAsset.CameraOff.LoadAsset();
        var bounds = new Bounds(renderer.sprite.vertices[0], Vector3.zero);
        foreach (var vertex in renderer.sprite.vertices)
            bounds.Encapsulate(vertex);
        renderer.transform.localPosition = Vector3.down * bounds.min.y;
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = bounds.size;
        collider.offset = renderer.transform.localPosition + bounds.center;
        var radians = angle * Mathf.Deg2Rad;
        Cameras[id] = new CameraRecord { Owner = ownerId, Object = go, Direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) };
        if (AmongUsClient.Instance.AmHost)
            Coroutines.Start(CaptureCamera(id));
    }

    public static IEnumerator CaptureCamera(int id)
    {
        var record = Cameras[id];
        yield return new WaitForSeconds(OptionGroupSingleton<VisionaryOptions>.Instance.CaptureDelay);
        while (MeetingHud.Instance || ExileController.Instance)
            yield return null;
        if (!Cameras.TryGetValue(id, out var current) || current != record || !record.Object)
            yield break;
        var owner = Utils.PlayerById(record.Owner);
        if (!owner || owner.Data.Role is not TheVisionary || owner.Data.IsDead || owner.Data.Disconnected)
            yield break;
        var position = (Vector2)record.Object.transform.position;
        var jammed = Utils.IsActive(SystemTypes.Comms);
        record.Image = jammed ? null : CapturePhoto(position, record.Direction);
        if (record.Image?.Length > VisionaryPhotoRpc.MaxPhotoBytes)
            record.Image = null;

        RpcCameraReady(PlayerControl.LocalPlayer, id, record.Image == null);
    }

    [MethodRpc((uint)CustomRPC.VisionaryCameraReady, LocalHandling = RpcLocalHandling.After)]
    public static void RpcCameraReady(PlayerControl source, int id, bool failed)
    {
        if (!source.IsHost() || !Cameras.TryGetValue(id, out var record) || !record.Object)
            return;
        record.Ready = true;
        record.Failed = failed;
        record.Object.GetComponentInChildren<SpriteRenderer>().sprite = (failed ? NewModAsset.CameraDisabled : NewModAsset.CameraEnabled).LoadAsset();
    }

    [MethodRpc((uint)CustomRPC.VisionaryInteractCamera, LocalHandling = RpcLocalHandling.After)]
    public static void RpcInteractCamera(PlayerControl source, int id)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || !Cameras.TryGetValue(id, out var record) || !record.Object)
            return;
        var offset = (Vector2)record.Object.transform.position - source.GetTruePosition();
        if (offset.magnitude > 1.25f || PhysicsHelpers.AnythingBetween(source.GetTruePosition(), record.Object.transform.position, Constants.ShipAndObjectsMask, false, record.Object.GetComponent<Collider2D>(), record.Object.transform))
            return;
        if (source.PlayerId == record.Owner && !record.Ready)
            return;
        var collected = source.PlayerId == record.Owner && record.Image != null;
        RpcResolveCamera(PlayerControl.LocalPlayer, id, collected);
        if (collected)
            Coroutines.Start(SendPhoto(id, source.OwnerId, -1));
    }

    [MethodRpc((uint)CustomRPC.VisionaryResolveCamera, LocalHandling = RpcLocalHandling.After)]
    public static void RpcResolveCamera(PlayerControl source, int id, bool collected)
    {
        if (!source.IsHost() || !Cameras.TryGetValue(id, out var record))
            return;
        Object.Destroy(record.Object);
        record.Object = null;
        record.Collected = collected;
        if (!collected)
            record.Image = null;
        if (record.Owner == PlayerControl.LocalPlayer.PlayerId)
        {
            if (collected)
                SelectedCamera = id;
            Coroutines.Start(CoroutinesHelper.CoNotify(collected ? "Camera retrieved. Preview to select evidence; broadcast it in a meeting." : "Your camera was disabled. Its evidence is lost."));
        }
    }

    [MethodRpc((uint)CustomRPC.VisionaryBroadcast, LocalHandling = RpcLocalHandling.After)]
    public static void RpcBroadcast(PlayerControl source, int id)
    {
        if (!AmongUsClient.Instance.AmHost || !MeetingHud.Instance || source.Data.Role is not TheVisionary || source.Data.IsDead || !Cameras.TryGetValue(id, out var record) || record.Owner != source.PlayerId || !record.Collected || record.Image == null || !BroadcastOwners.Add(source.PlayerId))
            return;
        Coroutines.Start(SendPhoto(id, -1, MeetingNumber));
    }

    public static IEnumerator SendPhoto(int id, int recipient, int meeting)
    {
        var record = Cameras[id];
        var image = record.Image;
        if (image.Length > VisionaryPhotoRpc.MaxPhotoBytes)
            yield break;
        for (var offset = 0; offset < image.Length; offset += VisionaryPhotoRpc.ChunkSize)
        {
            if (!Cameras.TryGetValue(id, out var current) || current != record || (meeting >= 0 && (!MeetingHud.Instance || MeetingNumber != meeting)))
                yield break;
            var count = Math.Min(VisionaryPhotoRpc.ChunkSize, image.Length - offset);
            var chunk = new byte[count];
            Array.Copy(image, offset, chunk, 0, count);
            var data = new VisionaryPhotoRpc.Data(id, meeting, image.Length, offset, chunk);
            if (recipient == PlayerControl.LocalPlayer.OwnerId)
                Rpc<VisionaryPhotoRpc>.Instance.Handle(PlayerControl.LocalPlayer, data);
            else
                Rpc<VisionaryPhotoRpc>.Instance.SendTo(recipient, data);
            if ((offset / VisionaryPhotoRpc.ChunkSize + 1) % VisionaryPhotoRpc.ChunksPerFrame == 0)
                yield return null;
        }
    }

    public static IEnumerator ShowScreenshots(float duration)
    {
        if (MeetingHud.Instance)
            yield break;
        yield return ShowPhoto(SelectedCamera, duration, true);
    }

    public static IEnumerator ShowPhoto(int id, float duration, bool preview = false)
    {
        if (IsShowing)
            yield break;
        var available = Cameras.Where(pair => pair.Value.Image != null && (preview ? pair.Value.Owner == PlayerControl.LocalPlayer.PlayerId && pair.Value.Collected : pair.Key == id)).OrderBy(pair => pair.Key);
        var photos = new List<(int Id, Sprite Sprite)>();
        try
        {
            foreach (var (photoId, record) in available)
            {
                var texture = new Texture2D(2, 2);
                if (!texture.LoadImage(record.Image))
                {
                    Object.Destroy(texture);
                    continue;
                }

                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                photos.Add((photoId, sprite));
            }

            if (photos.Count == 0)
                yield break;
            var panel = PhotoPanelMinigame.CreateMinigame(photos, duration, preview).gameObject;
            PhotoPanel = panel;
            while (panel && PhotoPanel == panel)
                yield return null;
            if (PhotoPanel == panel)
                PhotoPanel = null;
        }
        finally
        {
            foreach (var photo in photos)
            {
                Object.Destroy(photo.Sprite.texture);
                Object.Destroy(photo.Sprite);
            }
        }
    }

    public static byte[] CapturePhoto(Vector2 position, Vector2 direction)
    {
        var options = OptionGroupSingleton<VisionaryOptions>.Instance;
        var height = (int)options.PhotoHeight;
        var width = Mathf.RoundToInt(height * options.PhotoAspect);
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        var target = RenderTexture.GetTemporary(width, height, 24);
        var camera = new GameObject("VisionaryPhoto").AddComponent<Camera>();
        camera.enabled = false;
        camera.orthographic = true;
        camera.nearClipPlane = Camera.main.nearClipPlane;
        camera.farClipPlane = Camera.main.farClipPlane;
        camera.orthographicSize = options.PhotoViewHeight / 2f;
        camera.aspect = options.PhotoAspect;
        camera.cullingMask = (Camera.main.cullingMask | Constants.LivingPlayersOnlyMask) & ~LayerMask.GetMask("UI", "Shadow", "Lighting");
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.transform.position = new Vector3(position.x, position.y, Camera.main.transform.position.z);
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        camera.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Repeat(angle + 90f, 180f) - 90f);
        camera.targetTexture = target;
        var previous = RenderTexture.active;
        var hidden = new List<Renderer>();
        try
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var concealed = player.Data.IsDead || player.inVent || player.PhantomFadeActive || player.HasModifier<InVoid>();
                foreach (var renderer in player.GetComponentsInChildren<Renderer>())
                {
                    if (!renderer.enabled || renderer.forceRenderingOff)
                        continue;
                    if (concealed || renderer.GetComponentInParent<TMP_Text>())
                    {
                        hidden.Add(renderer);
                        renderer.forceRenderingOff = true;
                    }
                    else if (renderer.TryCast<SpriteRenderer>())
                    {
                        camera.cullingMask |= 1 << renderer.gameObject.layer;
                    }
                }
            }

            foreach (var record in Cameras.Values)
                if (record.Object)
                {
                    var renderer = record.Object.GetComponentInChildren<SpriteRenderer>();
                    if (!renderer.forceRenderingOff)
                    {
                        hidden.Add(renderer);
                        renderer.forceRenderingOff = true;
                    }
                }

            foreach (var body in Object.FindObjectsOfType<DeadBody>())
            foreach (var renderer in body.GetComponentsInChildren<SpriteRenderer>())
                if (renderer.enabled && !renderer.forceRenderingOff)
                    camera.cullingMask |= 1 << renderer.gameObject.layer;
            camera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            return texture.EncodeToJPG((int)options.PhotoQuality);
        }
        finally
        {
            foreach (var renderer in hidden)
                renderer.forceRenderingOff = false;
            camera.targetTexture = null;
            Object.Destroy(camera.gameObject);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Object.Destroy(texture);
        }
    }

    public static void DeleteAllScreenshots()
    {
        Object.Destroy(PhotoPanel);
        PhotoPanel = null;
        foreach (var record in Cameras.Values)
            Object.Destroy(record.Object);
        Cameras.Clear();
        BroadcastOwners.Clear();
        VisionaryPhotoRpc.Transfers.Clear();
        SelectedCamera = -1;
        MeetingNumber = 0;
    }
}