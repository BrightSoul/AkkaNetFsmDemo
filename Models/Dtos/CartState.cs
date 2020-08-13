using System.Collections.Generic;

namespace AkkanetFsmDemo.Models.Dto
{
    public class CartState
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();
        public bool IsConfirmed { get; set; } = false;
    }
}