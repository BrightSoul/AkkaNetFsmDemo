using System;

namespace AkkanetFsmDemo.Models.DomainEvents
{
    public class ProductRemoved : IDomainEvent
    {
        public ProductRemoved(string productName)
        {
            this.ProductName = productName;
        }
        public string ProductName { get; private set; }
    }
}