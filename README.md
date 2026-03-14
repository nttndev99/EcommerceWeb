# 🛒 Excommerce — Admin Dashboard

**ASP.NET Core MVC · Clean Architecture · EF Core · Repository + UoW**

---

## 📐 Kiến trúc tổng thể

```
Excommerce/
├── Excommerce.Domain/          # Layer 1 — Entities, Interfaces (không phụ thuộc gì)
├── Excommerce.Application/     # Layer 2 — DTOs, Services, Business Logic
├── Excommerce.Infrastructure/  # Layer 3 — EF Core, Repositories, UnitOfWork
└── Excommerce.MVC/             # Layer 4 — MVC Controllers, Views, Filters
```

### Dependency Flow (Clean Architecture)

```
Web → Application → Domain
Infrastructure → Domain
Web → Infrastructure (chỉ qua DI)
```

---

## 🗂 Cấu trúc file chi tiết

### 🔵 Ecommerce.Domain
| File | Vai trò |
|------|---------|
| `Common/BaseEntity.cs` | Base class với Id, CreatedAt, UpdatedAt, IsDeleted |
| `Entities/Category.cs` | Entity danh mục (self-referencing) |
| `Entities/Product.cs` | Entity sản phẩm |
| `Entities/ProductVariant.cs` | Entity sản phẩm |
| `Entities/ProductVariant.cs` | Entity sản phẩm |

| `Interfaces/IRepository.cs` | Generic Repository interface |
| `Interfaces/IProductRepository.cs` | Specific Product queries |
| `Interfaces/ICategoryRepository.cs` | Specific Category queries |
| `Interfaces/IUnitOfWork.cs` | Unit of Work interface |

### 🟢 Ecommerce.Application
| File | Vai trò |
|------|---------|
| `Common/PagedResult.cs` | Generic paging + ServiceResult wrapper |
| `DTOs/Product/ProductDto.cs` | DTO read / CreateProductDto / UpdateProductDto / Filter |
| `DTOs/Category/CategoryDto.cs` | DTO read / CreateCategoryDto / UpdateCategoryDto / Filter |
| `Interfaces/IProductService.cs` | Service contract |
| `Interfaces/ICategoryService.cs` | Service contract |
| `Services/ProductService.cs` | Business logic (CRUD, validation, mapping) |
| `Services/CategoryService.cs` | Business logic (CRUD, hierarchy check) |

### 🟡 Ecommerce.Infrastructure
| File | Vai trò |
|------|---------|
| `Data/AppDbContext.cs` | EF Core DbContext + Fluent API + Seed + Soft-Delete filter |
| `Repositories/Repository.cs` | Generic Repository<T> implementation |
| `Repositories/ProductRepository.cs` | Optimized LINQ Search + dynamic sort |
| `Repositories/CategoryRepository.cs` | Category search with children/products |
| `UnitOfWork/UnitOfWork.cs` | Aggregates repos, manages transactions |

### 🔴 Ecommerce.Web
| File | Vai trò |
|------|---------|
| `Program.cs` | DI registration, middleware pipeline |
| `Filters/ActionFilters.cs` | ValidateModelFilter, AuditLogFilter, GlobalExceptionFilter, AdminAuthFilter |
| `Controllers/ProductsController.cs` | CRUD + Search + Filter + Paging + AJAX |
| `Controllers/CategoriesController.cs` | CRUD + Search + Filter + Paging + AJAX |
| `Views/Shared/_Layout.cshtml` | Dark admin dashboard layout |
| `Views/Products/Index.cshtml` | List + Filter bar + Pagination + Toggle |
| `Views/Products/Create.cshtml` | Form với Model Validation |
| `Views/Products/Edit.cshtml` | Pre-filled form |
| `Views/Categories/Index.cshtml` | List + Filter + Toggle |
| `Views/Categories/Create.cshtml` | Form |
| `Views/Categories/Edit.cshtml` | Form |

---

## ✅ Các kỹ thuật được áp dụng

### 1. Clean Architecture
- Domain không phụ thuộc bất kỳ layer nào
- Application chỉ dùng Domain interfaces
- Infrastructure implement Domain interfaces
- Web chỉ biết Application services (qua DI)

