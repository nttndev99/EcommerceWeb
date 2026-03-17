# 📄 FUNCTIONAL SPECIFICATION DOCUMENT (FSD)

## 🛒 Project: Ecommerce System

---

# 1. 📌 Overview

## 1.1 Purpose

Tài liệu FSD mô tả chi tiết cách hệ thống hoạt động ở mức **function + logic + flow**, giúp:

* Dev implement đúng
* Tester test chính xác
* BA/PM hiểu rõ luồng hệ thống

---

## 1.2 Scope

Bao gồm:

* Admin (MVC)
* API (REST)
* Business logic (Application Layer)

---

# 2. 🧩 Functional Modules

---

# 2.1 Category Module

## 2.1.1 Create Category

### Flow

```
Admin → Enter Category Info → Submit → Validate → Save DB → Return Result
```

### Input

* Name (required)
* ParentId (optional)

### Validation

* Name không được trống
* Slug phải unique

### Output

* Success / Error message

---

## 2.1.2 Get Category Tree

### Logic

* Load tất cả categories
* Build tree theo ParentId

---

# 2.2 Product Module

---

## 2.2.1 Create Product

### Flow

```
Admin → Create Product → Add Variants → Save → DB
```

### Input

* Name
* CategoryId
* Description
* Variants[]

### Validation

* Product phải có ít nhất 1 variant
* Category phải tồn tại

---

## 2.2.2 Get Product List

### Logic

* Query Products
* Include Category
* Include Variants
* Paging + Filtering

---

## 2.2.3 Update Product

### Logic

* Update basic info
* Sync variants (Add / Update / Delete)

---

# 2.3 Product Variant Module

---

## 2.3.1 Create Variant

### Input

* SKU
* Price
* Size
* Color

### Validation

* SKU phải unique
* Price > 0

---

# 2.4 Inventory Module

---

## 2.4.1 Update Inventory

### Flow

```
Admin → Update Quantity → Save → DB
```

---

## 2.4.2 Reserve Inventory (Quan trọng)

### Flow

```
Order Created → Check Stock → Reserve Quantity → Save
```

### Logic

```
if (Quantity - ReservedQuantity < OrderQuantity)
    → Reject Order
else
    → ReservedQuantity += OrderQuantity
```

---

# 2.5 Order Module

---

## 2.5.1 Create Order

### Flow

```
Customer → Checkout → Create Order → Validate → Save
```

### Steps

1. Validate user
2. Validate product + variant
3. Check inventory
4. Reserve inventory
5. Create Order + OrderItems

---

## 2.5.2 Update Order Status

### Allowed Transitions

```
Pending → Confirmed
Confirmed → Shipping
Shipping → Delivered
Pending → Cancelled
```

---

## 2.5.3 Cancel Order

### Logic

```
Release ReservedQuantity
Update Status = Cancelled
```

---

# 2.6 Order Tracking Module

---

## 2.6.1 Add Tracking

### Flow

```
Update Status → Insert OrderTracking
```

---

# 2.7 Authentication Module

---

## 2.7.1 Register

### Flow

```
User → Register → Validate → Create User → Return Token
```

---

## 2.7.2 Login

### Flow

```
User → Login → Validate → Generate JWT → Return Token
```

---

# 3. 🔄 API Specifications

---

## 3.1 Product API

### Create Product

```
POST /api/products
```

### Request

```json
{
  "name": "T-Shirt",
  "categoryId": 1,
  "variants": [
    {
      "sku": "TS-001",
      "price": 100
    }
  ]
}
```

---

## 3.2 Order API

### Create Order

```
POST /api/orders
```

---

# 4. 🧠 Business Logic Details

---

## 4.1 Inventory Calculation

```
Available = Quantity - ReservedQuantity
```

---

## 4.2 Pricing

```
OrderItem.TotalPrice = Quantity * UnitPrice
Order.TotalAmount = Sum(OrderItems)
```

---

# 5. ⚠️ Error Handling

| Case             | Behavior     |
| ---------------- | ------------ |
| SKU duplicate    | Reject       |
| Out of stock     | Reject order |
| Invalid category | Reject       |

---

# 6. 🔐 Security Flow

* JWT authentication
* Role-based authorization
* Admin only access Admin APIs

---

# 7. 🧪 Validation Rules

* Required fields check
* Data type validation
* Business rule validation

---

# 8. 🚀 Performance Optimization

* Use projection (Select DTO)
* Avoid N+1 query
* Use pagination

---

# 9. 🧬 Mapping (Clean Architecture)

| Layer      | Responsibility  |
| ---------- | --------------- |
| Controller | Receive request |
| Service    | Business logic  |
| Repository | Data access     |

---

# 10. 📊 Sequence Example (Order)

```
User → API → OrderService
    → Validate
    → InventoryService
    → Reserve
    → Save Order
    → Return Response
```

---

# 11. ✅ Acceptance Criteria

* Order không vượt tồn kho
* SKU unique
* CRUD hoạt động chính xác
* API trả đúng format

---

# 12. 📌 Notes

* Luôn validate ở Service layer
* Không xử lý business logic trong Controller
* Sử dụng DTO để tách layer

---

# ✅ Conclusion

FSD này mô tả:

* Chi tiết flow hệ thống
* Business logic cụ thể
* API & validation


---
