# Paging - Helper (DUNG UI - SEO)
## File
@addTagHelper *, Ecommerce.MVC  @* ✅ Dòng này kích hoạt PageLinkTagHelper *@
```
## **ViewModel** của từng màn hình có paging
```
MVC/Areas/Admin/Models/CategoriesListViewModel.cs  ← có PagingInfo
MVC/Areas/Admin/Models/ProductListViewModel.cs     ← có PagingInfo
```
---

## Tóm tắt
```
_ViewImports.cshtml
    ↓ đăng ký
PageLinkTagHelper.cs
    ↓ dùng
PagingInfo.cs  ←── ViewModel chứa PagingInfo
    ↓ render ra
<div page-model="@Model.PagingInfo" ...>  ← trong View


# Filter - Search - Paging - Soft Delete - Hard Delete - Restore  - (Category) + Product + Inventory + ProductVariant + Order + Customer(Identity + Order)
Domain
Application: 
    Common: PagedResult, Result, SlugHelper (common)
    DTOs: Category
    Interface: ICategoryRepository, ICategoryService
    Service: CategoryService
    ApplicationServiceRegistration (common)
Infrastructure:
    Persistence: EcommerceDbContext, EntityConfiguration (common)
    Repository: CategoryRepository
    UnitOfWork (common)
    InfrastructureServiceRegistration (common)
MVC:
    CategoriesController 
    View: index, trash, create, edit, softdelete, harddelete, detail, _categorytoast
    Filters: ActionFilter (common)

# Auth + Email(Identity) + Roles-User(ADMIN) -------
Identitiy(Serice) + Auth(Service-ConfigureApplicationCookie(login) ) + Email(Service) + RolesManager(Service) + ManagerUser(Service)