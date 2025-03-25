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
    public class ChartController : Controller
    {
        private readonly MandalaRepository _repository;

        public ChartController(MandalaRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Chart(long? id)
        {
            long selectedId = id ?? 1;

            // Lấy ID của user hiện tại từ Claims (được lưu khi đăng nhập)
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Kiểm tra quyền truy cập:
            // Chỉ cho phép nếu user là người tạo Mandala hoặc Mandala được share cho user đó
            bool accessible = _repository.IsMandalaAccessible(currentUserId, selectedId);
            if (!accessible)
            {
                return Forbid();
            }

            // Lấy thông tin Mandala
            var targets9 = _repository.GetMandalaTargetsByIdMandala(selectedId);
            var targets3 = _repository.GetMandalaTargets3x3(selectedId);
            int classMandala = _repository.GetMandalaClassById(selectedId);

            // Truyền class sang view để kiểm tra giao diện hiển thị
            ViewBag.MandalaClass = classMandala;
            ViewBag.Avatar = _repository.GetAvatarPathByUserId(currentUserId);
            ViewBag.MandalaName = _repository.GetMandalaNameById(selectedId);
            ViewBag.Id = selectedId;

            var model = new MandalaChartViewModel
            {
                GridSize = 3, // mặc định hiển thị 3x3 nếu class = 1
                Placeholders9 = targets9,
                Placeholders3 = targets3
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpgradeMandalaClass(long mandalaId)
        {
            try
            {
                // Lấy ID của người dùng hiện tại từ Claims
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                DateTime now = DateTime.Now;

                // Cập nhật class cho Mandala từ 1 lên 2 (có thể bổ sung kiểm tra điều kiện ở repository)
                _repository.UpdateMandalaClass(mandalaId, 2, now, currentUserId);
                return Json(new { success = true, message = "Mandala class upgraded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error upgrading class: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveMandalaTargets(long mandalaId, List<string> data)
        {
            try
            {
                if (mandalaId <= 0 || data == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                // Lấy ID người dùng hiện tại từ Claims
                long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                DateTime now = DateTime.Now;

                _repository.SaveMandalaTargets(mandalaId, data, now, currentUserId);
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
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
