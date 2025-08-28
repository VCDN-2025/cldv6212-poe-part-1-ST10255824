using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ABC_Retail.Models
{
    public class Customer : ITableEntity
    {
        [Key]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        // ITableEntity Implementation
        public string PartitionKey { get; set; }

        public string RowKey { get; set; } 

        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }



    }
}
