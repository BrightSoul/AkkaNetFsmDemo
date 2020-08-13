using System;

namespace AkkanetFsmDemo.Models.Commands
{
    public class RemoveProduct : ICommand
    {
        public RemoveProduct(string productName)
        {
            this.ProductName = productName;
        }
        public string ProductName { get; private set; }
    }
}