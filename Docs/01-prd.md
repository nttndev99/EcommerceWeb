# Product Requirement Document

# 🧩 Ecommerce - ERD (Production Ready)

---

# 1. 📌 Overview

Hệ thống **Ecommerce** được thiết kế theo chuẩn production, đảm bảo:

* Khả năng mở rộng (scalable)
* Dễ bảo trì (maintainable)
* Tối ưu hiệu năng (performance)

---

# 2. 🏗️ Database Structure

## 2.1 Categories

```sql
Categories
----------
Id (PK)
Name (nvarchar 255)
Slug (varchar 255, unique)
ParentId (FK -> Categories.Id, nullable)
Description (nvarchar 500)
IsActive (bit)
IsDeleted (bit)
CreatedAt
UpdatedAt
```

---

## 2.2 Products

```sql
Products
----------
Id (PK)
Name (nvarchar 255)
Slug (varchar 255, unique)
CategoryId (FK)
Description (nvarchar max)
Brand (nvarchar 100)
IsActive (bit)
IsDeleted (bit)
CreatedAt
UpdatedAt
```

---

## 2.3 ProductVariants

```sql
ProductVariants
----------
Id (PK)
ProductId (FK)
SKU (varchar 100, unique)
Price (decimal 18,2)
CompareAtPrice (decimal 18,2, nullable)
Color (nvarchar 50)
Size (nvarchar 50)
Weight (decimal, nullable)
IsActive (bit)
CreatedAt
UpdatedAt
```

---

## 2.4 ProductImages

```sql
ProductImages
----------
Id (PK)
ProductId (FK)
ImageUrl (varchar 500)
IsPrimary (bit)
SortOrder (int)
CreatedAt
```

---

## 2.5 Inventories

```sql
Inventories
----------
Id (PK)
VariantId (FK -> ProductVariants.Id)
Quantity (int)
ReservedQuantity (int)
Location (nvarchar 100)
UpdatedAt
```

### Rule:

```
AvailableQuantity = Quantity - ReservedQuantity
```

---

## 2.6 Customers

```sql
Customers
----------
Id (PK)
UserId (FK -> AspNetUsers.Id)
FullName (nvarchar 255)
Phone (varchar 20)
Address (nvarchar 500)
CreatedAt
```

---

## 2.7 Orders

```sql
Orders
----------
Id (PK)
UserId (FK -> AspNetUsers.Id)
TotalAmount (decimal 18,2)
Status (varchar 50)
PaymentStatus (varchar 50)
ShippingAddress (nvarchar 500)
CreatedAt
UpdatedAt
```

---

## 2.8 OrderItems

```sql
OrderItems
----------
Id (PK)
OrderId (FK)
ProductId (FK)
VariantId (FK)
Quantity (int)
UnitPrice (decimal 18,2)
TotalPrice (decimal 18,2)
```

---

## 2.9 OrderTracking

```sql
OrderTracking
----------
Id (PK)
OrderId (FK)
Status (varchar 50)
Note (nvarchar 500)
CreatedAt
```

---

## 2.10 Identity Tables (ASP.NET Core Identity)

```sql
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetRoleClaims
AspNetUserLogins
AspNetUserTokens
```

---

# 3. 🔗 Relationships

```
Categories 1 --- N Categories (Parent-Child)

Categories 1 --- N Products

Products 1 --- N ProductVariants
Products 1 --- N ProductImages

ProductVariants 1 --- N Inventories
ProductVariants 1 --- N OrderItems

Orders 1 --- N OrderItems
Orders 1 --- N OrderTracking

AspNetUsers 1 --- N Orders
AspNetUsers 1 --- N Customers
```

---

# 4. 🧠 Business Rules

## Product

* Mỗi product phải thuộc 1 category
* Có ít nhất 1 variant

## Inventory

* Không cho phép số lượng âm
* Khi đặt hàng:

```
ReservedQuantity += QuantityOrdered
```

## Order Status Flow

```
Pending → Confirmed → Shipping → Delivered → Cancelled
```

---

# 5. ⚡ Indexing Strategy

```sql
-- Product
CREATE INDEX IX_Product_Slug ON Products(Slug);
CREATE INDEX IX_Product_CategoryId ON Products(CategoryId);

-- Variant
CREATE UNIQUE INDEX IX_Variant_SKU ON ProductVariants(SKU);

-- Order
CREATE INDEX IX_Order_UserId ON Orders(UserId);
CREATE INDEX IX_Order_Status ON Orders(Status);
```

---

# 6. 🧬 BaseEntity (Recommended)

```sql
CreatedAt
UpdatedAt
CreatedBy
UpdatedBy
IsDeleted
DeletedAt
```

---

# 7. 📊 ERD Diagram (Text)

```
Categories
   └── Products
         ├── ProductVariants
         │       ├── Inventories
         │       └── OrderItems
         └── ProductImages

Orders
   ├── OrderItems
   └── OrderTracking

AspNetUsers
   ├── Orders
   └── Customers
```

---

# 8. 🚀 Future Extensions

## Cart System

* Carts
* CartItems

## Discount System

* Coupons
* OrderCoupons

## Review System

* ProductReviews

## Multi Warehouse

* Warehouses
* Inventory per warehouse

---

# 9. 🎯 Notes

* Sử dụng **SKU unique** để quản lý inventory
* Dùng **soft delete** thay vì hard delete
* Tách **variant khỏi product** để scale tốt hơn
* Chuẩn bị sẵn để tách microservices

---

# ✅ Kết luận

ERD này:

* Chuẩn production
* Dễ mở rộng
* Phù hợp Clean Architecture
* Sẵn sàng scale lớn (multi warehouse, microservices)

---



