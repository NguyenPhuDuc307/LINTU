using LMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LMS.Controllers;

[Route("File")]
public class FileController(IStorageService storageService) : Controller
{
    // Upload Ảnh
    [HttpPost("UploadImage")]
    public async Task<IActionResult> UploadImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return Json(new { error = "File không hợp lệ!" });

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            await storageService.SaveFileAsync(stream, fileName);
        }

        var fileUrl = storageService.GetFileUrl(fileName);
        return Json(new { link = fileUrl });
    }

    // Upload Video
    [HttpPost("UploadVideo")]
    public async Task<IActionResult> UploadVideo(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return Json(new { error = "File không hợp lệ!" });

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            await storageService.SaveFileAsync(stream, fileName);
        }

        var fileUrl = storageService.GetFileUrl(fileName);
        return Json(new { link = fileUrl });
    }

    // Xóa file
    [HttpPost("DeleteFile")]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        await storageService.DeleteFileAsync(fileName);
        return Json(new { success = true, message = "Xóa file thành công!" });
    }
}
