using Content.Shared.Shipyard.Components;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.Access.Components;
using Robust.Shared.GameObjects;

namespace Content.Shared.Shipyard
{


    [NetSerializable, Serializable]
    public enum ShipyardConsoleUiKey : byte
    {
        Shipyard,
        HoSConsole,
        SAConsole
    }

    [UsedImplicitly]
    public abstract class SharedShipyardSystem : EntitySystem
    {
        
        
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SharedShipyardConsoleComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<SharedShipyardConsoleComponent, ComponentRemove>(OnComponentRemove);
            SubscribeLocalEvent<SharedShipyardConsoleComponent, ComponentGetState>(OnGetState);
            SubscribeLocalEvent<SharedShipyardConsoleComponent, ComponentHandleState>(OnHandleState);
        }

        private void OnHandleState(EntityUid uid, SharedShipyardConsoleComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not ShipyardConsoleComponentState state) return;
            component.AccessLevels = state.AccessLevels;
        }

        private void OnGetState(EntityUid uid, SharedShipyardConsoleComponent component, ref ComponentGetState args)
        {
            args.State = new ShipyardConsoleComponentState(component.AccessLevels);
        }

        private void OnComponentInit(EntityUid uid, SharedShipyardConsoleComponent component, ComponentInit args)
        {
            _itemSlotsSystem.AddItemSlot(uid, SharedShipyardConsoleComponent.TargetIdCardSlotId, component.TargetIdSlot);
        }

        private void OnComponentRemove(EntityUid uid, SharedShipyardConsoleComponent component, ComponentRemove args)
        {
            _itemSlotsSystem.RemoveItemSlot(uid, component.TargetIdSlot);
        }

        [Serializable, NetSerializable]
        private sealed class ShipyardConsoleComponentState : ComponentState
        {
            public List<string> AccessLevels;

            public ShipyardConsoleComponentState(List<string> accessLevels)
            {
                AccessLevels = accessLevels;
            }
        }
    }
}
