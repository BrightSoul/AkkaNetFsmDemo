using System.Collections.Generic;
using System.Linq;
using AkkanetFsmDemo.Models.Dto;

namespace AkkanetFsmDemo.Models.Responses
{
    public class CartResponse : IResponse
    {
        private CartResponse(IReadOnlyCollection<CartLineResponse> lines, bool isConfirmed)
        {
            Lines = lines;
            IsConfirmed = isConfirmed;
        }
        public static CartResponse FromCartState(CartState cartStatus)
        {
            return new CartResponse(
                lines: cartStatus.Lines.Select(line => CartLineResponse.FromCartLine(line)).ToList().AsReadOnly(),
                isConfirmed: cartStatus.IsConfirmed
            );
        }
        public IReadOnlyCollection<CartLineResponse> Lines { get; }
        public bool IsConfirmed { get; }
        public string Name => "Cart";
    }

    public class CartLineResponse
    {
        private CartLineResponse(string productName, int quantity)
        {
            ProductName = productName;
            Quantity = quantity;
        }

        public string ProductName { get; set; }
        public int Quantity { get; set; }

        public static CartLineResponse FromCartLine(CartLine cartLine)
        {
            return new CartLineResponse(cartLine.ProductName, cartLine.Quantity);
        }
    }
}