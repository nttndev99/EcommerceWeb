# Folder Structure - Clean Architecture
SmartEcommerce/
│
├── SmartEcommerce.Domain/              # Không phụ thuộc bất kỳ layer nào
│   ├── Entities/
│   │   ├── Category.cs
│   │   └── Product.cs
│   ├── Interfaces/                     # ✅ Repository interfaces để đây
│   │   ├── ICategoryRepository.cs
│   │   └── IProductRepository.cs
│   ├── Enums/
│   ├── ValueObjects/
│   ├── Events/
│   └── Common/
│       └── BaseEntity.cs               # ✅ IsDeleted, CreatedAt, UpdatedAt
│
├── SmartEcommerce.Application/         # Business logic, không phụ thuộc Infrastructure
│   ├── Interfaces/                     # ✅ Service interfaces để đây
│   │   ├── ICategoryService.cs
│   │   └── IProductService.cs
│   ├── Services/                       # ✅ Service implementations để đây
│   │   ├── CategoryService.cs
│   │   └── ProductService.cs
│   ├── DTOs/                           # ✅ Tất cả DTOs tập trung đây
│   │   ├── Category/
│   │   │   ├── CategoryDto.cs
│   │   │   ├── CreateCategoryDto.cs
│   │   │   └── UpdateCategoryDto.cs
│   │   └── Product/
│   ├── Mappings/                       # AutoMapper profiles
│   └── Behaviors/                      # MediatR behaviors (nếu dùng)
│
├── SmartEcommerce.Infrastructure/      # EF Core, SQL, external services
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/
│   │       └── CategoryConfiguration.cs
│   ├── Repositories/                   # ✅ Repository implementations để đây
│   │   ├── CategoryRepository.cs
│   │   └── ProductRepository.cs
│   ├── Migrations/
│   └── DependencyInjection.cs
│
├── SmartEcommerce.MVC/                 # Admin UI - chỉ dùng Application DTOs
│   ├── Areas/
│   │   └── Admin/
│   │       ├── Controllers/
│   │       └── Views/
│   ├── ViewModels/                     # ✅ Chỉ để ViewModels cho Razor (PagingVM, ...)
│   └── Program.cs
│
└── SmartEcommerce.API/                 # REST API - chỉ dùng Application DTOs
    ├── Controllers/
    └── Program.cs

# Refernce
Domain         --------------- (Không có) ----- Đảm bảo tính thuần túy (Pure logic).
Application    --------------- Domain ----- Để sử dụng Entities và Domain Logic.
Infrastructure --------------- Application -----Để thực thi (Implement) các Interface từ Application.
WebUI          --------------- Application, Infrastructure ----- Để điều phối luồng và cấu hình DI lúc khởi chạy.

# OOP
Domain
 ├─ Entities (Encapsulation)
 ├─ BaseEntity (Inheritance)
Application
 ├─ Interfaces (Abstraction) / IRepositories
 ├─ Services (Polymorphism)

Infrastructure
 ├─ EFRepositories
MVC
 ├─ Controllers (DI + OOP)
 ├─ ViewModels


# AJAX vs TagHelper
Admin/Category  → AJAX      ✅ (đang build admin panel)
Admin/Product   → AJAX      ✅
Admin/Order     → AJAX      ✅

Shop/Product    → TagHelper ✅ (trang public, cần SEO)
Shop/Category   → TagHelper ✅