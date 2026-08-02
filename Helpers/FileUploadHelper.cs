using Microsoft.AspNetCore.Http;

namespace FoodOne.Helpers
{
    public class FileUploadHelper
    {
        public static async Task<string> UploadFile(IFormFile file,string folderName)
        {
            // Example:
            // wwwroot/uploads/category
            string uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                folderName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            string fileName = Guid.NewGuid().ToString()
                              + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }


        public static bool DeleteFile(string fileName,string folderName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
            string filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                folderName,
                fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
    }
}
