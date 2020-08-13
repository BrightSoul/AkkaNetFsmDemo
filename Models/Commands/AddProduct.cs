using System;

namespace AkkanetFsmDemo.Models.Commands
{
    public class AddProduct : ICommand
    {
        public AddProduct(string productName)
        {
            this.ProductName = productName;
        }
        public string ProductName { get; private set; }
    }
}