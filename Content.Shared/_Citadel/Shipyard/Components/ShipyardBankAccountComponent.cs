using Content.Shared.Shipyard;
using Robust.Shared.GameStates;

namespace Content.Shared.Shipyard.Components;

/// <summary>
/// Added to the abstract representation of a person to track its money.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed class ShipyardBankAccountComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("balance")]
    public int Balance = 40000;

}
