using Content.Shared._Den.Vampire.Components;
using Content.Shared._Den.Vampire.Events;
using Content.Shared.Body.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Timing;


namespace Content.Shared._Den.Vampire;

/// <summary>
/// </summary>
public sealed class VampireDrinkBloodSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireDrinkBloodComponent, VampireDrinkBloodAbility>(OnFeedStart);
        SubscribeLocalEvent<VampireDrinkBloodComponent, VampireDrinkBloodAbilityDoAfterEvent>(OnFeedEnd);
        SubscribeLocalEvent<VampireDrinkBloodComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<VampireDrinkBloodComponent> entity, ref MapInitEvent args)
    {
       _action.AddAction(entity, ref entity.Comp.Action, entity.Comp.ActionProto, entity);
    }

    private void OnFeedStart(Entity<VampireDrinkBloodComponent> ent, ref VampireDrinkBloodAbility args)
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

    private void OnFeedEnd(Entity<VampireDrinkBloodComponent> ent, ref VampireDrinkBloodAbilityDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        var target = args.Args.Target;

        if(target is null)
            return;

        // if (target is null || !TryComp<BloodstreamComponent>(target, out var targedBloodstream))
        //     return;

        DrinkBlood(ent, target.Value);

        _popup.PopupPredicted(Loc.GetString("vampire-feeding-successful-vampire", ("target", target.Value)), ent, ent, PopupType.Medium);
        _popup.PopupPredicted(Loc.GetString("vampire-feeding-successful-target", ("vampire", ent.Owner)), ent.Owner, target.Value, PopupType.MediumCaution);
    }

    public void DrinkBlood(Entity<VampireDrinkBloodComponent> ent, EntityUid target)
    {
        if (!_gameTiming.IsFirstTimePredicted)
            return;

        if (!TryComp(ent, out VampireThirstBloodComponent? thirstBlood) || !TryComp<BloodstreamComponent>(target, out var targedBloodstream))
            return;

        var bloodReagent = targedBloodstream.BloodReagent;
        if (!ent.Comp.BloodTarget.Contains(bloodReagent))
            return;

        var bloodSolution = targedBloodstream.BloodSolution;

        if (bloodSolution is null)
            return;

        if (_mobStateSystem.IsDead(target))
            return;

        _solutionContainer.SplitSolution(bloodSolution.Value,  ent.Comp.BloodDrainAmountRemove);

        thirstBlood.CurrentThirstBlood += ent.Comp.BloodDrainAmountGive;

    }
}
