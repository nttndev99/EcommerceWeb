using Ecommerce.Application.Common;

namespace Ecommerce.Application.DTOs.Customer;

// ─────────────────────────────────────────────
// READ
// ─────────────────────────────────────────────
public class CustomerDto
{
    public string   Id           { get; set; } = string.Empty;
    public string   FullName     { get; set; } = string.Empty;
    public string   Email        { get; set; } = string.Empty;
    public string?  PhoneNumber  { get; set; }
    public string?  AvatarUrl    { get; set; }
    public string?  Gender       { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string?  AddressLine  { get; set; }
    public string?  Ward         { get; set; }
    public string?  District     { get; set; }
    public string?  Province     { get; set; }
    public bool     IsActive     { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int      TotalOrders  { get; set; }
    public decimal  TotalSpent   { get; set; }
}

public class CustomerListDto
{
    public string   Id          { get; set; } = string.Empty;
    public string   FullName    { get; set; } = string.Empty;
    public string   Email       { get; set; } = string.Empty;
    public string?  PhoneNumber { get; set; }
    public string?  AvatarUrl   { get; set; }
    public bool     IsActive    { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int      TotalOrders { get; set; }
    public decimal  TotalSpent  { get; set; }
}

// ─────────────────────────────────────────────
// UPDATE (Admin edit customer)
// ─────────────────────────────────────────────
public class UpdateCustomerDto
{
    public string   Id          { get; set; } = string.Empty;
    public string   FullName    { get; set; } = string.Empty;
    public string?  PhoneNumber { get; set; }
    public string?  Gender      { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string?  AddressLine { get; set; }
    public string?  Ward        { get; set; }
    public string?  District    { get; set; }
    public string?  Province    { get; set; }
    public bool     IsActive    { get; set; }
}

// ─────────────────────────────────────────────
// FILTER
// ─────────────────────────────────────────────
public class CustomerFilterParams : PaginationParams
{
    public string? Search        { get; set; }
    public bool?   IsActive      { get; set; }
    public string  SortBy        { get; set; } = "createdAt";
    public string  SortDirection { get; set; } = "desc";
}