using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using NewMod.Pathfinding;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Debugging;

[RegisterInIl2Cpp]
public sealed class PathfindingNpc(nint ptr) : MonoBehaviour(ptr)
{
    public float Speed = 2.5f;
    public ZiplineConsole zipConsole;
    public HandZiplinePoolable zipHand;
    public int zipPhase;
    public float zipTime;
    public Vector2 zipStart;
    public Vector3 handStart;
    public bool zipDownLoop;
    public Vent ventExit;
    public Il2CppSystem.Collections.IEnumerator ventAnimation;
    public int ventPhase;
    public MovingPlatformBehaviour platform;
    public MapPathRequest platformAlternative;
    public int platformPhase;
    public Vector2 platformStart;
    public Vector2 platformEnd;
    public Vector2 platformExit;
    public OpenableDoor waitingDoor;
    public AudioSource traversalAudio;
    public Vector2 lastClearPosition;
    public float progress;
    public float recoveryWait;
    public PlayerControl visual;
    public PathfindingPreview preview;
    public MapPathRequest request;
    public MapPath path;
    public readonly Queue<(Vector2 Position, string Name)> stops = new();
    public readonly Queue<Vector2> traversalPoints = new();
    public Vector2 destination;
    public string destinationName;
    public int waypoint;
    public int retries;
    public int visited;
    public int skipped;
    public bool touring;
    public bool returning;
    public bool crossing;
    public float crossingTime;
    public float traversalSpeed;
    public PathCrossing interaction;
    public DeconSystem decon;

    [HideFromIl2Cpp]
    public void Initialize(PathfindingPreview owner, bool tour)
    {
        preview = owner;
        traversalAudio = gameObject.AddComponent<AudioSource>();
        traversalAudio.playOnAwake = false;
        traversalAudio.outputAudioMixerGroup = SoundManager.Instance.SfxChannel;
        touring = tour;
        visual = Instantiate(AmongUsClient.Instance.PlayerPrefab);
        visual.transform.SetParent(transform, true);
        visual.notRealPlayer = true;
        visual.enabled = false;
        visual.NetTransform.enabled = false;
        visual.MyPhysics.enabled = false;
        visual.Collider.enabled = false;
        visual.MyPhysics.body.isKinematic = true;
        visual.MyPhysics.body.velocity = Vector2.zero;
        PlayerControl.AllPlayerControls.Remove(visual);
        visual.cosmetics.enabled = true;
        visual.cosmetics.Visible = true;
        visual.cosmetics.SetName("Pathfinding NPC");
        visual.cosmetics.ToggleName(true);
        visual.cosmetics.SetNamePosition(new Vector3(0f, 0.8f, -0.5f));
        visual.cosmetics.ToggleHat(false);
        visual.cosmetics.TogglePet(false);
        visual.cosmetics.ToggleVisor(false);
        visual.cosmetics.currentBodySprite.Visible = true;
        visual.cosmetics.SetBodyColor(10);
        var shadow = visual.gameObject.AddComponent<NoShadowBehaviour>();
        shadow.rend = visual.cosmetics.currentBodySprite.BodySprite;
        shadow.hitOverride = visual.Collider;
        SetPosition(owner.start);
        lastClearPosition = owner.start;
        if (tour && !BuildTour(owner.start))
            return;
        if (!tour)
            stops.Enqueue((owner.goal, "marked goal"));
        NextStop();
    }

    public bool BuildTour(Vector2 start)
    {
        var probe = MapPathfinding.CreateRequest(start, start);
        if (probe.grid == null)
        {
            probe.Cancel();
            Finish("NPC start is not on clear floor.");
            return false;
        }

        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (!room.roomArea)
                continue;
            var center = (Vector2)room.roomArea.bounds.center;
            var best = float.MaxValue;
            var point = center;
            var bounds = room.roomArea.bounds;
            for (var y = bounds.min.y; y <= bounds.max.y; y += probe.options.CellSize)
            for (var x = bounds.min.x; x <= bounds.max.x; x += probe.options.CellSize)
            {
                var candidate = new Vector2(x, y);
                var distance = Vector2.Distance(candidate, center);
                if (distance >= best || !room.roomArea.OverlapPoint(candidate) || !probe.grid.bounds.Contains(candidate) || !probe.IsClear(candidate))
                    continue;
                best = distance;
                point = candidate;
            }

            if (best < float.MaxValue)
                stops.Enqueue((point, room.name));
            else
                skipped++;
        }

