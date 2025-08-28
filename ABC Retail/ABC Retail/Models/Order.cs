using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.Models
{
    public class Order : ITableEntity
    {
        [Key]
        public string OrderId { get; set; }  // Unique Order Id (will map to RowKey)

        public string CustomerId { get; set; }  // RowKey of Customer
        public string ProductId { get; set; }   // RowKey of Product

        public int Quantity { get; set; }
        public int TotalPrice { get; set; } // Quantity * Product.Price

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // ITableEntity Implementation
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }        

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
