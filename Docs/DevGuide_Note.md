# Folder Structure - Clean Architecture
SmartEcommerce/
│
├── SmartEcommerce.Domain/              # Không phụ thuộc bất kỳ layer nào
│   ├── Entities/
│   ├── Enums/
│   └── Common/           
│
├── SmartEcommerce.Application/         # Business logic, phụ thuộc Domain
│   ├── Interfaces/                     
│   │   ├── Repositories/               # Resopitory interfaces 
│   │   └── Services/.....              # Service interfaces 
│   ├── Services/                       # Service implementations đ
│   │   └── CategoryService.cs......
│   ├── DTOs/                           # Tất cả DTOs tập trung đây
│   │   └── Category/.....
│   ├── Common/   
│   ├── Mappings/                      # AutoMapper profiles
│   └── Behaviors/                      # MediatR behaviors (nếu dùng)
│
├── SmartEcommerce.Infrastructure/      # EF Core, SQL, external services. Phụ thuộc Application
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/
│   ├── Repositories/                   # Repository implementations 
│   ├── Migrations/
│   ├── UnitOfWork/
│   └── DependencyInjection.cs
│
├── SmartEcommerce.MVC/                 # Admin UI - chỉ dùng Application DTOs. Phụ thuộc Aplication và Infrastructure
│   ├── Areas/
│   │   └── Admin/                      # Manager, CRUD 
│   │       ├── Controllers/
│   │       └── Views/
│   ├── Controllers/                    #Chỉ để ViewModels cho Razor (PagingVM, ...) (READ, SERVICE ORDER, LOGIN.....)
│   ├── ViewModels/                     
│   └── Program.cs
│
└── SmartEcommerce.API/                 # REST API - chỉ dùng Application DTOs. Phụ thuộc Aplication và Infrastructure
    ├── Controllers/
    └── Program.cs

# Refernce
Domain         --------------- (Không có) ----- Đảm bảo tính thuần túy (Pure logic).
Application    --------------- Domain ----- Để sử dụng Entities và Domain Logic.
Infrastructure --------------- Application -----Để thực thi (Implement) các Interface từ Application.
WebUI          --------------- Application, Infrastructure ----- Để điều phối luồng và cấu hình DI lúc khởi chạy.
API            --------------- Aplication và Infrastructure
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


# Database Design

## Main Tables

Categories
Products
ProductVariants
ProductImages
Inventories
OrderItems
Orders
OrderTracking
Customers

(Identity)
AspNetUsers
AspNetUserRoles
AspNetUserClaims
AspNetRoles
AspNetRoleClaims
AspNetUserLogins
AspNetUserTokens


## Relationships
Categories 1 - 1 Categories
Categories 1 - N Products
Product 1 - N ProductVariants
Product 1 - N ProductImages
Product 1 - N Inventories
Product 1 - N OrderItems
Orders 1 - N OrderItems
ProductVariants 1 - N Inventories
ProductVariants 1 - N OrderItems
AspNetUsers 1 - N Orders 

(Identity)
AspNetUsers 1 - N AspNetUserTokens
AspNetUsers 1 - N AspNetUserLogins
AspNetUsers 1 - N AspNetUserClaims
AspNetUsers 1 - N AspNetUserRoles
AspNetUserRoles 1 - N AspNetRoles
AspNetRoles 1 - N AspNetRoleClaims
