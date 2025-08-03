using Content.Shared._Den.Vampire.Umbrae.Abilities.Components;
using Content.Shared._Den.Vampire.Umbrae.Events;
using Content.Shared.Actions;

namespace Content.Shared._Den.Vampire.Umbrae.Abilities;

public abstract class SharedUmbraeExtinguishAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UmbraeExtinguishAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UmbraeExtinguishAbilityComponent, UmbraeExtinguishAbilityEvent>(OnExtinguish);
    }

    private void OnMapInit(Entity<UmbraeExtinguishAbilityComponent> ent, ref MapInitEvent args)
    {
        _action.AddAction(ent, ref ent.Comp.Action, ent.Comp.ActionProto, ent);
    }

    public abstract void OnExtinguish(Entity<UmbraeExtinguishAbilityComponent> ent, ref UmbraeExtinguishAbilityEvent args);
}
