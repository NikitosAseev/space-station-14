using Content.Shared._Den.Vampire.Components;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Mobs.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;

namespace Content.Shared._Den.Vampire;

public sealed class VampireBloodEssenceSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public void DrinkBlood(Entity<VampireDrinkBloodComponent> ent, Entity<BloodstreamComponent>? target)
    {
        if (target is null)
        {
            Logger.Info($"target {target} is null");
            return;
        }

        var bloodReagent = target.Value.Comp.BloodReagent;

        Logger.Info($"[DrinkBlood] Attempting to drink blood from {target} by {ent}");

        if (!ent.Comp.BloodTarget.Contains(bloodReagent))
        {
            Logger.Info($"[DrinkBlood] Target's blood reagent '{bloodReagent}' is not suitable for {ent}.");
            return;
        }

        if (!TryComp(ent, out VampireThirstBloodComponent? thirstBlood))
        {
            Logger.Info($"[DrinkBlood] {ent} does not have VampireThirstBloodComponent.");
            return;
        }

        if (!TryComp(target, out SolutionContainerManagerComponent? solutions))
        {
            Logger.Info($"[DrinkBlood] {target} does not have SolutionContainerManagerComponent.");
            return;
        }

        if (!_solutionContainerSystem.ResolveSolution((target.Value, solutions), bloodReagent, ref target.Value.Comp.BloodSolution, out var bloodSolution))
        {
            Logger.Info($"[DrinkBlood] Failed to resolve blood solution '{bloodReagent}' on {target}.");
            return;
        }

        if (bloodSolution.Volume <= FixedPoint2.New(5))
        {
            Logger.Info($"[DrinkBlood] Blood volume too low: {bloodSolution.Volume} in {target}.");
            return;
        }

        if (_mobStateSystem.IsDead(target.Value))
        {
            Logger.Info($"[DrinkBlood] Target {target} is dead, cannot drink blood.");
            return;
        }

        var drainAmount = FixedPoint2.New(ent.Comp.BloodDrainAmount);

        Logger.Info($"[DrinkBlood] Drinking {drainAmount}u of blood from {target} by {ent}.");
        Logger.Info($"[DrinkBlood] Blood volume before: {bloodSolution.Volume}");
        Logger.Info($"[DrinkBlood] Thirst before: {thirstBlood.CurrentThirstBlood}");

        bloodSolution.Volume -= drainAmount;
        thirstBlood.CurrentThirstBlood += ent.Comp.BloodDrainAmount;

        Logger.Info($"[DrinkBlood] Blood volume after: {bloodSolution.Volume}");
        Logger.Info($"[DrinkBlood] Thirst after: {thirstBlood.CurrentThirstBlood}");
    }
}

