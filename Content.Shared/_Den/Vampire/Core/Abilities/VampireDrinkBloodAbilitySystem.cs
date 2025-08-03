using Content.Shared._Den.Vampire.Components;
using Content.Shared._Den.Vampire.Core.Abilities.Components;
using Content.Shared._Den.Vampire.Core.Events;
using Content.Shared.Body.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;

namespace Content.Shared._Den.Vampire.Core.Abilities;

/// <summary>
/// </summary>
public sealed class VampireDrinkBloodAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireDrinkBloodAbilityComponent, VampireDrinkBloodAbilityEvent>(OnFeedStart);
        SubscribeLocalEvent<VampireDrinkBloodAbilityComponent, VampireDrinkBloodAbilityDoAfterEvent>(OnFeedEnd);
        SubscribeLocalEvent<VampireDrinkBloodAbilityComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<VampireDrinkBloodAbilityComponent> entity, ref MapInitEvent args)
    {
       _action.AddAction(entity, ref entity.Comp.Action, entity.Comp.ActionProto, entity);
    }

    private void OnFeedStart(Entity<VampireDrinkBloodAbilityComponent> ent, ref VampireDrinkBloodAbilityEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var target = args.Target;

        if (target == args.Performer)
            return;

        if (!HasComp<BloodstreamComponent>(target))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.DrinkBloodDuration, new VampireDrinkBloodAbilityDoAfterEvent(), ent, target: target, used: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnWeightlessMove = false,
        });

        _popup.PopupPredicted(Loc.GetString("vampire-feeding-on-vampire", ("target", target)), ent, ent, PopupType.Medium);
        _popup.PopupPredicted(Loc.GetString("vampire-feeding-on-target", ("vampire", ent)), ent, target, PopupType.LargeCaution);
    }

    private void OnFeedEnd(Entity<VampireDrinkBloodAbilityComponent> ent, ref VampireDrinkBloodAbilityDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        var target = args.Args.Target;

        if (target != null)
        {
            DrinkBlood(ent, target.Value);
            _popup.PopupPredicted(Loc.GetString("vampire-feeding-successful-vampire", ("target", target.Value)), ent, ent, PopupType.Medium);
            _popup.PopupPredicted(Loc.GetString("vampire-feeding-successful-target", ("vampire", ent.Owner)), ent.Owner, target.Value, PopupType.MediumCaution);
        }
    }

    public void DrinkBlood(Entity<VampireDrinkBloodAbilityComponent> ent, EntityUid target)
    {
        if (!TryComp<VampireThirstBloodComponent>(ent, out var thirst) ||
            !TryComp<BloodstreamComponent>(target, out var bloodstream))
        {
            Logger.Info($"[Vampire] DrinkBlood aborted: missing component on ent={ent} or target={target}");
            return;
        }

        _solutionContainer.ResolveSolution(target, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution);

        if (bloodSolution is null)
        {
            Logger.Info($"[Vampire] No blood solution found on target={target}");
            return;
        }

        var reagent = bloodstream.BloodReagent;
        var reagentIdString = bloodstream.BloodReagent.Id;
        var reagentId = new ReagentId(reagentIdString, null);
        var toRemove = ent.Comp.BloodDrainAmountRemove;

        if (!bloodSolution.TryGetReagentQuantity(reagentId, out var available) || available <= FixedPoint2.Zero)
        {
            Logger.Info($"[Vampire] No available blood to remove on target={target} (requested={toRemove}, available={available})");
            return;
        }

        var actualRemove = FixedPoint2.Min(toRemove, available);
        Logger.Info($"[Vampire] Removing blood from target={target}: requested={toRemove}, actual={actualRemove}");

        if (_mobState.IsDead(target))
            return;

        bloodSolution.RemoveReagent(reagent, actualRemove);
        thirst.CurrentThirstBlood += ent.Comp.BloodDrainAmountGive;

        Logger.Info($"[Vampire] Thirst increased on ent={ent}: now={thirst.CurrentThirstBlood}");
    }
}
