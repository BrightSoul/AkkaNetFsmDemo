using System;

namespace AkkanetFsmDemo.Models.DomainEvents
{
    public class ProductAdded : IDomainEvent
    {
        public ProductAdded(string productName)
        {
            this.ProductName = productName;
        }
        public string ProductName { get; private set; }
    }
}