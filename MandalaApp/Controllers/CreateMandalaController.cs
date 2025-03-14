using MandalaApp.DataAccess;
using MandalaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace MandalaApp.Controllers
{
    public class CreateMandalaController : Controller
    {
        private readonly MandalaRepository _repository;
        private readonly IConfiguration _configuration;

        public CreateMandalaController(IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new MandalaRepository(configuration);
        }

        [HttpGet]
        public IActionResult CreateMandala()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateMandala(Mandala model, string Target)
        {
            if (ModelState.IsValid)
            {
                // Thiết lập các thuộc tính mặc định cho Mandala
                model.CreatedDate = DateTime.Now;
                // Ví dụ: Lấy CreatedUserID từ thông tin người dùng đang đăng nhập,
                // hoặc thiết lập mặc định nếu chưa tích hợp xác thực
                model.CreatedUserID = 1;
                // Thiết lập ngày sửa và người sửa bằng giá trị của ngày tạo và người tạo
                model.ModifiedDate = model.CreatedDate;
                model.ModifiedUserID = model.CreatedUserID;

                // Lưu Mandala và lấy ID mới tạo (giả sử repository.InsertMandala trả về long)
                long newMandalaId = _repository.InsertMandala(model);

                // Tạo bản ghi MandalaTarget với MandalaLv = 1
                var mandalaTarget = new MandalaTarget
                {
                    MandalaID = newMandalaId,
                    MandalaLv = 1,
                    Target = Target
                };

                _repository.InsertMandalaTarget(mandalaTarget);

                // Sau khi tạo xong, chuyển hướng về trang chủ hoặc trang danh sách
                return RedirectToAction("Index", "Index");
            }
            // Nếu có lỗi, hiển thị lại view với model hiện tại
            return View(model);
        }
    }
}
