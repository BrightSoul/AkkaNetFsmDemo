using System.Collections.Generic;

namespace AkkanetFsmDemo.Models.Dto
{
    public class CartLine
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; } = 0;
    }
}