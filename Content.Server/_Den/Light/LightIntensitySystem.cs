using Robust.Shared.Physics;
using Robust.Shared.Threading;
using System.Numerics;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Shared.Mobs.Systems;
using Content.Shared._Den.Light;
using Content.Shared.Physics;
using Robust.Shared.Map;

namespace Content.Server._Den.Light;
public sealed class LightIntensitySystem : EntitySystem
{

    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;


    private readonly B2DynamicTree<EntityUid> _sourceTree = new();
    private readonly Dictionary<EntityUid, SourceData> _sourceDataMap  = new();
    private readonly Dictionary<EntityUid, DynamicTree.Proxy> _proxyMap = new();
    private readonly List<Entity<LightDetectionComponent>> _detectors = new();
    private readonly struct SourceData
    {
        public readonly EntityUid LighEntity;
        public readonly MapId LightMapId;
        public readonly Vector2 LighWorldPosition;
        public readonly float LightRadius;

        public SourceData(EntityUid lightEntity, MapId lighmapId, Vector2 lighWorldPosition, float lightRadius)
        {
            LighEntity = lightEntity;
            LightMapId = lighmapId;
            LighWorldPosition = lighWorldPosition;
            LightRadius = lightRadius;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightDetectionComponent, ComponentStartup>(OnReceiverStartup);
    }

    private void OnReceiverStartup(Entity<LightDetectionComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.NextUpdate = _timing.CurTime;

    }

    public void UpdateSource()
    {
        var query = EntityQueryEnumerator<PointLightComponent, TransformComponent>();

        while (query.MoveNext(out var ent, out var light, out var xform))
        {
            if (!light.Enabled || light.Energy <= 0f || Terminating(ent))
            {
                RemoveSource(ent);
                continue;
            }

            var worldPos = _transform.GetWorldPosition(xform);
            var mapId = xform.MapID;
            var radius = light.Radius;

            var aabb = Box2.CenteredAround(worldPos, new Vector2(radius * 2f, radius * 2f));

            if (_proxyMap.TryGetValue(ent, out var proxy) && proxy != DynamicTree.Proxy.Free)
            {
                _sourceTree.MoveProxy(proxy, aabb);
            }
            else
            {
                var newProxy = _sourceTree.CreateProxy(aabb, uint.MaxValue, ent);
                _proxyMap[ent] = newProxy;
            }

            _sourceDataMap[ent] = new SourceData(ent, mapId, worldPos, radius);
        }
    }

    private void RemoveSource(EntityUid ent)
    {
        if (_proxyMap.TryGetValue(ent, out var proxy) && proxy != DynamicTree.Proxy.Free)
        {
            _sourceTree.DestroyProxy(proxy);
            _proxyMap.Remove(ent);
        }

        _sourceDataMap.Remove(ent);

    }

    private void DetectLight(Entity<LightDetectionComponent> ent)
    {
        var worldPos = _transform.GetWorldPosition(ent.Owner);

        if ((ent.Comp.LastKnownPosition - worldPos).LengthSquared() < 0.01f)
            return;

        ent.Comp.LastKnownPosition = worldPos;
        var mapId = _transform.GetMapId(ent.Owner);
        var totalIntensity = 0f;

        foreach (var (_, source) in _sourceDataMap)
        {
            if (source.LightMapId != mapId)
                continue;

            var offset = source.LighWorldPosition - worldPos;
            var dist = offset.Length();
            if (dist > source.LightRadius  || dist <= 0.01f) // dist <= 0.01f so the debug stops crashing
                continue;

            var direction = offset / dist;

            var ray = new CollisionRay(source.LighWorldPosition, direction, (int)CollisionGroup.Opaque);

            var rayResults = _physicsSystem.IntersectRay(
                source.LightMapId,
                ray,
                dist,
                source.LighEntity);

            var blocked  = false;

            foreach (var hit in rayResults)
            {
                if (hit.HitEntity != source.LighEntity && hit.HitEntity != ent.Owner)
                {
                    blocked  = true;
                    break;
                }
            }

            if (blocked)
                continue;

            var falloff  = 1f - (dist / source.LightRadius );
            totalIntensity += falloff;
        }

        ent.Comp.CurrentIntensity = totalIntensity;
        ent.Comp.IsOnLight = totalIntensity > 0f;
    }

    private readonly record struct LightDetectionJob : IParallelRobustJob
    {
        public int BatchSize => 4;
        public required LightIntensitySystem System { get; init; }
        public required List<Entity<LightDetectionComponent>> Detectors { get; init; }

        public void Execute(int index)
        {
            var ent = Detectors[index];
            System.DetectLight(ent);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateSource();

        _detectors.Clear();

        var query = EntityQueryEnumerator<LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobStateSystem.IsDead(uid) || _timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = _timing.CurTime + comp.UpdateInterval;
            _detectors.Add((uid, comp));
        }

        var job = new LightDetectionJob
        {
            System = this,
            Detectors = _detectors
        };

        _parallel.ProcessNow(job, _detectors.Count);
    }
}
