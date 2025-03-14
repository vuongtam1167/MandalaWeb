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

        public IActionResult Index()
        {
            // Lấy ID của user hiện tại từ Claim
            long currentUserId = Convert.ToInt64(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            List<MandalaHome> mandalahomes = _repository.GetMandalaHomes(currentUserId);
            return View(mandalahomes);
        }
    }
}