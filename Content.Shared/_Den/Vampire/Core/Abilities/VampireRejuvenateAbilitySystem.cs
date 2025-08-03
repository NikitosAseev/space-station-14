using Content.Shared._Den.Vampire.Core.Abilities.Components;
using Content.Shared._Den.Vampire.Core.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Drunk;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;

namespace Content.Shared._Den.Vampire.Core.Abilities;

public sealed class VampireRejuvenateAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedDrunkSystem _drunkSystem = default!;
    [Dependency] private readonly SharedStutteringSystem _stuttering = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;


    public override void Initialize()
    {

        base.Initialize();

        SubscribeLocalEvent<VampireRejuvenateAbilityComponent, VampireRejuvenateAbilityEvent>(OnRejuvenate);
        SubscribeLocalEvent<VampireRejuvenateAbilityComponent, MapInitEvent>(OnMapInit);

    }

    private void OnMapInit(Entity<VampireRejuvenateAbilityComponent> entity, ref MapInitEvent args)
    {
        _action.AddAction(entity, ref entity.Comp.Action, entity.Comp.ActionProto, entity);
    }

    public void OnRejuvenate(Entity<VampireRejuvenateAbilityComponent> ent, ref VampireRejuvenateAbilityEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<VampireRejuvenateAbilityComponent>(ent.Owner, out var rejuvenateComp))
            return;

        RemComp<KnockedDownComponent>(ent);
        RemComp<StunnedComponent>(ent);

        if (TryComp<StaminaComponent>(ent, out var stamina))
        {
            stamina.Critical = false; // Takes us out of stam crit immediately.
            // Notably, we don't get any stamina resistance after this from after stam-crit effects.
            // So it is easy to stam-crit the vampire again.
            _stamina.TakeStaminaDamage(ent, rejuvenateComp.StamHealing, stamina);
        }

        _drunkSystem.TryRemoveDrunkenessTime(ent, rejuvenateComp.StatusEffectReductionTime.TotalSeconds);
        _stuttering.DoRemoveStutterTime(ent, rejuvenateComp.StatusEffectReductionTime.TotalSeconds);

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(ent):user} used Rejuvenate.");
        _popup.PopupEntity(Loc.GetString("vampire-rejuvenate-popup"), ent, ent, PopupType.Medium);

        args.Handled = true;
    }
}