### 2. Dependency Injection (DI)
```csharp
// Program.cs
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<AuditLogFilter>();
```

### 3. Repository Pattern
```csharp
// Generic IRepository<T> với CRUD + Paging
// IProductRepository extends với SearchProductsAsync
// IsCategoryRepository extends với HasProductsAsync
```

### 4. Unit of Work
```csharp
// IUnitOfWork gom tất cả repositories
// SaveChangesAsync() một lần duy nhất
// BeginTransaction / Commit / Rollback
var product = new Product { ... };
await _uow.Products.AddAsync(product);
await _uow.SaveChangesAsync(); // 1 lần commit
```

### 5. Model Binding
```csharp
// Auto binding từ query string
public async Task<IActionResult> Index(
    [FromQuery] string? keyword,
    [FromQuery] int? categoryId,
    [FromQuery] decimal? minPrice,
    ...)

// Auto binding từ form POST
public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
```

### 6. Model Validation
```csharp
// Data Annotations trên DTO
[Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
[StringLength(200, MinimumLength = 2)]
[Range(0.01, 999_999_999)]
public decimal Price { get; set; }

// Controller check
if (!ModelState.IsValid) return View(dto);
```

### 7. Action Filters
```csharp
// AuditLogFilter    — log mọi action kèm thời gian thực thi
// GlobalExceptionFilter — bắt tất cả exception chưa handle
// ValidateModelFilter   — tự động validate ModelState
// AdminAuthFilter       — kiểm tra quyền Admin

[ServiceFilter(typeof(AuditLogFilter))]
public class ProductsController : Controller { ... }
```

### 8. Data Transfer Objects
```csharp
// Domain Entity  → ProductDto       (read)
// ProductDto     → CreateProductDto (write/validation)
// UpdateProductDto extends CreateProductDto + Id
// ProductFilterDto cho search/filter/sort/page
```

### 9. Entity Framework Core
```csharp
// Fluent API config: index, constraints, relationships
// Global Query Filter: HasQueryFilter(x => !x.IsDeleted)
// Auto timestamp: override SaveChangesAsync
// Seed Data: mb.Entity<Product>().HasData(...)
// AsNoTracking() cho read queries
```

### 10. Performance LINQ
```csharp
// AsNoTracking() — không track entity khi chỉ đọc
// Include() — eager loading tránh N+1
// CountAsync() chạy trước Skip/Take
// Switch expression cho dynamic sort
// IgnoreQueryFilters() khi cần check soft-deleted
// Composite index: IsActive + IsFeatured
```

---

## 🚀 Chạy ứng dụng

```bash
# 1. Clone & restore
cd Excommerce.Web
dotnet restore

# 2. Run (SQLite auto-create on first run)
dotnet run

# 3. Truy cập
# http://localhost:5000/Products
# http://localhost:5000/Categories
```

### Migration (nếu cần)
```bash
cd Excommerce.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Excommerce.Web
dotnet ef database update --startup-project ../Excommerce.Web
```

---

## 🔌 Endpoints

| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/Products` | List + Search + Filter + Paging |
| GET | `/Products/Create` | Form thêm mới |
| POST | `/Products/Create` | Submit tạo |
| GET | `/Products/Edit/{id}` | Form chỉnh sửa |
| POST | `/Products/Edit/{id}` | Submit cập nhật |
| GET | `/Products/Details/{id}` | Chi tiết |
| POST | `/Products/Delete/{id}` | Soft delete (AJAX) |
| POST | `/Products/ToggleStatus/{id}` | Bật/tắt (AJAX) |
| GET | `/Categories` | List + Filter |
| GET | `/Categories/Create` | Form thêm |
| POST | `/Categories/Create` | Submit |
| GET | `/Categories/Edit/{id}` | Form sửa |
| POST | `/Categories/Edit/{id}` | Submit |
| POST | `/Categories/Delete/{id}` | Soft delete |
| POST | `/Categories/ToggleStatus/{id}` | Toggle |

---

## 🎨 Giao diện

- Dark theme admin dashboard
- Sidebar navigation
- Filter bar với search, dropdown, range
- Responsive data table với sort indicators
- Toggle switch cho trạng thái
- Modal xác nhận xóa (AJAX)
- Toast notification
- Pagination với page range
