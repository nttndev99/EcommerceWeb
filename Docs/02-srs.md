# Software Requirements Specification (SRS)
## Ecommerce Web Platform

| Field       | Detail                     |
|-------------|----------------------------|
| Version     |                            |
| Date        |                            |
| Status      |                            |

---

## 1. Introduction

### 1.1 Purpose
Tài liệu này mô tả đầy đủ các yêu cầu phần mềm cho hệ thống Ecommerce Web Platform — bao gồm yêu cầu chức năng, phi chức năng, ràng buộc hệ thống và giao diện ngoài.

### 1.2 Scope
Hệ thống bao gồm:
- **REST API** (ASP.NET Core) — backend được test/run với swagger
- **MVC** (ASP.NET Core) — backend phục vụ cho razor view
- **SQL Server** — cơ sở dữ liệu quan hệ

### 1.3 Definitions

| Thuật ngữ       | Định nghĩa                                                    |
|-----------------|---------------------------------------------------------------|
| Product         | Sản phẩm gốc — chứa thông tin chung, giá cơ bản              |
| ProductVariant  | Biến thể sản phẩm — phân biệt theo màu, size, chất liệu      |
| Inventory       | Bản ghi tồn kho — gắn với Product hoặc ProductVariant        |
| Order           | Đơn hàng của khách — chứa nhiều OrderItem                    |
| OrderItem       | Dòng hàng trong đơn — snapshot giá tại thời điểm đặt         |
| Soft Delete     | Đánh dấu IsDeleted = true thay vì xoá khỏi database          |
| Reserved Stock  | Số lượng hàng đang được giữ cho đơn hàng chưa giao           |
| Available Stock | Quantity - ReservedQuantity                                   |
| JWT             | JSON Web Token — dùng để xác thực API request                |

---

## 2. Overall Description

### 2.1 System Architecture

```
┌─────────────────────────────────────────────────┐
│              Frontend (Template / Bootstrap)    │
│   Storefront (Customer) | Admin Panel (Admin)   │
└──────────────────┬──────────────────────────────┘
                   │ HTTPS / REST / JSON
┌──────────────────▼──────────────────────────────┐
│           ASP.NET Core Web MVC/API              │
│  ┌──────────┐ ┌──────────┐ ┌─────────────────┐  │
│  │Controllers│ │Middleware│ │   JWT Auth     │  │
│  └────┬─────┘ └──────────┘ └─────────────────┘  │
│  ┌────▼───────────────────────────────────────┐  │
│  │         Application Layer (Services)        │  │
│  │  OrderService | ProductService | AuthService│  │
│  └────┬───────────────────────────────────────┘  │
│  ┌────▼───────────────────────────────────────┐  │
│  │      Infrastructure (Repositories + UoW)   │  │
│  └────┬───────────────────────────────────────┘  │
└───────┼─────────────────────────────────────────┘
        │ EF Core
┌───────▼──────────┐
│   SQL Server DB   │
└──────────────────┘
```

### 2.2 User Classes

| Class    | Permissions                                                  |
|----------|--------------------------------------------------------------|
| Admin    | Full access — CRUD mọi entity, quản lý users, xem thống kê  |
| Customer | Xem sản phẩm, đặt hàng, xem orders của mình, huỷ đơn        |
| Guest    | Chỉ xem sản phẩm — không đặt hàng                           |

### 2.3 Assumptions
- Người dùng có trình duyệt web (Chrome 100+, Firefox 100+, Edge 100+)
- Kết nối internet ổn định
- SQL Server được cài đặt và cấu hình sẵn

---

## 3. Functional Requirements

---

### 3.1 Authentication & Authorization

#### FR-AUTH-001: Đăng ký tài khoản
- **Input:** FullName, Email, Password (≥6 ký tự, có số)
- **Process:**
  1. Kiểm tra email chưa tồn tại trong hệ thống
  2. Tạo AppUser với IsActive = true
  3. Gán role Customer
  4. Gửi email chào mừng qua SMTP
- **Output:** Tài khoản được tạo; trả HTTP 201
- **Error:** Email đã tồn tại → HTTP 400

#### FR-AUTH-002: Đăng nhập
- **Input:** Email, Password, RememberMe (bool)
- **Process:**
  1. Tìm user theo Email
  2. Kiểm tra IsActive = true
  3. Xác thực password — lockout sau 5 lần sai
  4. Sinh JWT token (API) hoặc set Cookie (MVC)
