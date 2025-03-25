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
    public class TableController : Controller
    {
        private readonly MandalaRepository _repository;

        public TableController(MandalaRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Table(long mandalaId)
        {
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            bool accessible = _repository.IsMandalaAccessible(currentUserId, mandalaId);
            if (!accessible)
            {
                return Forbid();
            }

            var details = _repository.GetMandalaDetailsByMandalaId(mandalaId);
            // Nếu danh sách rỗng thì thêm một đối tượng trống để View luôn có ít nhất 1 dòng
            if (details == null || details.Count == 0)
            {
                details = new List<MandalaDetail>
        {
            new MandalaDetail() // Tạo 1 detail trống
        };
            }

            var targetOptions = _repository.GetTargetOptions(mandalaId);
            string avatar = _repository.GetAvatarPathByUserId(currentUserId);
            ViewBag.Avatar = avatar;
            ViewBag.MandalaName = _repository.GetMandalaNameById(mandalaId);
            ViewBag.Id = mandalaId;
            ViewBag.TargetOptions = targetOptions;

            return View(details);
        }

        [HttpPost]
        public IActionResult Save([FromBody] SaveMandalaRequest request)
        {
            try
            {
                if (request == null || request.MandalaId <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                // Lấy ID của user đang đăng nhập
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                DateTime now = DateTime.Now;

                _repository.DeleteMandalaDetails(request.DeletedIds);
                _repository.SaveMandalaDetails(request.MandalaId, request.Details, now, currentUserId);

                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving data: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateMandalaName(long id, string name)
        {
            try
            {
                // Lấy ID của người dùng hiện tại từ Claims
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                DateTime now = DateTime.Now;

                // Gọi repository để cập nhật tên và cập nhật ngày sửa, người sửa
                _repository.UpdateMandalaName(id, name, now, currentUserId);
                return Json(new { success = true, message = "Mandala name updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}