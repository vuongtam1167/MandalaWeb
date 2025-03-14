using MandalaApp.DataAccess;
using MandalaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Linq;

namespace MandalaApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly MandalaRepository _repository;

        public ProfileController(MandalaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            // Lấy ID của user đang đăng nhập từ Claim (ClaimTypes.NameIdentifier)
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
            // Cập nhật thông tin người dùng (Name, Email, Avatar, Status)
            _repository.UpdateUser(updatedUser);
            TempData["SuccessMessage"] = "Thông tin profile đã được cập nhật thành công.";
            return RedirectToAction("Profile");
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

            // Kiểm tra mật khẩu mới: ít nhất 8 ký tự
            if (model.newPassword.Length < 8)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 8 ký tự." });
            }

            // Kiểm tra mật khẩu mới: chứa ít nhất 1 số
            if (!model.newPassword.Any(char.IsDigit))
            {
                return Json(new { success = false, message = "Mật khẩu mới phải chứa ít nhất 1 số." });
            }

            // Bổ sung điều kiện: mật khẩu mới phải chứa ít nhất 1 ký tự chữ
            if (!model.newPassword.Any(char.IsLetter))
            {
                return Json(new { success = false, message = "Mật khẩu mới phải chứa ít nhất 1 ký tự chữ." });
            }

            // Kiểm tra nhập lại mật khẩu khớp với mật khẩu mới
            if (!string.Equals(model.newPassword, model.confirmNewPassword))
            {
                return Json(new { success = false, message = "Nhập lại mật khẩu không khớp." });
            }

            // Nếu thỏa tất cả, cập nhật mật khẩu mới (mã hóa MD5)
            user.Password = model.newPassword;
            _repository.UpdateUserPassword(user);

            return Json(new { success = true, message = "Mật khẩu đã được cập nhật thành công." });
        }


    }

    public class ChangePasswordModel
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmNewPassword { get; set; }
    }
}