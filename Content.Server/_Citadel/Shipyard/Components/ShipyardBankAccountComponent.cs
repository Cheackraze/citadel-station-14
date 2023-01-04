using Content.Server.Shipyard.Systems;

namespace Content.Server.Shipyard.Components;

/// <summary>
/// Added to the abstract representation of a person to track its money.
/// </summary>
[RegisterComponent, Access(typeof(ShipyardSystem))]
public sealed class ShipyardBankAccountComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("balance")]
    public int Balance = 40000;

}
