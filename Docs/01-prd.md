# Product Requirement Document (PRD)
## Ecommerce Web Platform

| Field       | Detail                          |
|-------------|---------------------------------|
| Version     |                                 |
| Status      | In Development                  |
| Date        |                                 |
| Stack       | ASP.NET Core MVC/API            |

---

## 1. Product Overview

Hệ thống thương mại điện tử cho phép khách hàng mua sắm trực tuyến và quản trị viên quản lý toàn bộ vận hành bao gồm danh mục, sản phẩm, đơn hàng, kho hàng và người dùng.

**Mục tiêu kinh doanh:**
- Cung cấp nền tảng bán hàng trực tuyến 
- Quản lý tồn kho theo thời gian thực
- Hỗ trợ đa biến thể sản phẩm (màu sắc, kích cỡ, chất liệu)
- Theo dõi trạng thái đơn hàng từ khi tạo đến khi giao hàng

---

## 2. Stakeholders

| Role          | Mô tả                                                    |
|---------------|----------------------------------------------------------|
| Admin         | Quản lý toàn bộ hệ thống — sản phẩm, đơn hàng, người dùng |
| Customer      | Người mua hàng đã đăng ký tài khoản                      |
| Guest         | Người dùng chưa đăng nhập — chỉ xem sản phẩm            |

---

## 3. User Stories

### Admin
- Quản lý danh mục sản phẩm theo cây phân cấp (cha-con)
- Tạo sản phẩm với nhiều biến thể (màu, size) và ảnh riêng
- Theo dõi tồn kho và nhận cảnh báo khi hàng sắp hết
- Cập nhật trạng thái đơn hàng theo từng bước
- Quản lý tài khoản người dùng — khoá/mở khoá, phân quyền
- Xem thống kê đơn hàng theo trạng thái

### Customer
- Đăng ký/đăng nhập tài khoản
- Xem danh sách sản phẩm với bộ lọc và tìm kiếm
- Xem chi tiết sản phẩm với đầy đủ biến thể và ảnh
- Đặt hàng và chọn phương thức thanh toán (COD / Online)
- Xem lịch sử đơn hàng của mình
- Huỷ đơn hàng khi còn ở trạng thái Pending / Processing
- Reset mật khẩu qua email

---

## 4. Feature List

### P0 — Must Have (MVP)
| Feature                        | Module        |
|--------------------------------|---------------|
| Đăng ký / Đăng nhập / Logout   | Auth          |
| Forgot & Reset Password        | Auth          |
| Quản lý danh mục (CRUD + Trash)| Category      |
| Quản lý sản phẩm (CRUD + Trash)| Product       |
| Quản lý biến thể sản phẩm      | ProductVariant|
| Quản lý ảnh sản phẩm           | ProductImage  |
| Quản lý tồn kho                | Inventory     |
| Tạo đơn hàng                   | Order         |
| Cập nhật trạng thái đơn hàng   | Order         |
| Xem lịch sử đơn hàng           | Order         |
| Quản lý người dùng (Admin)     | User          |
| Phân quyền Role                | Role          |

### P1 — Should Have
| Feature                        | Module        |
|--------------------------------|---------------|
| Soft Delete + Restore          | Category/Product/Variant |
| Low Stock Alert                | Inventory     |
| Order cancel (Customer)        | Order         |
| Email welcome + reset password | Email         |
| Thống kê đơn hàng theo status  | Dashboard     |
| Trang chủ: tab New/Sale/Featured | Storefront  |

### P2 — Nice to Have
| Feature                        | Module        |
|--------------------------------|---------------|
| Coupon / Discount              | Coupon        |
| Order Tracking chi tiết        | OrderTracking |
| Payment Gateway (VNPay/Momo)   | Payment       |
| Product Review & Rating        | Review        |
| Upload ảnh (Cloudinary)        | Media         |
| Tìm kiếm nâng cao              | Search        |

---

## 5. Domain Model Summary (từ ERD)

```
AppUser
  └── Order (1:N) — places
        └── OrderItem (1:N) — contains
              ├── Product (N:1) — referenced in
              └── ProductVariant (N:1, optional) — referenced in

Category (self-ref parent-child)
  └── Product (1:N) — has
        ├── ProductVariant (1:N) — has
        │     └── Inventory (1:N) — stocked in
        ├── ProductImage (1:N) — has
        └── Inventory (1:N) — tracked in (product-level)
```

---

## 6. Non-Functional Requirements

| Category       | Requirement                                               |
|----------------|-----------------------------------------------------------|
| Security       | JWT (API) + Cookie HttpOnly (MVC), Role-based Authorization |
| Performance    | Tất cả list endpoint phân trang, AsNoTracking cho read    |
| Availability   | 99.9% uptime, retry on failure (3 lần)                   |
| Scalability    | Stateless API — có thể scale horizontal                  |
| Data Integrity | Transaction bắt buộc cho Create/Update Order             |
| Soft Delete    | Product, Category, Variant — không xoá vật lý           |

---

## 7. Constraints & Assumptions

- Database: SQL Server (EF Core Code-First)
- Auth: ASP.NET Core Identity
- Email: Mailtrap (test), SMTP thật (production)
- Ảnh sản phẩm: lưu URL — chưa có file upload trong MVP
- Coupon: field có sẵn trong Order, chưa implement logic
- Payment: chỉ có COD trong MVP, Online Payment là P2

---

## 8. Development Checklist

### Backend
- [x] Layered Architecture (Domain / Application / Infrastructure / MVC,API)
- [x] EF Core + SQL Server Migrations
- [x] Identity + JWT Auth
- [x] Category CRUD + Soft Delete + Restore
- [x] Product + Variant + Image CRUD
- [x] Inventory Management
- [x] Order State Machine
- [x] Customer / User / Role Management
- [ ] Transaction cho Order (bug đã phát hiện — cần fix)
- [ ] Coupon entity + logic
- [ ] Order Tracking API
- [ ] Image Upload

### Frontend (Template/Bootstrap)
- [ ] Project setup + Interceptors
- [ ] AuthService + Guards
- [ ] Storefront: Product list, Detail
- [ ] Checkout + Order flow
- [ ] Admin Panel: Dashboard, Products, Orders, Users