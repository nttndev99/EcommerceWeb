# Functional Specification Document (FSD)
## Ecommerce Web Platform

| Field   | Detail        |
|---------|---------------|
| Version |               |
| Date    |               |
| Status  |               |

---

## 1. System Modules Overview

```
┌─────────────────────────────────────────────────────────┐
│                    Ecommerce Platform                    │
├──────────────┬──────────────┬──────────────┬────────────┤
│     Auth     │   Catalog    │    Orders    │   Admin    │
│  Register    │  Category    │  Create      │  Users     │
│  Login       │  Product     │  Track       │  Roles     │
│  ForgotPwd   │  Variant     │  Cancel      │  Customers │
│  ResetPwd    │  Image       │  Statistics  │  Dashboard │
│              │  Inventory   │              │            │
└──────────────┴──────────────┴──────────────┴────────────┘
```

---

## 2. Detailed Function Specifications

---

### MODULE: AUTH

---

#### FUNC-AUTH-001: Register

**Trigger:** POST `/api/v1/auth/register`

**Input:**
```json
{
  "fullName": "Nguyen Van A",
  "email": "user@example.com",
  "password": "Pass123"
}
```

**Validation:**
- FullName: required, max 200 chars
- Email: required, valid format, unique
- Password: required, min 6 chars, phải có ít nhất 1 số

**Flow:**
```
1. FindByEmailAsync(email)
   → tồn tại → Return 400 "Email already exists"
2. new AppUser { FullName, Email, UserName=Email, IsActive=true, CreatedAt=UtcNow }
3. UserManager.CreateAsync(user, password)
   → failed → Return 400 errors[0].Description
4. AddToRoleAsync(user, "Customer")
5. EmailService.SendWelcomeAsync(email, fullName)
6. Return 201
```

**Output (201):**
```json
{ "success": true, "message": "Registration successful." }
```

---

#### FUNC-AUTH-002: Login

**Trigger:** POST `/api/v1/auth/login`

**Input:**
```json
{
  "email": "user@example.com",
  "password": "Pass123",
  "rememberMe": false
}
```

**Flow:**
```
1. FindByEmailAsync(email)
   → null → Return 401
2. user.IsActive == false → Return 401
3. PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true)
   → IsLockedOut → Return 423 "Account is locked"
   → !Succeeded → Return 401
4. Sinh JWT token (HS256, claims: userId, email, roles)
5. Return 200 + token
```

**Output (200):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGci...",
    "userId": "abc123",
    "email": "user@example.com",
    "fullName": "Nguyen Van A",
    "roles": ["Customer"]
  }
}
```

---

#### FUNC-AUTH-003: Forgot Password

**Trigger:** POST `/api/v1/auth/forgot-password`

**Input:** `{ "email": "user@example.com" }`

**Flow:**
```
1. FindByEmailAsync(email)
   → null → Return 200 (không lộ thông tin)
2. GeneratePasswordResetTokenAsync(user)
3. resetLink = baseUrl + "/auth/reset-password?token=...&email=..."
4. EmailService.SendResetPasswordAsync(email, resetLink)
5. Return 200
```

**Note:** Token hết hạn sau 1 giờ (cấu hình trong Identity options).

---

#### FUNC-AUTH-004: Reset Password

**Trigger:** POST `/api/v1/auth/reset-password`

**Input:**
```json
{
  "email": "user@example.com",
  "token": "CfDJ8...",
  "newPassword": "NewPass456",
  "confirmPassword": "NewPass456"
}
```

**Flow:**
```
1. newPassword != confirmPassword → Return 400
2. FindByEmailAsync(email) → null → Return 400
3. ResetPasswordAsync(user, token, newPassword)
   → !Succeeded → Return 400 errors[0].Description
4. Return 200
```

---

### MODULE: CATEGORY

---

#### FUNC-CAT-001: Get Paged

**Trigger:** GET `/api/v1/categories?pageNumber=1&pageSize=10&search=&parentId=&isActive=`

**Query:**
```sql
SELECT c.Id, c.Name, c.Slug, c.IsActive, c.DisplayOrder,
       p.Name AS ParentName
