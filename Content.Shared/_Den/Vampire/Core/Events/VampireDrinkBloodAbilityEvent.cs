using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Den.Vampire.Core.Events;

public sealed partial class VampireDrinkBloodAbilityEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class VampireDrinkBloodAbilityDoAfterEvent : SimpleDoAfterEvent;
