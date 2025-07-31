using Content.Shared._Den.Vampire.Umbrae.Abilities;
using Content.Shared._Den.Vampire.Umbrae.Abilities.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;
using Content.Shared._Den.Light;


namespace Content.Server._Den.Vampire.Umbrae.Abilities;

public sealed class UmbraeCloakOfDarknessAbilitySystem : SharedUmbraeCloakOfDarknessAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UmbraeCloakOfDarknessAbilityComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<UmbraeCloakOfDarknessAbilityComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.NextCloakOfDarknessUpdate = _timing.CurTime;
    }

    public void CalculateLocalLight(Entity<UmbraeCloakOfDarknessAbilityComponent> ent, StealthComponent stealth)
    {
        if (!TryComp<LightDetectionComponent>(ent.Owner, out var lightDetect))
            return;

        var light = lightDetect.CurrentIntensity;

        var weight = 1f / (1f + light);
        var visibility = MathHelper.Lerp(stealth.MinVisibility, stealth.MaxVisibility, weight);
        var lastVisibility = _stealth.GetVisibility(ent, stealth);
        var resultVisibility = MathHelper.Lerp(lastVisibility, visibility, ent.Comp.Smoothing);

        Logger.Info($"resultVisibility : {resultVisibility}");

        _stealth.SetVisibility(ent, resultVisibility, stealth);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<UmbraeCloakOfDarknessAbilityComponent, StealthComponent>();
        while (query.MoveNext(out var ent, out var comp, out var stealth))
        {
            if(!stealth.Enabled)
                continue;

            if (_timing.CurTime >= comp.NextCloakOfDarknessUpdate)
            {
                comp.NextCloakOfDarknessUpdate = _timing.CurTime + comp.UpdateCloakOfDarknessInterval;
                CalculateLocalLight((ent, comp), stealth);
            }
        }
    }
}
