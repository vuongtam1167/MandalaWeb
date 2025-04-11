using MandalaApp.DataAccess;
using MandalaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class TableController : Controller
{
    private readonly MandalaRepository _repository;

    public TableController(MandalaRepository repository)
    {
        _repository = repository;
    }

    string getGroupName(int mandalaLv)
    {
        if (mandalaLv >= 2 && mandalaLv <= 9) return "M0";
        if (mandalaLv >= 10 && mandalaLv <= 17) return "M1";
        if (mandalaLv >= 18 && mandalaLv <= 25) return "M2";
        if (mandalaLv >= 26 && mandalaLv <= 33) return "M3";
        if (mandalaLv >= 34 && mandalaLv <= 41) return "M4";
        if (mandalaLv >= 42 && mandalaLv <= 49) return "M5";
        if (mandalaLv >= 50 && mandalaLv <= 57) return "M6";
        if (mandalaLv >= 58 && mandalaLv <= 65) return "M7";
        if (mandalaLv >= 66 && mandalaLv <= 73) return "M8";
        return "Khác";
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
        // Cập nhật targetOptions: thêm tên nhóm dựa trên giá trị MandalaLv
        foreach (var item in targetOptions)
        {
            item.Text = getGroupName(_repository.GetMandalaLvByTarget(mandalaId, item.Value)) + " - " + item.Value;
        }
        ViewBag.TargetOptions = targetOptions;

        // Xác định quyền: owner / write (toàn quyền) hoặc read (chỉ xem)
        string permission = _repository.GetMandalaPermission(currentUserId, mandalaId);
        ViewBag.MandalaPermission = permission;

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

            // Cập nhật tên
            _repository.UpdateMandalaName(id, name, now, currentUserId);
            return Json(new { success = true, message = "Mandala name updated successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }
}
