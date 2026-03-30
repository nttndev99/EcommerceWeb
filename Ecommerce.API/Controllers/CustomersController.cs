using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Customer;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    // ═══════════════════════════════════════════════════════════════
    // CUSTOMERS
    // ═══════════════════════════════════════════════════════════════
    [Authorize(Policy = "RequireAdmin")]
    public class CustomersController : BaseApiController
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
            => _customerService = customerService;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] CustomerFilterParams filter, CancellationToken ct)
        {
            var result = await _customerService.GetPagedAsync(filter, ct);
            return Paged(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var customer = await _customerService.GetByIdAsync(id, ct);
            return customer is null ? NotFound("Customer not found.") : Ok(customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
        {
            dto.Id = id;
            var result = await _customerService.UpdateAsync(dto, ct);
            return FromResult(result, "Customer updated.");
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(string id, CancellationToken ct)
        {
            var result = await _customerService.SetActiveAsync(id, isActive: true, ct);
            return FromResult(result, "Customer activated.");
        }

        [HttpPost("{id}/block")]
        public async Task<IActionResult> Block(string id, CancellationToken ct)
        {
            var result = await _customerService.SetActiveAsync(id, isActive: false, ct);
            return FromResult(result, "Customer blocked.");
        }
    }
}