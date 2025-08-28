using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableStorageService _tableStorageServices;
        private readonly QueueService _queueService;

        public CustomerController(TableStorageService tableStorageServices, QueueService queueService)
        {
            _tableStorageServices = tableStorageServices;
            _queueService = queueService;
        }
        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageServices.GetAllCustomersAsync();
            return View(customers);
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey, string firstName, string lastName)
        {
            await _tableStorageServices.DeleteCustomerAsync(partitionKey, rowKey);


            string message = $"Customer Deleted: {firstName} {lastName}";
            await _queueService.SendMessage(message);

            return RedirectToAction("Index");
        }
        [HttpPost]
        
        public async Task<IActionResult> AddCustomer(Customer customer)
        {
            customer.PartitionKey = "CustomersPartition";
            customer.RowKey = Guid.NewGuid().ToString();

            await _tableStorageServices.AddCustomerAsync(customer);

            string message = $"Customer Added: {customer.FirstName} {customer.LastName}, Email: {customer.Email}";
            await _queueService.SendMessage(message);


            return RedirectToAction("Index");


                
        }


        [HttpGet]

       public IActionResult AddCustomer()
        {
            return View();
        }
    }
}