FROM Categories c
LEFT JOIN Categories p ON c.ParentId = p.Id
WHERE c.IsDeleted = 0
  AND (@search IS NULL OR c.Name LIKE '%'+@search+'%')
  AND (@parentId IS NULL OR c.ParentId = @parentId)
  AND (@isActive IS NULL OR c.IsActive = @isActive)
ORDER BY c.DisplayOrder ASC
OFFSET (@page-1)*@size ROWS FETCH NEXT @size ROWS ONLY
```

**Output:**
```json
{
  "items": [...],
  "totalCount": 25,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 3
}
```

---

#### FUNC-CAT-002: Create Category

**Trigger:** POST `/api/v1/categories` `[RequireAdmin]`

**Input:**
```json
{
  "name": "Thời trang nam",
  "slug": "",
  "description": "...",
  "imageUrl": "https://...",
  "parentId": null,
  "displayOrder": 1
}
```

**Flow:**
```
1. slug = IsNullOrEmpty(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug
2. SlugExistsAsync(slug) → true → Return 400 "Slug exists"
3. parentId != null → ExistsAsync(parentId && !IsDeleted) → false → Return 400
4. Insert Category
5. Return 201 + CategoryDto
```

**Slug generation rules:**
- Lowercase, remove diacritics
- Replace spaces with `-`
- Remove special characters
- Example: "Thời trang nam" → "thoi-trang-nam"

---

#### FUNC-CAT-003: Soft Delete

**Trigger:** DELETE `/api/v1/categories/{id}` `[RequireAdmin]`

**Flow:**
```
1. GetByIdAsync(id) → null → Return 404
2. category.IsDeleted == true → Return 404
3. Cascade: tất cả Children → IsDeleted = true, UpdatedAt = UtcNow
4. category.IsDeleted = true, UpdatedAt = UtcNow
5. SaveChangesAsync
6. Return 200
```

---

### MODULE: PRODUCT

---

#### FUNC-PRD-001: Get Paged

**Trigger:** GET `/api/v1/products?pageNumber=1&pageSize=10&search=&categoryId=&status=&isFeatured=&brand=&minPrice=&maxPrice=&inStock=&sortBy=createdAt&sortDirection=desc`

**Filter Logic:**
```
search      → p.Name LIKE || p.SKU LIKE || p.Brand LIKE
categoryId  → p.CategoryId =
status      → p.Status =
isFeatured  → p.IsFeatured =
brand       → p.Brand LIKE
minPrice    → p.BasePrice >=
maxPrice    → p.BasePrice <=
inStock=true  → EXISTS inventory with AvailableQty > 0
inStock=false → NOT EXISTS inventory with AvailableQty > 0
```

**Computed fields:**
```
TotalStock      = SUM(Inventory.Quantity - Inventory.ReservedQuantity) WHERE ProductId = p.Id
PrimaryImageUrl = first Image WHERE IsPrimary = true
CategoryName    = Category.Name WHERE Id = p.CategoryId
VariantCount    = COUNT(Variants) WHERE !IsDeleted
```

---

#### FUNC-PRD-002: Create Product

**Trigger:** POST `/api/v1/products` `[RequireAdmin]`

**Input:**
```json
{
  "name": "Áo thun basic",
  "slug": "",
  "description": "...",
  "shortDescription": "...",
  "basePrice": 199000,
  "salePrice": 149000,
  "sku": "AT-BASIC-001",
  "status": "Active",
  "isFeatured": false,
  "categoryId": 5,
  "brand": "LocalBrand",
  "weight": 0.3,
  "tags": "ao,thun,basic"
}
```

**Flow:**
```
1. slug = auto-gen nếu trống
2. SlugExistsAsync → conflict → Return 400
3. CategoryExistsAsync(!IsDeleted) → false → Return 400
4. Insert Product
5. Insert Inventory { ProductId, ProductVariantId=null, Quantity=0 }
6. SaveChangesAsync
7. Return 201 + ProductDto
```

---

#### FUNC-PRD-003: Get Home Products

**Trigger:** GET `/api/v1/products/home?tab=new|sale|featured`

**Filter:**
- Chỉ lấy: `Status = Active AND IsDeleted = false`
- Chỉ lấy sản phẩm còn hàng: `AvailableStock > 0`
- Limit: 8 sản phẩm

**Tab logic:**
```
"new"      → OrderByDescending(CreatedAt)
"sale"     → WHERE SalePrice IS NOT NULL → OrderByDescending(CreatedAt)
"featured" → WHERE IsFeatured = true → OrderByDescending(CreatedAt)
```

**Extra field:** `IsNew = CreatedAt > UtcNow.AddDays(-7)`

---

### MODULE: PRODUCT VARIANT

---

#### FUNC-VAR-001: Create Variant

**Trigger:** POST `/api/v1/products/{id}/variants` `[RequireAdmin]`

**Input:**
```json
{
  "productId": 12,
  "name": "Đỏ / XL",
  "sku": "AT-BASIC-RED-XL",
  "price": 199000,
  "salePrice": 149000,
  "color": "Đỏ",
  "colorHex": "#FF0000",
  "size": "XL",
  "material": "Cotton",
  "imageUrl": "https://...",
  "isActive": true,
  "displayOrder": 1,
  "initialStock": 50
}
```

**Flow:**
```
1. SKUExistsAsync → conflict → Return 400
2. Insert ProductVariant
3. SaveChangesAsync
4. Insert Inventory { ProductId, ProductVariantId=variant.Id, Quantity=initialStock }
5. SaveChangesAsync
6. Return 201 + VariantDto
```

---

#### FUNC-VAR-002: Hard Delete Variant

**Trigger:** DELETE `/api/v1/products/{id}/variants/{variantId}/hard` `[RequireAdmin]`

**Flow:**
```
1. IgnoreQueryFilters → FindAsync(id)
2. Load Inventories WHERE ProductVariantId = id
3. RemoveRange(inventories)
4. Remove(variant)
5. SaveChangesAsync
```

---

### MODULE: INVENTORY

---

#### FUNC-INV-001: Adjust Stock

**Trigger:** POST `/api/v1/inventories/{id}/adjust` `[RequireAdmin]`

**Input:**
```json
{
  "id": 3,
  "delta": -5,
  "reason": "Hàng bị lỗi khi kiểm kê"
}
```

**Flow:**
```
1. GetByIdAsync(id) → null → Return 404
2. newQty = inventory.Quantity + delta
3. newQty < 0 → Return 400 "Insufficient stock"
4. newQty < inventory.ReservedQuantity → Return 400 "Cannot go below reserved"
5. inventory.Quantity = newQty
6. inventory.LastStockUpdate = UtcNow
7. SaveChangesAsync
```

---

### MODULE: ORDER

---

#### FUNC-ORD-001: Create Order (Full Flow)

**Trigger:** POST `/api/v1/orders` `[Authorize]`

**Input:**
```json
{
  "paymentMethod": "COD",
  "recipientName": "Nguyen Van A",
  "recipientPhone": "0901234567",
  "addressLine": "123 Nguyen Hue",
  "ward": "Ben Nghe",
  "district": "Quan 1",
  "province": "Ho Chi Minh",
  "shippingFee": 30000,
  "couponCode": null,
  "note": "Giao buổi sáng",
  "items": [
    { "productId": 12, "productVariantId": 5, "quantity": 2 },
    { "productId": 8,  "productVariantId": null, "quantity": 1 }
  ]
}
```

**Full Flow (Transaction):**
```
BEGIN TRANSACTION

FOR EACH item IN dto.Items:
  1. quantity <= 0 → Rollback → Return 400

  2. product = Products.Query()
               .Include(Images)
               .Where(Id = item.ProductId && Status = Active)
     → null → Rollback → Return 400 "Product not found"

  3. unitPrice = product.SalePrice ?? product.BasePrice
     sku       = product.SKU
     imageUrl  = product.Images.OrderBy(DisplayOrder).First(IsPrimary)?.ImageUrl

  4. IF item.ProductVariantId != null:
       variant = ProductVariants.Query()
                 .Where(Id = variantId && ProductId = item.ProductId && IsActive)
       → null → Rollback → Return 400 "Variant not found"
       unitPrice   = variant.SalePrice ?? variant.Price
       variantName = variant.Name
       sku         = variant.SKU ?? sku
       imageUrl    = variant.ImageUrl ?? imageUrl

  5. inventory = Inventories.Query()
                 .Where(ProductId = item.ProductId
                        && ProductVariantId = item.ProductVariantId)
     → null → Rollback → Return 400 "No inventory record"

  6. inventory.AvailableQuantity < item.Quantity
     → Rollback → Return 400 "Insufficient stock: available={n}"

  7. inventory.ReservedQuantity += item.Quantity
     inventory.LastStockUpdate   = UtcNow

  8. orderItems.Add(new OrderItem { snapshot fields... })
     subTotal += unitPrice * quantity

orderCode = GenerateOrderCodeAsync()  -- format: ORD-YYYYMMDD-NNNN
order = new Order { all fields, Items = orderItems }

Orders.AddAsync(order)
SaveChangesAsync()   -- 1 lần duy nhất, save cả order + inventory

COMMIT

Return 201 + OrderDto
```

**Order Code Format:**
```
ORD-20260330-0001
     │         └── sequence number (reset daily or global auto-increment)
     └── date YYYYMMDD
```

---

#### FUNC-ORD-002: Update Order Status

**Trigger:** PATCH `/api/v1/orders/{id}/status` `[RequireAdmin]`

**Input:**
```json
{
  "orderStatus": "Shipped",
  "cancelReason": null
}
```

**State Machine Table:**

| Current       | Allowed Next                      |
|---------------|-----------------------------------|
| Pending       | Processing, Cancelled             |
| Processing    | Shipped, Cancelled                |
| Shipped       | Delivered, Cancelled              |
| Delivered     | — (terminal)                      |
| Cancelled     | — (terminal)                      |

**Flow (Transaction):**
```
BEGIN TRANSACTION

1. GetByIdAsync(id) → null → Return 404
2. allowed = StateTransitionTable[order.OrderStatus]
3. dto.OrderStatus NOT IN allowed → Return 400 "Invalid transition"

4. IF → Cancelled:
   FOR EACH item:
     inv.ReservedQuantity = MAX(0, inv.ReservedQuantity - item.Quantity)
     inv.LastStockUpdate  = UtcNow
   order.CancelReason = dto.CancelReason
   order.CancelledAt  = UtcNow

5. IF → Delivered:
   FOR EACH item:
     inv.ReservedQuantity = MAX(0, inv.ReservedQuantity - item.Quantity)
     inv.Quantity         = MAX(0, inv.Quantity         - item.Quantity)
     inv.LastStockUpdate  = UtcNow
   order.DeliveredAt = UtcNow

6. IF → Shipped:   order.ShippedAt = UtcNow

7. order.OrderStatus = dto.OrderStatus
8. UpdateAsync(order)
9. SaveChangesAsync()

COMMIT

Return 200 + OrderDto
```

---

#### FUNC-ORD-003: Cancel Order (Customer)

**Trigger:** POST `/api/v1/orders/{id}/cancel` `[Authorize]`

**Input:** `"Tôi muốn đổi địa chỉ giao hàng"` (plain string)

**Guards:**
```
1. GetByIdAsync(id) → null → Return 404
2. !IsAdmin && order.UserId != CurrentUserId → Return 403
3. !IsAdmin && order.OrderStatus NOT IN {Pending, Processing} → Return 400
```

**Delegate:** Gọi `UpdateOrderStatusAsync({ Id=id, OrderStatus=Cancelled, CancelReason=reason })`

---

### MODULE: CUSTOMER (Admin)

---

#### FUNC-CUS-001: Activate / Block

**Trigger:** PATCH `/api/v1/customers/{id}/status` `[RequireAdmin]`

**Input:**
```json
{ "isActive": true }
```

**Flow:**
```
1. FindByIdAsync(id) → null → Return 404
2. user.IsActive = isActive
3. UpdateAsync(user)
4. IF isActive = true:
     SetLockoutEnabledAsync(user, false)
     SetLockoutEndDateAsync(user, null)
   IF isActive = false:
     SetLockoutEnabledAsync(user, true)
     SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue)
