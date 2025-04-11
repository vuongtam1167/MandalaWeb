using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MandalaApp.DataAccess;
using MandalaApp.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MandalaApp.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly IConfiguration _configuration;

        public RegisterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Nếu đã đăng nhập, chuyển hướng sang trang chủ
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(string firstName, string lastName, string username, string email, string password)
        {
            // Kiểm tra các ô không được để trống
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Các ô không được để trống.";
                return View();
            }

            var repository = new MandalaRepository(_configuration);

            // Kiểm tra điều kiện mật khẩu
            if (!IsValidPassword(password))
            {
                ViewBag.ErrorMessage = "Mật khẩu phải có ít nhất 8 kí tự, chứa ít nhất 1 số và 1 chữ cái.";
                return View();
            }

            // Kiểm tra xem username đã tồn tại chưa
            if (repository.IsUserExists(username))
            {
                ViewBag.ErrorMessage = "Tên người dùng đã tồn tại!";
                return View();
            }

            if (repository.IsEmailExists(email))
            {
                ViewBag.ErrorMessage = "Email này đã được dùng!";
                return View();
            }

            string hashedPassword = ComputeMD5Hash(password);

            var user = new User
            {
                UserName = username,
                Password = hashedPassword,
                Name = $"{firstName} {lastName}",
                Email = email,
                CreatedDate = DateTime.Now,
                Status = true
            };

            repository.InsertUser(user);
            return RedirectToAction("Login", "Login");
        }

        private string ComputeMD5Hash(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // Phương thức kiểm tra mật khẩu có ít nhất 8 kí tự, chứa ít nhất 1 chữ cái và 1 số.
        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                return false;
            }

            bool hasLetter = false;
            bool hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                }
                if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
                if (hasLetter && hasDigit)
                {
                    break;
                }
            }
            return hasLetter && hasDigit;
        }
    }
}