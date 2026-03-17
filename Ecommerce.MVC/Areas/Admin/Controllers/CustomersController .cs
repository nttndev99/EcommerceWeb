using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Customer;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.Web.Controllers;
[Area("Admin")]
[Authorize(Policy = "RequireAdmin")]

public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // GET /Customers
    public async Task<IActionResult> Index(CustomerFilterParams filter, CancellationToken ct)
    {
        var result = await _customerService.GetPagedAsync(filter, ct);

        ViewBag.Filter       = filter;
        ViewBag.TotalAll     = await _customerService.CountAsync(null,  ct);
        ViewBag.TotalActive  = await _customerService.CountAsync(true,  ct);
        ViewBag.TotalBlocked = await _customerService.CountAsync(false, ct);

        return View(result);
    }

    // GET /Customers/Detail/{id}
    public async Task<IActionResult> Detail(string id, CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        if (customer is null) return NotFound();

        var orders = await _customerService.GetOrdersAsync(id, new PaginationParams(), ct);
        ViewBag.Orders = orders;

        return View(customer);
    }

    // GET /Customers/Edit/{id}
    public async Task<IActionResult> Edit(string id, CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        if (customer is null) return NotFound();

        var dto = new UpdateCustomerDto
        {
            Id          = customer.Id,
            FullName    = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Gender      = customer.Gender,
            DateOfBirth = customer.DateOfBirth,
            AddressLine = customer.AddressLine,
            Ward        = customer.Ward,
            District    = customer.District,
            Province    = customer.Province,
            IsActive    = customer.IsActive,
        };

        return View(dto);
    }

    // POST /Customers/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateCustomerDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _customerService.UpdateAsync(dto, ct);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(dto);
        }

        TempData["Success"] = "Customer updated successfully.";
        return RedirectToAction(nameof(Detail), new { id = dto.Id });
    }

    // POST /Customers/SetActive
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(string id, bool isActive, CancellationToken ct)
    {
        var result = await _customerService.SetActiveAsync(id, isActive, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? (isActive ? "Customer activated." : "Customer deactivated.")
            : result.Error;

        return RedirectToAction(nameof(Detail), new { id });
    }
}