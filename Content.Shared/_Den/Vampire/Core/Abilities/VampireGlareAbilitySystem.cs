using Content.Shared._Den.Vampire.Core.Abilities.Components;
using Content.Shared._Den.Vampire.Core.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Map;

namespace Content.Shared._Den.Vampire.Core.Abilities;

public sealed class VampireGlareAbilitySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStaminaSystem  _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireGlareAbilityComponent, VampireGlareAbilityEvent>(OnGlare);
        SubscribeLocalEvent<VampireGlareAbilityComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<VampireGlareAbilityComponent> entity, ref MapInitEvent args)
    {
        _action.AddAction(entity, ref entity.Comp.Action, entity.Comp.ActionProto, entity);
    }

    public void OnGlare(Entity<VampireGlareAbilityComponent> ent, ref VampireGlareAbilityEvent args)
    {
        var (coords,facing) = _transform.GetMoverCoordinateRotation(ent, Transform(ent));
        facing = new Angle(facing.ToWorldVec());

        _popup.PopupPredicted(Loc.GetString("vampire-glare-alert", ("vampire", ent)), ent, ent, PopupType.Medium);

        PredictedSpawnAttachedTo(ent.Comp.FlashEffectProto, coords);

        // todo: Make it to where when the vampire is on the ground or restrained, all sides count as a side attack.
        var glareDirections = new[]
        {
            (0, ent.Comp.DamageFront, true, true),     // Front
            (-90, ent.Comp.DamageSides, true, false),  // Left
            (90, ent.Comp.DamageSides, true, false),   // Right
            (180, ent.Comp.DamageRear, false, false)   // Rear
        };

        foreach (var (angleOffset, damage, knockdown, stun) in glareDirections)
        {
            GlareStun(ent, coords, facing + Angle.FromDegrees(angleOffset), damage, knockdown, stun);
        }

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
                _stun.TryKnockdown(target, user.Comp.KnockdownTime, false);
            if (stun)
                _stun.TryUpdateStunDuration(target, user.Comp.StunTime);

            _stamina.TakeStaminaDamage(target, damage);
        }
    }
}