- **Output:** Token + thông tin user; HTTP 200
- **Error:** Sai credentials → HTTP 401; Bị khóa → HTTP 423

#### FR-AUTH-003: Đăng xuất
- Xóa cookie phía server / hướng dẫn client xóa token
- Redirect về trang Login

#### FR-AUTH-004: Quên mật khẩu
- **Input:** Email
- **Process:** Sinh reset token → gửi link qua email (hết hạn sau 1 giờ)
- **Note:** Không lộ thông tin email có tồn tại hay không

#### FR-AUTH-005: Reset mật khẩu
- **Input:** Token, Email, NewPassword, ConfirmPassword
- **Validate:** Token còn hiệu lực, NewPassword == ConfirmPassword
- **Output:** Mật khẩu được cập nhật; redirect Login

---

### 3.2 Category Management

#### FR-CAT-001: Danh sách danh mục
- Phân trang (default 10,25,50/trang)
- Filter: tên, IsActive, ParentId
- Hiển thị tên danh mục cha

#### FR-CAT-002: Tạo danh mục
- **Input:** Name, Slug (auto-gen nếu trống), Description, ImageUrl, ParentId, DisplayOrder
- **Validate:** Slug unique toàn hệ thống
- **Process:** Tạo Category với IsDeleted = false, IsActive = true

#### FR-CAT-003: Cập nhật danh mục
- **Validate:** Slug unique (ngoại trừ chính nó)
- Cập nhật UpdatedAt = UtcNow

#### FR-CAT-004: Soft Delete
- Set IsDeleted = true, cascade sang tất cả Children
- **Guard:** Không hard delete nếu còn Children chưa xóa

#### FR-CAT-005: Restore
- Set IsDeleted = false
- Không tự động restore Children

#### FR-CAT-006: Hard Delete
- **Guard:** Còn Products → trả Failure
- Xóa vật lý khỏi database

---

### 3.3 Product Management

#### FR-PRD-001: Danh sách sản phẩm
- Phân trang + Filter (Search, CategoryId, Status, IsFeatured, Brand, MinPrice, MaxPrice, InStock)
- Sort: name / price / status / createdAt (asc/desc)
- Hiển thị: TotalStock = SUM(Inventory.Quantity - Inventory.ReservedQuantity)

#### FR-PRD-002: Chi tiết sản phẩm
- Lấy theo Id hoặc Slug
- Bao gồm: CategoryName, TotalStock, PrimaryImageUrl

#### FR-PRD-003: Tạo sản phẩm
- **Input:** Name, Slug, Description, ShortDescription, BasePrice, SalePrice, SKU, Status, IsFeatured, CategoryId, Brand, Weight, Tags
- **Validate:** Slug unique; CategoryId tồn tại và chưa bị xóa
- **Side Effect:** Tự động tạo Inventory record (Quantity = 0) cho product-level

#### FR-PRD-004: Cập nhật sản phẩm
- Validate Slug unique (ngoại trừ chính nó)
- Cập nhật UpdatedAt

#### FR-PRD-005: Soft Delete
- Set IsDeleted = true; cascade sang tất cả Variants

#### FR-PRD-006: Restore
- Set IsDeleted = false (không tự restore Variants)

#### FR-PRD-007: Hard Delete
- **Guard:** Còn Variants → trả Failure

#### FR-PRD-008: Sản phẩm trang chủ
- Lấy tối đa 8 sản phẩm theo tab: `new` / `sale` / `featured`
- Chỉ lấy sản phẩm Active và còn hàng (AvailableStock > 0)

---

### 3.4 Product Variant Management

#### FR-VAR-001: Danh sách variant theo product
- Phân trang; filter: search (Name/SKU), IsActive
- Sort: name / price / displayOrder

#### FR-VAR-002: Tạo variant
- **Input:** ProductId, Name, SKU, Price, SalePrice, Color, ColorHex, Size, Material, ImageUrl, IsActive, DisplayOrder
- **Validate:** SKU unique toàn hệ thống
- **Side Effect:** Tự động tạo Inventory record cho variant

#### FR-VAR-003: Cập nhật variant
- Validate SKU unique (ngoại trừ chính nó)

#### FR-VAR-004: Soft Delete / Restore / Hard Delete
- Hard Delete: xóa cả Inventory records của variant đó

