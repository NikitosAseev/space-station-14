using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Den.Vampire.Components;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(VampireDrinkBloodSystem))]
public sealed partial class VampireDrinkBloodComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan DrinkBloodDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public EntProtoId ActionProto = "ActionVampireDrinkBlood";

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public List<ProtoId<ReagentPrototype>> BloodTarget = new()
    {
        "Blood",
        "CopperBlood",
        "InsectBlood"
    };

    [DataField, AutoNetworkedField]
    public FixedPoint2 BloodDrainAmountRemove = 20;

    [DataField, AutoNetworkedField]
    public float BloodDrainAmountGive = 20;
}
