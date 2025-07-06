using Content.Shared._Den.Vampire.Abilities.Components;
using Content.Shared._Den.Vampire.Events;
using Content.Shared.Stunnable;
using Content.Shared.Damage.Systems;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Shared._Den.Vampire.Abilities;

public sealed class VampireGlareAbilitySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStaminaSystem  _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stuns = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireGlareAbilityComponent, VampireGlareAbility>(OnGlare);
    }

    public void OnGlare(Entity<VampireGlareAbilityComponent> ent, ref VampireGlareAbility args)
    {
        var (coords,facing) = _transform.GetMoverCoordinateRotation(ent, Transform(ent));
        facing = new Angle(facing.ToWorldVec());

        _popup.PopupPredicted(Loc.GetString("vampire-glare-alert", ("vampire", ent)), ent, ent, PopupType.Medium);

        PredictedSpawnAttachedTo(ent.Comp.FlashEffectProto, coords);

        // todo: Make it to where when the vampire is on the ground or restrained, all sides count as a side attack.
        GlareStun(ent, coords, facing, ent.Comp.DamageFront, true, true);
        GlareStun(ent, coords, facing + Angle.FromDegrees(-90), ent.Comp.DamageSides, true);
        GlareStun(ent, coords, facing + Angle.FromDegrees(90),  ent.Comp.DamageSides, true);
        GlareStun(ent, coords, facing + Angle.FromDegrees(180), ent.Comp.DamageRear);

        args.Handled = true;
    }

    private void GlareStun(Entity<VampireGlareAbilityComponent> user,
        EntityCoordinates coords,
        Angle angle,
        float damage,
        bool knockdown = false,
        bool stun = false)
    {
        var nearbyEntities = _lookup.GetEntitiesInArc(coords, user.Comp.Range, angle, 90, LookupFlags.Uncontained);
        foreach (var target in nearbyEntities)
        {
            if (target == user.Owner)
                continue;
            if (knockdown)
                _stuns.TryKnockdown(target, user.Comp.KnockdownTime, false);
            if (stun)
                _stuns.TryStun(target, user.Comp.StunTime, false);
            _stamina.TakeStaminaDamage(target, damage);
        }
    }
}