#### FR-VAR-005: Empty Trash
- Xóa vĩnh viễn toàn bộ variants đã soft-delete của một product

---

### 3.5 Product Image Management

#### FR-IMG-001: Danh sách ảnh
- Lấy toàn bộ ảnh theo ProductId, sắp xếp theo DisplayOrder

#### FR-IMG-002: Thêm ảnh
- **Input:** ProductId, ImageUrl, AltText, IsPrimary, DisplayOrder
- Nếu IsPrimary = true: set IsPrimary = false cho tất cả ảnh khác của product

#### FR-IMG-003: Xóa ảnh
- Hard delete trực tiếp (ảnh không có soft delete)

---

### 3.6 Inventory Management

#### FR-INV-001: Danh sách tồn kho
- Phân trang; filter IsLowStock (AvailableQuantity ≤ LowStockThreshold)
- Hiển thị: ProductName, VariantName, SKU, Quantity, Reserved, Available

#### FR-INV-002: Cập nhật tồn kho
- Set Quantity trực tiếp (nhập kho đợt mới)
- **Validate:** Quantity ≥ ReservedQuantity

#### FR-INV-003: Điều chỉnh tồn kho
- Cộng/trừ Delta vào Quantity (kiểm kê, hàng hỏng)
- **Validate:** Quantity sau điều chỉnh ≥ 0

---

### 3.7 Order Management

#### FR-ORD-001: Tạo đơn hàng
**Process (trong 1 Transaction):**
1. Validate: Items không rỗng, Quantity > 0
2. Với mỗi item:
   - Load Product (phải Active)
   - Load Variant nếu có (phải IsActive)
   - Resolve giá: `SalePrice ?? BasePrice`
   - Load Inventory — kiểm tra AvailableQuantity ≥ Quantity
   - `ReservedQuantity += Quantity`
3. Apply Coupon (nếu có — hiện là placeholder)
4. Sinh OrderCode (format: ORD-YYYYMMDD-NNNN)
5. Lưu Order + OrderItems + Inventory trong 1 SaveChangesAsync

**Input:** UserId (từ JWT), Items[], ShippingAddress, PaymentMethod, CouponCode, ShippingFee, Note

#### FR-ORD-002: Cập nhật trạng thái đơn hàng (Admin)
**State machine:**
```
Pending    → Processing | Cancelled
Processing → Shipped    | Cancelled
Shipped    → Delivered  | Cancelled
Delivered  → (terminal)
Cancelled  → (terminal)
```

**Side effects:**
- → Cancelled: `ReservedQuantity -= item.Quantity` (release stock)
- → Delivered: `ReservedQuantity -= qty` AND `Quantity -= qty` (deduct vĩnh viễn)
- → Shipped: `ShippedAt = UtcNow`
- → Delivered: `DeliveredAt = UtcNow`
- → Cancelled: `CancelledAt = UtcNow`, lưu CancelReason

**Tất cả trong 1 Transaction.**

#### FR-ORD-003: Cập nhật trạng thái thanh toán (Admin)
- PaymentStatus: Unpaid → Paid | Refunded
- Nếu → Paid: `PaidAt = UtcNow`

#### FR-ORD-004: Huỷ đơn hàng (Customer)
- **Guard:** Chỉ được huỷ khi OrderStatus ∈ {Pending, Processing}
- **Guard:** Chỉ được huỷ đơn của chính mình
- Gọi UpdateOrderStatusAsync với Cancelled

#### FR-ORD-005: Xem danh sách đơn hàng
- Admin: xem tất cả, có thể filter theo UserId
- Customer: chỉ xem orders của mình (filter UserId = CurrentUser)

#### FR-ORD-006: Thống kê đơn hàng (Admin)
- Count theo từng OrderStatus
- Tổng doanh thu (Delivered orders)

---

### 3.8 Customer Management (Admin)

#### FR-CUS-001: Danh sách khách hàng
- Phân trang, tìm kiếm theo Email/FullName

#### FR-CUS-002: Chi tiết khách hàng
- Thông tin cá nhân + địa chỉ mặc định + lịch sử đơn hàng

#### FR-CUS-003: Cập nhật thông tin

#### FR-CUS-004: Activate / Block
- Activate: `IsActive = true`, `LockoutEnabled = false`
- Block: `IsActive = false`, `LockoutEnabled = true`, `LockoutEnd = MaxValue`

---

