using Content.Shared.Shipyard;
using Content.Client.Shipyard.UI;
using Content.Shared.Shipyard.BUI;
using Content.Shared.Shipyard.Events;
using Content.Shared.Shipyard.Prototypes;
using Content.Shared.IdentityManagement;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.Shipyard.BUI
{
    public sealed class ShipyardConsoleBoundUserInterface : BoundUserInterface
    {
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

            var entityManager = IoCManager.Resolve<IEntityManager>();
            var sysManager = entityManager.EntitySysManager;
            var spriteSystem = sysManager.GetEntitySystem<SpriteSystem>();
            _menu = new ShipyardConsoleMenu(IoCManager.Resolve<IPrototypeManager>(), spriteSystem);
            var localPlayer = IoCManager.Resolve<IPlayerManager>()?.LocalPlayer?.ControlledEntity;
            var description = new FormattedMessage();

            string accountName;

            if (entityManager.TryGetComponent<MetaDataComponent>(localPlayer, out var metadata))
                accountName = Identity.Name(localPlayer.Value, entityManager);
            else
                accountName = string.Empty;

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

            Name = cState.Name;
            Populate();
            _menu?.UpdateBankData(Name, Balance);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            _menu?.Dispose();
        }


        private void ApproveOrder(ButtonEventArgs args)
        {
            if (args.Button.Parent?.Parent is not VesselRow row || row.Vessel == null)
                return;

            var vesselId = row.Vessel.ID;
            var price = row.Vessel.Price;
            SendMessage(new ShipyardConsolePurchaseMessage(vesselId, price));
        }
    }
}
