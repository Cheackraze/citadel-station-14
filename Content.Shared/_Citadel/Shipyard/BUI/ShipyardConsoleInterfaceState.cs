using Robust.Shared.Serialization;

namespace Content.Shared.Shipyard.BUI;

[NetSerializable, Serializable]
public sealed class ShipyardConsoleInterfaceState : BoundUserInterfaceState
{
    public string Name;
    public int Balance;

    public ShipyardConsoleInterfaceState(string name, int balance)
    {
        Name = name;
        Balance = balance;
    }
}