### 3.9 User Management (Admin)

| FR Code     | Feature             |
|-------------|---------------------|
| FR-USR-001  | Danh sách users (paged) |
| FR-USR-002  | Chi tiết user       |
| FR-USR-003  | Tạo user + gán role |
| FR-USR-004  | Cập nhật thông tin  |
| FR-USR-005  | Lock / Unlock tài khoản |
| FR-USR-006  | Reset password      |
| FR-USR-007  | Xóa user            |

---

### 3.10 Role Management (Admin)

| FR Code     | Feature             |
|-------------|---------------------|
| FR-ROLE-001 | Danh sách roles     |
| FR-ROLE-002 | Tạo role            |
| FR-ROLE-003 | Xóa role (guard: còn users) |
| FR-ROLE-004 | Assign role cho user |
| FR-ROLE-005 | Remove role khỏi user |

---

## 4. Non-Functional Requirements

### 4.1 Performance
| Metric                | Target                        |
|-----------------------|-------------------------------|
| API response time     | < 500ms (p95)                 |
| List endpoint         | Phân trang bắt buộc          |
| DB queries            | AsNoTracking cho read-only    |
| DB connection retry   | EnableRetryOnFailure(3)       |
| Command timeout       | 30 giây                       |

### 4.2 Security
| Requirement                                   | Implementation               |
|-----------------------------------------------|------------------------------|
| Authentication                                | JWT Bearer (API)             |
| Cookie security                               | HttpOnly, Secure, SameSite   |
| Authorization                                 | Policy-based (RequireAdmin)  |
| Account lockout                               | Sau 5 lần sai password       |
| Password complexity                           | ≥6 ký tự, có ít nhất 1 số   |
| SQL Injection                                 | EF Core parameterized queries|
| Token expiry                                  | Configurable, ClockSkew = 0  |

### 4.3 Reliability
- Transaction bắt buộc cho Create Order và Update Order Status
- Rollback khi có lỗi giữa chừng
- Soft Delete thay vì xóa vật lý để tránh mất dữ liệu

### 4.4 Maintainability
- Clean Architecture — tách biệt Domain / Application / Infrastructure / API
- Repository Pattern + Unit of Work
- Result<T> pattern — không throw exception cho business errors

### 4.5 API Standards
- RESTful — đúng HTTP verb và status code
- Version prefix: `/api/v1/`
- Response format: `{ success, message, data }` hoặc `PagedResult<T>`
- CORS: `http://localhost:4200` (dev), cấu hình qua appsettings

---

## 5. External Interface Requirements

### 5.1 Database Interface
- SQL Server 2019+
- EF Core Code-First Migrations
- Connection string qua appsettings.json → `GetConnectionString("EcommerceConnection")`

### 5.2 Email Interface
- SMTP provider: Mailtrap (test) / SMTP thật (production)
- Config: Host, Port, UserName, Password, From, DisplayName
- Triggers: Welcome email (register), Reset password link

### 5.3 Frontend Interface
- Angular 17+ SPA
- Giao tiếp qua REST JSON
- Auth: Bearer token trong Authorization header
- CORS preflight support

---

## 6. Data Requirements

### 6.1 Entities

| Entity         | PK   | Soft Delete | Key Constraints                       |
|----------------|------|-------------|---------------------------------------|
| AppUser        | string (Identity) | Lock | Email unique              |
| Category       | int  | IsDeleted   | Slug unique; ParentId self-ref        |
| Product        | int  | IsDeleted   | Slug unique; CategoryId FK            |
| ProductVariant | int  | IsDeleted   | SKU unique; ProductId FK              |
| ProductImage   | int  | —           | ProductId FK                          |
| Inventory      | int  | —           | ProductId + ProductVariantId unique   |
| Order          | int  | —           | OrderCode unique; UserId FK           |
| OrderItem      | int  | —           | OrderId + ProductId + VariantId       |

### 6.2 Inventory Rules
```
AvailableQuantity = Quantity - ReservedQuantity  (computed, always >= 0)
IsLowStock        = AvailableQuantity <= LowStockThreshold
IsOutOfStock      = AvailableQuantity <= 0
```

### 6.3 Order Rules
```
SubTotal = SUM(UnitPrice * Quantity)
Total    = SubTotal + ShippingFee - DiscountAmount
TotalPrice (OrderItem) = UnitPrice * Quantity  (computed)
```