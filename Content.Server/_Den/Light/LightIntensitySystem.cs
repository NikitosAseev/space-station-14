using System.Linq;
using Robust.Shared.Physics;
using Robust.Shared.Threading;
using System.Numerics;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Shared.Mobs.Systems;
using Content.Shared._Den.Light;
using Content.Shared.Physics;

namespace Content.Server._Den.Light;
public sealed class LightIntensitySystem : EntitySystem
{

    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;

    private readonly Dictionary<EntityUid, SourceData> _sources = new();

    private record struct SourceData(EntityUid Ent, TransformComponent Xform , Vector2 WorldPosition, float Radius);

    public override void Initialize()
    {
        SubscribeLocalEvent<LightDetectionComponent, ComponentStartup>(OnReceiverStartup);

    }

    private void OnReceiverStartup(Entity<LightDetectionComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.NextUpdate = _timing.CurTime;
    }


    public void UpdateLight()
    {
        var toRemove = new HashSet<EntityUid>(_sources.Keys);

        var query = EntityQueryEnumerator<PointLightComponent, TransformComponent>();

        var addedCount = 0;
        var removedCount = 0;


        while (query.MoveNext(out var ent, out var comp, out var xform))
        {
            if (!comp.Enabled || comp.Energy <= 0f || Terminating(ent))
            {
                _sources.Remove(ent);
                continue;
            }

            var worldPos = _transform.GetWorldPosition(xform);
            var data = new SourceData(ent, xform , worldPos, comp.Radius);
            _sources[ent] = data;
            toRemove.Remove(ent);
        }

        foreach (var ent in toRemove)
        {
            _sources.Remove(ent);
        }

    }

    private void DetectLight(Entity<LightDetectionComponent> ent)
    {
        var worldPos = _transform.GetWorldPosition(ent.Owner);

        if ((ent.Comp.LastKnownPosition - worldPos).LengthSquared() < 0.01f)
            return;

        ent.Comp.LastKnownPosition = worldPos;

        ent.Comp.IsOnLight = false;

        var accumulatedIntensity = 0f;

        var contributingSources = new List<(EntityUid SourceEnt, float Intensity)>();

        foreach (var (_, source) in _sources)
        {
            var dist = (source.WorldPosition - worldPos).Length();
            if (dist > source.Radius)
                continue;

            if (dist <= 0.01f) // So the debug stops crashing
                continue;

            var direction = (worldPos - source.WorldPosition).Normalized();

            var ray = new CollisionRay(source.WorldPosition, direction, (int)CollisionGroup.Opaque);

            var rayResults = _physicsSystem.IntersectRay(
                source.Xform.MapID,
                ray,
                dist,
                source.Ent);

            var hasBeenBlocked = false;

            foreach (var hit in rayResults)
            {
                if (hit.HitEntity != source.Ent && hit.HitEntity != ent.Owner)
                {
                    hasBeenBlocked = true;
                    break;
                }
            }

            if (hasBeenBlocked)
                continue;

            var falloff  = 1f - (dist / source.Radius);
            accumulatedIntensity += falloff;
            contributingSources.Add((source.Ent, falloff));
        }


        if (accumulatedIntensity > 0f)
        {
            ent.Comp.IsOnLight = true;
            ent.Comp.CurrentIntensity = accumulatedIntensity;
        }
        else
        {
            ent.Comp.IsOnLight = false;
            ent.Comp.CurrentIntensity = 0f;
        }

        var time = _timing.CurTime;
        var sourceDetails = string.Join(", ", contributingSources.Select(cs => $"[{cs.SourceEnt}: {cs.Intensity:F2}]"));
        Logger.Info($"[{time}] Entity {ent.Owner} - CurrentIntensity: {ent.Comp.CurrentIntensity:F3}, IsOnLight: {ent.Comp.IsOnLight}, SourcesCount: {contributingSources.Count}, Sources: {sourceDetails}");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateLight();

        var query = EntityQueryEnumerator<LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobStateSystem.IsDead(uid))
                continue;

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = _timing.CurTime + comp.UpdateInterval;
            DetectLight((uid, comp));
        }

    }

}