5. Return 200
```

---

### MODULE: USER MANAGER (Admin)

---

#### FUNC-USR-001: Create User (Admin)

**Trigger:** POST `/api/v1/users` `[RequireAdmin]`

**Input:**
```json
{
  "fullName": "Staff A",
  "email": "staff@example.com",
  "password": "Staff@123",
  "role": "Admin"
}
```

**Flow:**
```
1. FindByEmailAsync → exists → Return 400
2. RoleExistsAsync(role) → false → Return 400
3. CreateAsync(user, password)
4. AddToRoleAsync(user, role)
5. Return 201
```

---

## 3. API Response Standards

### Success Response
```json
{
  "success": true,
  "message": "Product created.",
  "data": { ... }
}
```

### Paged Response
```json
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Error Response
```json
{
  "success": false,
  "message": "Product not found.",
  "errors": []
}
```

### HTTP Status Codes

| Code | Meaning                               |
|------|---------------------------------------|
| 200  | OK — update/delete thành công         |
| 201  | Created — tạo mới thành công          |
| 400  | Bad Request — validation / business error |
| 401  | Unauthorized — chưa đăng nhập         |
| 403  | Forbidden — không đủ quyền            |
| 404  | Not Found                             |
| 423  | Locked — tài khoản bị khóa            |
| 500  | Internal Server Error                 |