        stops.Enqueue((start, "marked start"));
        probe.Cancel();
        return true;
    }

    public void NextStop()
    {
        if (stops.Count == 0)
        {
            Finish(touring ? $"NPC returned to start. Visited {visited} rooms; skipped {skipped}." : "NPC reached the marked goal.");
            return;
        }

        (destination, destinationName) = stops.Dequeue();
        returning = touring && stops.Count == 0;
        retries = 0;
        Plan();
    }

    public void Plan()
    {
        request?.Cancel();
        request = new MapPathRequest(ShipStatus.Instance, visual.GetTruePosition(), destination, new PathOptions(CellSize: retries > 0 ? 0.2f : 0.35f, WaitForDoors: true, UseVents: PlayerControl.LocalPlayer.Data.Role.CanVent), PlayerControl.LocalPlayer);
        path = null;
        SetWalking(false);
        preview.message = $"NPC finding route to {destinationName}...";
    }

    public void FixedUpdate()
    {
        if (!visual || !preview || MeetingHud.Instance || !ShipStatus.Instance)
        {
            if (preview) preview.message = "NPC stopped: meeting or map unavailable.";
            Dispose();
            return;
        }

        if (crossing)
        {
            Traverse();
            return;
        }

        if (request == null)
            return;
        if (waitingDoor)
        {
            if (!waitingDoor.IsOpen)
            {
                SetWalking(false);
                preview.message = $"NPC waiting for {waitingDoor.name} to open.";
                return;
            }

            waitingDoor = null;
            preview.message = $"NPC continuing to {destinationName}";
        }

        if (recoveryWait > 0f)
        {
            recoveryWait -= Time.fixedDeltaTime;
            return;
        }

        if (path == null)
        {
            request.Step();
            if (request.Status == PathStatus.Searching)
                return;
            if (request.Status != PathStatus.Found)
            {
                if ((request.Status == PathStatus.MapChanged || request.Status == PathStatus.InvalidEndpoint) && retries++ < 8)
                {
                    RecoverPath();
                    recoveryWait = 0.25f;
                    Plan();
                }
                else if (!request.IsClear(visual.GetTruePosition()))
                    Finish("NPC stopped: current position is blocked; remaining tour stops were preserved.");
                else if (touring && !returning)
                {
                    skipped++;
                    Info($"Pathfinding NPC skipped {destinationName}: {request.Status}");
                    NextStop();
                }
                else
                    Finish($"NPC could not reach {destinationName}: {request.Status}.");

                return;
            }

            path = request.Result;
            waypoint = 1;
            preview.DrawPath(path);
            preview.message = $"NPC travelling to {destinationName} | {stops.Count} stops remaining";
        }

        if (waypoint >= path.Points.Length)
        {
            if (touring && !returning) visited++;
            NextStop();
            return;
        }

        foreach (var step in path.Crossings)
            if (step.PointIndex == waypoint - 1)
            {
                BeginTraversal(step);
                return;
            }

        var position = visual.GetTruePosition();
        var next = Vector2.MoveTowards(position, path.Points[waypoint], Speed * Time.fixedDeltaTime);
        if (!request.CanMove(position, next))
        {
            SetWalking(false);
            waitingDoor = FindClosedDoor(position, path.Points[waypoint]);
            if (waitingDoor) return;
            if (retries++ < 8)
            {
                RecoverPath();
                recoveryWait = 0.25f;
                Plan();
            }
            else
            {
                var blocker = request.IsClear(position) ? "segment collision" : request.overlaps[0].name;
                Info($"Pathfinding NPC blocked at {position}, target={path.Points[waypoint]}, blocker={blocker}");
                Finish($"NPC stopped after recovery attempts: {blocker}.");
            }

            return;
        }

        SetWalking(true);
        Face(next - position);
        lastClearPosition = position;
        SetPosition(next);
        progress += Vector2.Distance(position, next);
        if (progress >= 0.5f)
        {
            retries = 0;
            progress = 0f;
        }

        if ((next - path.Points[waypoint]).sqrMagnitude == 0f)
            waypoint++;
    }

    [HideFromIl2Cpp]
    public void BeginTraversal(PathCrossing step)
    {
        if (!step.Source || !step.Source.Cast<Behaviour>().isActiveAndEnabled)
        {
            Finish("NPC stopped: crossing is no longer available.");
            return;
        }

        interaction = step;
        crossing = true;
        crossingTime = 0f;
        traversalPoints.Clear();
        traversalSpeed = Speed;
        SetWalking(false);
        if (step.Type == PathTraversal.Ladder)
        {
            var ladder = step.Source.Cast<Ladder>();
            traversalPoints.Enqueue(ladder.transform.position);
            traversalPoints.Enqueue(ladder.Destination.transform.position);
            traversalPoints.Enqueue(path.Points[waypoint]);
            visual.cosmetics.SetFlipXWithoutPet(false);
            visual.MyPhysics.Animations.PlayClimbAnimation(ladder.IsTop);
            traversalSpeed = Speed * (ladder.IsTop ? 2f : 1f);
        }
        else if (step.Type == PathTraversal.Zipline)
        {
            zipConsole = step.Source.Cast<ZiplineConsole>();
            zipPhase = 0;
            zipTime = 0f;
        }
        else if (step.Type == PathTraversal.Vent)
        {
            ventExit = null;
            var vent = step.Source.Cast<Vent>();
            foreach (var next in new[] { vent.Left, vent.Right, vent.Center })
                if (next && Vector2.Distance(next.transform.position + next.Offset, path.Points[waypoint]) <= next.UsableDistance)
                {
                    ventExit = next;
                    break;
                }

            if (!PlayerControl.LocalPlayer.Data.Role.CanVent || !ventExit)
            {
                Finish("NPC vent traversal is not permitted.");
                return;
            }

            ventPhase = 0;
            ventAnimation = null;
        }
        else if (step.Type == PathTraversal.MovingPlatform)
        {
            platform = step.Source.Cast<MovingPlatformBehaviour>();
            var parent = platform.transform.parent;
            var left = (Vector2)parent.TransformPoint(platform.LeftUsePosition);
            var right = (Vector2)parent.TransformPoint(platform.RightUsePosition);
            var fromLeft = Vector2.Distance(visual.GetTruePosition(), left) < Vector2.Distance(visual.GetTruePosition(), right);
            platformStart = parent.TransformPoint(fromLeft ? platform.LeftPosition : platform.RightPosition);
            platformEnd = parent.TransformPoint(fromLeft ? platform.RightPosition : platform.LeftPosition);
            platformExit = parent.TransformPoint(fromLeft ? platform.RightUsePosition : platform.LeftUsePosition);
            platformPhase = 0;
        }
        else if (step.Type == PathTraversal.Decontamination)
        {
            decon = null;
            foreach (var system in ShipStatus.Instance.GetComponentsInChildren<DeconSystem>())
                if (system.UpperDoor == step.Source || system.LowerDoor == step.Source)
                {
                    decon = system;
                    break;
                }

            if (!decon)
            {
                Finish("NPC stopped: decontamination system not found.");
                return;
            }
        }

        if (step.Type != PathTraversal.Ladder) traversalPoints.Enqueue(path.Points[waypoint]);
        preview.message = $"NPC using {step.Type} toward {destinationName}";
    }

    public void Traverse()
    {
        crossingTime += Time.fixedDeltaTime;
        if (!interaction.Source || (interaction.Type is not (PathTraversal.Door or PathTraversal.MovingPlatform) && crossingTime > 30f))
        {
            Finish("NPC stopped: crossing unavailable or timed out.");
            return;
        }

        if (interaction.Type == PathTraversal.Zipline)
        {
            TraverseZipline();
            return;
        }

        if (interaction.Type == PathTraversal.Vent)
        {
            TraverseVent();
            return;
        }

        if (interaction.Type == PathTraversal.MovingPlatform)
        {
            TraversePlatform();
            return;
        }

        var position = visual.GetTruePosition();
        if (interaction.Type == PathTraversal.Door && !request.CanMove(position, traversalPoints.Peek()))
        {
            SetWalking(false);
            preview.message = $"NPC waiting for {interaction.Source.name} to open.";
            return;
        }

        if (interaction.Type == PathTraversal.Decontamination && !request.CanMove(position, traversalPoints.Peek()))
        {
            SetWalking(false);
            if (decon.CurState == DeconSystem.States.Idle)
            {
                var upper = decon.UpperDoor == interaction.Source;
                if (decon.RoomArea.OverlapPoint(position)) decon.OpenFromInside(upper);
                else decon.OpenDoor(upper);
            }

            return;
        }

        if (interaction.Type is PathTraversal.Door or PathTraversal.Decontamination) SetWalking(true);
        if (interaction.Type == PathTraversal.Ladder && traversalPoints.Count == 1) SetWalking(true);
        var target = traversalPoints.Peek();
        var next = Vector2.MoveTowards(position, target, traversalSpeed * Time.fixedDeltaTime);
        if (interaction.Type != PathTraversal.Ladder) Face(next - position);
        SetPosition(next);
        if ((next - target).sqrMagnitude != 0f)
            return;
        traversalPoints.Dequeue();
        if (traversalPoints.Count > 0)
            return;
        CompleteTraversal();
    }

    public void CompleteTraversal()
    {
        ReleaseZipline();
        ReleasePlatform();
        var landing = path.Points[waypoint];
        SetPosition(landing);
        if (!request.IsClear(visual.GetTruePosition()))
        {
            Finish("NPC crossing exit is blocked; stopped before starting the next tour leg.");
            return;
        }

        lastClearPosition = landing;
        retries = 0;
        crossing = false;
        waypoint++;
        visual.MyPhysics.Animations.PlayIdleAnimation();
        visual.cosmetics.AnimateSkinIdle();
    }

    public OpenableDoor FindClosedDoor(Vector2 from, Vector2 to)
    {
        var delta = to - from;
        var count = Physics2D.CircleCast(from, request.options.Radius, delta.normalized, request.filter, request.crossingHits, delta.magnitude);
        for (var i = 0; i < count; i++)
        {
            var collider = request.crossingHits[i].collider;
            var door = collider.GetComponentInParent<OpenableDoor>();
            if (door && !door.IsOpen) return door;
        }

        return null;
    }

    public bool RecoverPath()
    {
        var position = visual.GetTruePosition();
        if (!request.IsClear(position))
        {
            if (Vector2.Distance(position, lastClearPosition) <= 0.15f && request.IsClear(lastClearPosition))
            {
                SetPosition(lastClearPosition);
                return true;
            }

            return false;
        }

        var target = path != null && waypoint < path.Points.Length ? path.Points[waypoint] : destination;
        var direction = (target - position).normalized;
        var side = new Vector2(-direction.y, direction.x);
        for (var i = 1; i <= 4; i++)
        for (var sign = -1; sign <= 1; sign += 2)
        {
            var candidate = position + side * (sign * i * 0.05f) - direction * 0.025f;
            if (!request.CanMove(position, candidate) || !request.CanMove(candidate, Vector2.MoveTowards(candidate, target, 0.1f)))
                continue;
            lastClearPosition = position;
            SetPosition(candidate);
            return true;
        }

        return false;
    }

    public void TraverseZipline()
    {
        if (!zipConsole || !zipConsole.zipline)
        {
            Finish("NPC zipline disappeared.");
            return;
        }

        var zipline = zipConsole.zipline;
        var top = zipConsole.atTop;
        zipTime += Time.fixedDeltaTime;
        if (zipPhase == 0)
        {
            var handle = (Vector2)zipline.GetHandlePos(top) + visual.Collider.offset;
            var next = WalkTowards(handle);
            if ((next - handle).sqrMagnitude != 0f) return;
            SetWalking(false);
            zipHand = Instantiate(zipline.handPool.Prefab).Cast<HandZiplinePoolable>();
            zipHand.transform.SetParent(transform, true);
            zipHand.gameObject.SetActive(true);
            zipHand.handRenderer.sharedMaterial = CosmeticsLayer.GetBodyMaterial(PlayerMaterial.MaskType.None);
            PlayerMaterial.SetColors(10, zipHand.handRenderer);
            zipHand.handRenderer.color = Color.white;
            handStart = (top ? zipline.upHandPosition : zipline.downHandPosition).position;
            zipHand.transform.position = handStart;
            if (top) zipHand.StartDownAnimation();
            else zipHand.StartUpAnimation();
            PlayEffect(zipline.attachSound, false);
            visual.cosmetics.AnimateSkinJump();
            StartCoroutine(visual.MyPhysics.Animations.CoPlayJumpAnimation());
            zipStart = next;
            zipTime = 0f;
            zipPhase = 1;
        }
        else if (zipPhase == 1)
        {
            var t = Mathf.Clamp01(zipTime / Mathf.Max(zipline.timeJump, 0.01f));
            var curve = top ? zipline.jumpZiplineCurve : zipline.jumpZiplineCurveBottom;
            SetPosition(zipStart + Vector2.up * curve.Evaluate(t));
            zipHand.transform.position = handStart + Vector3.up * zipline.jumpZiplineHandCurve.Evaluate(t);
            if (zipTime < zipline.timeJump + 0.1f) return;
            zipStart = visual.GetTruePosition();
            handStart = zipHand.transform.position - visual.transform.position;
            zipTime = 0f;
            zipDownLoop = false;
            PlayEffect(top ? zipline.downSound : zipline.upSound, !top);
            zipPhase = 2;
        }
        else if (zipPhase == 2)
        {
            if (top && !zipDownLoop && zipTime >= Mathf.Max(0f, zipline.downSound.length - 0.05f))
            {
                traversalAudio.Stop();
                PlayEffect(zipline.downLoopSound, true);
                zipDownLoop = true;
            }

            var drop = (Vector2)(top ? zipline.dropPositionBottom : zipline.dropPositionTop).position + visual.Collider.offset;
            var seconds = top ? zipline.downTravelTime : zipline.upTravelTime;
            var t = Mathf.Clamp01(zipTime / Mathf.Max(seconds, 0.1f));
            SetPosition(Vector2.Lerp(zipStart, drop, Mathf.SmoothStep(0f, 1f, t)));
            zipHand.transform.position = visual.transform.position + handStart;
            if (t < 1f) return;
            traversalAudio.Stop();
            PlayEffect(zipline.detachSound, false);
            if (top) zipHand.StartDownOutroAnimation();
            else zipHand.StartUpOutroAnimation();
            zipPhase = 3;
        }
        else
        {
            if (zipPhase == 5)
            {
                if (zipTime < 0.1f) return;
                CompleteTraversal();
                return;
            }

            var landing = top ? zipline.landingPositionBottom : zipline.landingPositionTop;
            var target = zipPhase == 3 ? (Vector2)zipline.transform.TransformPoint(landing.position) : path.Points[waypoint];
            var next = WalkTowards(target);
            if ((next - target).sqrMagnitude != 0f) return;
            if (zipPhase == 3) zipPhase = 4;
            else
            {
                SetWalking(false);
                zipTime = 0f;
                zipPhase = 5;
            }
        }
    }

    public void TraverseVent()
    {
        if (!ventExit || !request.VentAvailable(ventExit) || !PlayerControl.LocalPlayer.Data.Role.CanVent)
        {
            visual.cosmetics.Visible = true;
            Finish("NPC vent became unavailable.");
            return;
        }

        if (ventPhase == 0)
        {
            var vent = interaction.Source.Cast<Vent>();
            var target = (Vector2)(vent.transform.position + vent.Offset);
            var next = WalkTowards(target);
            if (next != target) return;
            SetWalking(false);
            vent.EnterVent(visual);
            PlayEffect(ShipStatus.Instance.VentEnterSound, false);
            visual.cosmetics.AnimateSkinEnterVent();
            ventAnimation = visual.MyPhysics.Animations.CoPlayEnterVentAnimation(vent.NumFramesUntilPlayerDisappears);
            ventPhase = 1;
        }

        if (ventPhase == 1)
        {
            if (ventAnimation.MoveNext()) return;
            SetWalking(false);
            visual.cosmetics.Visible = false;
            SetPosition(ventExit.transform.position);
            ventAnimation = ventExit.ExitVent(visual);
            ventPhase = 2;
        }

        if (ventPhase == 2)
        {
            if (ventAnimation.MoveNext()) return;
            visual.cosmetics.Visible = true;
            visual.cosmetics.AnimateSkinExitVent();
            ventAnimation = visual.MyPhysics.Animations.CoPlayExitVentAnimation();
            PlayEffect(ShipStatus.Instance.VentExitSound ? ShipStatus.Instance.VentExitSound : ShipStatus.Instance.VentEnterSound, false);
            ventPhase = 3;
        }

        if (ventPhase == 3)
        {
            if (ventAnimation.MoveNext()) return;
            ventAnimation = null;
            SetWalking(false);
            ventPhase = 4;
        }

        var exitTarget = path.Points[waypoint];
        var exitNext = WalkTowards(exitTarget);
        if (exitNext == exitTarget) CompleteTraversal();
    }

    public void TraversePlatform()
    {
        if (!platform || !platform.isActiveAndEnabled)
        {
            Finish("NPC platform is unavailable.");
            return;
        }

        if (platformPhase == 0)
        {
            var available = !platform.InUse && Vector2.Distance(platform.transform.position, platformStart) < 0.1f;
            if (!AmongUsClient.Instance.AmHost || !available)
            {
                preview.message = "NPC waiting for the real platform; checking another route.";
                if (platform.InUse && crossingTime < 5f) return;
                platformAlternative ??= new MapPathRequest(ShipStatus.Instance, visual.GetTruePosition(), destination, request.options with { UseMovingPlatforms = false }, PlayerControl.LocalPlayer);
                platformAlternative.Step();
                if (platformAlternative.Status == PathStatus.Found)
                {
                    request = platformAlternative;
                    path = request.Result;
                    platformAlternative = null;
                    platform = null;
                    crossing = false;
                    waypoint = 1;
                    preview.DrawPath(path);
                }

                return;
            }

            platformAlternative?.Cancel();
            platformAlternative = null;
            platform.Target = visual;
            platformPhase = 1;
        }

        if (platform.Target != visual)
        {
            Finish("NPC platform reservation was interrupted.");
            return;
        }

        var position = visual.GetTruePosition();
        var target = platformPhase == 1 ? platformStart : platformPhase == 2 ? platformEnd : platformPhase == 3 ? platformExit : path.Points[waypoint];
        SetWalking(platformPhase != 2);
        var next = Vector2.MoveTowards(position, target, Speed * Time.fixedDeltaTime);
        Face(next - position);
        SetPosition(next);
        if (platformPhase == 2) platform.transform.position = new Vector3(next.x, next.y, platform.transform.position.z);
        if ((next - target).sqrMagnitude != 0f) return;
        if (platformPhase == 1) PlayEffect(platform.MovingSound, true);
        if (platformPhase == 2) traversalAudio.Stop();
        if (platformPhase == 4) CompleteTraversal();
        else platformPhase++;
    }

    public void ReleasePlatform()
    {
        platformAlternative?.Cancel();
        platformAlternative = null;
        if (platform && visual && platform.Target == visual)
        {
            var parent = platform.transform.parent;
            var left = parent.TransformPoint(platform.LeftPosition);
            var right = parent.TransformPoint(platform.RightPosition);
            platform.Target = null;
            platform.SetSide(Vector2.Distance(platform.transform.position, left) <= Vector2.Distance(platform.transform.position, right));
        }

        platform = null;
    }

    public void PlayEffect(AudioClip clip, bool loop)
    {
        if (!clip || !Constants.ShouldPlaySfx()) return;
        if (!loop) traversalAudio.PlayOneShot(clip);
        else
        {
            traversalAudio.clip = clip;
            traversalAudio.loop = true;
            traversalAudio.Play();
        }
    }

    public void ReleaseZipline()
    {
        StopAllCoroutines();
        ventAnimation = null;
        if (traversalAudio) traversalAudio.Stop();
        if (zipHand) Destroy(zipHand.gameObject);
        zipHand = null;
        zipConsole = null;
    }

    public Vector2 WalkTowards(Vector2 target)
    {
        var position = visual.GetTruePosition();
        var next = Vector2.MoveTowards(position, target, Speed * Time.fixedDeltaTime);
        SetWalking(true);
        Face(next - position);
        SetPosition(next);
        return next;
    }

    public void SetPosition(Vector2 position)
    {
        var origin = position - visual.Collider.offset;
        visual.transform.position = new Vector3(origin.x, origin.y, origin.y / 1000f);
    }

    public void Face(Vector2 velocity)
    {
        if (Mathf.Abs(velocity.x) > 0.001f)
            visual.cosmetics.SetFlipXWithoutPet(velocity.x < 0f);
    }

    public void SetWalking(bool moving)
    {
        var animations = visual.MyPhysics.Animations;
        if (moving && !animations.IsPlayingRunAnimation())
        {
            animations.PlayRunAnimation();
            visual.cosmetics.AnimateSkinRun();
        }
        else if (!moving)
        {
            animations.PlayIdleAnimation();
            visual.cosmetics.AnimateSkinIdle();
        }
    }

    public void Finish(string message)
    {
        ReleasePlatform();
        ReleaseZipline();
        if (preview) preview.message = message;
        request?.Cancel();
        request = null;
        crossing = false;
        if (visual)
        {
            visual.cosmetics.Visible = true;
            SetWalking(false);
        }
    }

    public void ReleaseNpc()
    {
        ReleasePlatform();
        ReleaseZipline();
        request?.Cancel();
        request = null;
        if (visual) Destroy(visual.gameObject);
        visual = null;
    }

    public void Dispose()
    {
        ReleaseNpc();
        Destroy(gameObject);
    }

    public void OnDisable()
    {
        ReleaseNpc();
    }
}