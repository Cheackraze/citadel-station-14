using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.BUI;

[NetSerializable, Serializable]
public sealed class ATMInterfaceState : BoundUserInterfaceState
{
    public string Name;
    public int Balance;


    public ATMInterfaceState(string name, int balance)
    {
        Name = name;
        Balance = balance;
    }
}
