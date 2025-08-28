using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class OrderController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public OrderController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }

        // Display all orders
        public async Task<IActionResult> Index()
        {
            var orders = await _tableStorageService.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> AddOrder()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();

            ViewBag.Customers = customers.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.RowKey,       
                Text = $"{c.FirstName} {c.LastName}"
            }).ToList();

            ViewBag.Products = products.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = p.RowKey,        
                Text = p.Name
            }).ToList();

            return View();
        }

        // POST: Add Order
        [HttpPost]
        public async Task<IActionResult> AddOrder(Order order)
        {
            try
            {
                // Fetch customer and product by RowKey
                var customer = await _tableStorageService.GetCustomerByIdAsync("CustomersPartition", order.CustomerId);
                var product = await _tableStorageService.GetProductByIdAsync("ProductPartition", order.ProductId);

                if (customer == null || product == null)
                {
                    ModelState.AddModelError("", "Invalid customer or product selection.");
                    return View(order);
                }

                // Calculate total price
                order.TotalPrice = product.Price * order.Quantity;

                // Assign PartitionKey and RowKey for Table Storage
                order.PartitionKey = "OrdersPartition";
                order.RowKey = Guid.NewGuid().ToString();
                order.ETag = Azure.ETag.All;

                await _tableStorageService.AddOrderAsync(order);

                string message = $"Order Added: Customer={customer.FirstName} {customer.LastName}, Product={product.Name}, Quantity={order.Quantity}, Total={order.TotalPrice}";
                await _queueService.SendMessage(message);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding order: " + ex.Message);
                return View(order);
            }
        }

        // POST: Delete Order
        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            // Get order to include in queue message
            var order = await _tableStorageService.GetOrderByIdAsync(partitionKey, rowKey);

            if (order != null)
            {
                var customer = await _tableStorageService.GetCustomerByIdAsync("CustomersPartition", order.CustomerId);
                var product = await _tableStorageService.GetProductByIdAsync("ProductPartition", order.ProductId);

                await _tableStorageService.DeleteOrderAsync(partitionKey, rowKey);

                string message = $"Order Deleted: Customer={customer?.FirstName} {customer?.LastName}, Product={product?.Name}";
                await _queueService.SendMessage(message);
            }

            return RedirectToAction("Index");
        }
    }





    
}
