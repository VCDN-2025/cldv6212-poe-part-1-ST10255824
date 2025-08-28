using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class FilesController : Controller
    {
    private readonly AzureFileShareServices _fileShareServices;
        public FilesController(AzureFileShareServices fileShareServices)
        {
            _fileShareServices = fileShareServices;
        }

        public async Task<IActionResult> Index()
        {
            List<FileModel> files;
            try
            {
                files = await _fileShareServices.ListFilesAsync("uploads");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage =$"Failed to load Files :{ex.Message}";
                files = new List<FileModel>();
            }
            return View(files);
            


            

        }

        [HttpPost]

        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null  || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please Select a file to upload");
                return await Index();
            }
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    string directoryName = "uploads";
                    string fileName = file.FileName;
                    await _fileShareServices.UpLoadFileAsync(directoryName, fileName, stream);
                }
                TempData["Message"] = $"File '{file.FileName}' uploaded successfully";
            }
            catch (Exception e)
            { TempData["Message"] =$"File upload failed: { e.Message }";
                    }
            return RedirectToAction("Index");

        }

        [HttpGet]

        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be null or empty");

            }
            try
            {
                var fileStream = await _fileShareServices.DownloadFileAsync("uploads", fileName);
                if (fileStream == null)
                {
                    return NotFound($"File '{fileName}' not found");
                }
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception e)
            {
                return BadRequest($"Failed to download file: {e.Message}");
            }
        }

        
            
         
            
    }
}