---

## 4. Business Rules Summary

| Rule | Description |
|------|-------------|
| BR-001 | Slug phải unique toàn hệ thống (Category + Product) |
| BR-002 | SKU phải unique toàn hệ thống (Product + Variant) |
| BR-003 | Không hard delete Category nếu còn Products |
| BR-004 | Không hard delete Product nếu còn Variants |
| BR-005 | Hard delete Variant phải kéo theo xóa Inventory |
| BR-006 | Tạo Product tự động tạo Inventory record (Qty=0) |
| BR-007 | Tạo Variant tự động tạo Inventory record (Qty=initialStock) |
| BR-008 | AvailableQuantity = Quantity - ReservedQuantity, không âm |
| BR-009 | Create Order và Update Status phải trong Transaction |
| BR-010 | Customer chỉ cancel đơn Pending/Processing |
| BR-011 | Customer chỉ xem orders của chính mình |
| BR-012 | Delivered → deduct stock vĩnh viễn |
| BR-013 | Cancelled → chỉ release reserved (không deduct Quantity) |
| BR-014 | Giá snapshot khi đặt hàng — không thay đổi dù product đổi giá |
| BR-015 | Soft delete Category cascade sang Children |
| BR-016 | Soft delete Product cascade sang Variants |

---

## 5. Error Handling

| Scenario                          | HTTP | Message                              |
|-----------------------------------|------|--------------------------------------|
| Entity not found                  | 404  | "{Entity} #{id} not found."         |
| Slug/SKU duplicate                | 400  | "Slug/SKU '{value}' already exists." |
| Insufficient stock                | 400  | "Insufficient stock for '{name}'. Available: {n}." |
| Invalid order transition          | 400  | "Cannot change from '{from}' to '{to}'." |
| Customer cancel Shipped order     | 400  | "You can only cancel Pending or Processing orders." |
| Access own orders only            | 403  | "You can only view your own orders." |
| Account locked                    | 423  | "Account is locked out."             |
| Unauthenticated                   | 401  | "Unauthorized. Please provide a valid token." |
| Unauthorized role                 | 403  | "Forbidden. You do not have permission." |
| Unhandled exception               | 500  | "An unexpected error occurred."      |