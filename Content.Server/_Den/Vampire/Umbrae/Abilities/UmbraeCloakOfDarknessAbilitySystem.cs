using Content.Shared._Den.Vampire.Umbrae.Abilities;
using Content.Shared._Den.Vampire.Umbrae.Abilities.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;


namespace Content.Server._Den.Vampire.Umbrae.Abilities;

public sealed class UmbraeCloakOfDarknessAbilitySystem : SharedUmbraeCloakOfDarknessAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    public void CalculateLocalLight(Entity<UmbraeCloakOfDarknessAbilityComponent> ent, StealthComponent stealth)
    {
        var pos = _transform.GetMapCoordinates(ent);

        foreach (var lightEnt in _lookup.GetEntitiesInRange<PointLightComponent>(pos, ent.Comp.Range))
        {
            if (!TryComp<PointLightComponent>(lightEnt, out var light))
                continue;

            if (light.Enabled == false || light.Energy <= 0f)
                continue;

            var dist = (_transform.GetWorldPosition(ent) - _transform.GetWorldPosition(lightEnt)).Length();
            var lightRadius = light.Radius;

            var visibility = 1.0f - Math.Clamp(dist / lightRadius, 0f, 1f);

            _stealth.SetVisibility(ent, visibility, stealth);
            Logger.Info(
                $"[Umbrae] Light {lightEnt}: dist={dist:0.00}, radius={lightRadius:0.00}, visibility={visibility:0.00} for entity {ent}");
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<UmbraeCloakOfDarknessAbilityComponent, StealthComponent>();
        while (query.MoveNext(out var ent, out var comp, out var stealth))
        {
            if (_timing.CurTime < comp.NextUpdate || !stealth.Enabled)
                continue;

            comp.NextUpdate = _timing.CurTime + comp.UpdateInterval;

            Logger.Info($"[Umbrae] Updating visibility for {ent} at time {_timing.CurTime.TotalSeconds:0.00}");
            CalculateLocalLight((ent, comp), stealth);
        }
    }
}
