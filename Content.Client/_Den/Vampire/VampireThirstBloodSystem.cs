using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Shared.Alert.Components;
using Content.Shared._Den.Vampire.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Den.Vampire;

public sealed class VampireThirstBloodSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VampireThirstBloodComponent, UpdateAlertSpriteEvent>(OnUpdateAlertSprite);
        SubscribeLocalEvent<VampireThirstBloodComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
    }
    private void OnUpdateAlertSprite(EntityUid uid, VampireThirstBloodComponent component, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert != component.ThirstBloodAlert)
            return;

        var ratio = Math.Clamp(component.CurrentThirstBlood / component.MaxThirstBlood, 0.0f, 1.0f);
        var stateNumber = Math.Clamp((int)(ratio * component.ThirstBloodLayerStates), 0, component.ThirstBloodLayerStates - 1);

        _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), AlertVisualLayers.Base, $"vam{stateNumber}");
    }

    private void OnGetCounterAmount(Entity<VampireThirstBloodComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.ThirstBloodAlert != args.Alert)
            return;

        args.Amount = (int)ent.Comp.CurrentThirstBlood;
    }
}
