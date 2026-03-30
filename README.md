# Ecommerce Web Platform

A full-stack e-commerce solution built on **ASP.NET Core** with a layered architecture, providing both a REST API and a server-rendered MVC web UI.

---

## Table of Contents

- [Projects](#projects)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Getting Started](#getting-started)
- [Seeded Test Users](#seeded-test-users)
- [API Reference](#api-reference)
- [Web UI](#web-ui)
- [Known Gaps & In-Progress Work](#known-gaps--in-progress-work)
- [Documentation](#documentation)
- [Project Status](#project-status)

---

## Projects

| Project | Description |
|---|---|
| `Ecommerce.API` | REST API with JWT Bearer authentication and Swagger UI |
| `Ecommerce.MVC` | Web UI built with ASP.NET Core MVC/Razor and cookie-based authentication |
| `Ecommerce.Application` | Application services and business workflow logic |
| `Ecommerce.Domain` | Core domain entities |
| `Ecommerce.Infrastructure` | EF Core DbContext, ASP.NET Core Identity integration, repositories, services, and DB seeding |

---

## Tech Stack

- **.NET 8** (`net8.0`)
- ASP.NET Core MVC / Web API
- ASP.NET Core Identity
- JWT Bearer (API authentication)
- Entity Framework Core + SQL Server
- Swagger / OpenAPI

---

## Features

### Authentication & Authorization
- Register / Login
- Forgot password / Reset password
- Email confirmation (sent on registration; note: confirmed email is **not required** to sign in — see [configuration notes](#configuration-notes))
- Role-based authorization with two roles: `Admin` and `Customer`

### Catalog
- Category management with soft delete
- Product management with variants (SKU) and product images

### Inventory
- Stock records with reserved quantity tracking
- Low-stock threshold support

### Order Management
- Create orders (authenticated users)
- Cancel orders — allowed when status is `Pending` or `Processing`
- Update order status (Admin only)
- Update payment status (Admin only)
- Supported payment methods: `CreditCard`, `BankTransfer`, `Cash` *(see [Known Gaps](#known-gaps--in-progress-work) for payment flow status)*

### Startup Seeding
On first run, the API automatically runs EF Core migrations and seeds:
- Roles (`Admin`, `Customer`)
- Admin user
- Sample customers
- Sample catalog, inventory, and orders

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server instance
- (Optional) [Mailtrap](https://mailtrap.io/) account for email flows

### 1. Configure Settings

**`Ecommerce.API/appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your SQL Server connection string>"
  },
  "JwtSettings": {
    "SecretKey": "<a long random secret>"
  },
  "Smtp": {
    "Host": "smtp.mailtrap.io",
    "Port": 587,
    "Username": "<mailtrap username>",
    "Password": "<mailtrap password>"
  }
}
```

**`Ecommerce.MVC/appsettings.json`**
- Ensure `ConnectionStrings:DefaultConnection` matches the API config above.

### 2. Run the API

```bash
dotnet run --project Ecommerce.API
```

| Resource | URL |
|---|---|
| API Base URL | `http://localhost:5000` |
| Swagger UI | `http://localhost:5000/` |
| OpenAPI JSON | `http://localhost:5000/swagger/v1/swagger.json` |

> **Database & Seeding:** Migrations and seed data run automatically on startup via `db.Database.MigrateAsync()` and `AppDbSeeder.SeedAsync(...)`. You do **not** need to run `dotnet ef database update` manually.

### 3. Run the MVC UI (Optional)

```bash
dotnet run --project Ecommerce.MVC
```

> **Port conflict:** Both projects default to port `5000`. Before running both simultaneously, change the MVC port in `Ecommerce.MVC/Properties/launchSettings.json`:
> ```json
> "applicationUrl": "http://localhost:5001"
> ```

---

## Seeded Test Users

| Role | Email | Password |
|---|---|---|
| Admin | `admin@ecommerce.com` | `Admin@123` |
| Customer | `alice@example.com` | `Customer@123` |
| Customer | `bob@example.com` | `Customer@123` |
| Customer | `carol@example.com` | `Customer@123` |

---

## API Reference

### Base Route
All endpoints are prefixed with `/api/v1/[controller]`.  
Example: `/api/v1/products`, `/api/v1/orders`

### Authentication
Include the JWT token in every authenticated request:
```
Authorization: Bearer <token>
```

Role policies:
- `RequireAdmin` — requires role `Admin`
- `RequireCustomer` — requires role `Customer`

### Response Format

**Standard response:**
```json
{
  "success": true,
  "message": "OK",
  "data": { }
}
```

**Paged response:**
```json
{
  "success": true,
  "message": "OK",
  "data": [ ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 42,
    "totalPages": 5
  }
}
```

### Endpoints

#### Auth
| Method | Endpoint | Auth |
|---|---|---|
| `POST` | `/api/v1/auth/register` | Public |
| `POST` | `/api/v1/auth/login` | Public |
| `POST` | `/api/v1/auth/forgot-password` | Public |
| `POST` | `/api/v1/auth/reset-password` | Public |
| `GET` | `/api/v1/auth/confirm-email?userId=...&token=...` | Public |

#### Categories
| Method | Endpoint | Auth |
|---|---|---|
| `GET` | `/api/v1/categories` | Public |
| `POST` | `/api/v1/categories` | Admin |

#### Products
| Method | Endpoint | Auth |
|---|---|---|
| `GET` | `/api/v1/products` | Public |
| `GET` | `/api/v1/products/slug/{slug}` | Public |
| `POST` | `/api/v1/products` | Admin |
| `GET` | `/api/v1/products/{id}/variants` | Public |
| `POST` | `/api/v1/products/{id}/variants` | Admin |
| `GET` | `/api/v1/products/{id}/images` | Public |
| `POST` | `/api/v1/products/{id}/images` | Admin |

#### Orders
| Method | Endpoint | Auth |
|---|---|---|
| `POST` | `/api/v1/orders` | Authenticated |
| `POST` | `/api/v1/orders/{id}/cancel` | Authenticated |
| `PATCH` | `/api/v1/orders/{id}/status` | Admin |
| `PATCH` | `/api/v1/orders/{id}/payment` | Admin |

> Additional modules (inventory, users/customers/roles, order stats) may expose further endpoints. Refer to the Swagger UI at `http://localhost:5000/` for a full, live list.

---

## Web UI

The MVC app (`Ecommerce.MVC`) uses cookie-based authentication. Key routes:

| Page | URL |
|---|---|
| Login | `/Auth/Login` |
| Logout | `/Auth/Logout` |
| Access Denied | `/Auth/AccessDenied` |

Main areas:
- **Storefront** — product listing, product detail, shopping cart
- **Admin dashboard** — category/product/order management (requires `Admin` role)

Unauthenticated users attempting to access protected pages are redirected to `/Auth/Login`.

---


## Known Gaps & In-Progress Work

The following features are partially implemented or pending:

| Area | Status |
|---|---|
| **Payment flow** | Payment status can be updated by Admin, but end-to-end payment processing is not yet implemented. `PaymentMethod` values (`CreditCard`, `BankTransfer`, `Cash`) are accepted but not fully processed. |
| **Coupon / discount logic** | Not yet implemented |
| **Order tracking** | Not yet implemented |
| **Image upload** | Product image endpoints accept records but file upload/storage is not fully wired |
| **Transaction bug** | A known concurrency/transaction issue is under investigation |

---

## Documentation

Extended documentation lives in the `Docs/` folder:
| File | Description |
|---|---|
| `Docs/00-overview.md` | Project overview |
| `Docs/01-prd.md` | Product Requirements Document |
| `Docs/02-srs.md` | Software Requirements Specification |
| `Docs/03-fsd.md` | Functional Specification Document |

## Project Status

### ✅ Completed

#### Authentication & Authorization
- Đăng ký / đăng nhập tài khoản với JWT (API) và cookie (MVC)
- Quên mật khẩu / đặt lại mật khẩu qua email
- Xác nhận email sau khi đăng ký (email confirmation)
- Phân quyền theo vai trò: `Admin` và `Customer`

#### Catalog
- Quản lý danh mục sản phẩm với soft delete (không xóa cứng khỏi DB)
- Quản lý sản phẩm với biến thể (SKU), ảnh sản phẩm

#### Inventory
- Theo dõi số lượng tồn kho và số lượng đang giữ (reserved quantity)
- Hỗ trợ cảnh báo tồn kho thấp (low-stock threshold)

#### Order Management
- Tạo đơn hàng (người dùng đã xác thực)
- Hủy đơn khi trạng thái là `Pending` hoặc `Processing`
- Cập nhật trạng thái đơn hàng (Admin)
- Cập nhật trạng thái thanh toán (Admin)

#### Infrastructure
- Tự động chạy migration và seed dữ liệu khi khởi động
- Seed sẵn roles, admin, khách hàng mẫu, catalog, inventory, và đơn hàng mẫu

---

### 🔧 Cần Cải Thiện

#### 1. Luồng thanh toán (Payment Flow)
- **Hiện tại:** Admin có thể cập nhật trạng thái thanh toán thủ công, các giá trị `PaymentMethod` (`CreditCard`, `BankTransfer`, `Cash`) được lưu nhưng chưa xử lý thực tế.
- **Cần làm:** Tích hợp payment gateway hoặc xây dựng luồng xác nhận thanh toán end-to-end.

#### 2. Coupon / Giảm giá
- **Hiện tại:** Chưa có tính năng mã giảm giá.
- **Cần làm:** Thiết kế entity `Coupon`, logic áp dụng khi tạo đơn hàng, giới hạn số lần dùng.

#### 3. Theo dõi đơn hàng (Order Tracking)
- **Hiện tại:** Không có lịch sử thay đổi trạng thái đơn.
- **Cần làm:** Thêm bảng `OrderStatusHistory`, ghi nhận mỗi lần thay đổi trạng thái kèm timestamp và actor.

#### 4. Upload ảnh sản phẩm (Image Upload)
- **Hiện tại:** Endpoint nhận record ảnh nhưng chưa xử lý lưu trữ file thực tế.
- **Cần làm:** Tích hợp lưu file lên local disk hoặc cloud storage (Azure Blob / S3), trả về URL sau khi upload.

#### 5. Lỗi transaction / concurrency
- **Hiện tại:** Có lỗi đã biết liên quan đến concurrency hoặc transaction khi xử lý đơn hàng/tồn kho đồng thời.
- **Cần làm:** Điều tra và áp dụng optimistic concurrency (EF Core `RowVersion`) hoặc transaction scope phù hợp.

#### 6. Endpoint còn thiếu tài liệu
- **Hiện tại:** Một số module (inventory, users/roles, order stats) chưa được liệt kê đầy đủ trong README.
- **Cần làm:** Bổ sung bảng endpoint cho các module còn lại; duy trì README đồng bộ với Swagger.

#### 7. Cấu hình xác nhận email
- **Hiện tại:** `RequireConfirmedEmail` đang bị tắt trong `Program.cs`, nghĩa là người dùng có thể đăng nhập mà không cần xác nhận email.
- **Cần làm:** Quyết định và thống nhất chính sách — nếu muốn bắt buộc xác nhận, bật lại cờ và xử lý UX phù hợp.

---
