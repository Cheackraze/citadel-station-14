using Content.Shared.Shipyard;
using Content.Client.Shipyard.UI;
using Content.Client.Shipyard.Components;
using Content.Shared.Shipyard.BUI;
using Content.Shared.Shipyard.Events;
using Content.Shared.Shipyard.Components;
using Content.Shared.IdentityManagement;
using Robust.Client.GameObjects;
using Content.Shared.Containers.ItemSlots;
using Robust.Client.Player;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.Shipyard.BUI
{
    public sealed class ShipyardConsoleBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        [ViewVariables]
        private ShipyardConsoleMenu? _menu;

        [ViewVariables]
        public string? Name { get; private set; }

        [ViewVariables]
        public int Balance { get; private set; }

        public ShipyardConsoleBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();
            List<string> accessLevels;
            if (_entityManager.TryGetComponent<ShipyardConsoleComponent>(Owner.Owner, out var component))
            {
                accessLevels = component.AccessLevels;
                accessLevels.Sort();
            }
            else
            {
                accessLevels = new List<string>();
            }

            if (component != null && _entityManager.TryGetComponent<ShipyardBankAccountComponent>(component.TargetIdSlot.Item, out var bank))
            {
                Balance = bank.Balance;
            }
            else
            {
                Balance = 0;
            }
            
            var sysManager = _entityManager.EntitySysManager;
            var spriteSystem = sysManager.GetEntitySystem<SpriteSystem>();
            _menu = new ShipyardConsoleMenu(this, IoCManager.Resolve<IPrototypeManager>(), spriteSystem, accessLevels);
            var description = new FormattedMessage();


            if (component != null && component.TargetIdSlot.ID != null)
            {
                _menu.TargetIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(component.TargetIdSlot.ID));
            }
            _menu.OnClose += Close;

            _menu.OnOrderApproved += ApproveOrder;

            _menu.OpenCentered();

        }
        private void Populate()
        {
            if (_menu == null) return;

            _menu.PopulateProducts();
            _menu.PopulateCategories();
        }
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not ShipyardConsoleInterfaceState cState)
                return;

            Balance = cState.Balance;

            var castState = (ShipyardConsoleInterfaceState) state;
            Populate();
            _menu?.UpdateState(castState);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            _menu?.Dispose();
        }


        public void SubmitData(string newFullName, string newJobTitle, List<string> newAccessList)
        {
            // hardcoded for now because I am like 20 hours into ecs-ing my implementation of idconsole and cba rn
            if (newFullName.Length > 30)
                newFullName = newFullName[..30];

            if (newJobTitle.Length > 30)
                newJobTitle = newJobTitle[..30];

            SendMessage(new WriteToTargetIdMessage(
                newFullName,
                newJobTitle,
                newAccessList,
                string.Empty));
        }
        private void ApproveOrder(ButtonEventArgs args)
        {
 //           _sawmill.Error($"button is pressed");
            if (args.Button.Parent?.Parent is not VesselRow row || row.Vessel == null)
            {
 //               _sawmill.Error($"button is not a part of a vessel row, somehow");
                return;
            }
            var vesselId = row.Vessel.ID;
            var price = row.Vessel.Price;
            SendMessage(new ShipyardConsolePurchaseMessage(vesselId, price));
        }
    }
}
