using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MandalaApp.DataAccess;
using MandalaApp.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace MandalaApp.Controllers
{
    [Authorize]
    public class IndexController : Controller
    {
        private readonly MandalaRepository _repository;

        public IndexController(MandalaRepository repository)
        {
            _repository = repository;
        }

        // Trang Index
        public IActionResult Index()
        {
            // Lấy ID của user hiện tại từ Claim
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            string avatar = _repository.GetAvatarPathByUserId(currentUserId);
            // Lấy danh sách MandalaHome của user
            List<MandalaHome> mandalahomes = _repository.GetMandalaHomes(currentUserId);
            // Trả về view, Model là danh sách MandalaHome
            ViewBag.Avatar = avatar;
            return View(mandalahomes);
        }

        [HttpGet]
        public IActionResult SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<UserSearchResult>());
            }

            // Lấy ID của user hiện tại từ Claim
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Gọi repository với query và currentUserId để loại trừ user hiện tại
            var results = _repository.SearchUsers(query, currentUserId);
            return Json(results);
        }

        // Action xử lý Share
        // Chia sẻ Mandala cho một User cụ thể với quyền mặc định là "read"
        [HttpPost]
        public IActionResult Share([FromBody] ShareRequest request)
        {
            try
            {
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                _repository.ShareMandala(request.MandalaId, request.SharedUserId, request.Permission, currentUserId);
                return Ok(new { message = "Mandala shared successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Action xử lý Delete
        [HttpPost]
        public IActionResult Delete(long mandalaId)
        {
            try
            {
                // Lấy user ID của người thực hiện xóa
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                // Gọi repository để xóa Mandala (hoặc soft delete)
                _repository.DeleteMandala(mandalaId, currentUserId);
                return Ok(new { message = "Mandala deleted successfully." });
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý lỗi
                return BadRequest(ex.Message);
            }
        }
    }
}
