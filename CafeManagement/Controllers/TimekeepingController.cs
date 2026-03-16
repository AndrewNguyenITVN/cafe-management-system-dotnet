using CafeManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

public class TimekeepingController : Controller
{
    private readonly TimekeepingService _service;

    public TimekeepingController(TimekeepingService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Pin()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Pin(string pinCode)
    {
        var result = await _service.CheckInOrOutAsync(pinCode);

        ViewBag.Message = result;

        return View();
    }
}