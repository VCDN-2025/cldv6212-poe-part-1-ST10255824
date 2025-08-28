using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;


        public ProductController(BlobService blobService, TableStorageService tableStorageService, QueueService queueService)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            try
            {
                // Upload image to blob
                if (file != null)
                {
                    using var stream = file.OpenReadStream();
                    var imageUrl = await _blobService.UploadsAsync(stream, file.FileName);
                    product.ImageURL = imageUrl;
                }

                // Assign PartitionKey and RowKey
                product.PartitionKey = "ProductPartition";
                product.RowKey = Guid.NewGuid().ToString();
                product.ETag = Azure.ETag.All; // Required for Table Storage

                // Save to Table Storage
                await _tableStorageService.AddProductsAsync(product);

                string message = $"Product Added: {product.Name}, Quantity: {product.Quantity}, Price: {product.Price}";
                await _queueService.SendMessage(message);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log errors and return to view
                Console.WriteLine("Error adding product: " + ex.Message);
                return View(product);
            }
        }


        [HttpPost]

        public async Task<IActionResult> Delete(string partitionKey, string rowKey, Product product,string Name)
        {
            if (product != null && !string.IsNullOrEmpty(product.ImageURL))

            {
                await _blobService.DeleteBlobAsync(product.ImageURL);
            }
            await _tableStorageService.DeleteProductsAsync(partitionKey, rowKey);


            string message = $"Product Deleted: {Name}";
            await _queueService.SendMessage(message);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddProduct()
        {  return View(); 
        }


     }
}
