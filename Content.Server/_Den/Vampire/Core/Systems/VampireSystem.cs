using Content.Server._Den.Vampire.Components;
using Content.Server.Body.Components;
using Content.Server.Store.Systems;
using Content.Shared._Den.Vampire.Core.Abilities.Components;
using Content.Shared._Den.Vampire.Core.Events;
using Content.Shared.Store.Components;
using Content.Shared.Actions;
using Content.Shared.FixedPoint;

namespace Content.Server._Den.Vampire;

/// <summary>

/// </summary>
public sealed class VampireSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StoreComponent, VampireShopEvent>(OnShopOpenAction);
    }

    private void OnMapInit(Entity<VampireComponent> ent, ref MapInitEvent args)
    {
        RemComp<RespiratorComponent>(ent); // Don't need them to breath

        // Give the shop action
        _action.AddAction(ent, ref ent.Comp.ShopAction, ent.Comp.ActionVampireShopProto, ent);

        // These components all add actions on map init.
        EnsureComp<VampireDrinkBloodAbilityComponent>(ent);
        EnsureComp<VampireGlareAbilityComponent>(ent);
        EnsureComp<VampireRejuvenateAbilityComponent>(ent);
    }

    /// <summary>
    /// Opens or closes the shop for the user.
    /// </summary>
    private void OnShopOpenAction(Entity<StoreComponent> entity, ref VampireShopEvent args)
    {
        _storeSystem.ToggleUi(entity.Owner, entity.Owner, entity.Comp);
    }

    /// <summary>
    /// Handles depositing blood essence into the shop. This is intended to be used by other systems to deposit.
    /// </summary>
    /// <param name="entity">The entity with the Vampire component to deposit Blood Essence into</param>
    /// <param name="amount">The amount of Blood Essence to deposit</param>
    public void DepositEssence(Entity<VampireComponent> entity, float amount)
    {
        if (amount <= 0.0f)
            return;

        _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>
                { { entity.Comp.BloodEssenceCurrencyPrototype, amount } },
            entity);
    }
}
