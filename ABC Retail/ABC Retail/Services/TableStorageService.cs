using ABC_Retail.Models;
using Azure;
using Azure.Data.Tables;

namespace ABC_Retail.Services
{
    public class TableStorageService
    {
        public readonly TableClient _CustomerTableClient;
        public readonly TableClient _ProductTableClient;
        public readonly TableClient _OrderTableClient;
        public TableStorageService(string connectString)
        {
            _CustomerTableClient = new TableClient(connectString, "Customer");
            _ProductTableClient = new TableClient(connectString, "Product");
            _OrderTableClient = new TableClient(connectString, "Order");
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();
            await foreach (var customer in _CustomerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }
            return customers; 
        }

        public async Task AddCustomerAsync(Customer customer)
        {
        if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))

         {  

                throw new ArgumentException("Partition and Rowkey must be set");
            
         }
            try
            {
                await _CustomerTableClient.AddEntityAsync(customer);

            }
            catch (RequestFailedException ex)
            { 
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);

            }

         

        }

        public async Task DeleteCustomerAsync(string partitionkey,string rowkey)
        {
            await _CustomerTableClient.DeleteEntityAsync(partitionkey, rowkey);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _ProductTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }


        public async Task AddProductsAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))

            {

                throw new ArgumentException("Partition and Rowkey must be set");
               
            }
            await _ProductTableClient.CreateIfNotExistsAsync();

            // Required for insert
            product.ETag = ETag.All;
            try
            {
                await _ProductTableClient.AddEntityAsync(product);

            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);

            }






        }
        public async Task DeleteProductsAsync(string partitionkey, string rowkey)
        {
            await _ProductTableClient.DeleteEntityAsync(partitionkey, rowkey);
        }


        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();
            await foreach (var order in _OrderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }

        public async Task AddOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }

            await _OrderTableClient.CreateIfNotExistsAsync();

            // Required for insert
            order.ETag = ETag.All;

            try
            {
                await _OrderTableClient.AddEntityAsync(order);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Order Table Storage", ex);
            }
        }

        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            await _OrderTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }



        public async Task<Product?> GetProductByIdAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _ProductTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Not found
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _CustomerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Not found
            }
        }



        public async Task<Order?> GetOrderAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _OrderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Not found
            }
        }


       

        public async Task<Order> GetOrderByIdAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _OrderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }








    }
}
