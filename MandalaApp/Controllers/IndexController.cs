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

        // Trang Index: hiển thị danh sách MandalaHome của người dùng
        public IActionResult Index()
        {
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            string avatar = _repository.GetAvatarPathByUserId(currentUserId);
            List<MandalaHome> mandalahomes = _repository.GetMandalaHomes(currentUserId);
            ViewBag.Avatar = avatar;
            return View(mandalahomes);
        }

        [HttpGet]
        public IActionResult SearchUsers(string query, long mandalaid)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<UserSearchResult>());
            }
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var results = _repository.SearchUsers(query, currentUserId, mandalaid);
            return Json(results);
        }

        [HttpGet]
        public IActionResult SearchShare(long mandalaid, long usershare)
        {
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var results = _repository.SearchShare(mandalaid, usershare);
            return Json(results);
        }

        [HttpGet]
        public JsonResult GetMandalaStatusChart(int mandalaId)
        {
            var data = _repository.GetStatusChart(mandalaId);
            return Json(data); 
        }

        // Action Share: nếu share chưa tồn tại thì tạo mới, nếu đã tồn tại thì cập nhật quyền
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

        // Action RemoveShare: hủy chia sẻ Mandala cho user cụ thể
        [HttpPost]
        public IActionResult RemoveShare(long mandalaId, long sharedUserId)
        {
            try
            {
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                _repository.RemoveShareMandala(mandalaId, sharedUserId, currentUserId);
                return Ok(new { message = "Mandala share removed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Action Delete: xóa Mandala
        [HttpPost]
        public IActionResult Delete(long mandalaId)
        {
            try
            {
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                _repository.DeleteMandala(mandalaId, currentUserId);
                return Ok(new { message = "Mandala deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}