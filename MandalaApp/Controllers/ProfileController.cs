using MandalaApp.DataAccess;
using MandalaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace MandalaApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly MandalaRepository _repository;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(MandalaRepository repository, IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            User user = _repository.GetUserById(currentUserId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult SaveProfile(User updatedUser)
        {
            // Nếu Avatar chứa đường dẫn tạm (chứa "/temp/") thì chuyển file sang thư mục cố định
            if (!string.IsNullOrEmpty(updatedUser.Avatar) && updatedUser.Avatar.Contains("/temp/"))
            {
                // Ví dụ: Avatar = "/images/avatars/temp/filename.jpg"
                string tempRelativePath = updatedUser.Avatar;
                string tempPhysicalPath = Path.Combine(_environment.WebRootPath, tempRelativePath.TrimStart('/'));
                string permanentFolder = Path.Combine(_environment.WebRootPath, "images", "avatars");
                if (!Directory.Exists(permanentFolder))
                {
                    Directory.CreateDirectory(permanentFolder);
                }
                string fileName = Path.GetFileName(updatedUser.Avatar);
                string permanentPhysicalPath = Path.Combine(permanentFolder, fileName);
                // Di chuyển file từ thư mục tạm sang cố định
                System.IO.File.Move(tempPhysicalPath, permanentPhysicalPath);
                // Cập nhật lại đường dẫn Avatar
                updatedUser.Avatar = "/images/avatars/" + fileName;
            }

            // Cập nhật các thông tin còn lại của user
            _repository.UpdateUser(updatedUser);
            TempData["SuccessMessage"] = "Thông tin profile đã được cập nhật thành công.";
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (avatarFile != null && avatarFile.Length > 0)
            {
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                // Thư mục tạm theo user id
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "avatars", currentUserId.ToString(), "temp");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                // Tạo tên file độc nhất (có thể bổ sung user id nếu cần)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }
                // Đường dẫn tương đối của file (đã lưu trong thư mục temp)
                string relativePath = $"/images/avatars/{currentUserId}/temp/" + uniqueFileName;
                return Json(new { success = true, filePath = relativePath });
            }
            return Json(new { success = false, message = "Không có file được upload" });
        }

        [HttpPost]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.oldPassword) ||
               string.IsNullOrWhiteSpace(model.newPassword) || string.IsNullOrWhiteSpace(model.confirmNewPassword))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
            }

            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            User user = _repository.GetUserById(currentUserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User không tồn tại." });
            }

            // Kiểm tra mật khẩu cũ (sau khi mã hóa MD5)
            string hashedOldPassword = _repository.ComputeMD5HashPass(model.oldPassword);
            if (!string.Equals(user.Password, hashedOldPassword, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Mật khẩu cũ không đúng." });
            }

            if (model.newPassword.Length < 8)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 8 ký tự." });
            }

            if (!model.newPassword.Any(char.IsDigit))
            {
                return Json(new { success = false, message = "Mật khẩu mới phải chứa ít nhất 1 số." });
            }

            if (!model.newPassword.Any(char.IsLetter))
            {
                return Json(new { success = false, message = "Mật khẩu mới phải chứa ít nhất 1 ký tự chữ." });
            }

            if (!string.Equals(model.newPassword, model.confirmNewPassword))
            {
                return Json(new { success = false, message = "Nhập lại mật khẩu không khớp." });
            }

            user.Password = model.newPassword;
            _repository.UpdateUserPassword(user);

            return Json(new { success = true, message = "Mật khẩu đã được cập nhật thành công." });
        }
    }
}
