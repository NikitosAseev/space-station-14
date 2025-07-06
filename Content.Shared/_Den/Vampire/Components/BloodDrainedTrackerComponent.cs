using Robust.Shared.GameStates;

namespace Content.Shared._Den.Vampire.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodDrainedTrackerComponent : Component
{
    [DataField, AutoNetworkedField]
    public int BloodDrinkedAmount;

    [DataField, AutoNetworkedField]
    public int MaxBloodDrinkedAmount = 200;
}
