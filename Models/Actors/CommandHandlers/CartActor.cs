using System.Linq;
using Akka.Persistence;
using AkkanetFsmDemo.Models.CommandResults;
using AkkanetFsmDemo.Models.Commands;
using AkkanetFsmDemo.Models.DomainEvents;
using AkkanetFsmDemo.Models.Dto;
using AkkanetFsmDemo.Models.Options;
using AkkanetFsmDemo.Models.Responses;
using Microsoft.Extensions.Options;

namespace AkkanetFsmDemo.Models.Actors.CommandHandlers
{
    public class CartActor : ReceivePersistentActor
    {
        private readonly string persistenceId;
        public override string PersistenceId => persistenceId;
        private CartState cartState;

        public CartActor(IOptionsMonitor<ActorSystemOptions> options)
        {
            persistenceId = options.CurrentValue.PersistenceId;
            cartState = new CartState();
            ConfigureRecoverableEvents();

            //TODO: should we rely on Become or just have one single state and validate commands depending on the value of a "Status" field?
            EmptyCart();
        }

        //Machine states
        private void EmptyCart()
        {
            Command<GetCart>(Handle);
            Command<AddProduct>(Handle, Validate);
            CommandAny(Discard);
        }

        private void NonEmptyCart()
        {
            Command<GetCart>(Handle);
            Command<AddProduct>(Handle, Validate);
            Command<RemoveProduct>(Handle, Validate);
            Command<ConfirmCart>(Handle, Validate);
            CommandAny(Discard);
        }

        private void ConfirmedCart()
        {
            Command<GetCart>(Handle);
            CommandAny(Discard);
        }

        //Recoverable events        
        private void ConfigureRecoverableEvents()
        {
            Recover<ProductAdded>(Apply);
            Recover<ProductRemoved>(Apply);
            Recover<CartConfirmed>(Apply);
        }


        //Get cart
        private void Handle(GetCart command)
        {
            Respond(CartResponse.FromCartState(cartState));
        }

        //Add product
        private bool Validate(AddProduct command)
        {
            if (string.IsNullOrEmpty(command.ProductName)) {
                return Reject("Can't add a product with an empty name");
            }
            return Accept();
        }

        private void Handle(AddProduct command)
        {
            var domainEvent = new ProductAdded(command.ProductName);
            //TODO: Handle persistence errors https://getakka.net/articles/persistence/event-sourcing.html#persistence-status-handling
            Persist(domainEvent, Apply);
        }
        
        private void Apply(ProductAdded domainEvent) {
            var cartLine = cartState.Lines.SingleOrDefault(line => line.ProductName == domainEvent.ProductName);
            if (cartLine == null)
            {
                //It didn't exist, so add it
                cartLine = new CartLine { ProductName = domainEvent.ProductName, Quantity = 1 };
                cartState.Lines.Add(cartLine);
            }
            else
            {
                //Just update its quantity
                cartLine.Quantity++;
            }
            Become(NonEmptyCart);
        }

        //Remove product
        private bool Validate(RemoveProduct command)
        {
            if (!cartState.Lines.Any(line => line.ProductName == command.ProductName))
            {
                return Reject($"Can't remove product '{command.ProductName}' because it's not in the cart");
            }
            return Accept();
        }
        
        private void Handle(RemoveProduct command)
        {
            var domainEvent = new ProductRemoved(command.ProductName);
            Persist(domainEvent, Apply);
        }
        
        private void Apply(ProductRemoved domainEvent) {
            var cartLine = cartState.Lines.Single(line => line.ProductName == domainEvent.ProductName);
            if (cartLine.Quantity == 1)
            {
                cartState.Lines.Remove(cartLine);
            }
            else
            {
                cartLine.Quantity--;
            }
            if (!cartState.Lines.Any())
            {
                Become(EmptyCart);
            }
        }

        //Confirm cart
        private bool Validate(ConfirmCart command)
        {
            //If this command is acceptable by the current state, then we will accept it
            return Accept();
        }

        private void Handle(ConfirmCart command)
        {
            var domainEvent = new CartConfirmed();
            Persist(domainEvent, Apply);
        }

        private void Apply(CartConfirmed cartConfirmed) {
            cartState.IsConfirmed = true;
            Become(ConfirmedCart);
        }


        //Command handling results
        private bool Accept()
        {
            Sender.Tell(new CommandAccepted(), Self);
            return true;
        }
        private bool Reject(string reason)
        {
            Sender.Tell(new CommandRejected(reason), Self);
            return false;
        }

        private void Discard(object obj)
        {
            Sender.Tell(new CommandDiscarded(), Self);
        }

        private void Respond(IResponse response)
        {
            Sender.Tell(new CommandResponse(response), Self);
        }
    }
}