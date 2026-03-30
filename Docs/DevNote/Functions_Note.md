Ecommerce.API            ← Controllers, Middleware
Ecommerce.Application    ← Services, DTOs, Interfaces
Ecommerce.Domain         ← Entities, Enums, Domain Interfaces
Ecommerce.Infrastructure ← DbContext, Repositories, UnitOfWork, Identity

Common file: PagedResult, Result, SlugHelper, ApplicationServiceRegistration, EntityConfiguration, UnitOfWork, InfrastructureServiceRegistration, ActionFilter

# Functions Structures (Repository, Service)
Domain
└── Entities/
    └── Category.cs       

Application
├── Common/
│   ├── PagedResult.cs
│   ├── Result.cs
│   └── SlugHelper.cs
├── DTOs/
│   └── Category/
│       ├── CategoryDto.cs
│       ├── CategoryListDto.cs
│       ├── CreateCategoryDto.cs
│       └── UpdateCategoryDto.cs
├── Interfaces/
│   ├── ICategoryRepository.cs
│   └── ICategoryService.cs
├── Services/
│   └── CategoryService.cs
└── ApplicationServiceRegistration.cs
Infrastructure
├── Persistence/
│   ├── EcommerceDbContext.cs
│   └── EntityConfiguration/
│       └── CategoryConfiguration.cs
├── Repositories/
│   └── CategoryRepository.cs
├── UnitOfWork/
│   └── UnitOfWork.cs
└── InfrastructureServiceRegistration.cs
MVC
├── Controllers/
│   └── CategoriesController.cs
├── Views/
│   └── Categories/
│       ├── Index.cshtml          ← danh sách + filter + phân trang
│       ├── Trash.cshtml          ← thùng rác (soft deleted)
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       ├── Detail.cshtml
│       ├── _SoftDelete.cshtml    ← partial confirm modal
│       ├── _HardDelete.cshtml    ← partial confirm modal
│       └── _CategoryToast.cshtml ← partial toast notification
└── Filters/
    └── ActionFilter.cs           ← logging / validation common

# Implements step by step
1. Domain Entity           → Category.cs
2. EntityConfiguration     
3. DbContext migration
4. DTOs                    
5. ICategoryRepository     → interface
6. CategoryRepository      → implement
7. IUnitOfWork             → thêm ICategoryRepository prop
8. ICategoryService        → interface
9. CategoryService         → implement
10. DI Registration        → ApplicationServiceRegistration
11. CategoriesController   
12. Views                  → Index → Create → Edit → Detail → Trash
13. Partials               → _SoftDelete, _HardDelete, _CategoryToast
14. ActionFilter           → apply lên controller




