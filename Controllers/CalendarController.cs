using Microsoft.AspNetCore.Mvc;
using LMS.Data.Entities;
using System;
using System.Collections.Generic;
using LMS.Data;

namespace LMS.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API để trả về sự kiện
        public JsonResult GetEvents()
        {
            var events = _context.Events.Select(e => new
            {
                e.Id,
                e.Title,
                start = e.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = e.End.ToString("yyyy-MM-ddTHH:mm:ss")
            }).ToList();

            return Json(events);
        }

        // API để lưu sự kiện khi kéo và thả
        [HttpPost]
        public JsonResult SaveEvent([FromBody] Event eventData)
        {
            if (eventData != null)
            {
                _context.Events.Add(eventData);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
    }
}
