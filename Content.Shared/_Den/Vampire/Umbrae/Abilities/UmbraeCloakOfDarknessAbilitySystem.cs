using Content.Shared._Den.Vampire.Umbrae.Abilities.Components;
using Content.Shared._Den.Vampire.Umbrae.Events;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;


namespace Content.Shared._Den.Vampire.Umbrae.Abilities;

public abstract class SharedUmbraeCloakOfDarknessAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;


    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UmbraeCloakOfDarknessAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UmbraeCloakOfDarknessAbilityComponent, UmbraeCloakOfDarknessAbilityEvent>(OnCloakOfDarkness);
    }

    private void OnMapInit(Entity<UmbraeCloakOfDarknessAbilityComponent> ent, ref MapInitEvent args)
    {
        _action.AddAction(ent, ref ent.Comp.Action, ent.Comp.ActionProto, ent);
        Logger.Info($"[Umbrae] Action initialized for entity {ent}");
    }

    private void OnCloakOfDarkness(Entity<UmbraeCloakOfDarknessAbilityComponent> ent, ref UmbraeCloakOfDarknessAbilityEvent args)
    {
        if (args.Handled)
            return;

        EnsureComp<StealthComponent>(ent, out var stealth);

        var newState = !stealth.Enabled;
        _stealth.SetEnabled(ent, newState, stealth);
        Logger.Info($"[Umbrae] Cloak toggled for {ent}: stealth now {(newState ? "ENABLED" : "DISABLED")}");


        var msg = newState ? "You fade into darkness..." : "You emerge from the shadows.";
        _popup.PopupPredicted(Loc.GetString(msg), ent, ent);

        args.Handled = true;
    }
}
