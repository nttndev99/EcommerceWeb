# 📄 SOFTWARE REQUIREMENTS SPECIFICATION (SRS)

## 🛒 Project: Ecommerce System

---

# 1. 📌 Introduction

## 1.1 Purpose

Tài liệu này mô tả chi tiết các yêu cầu chức năng và phi chức năng cho hệ thống **Ecommerce**, phục vụ:

* Developer
* Tester
* Stakeholder

---

## 1.2 Scope

Hệ thống thương mại điện tử bao gồm:

* Admin (MVC)
* REST API
* Backend theo Clean Architecture

---

## 1.3 Definitions

| Term      | Meaning                         |
| --------- | ------------------------------- |
| SKU       | Mã định danh sản phẩm           |
| Variant   | Biến thể sản phẩm (size, color) |
| Inventory | Tồn kho                         |
| DTO       | Data Transfer Object            |

---

# 2. 🧭 Overall Description

## 2.1 Product Perspective

* Hệ thống độc lập
* Có thể mở rộng thành microservices

---

## 2.2 User Classes

### Admin

* Quản lý hệ thống

### Customer

* Mua hàng, theo dõi đơn

---

## 2.3 Operating Environment

* Backend: .NET (ASP.NET Core)
* Database: SQL Server
* Frontend: Razor MVC
* API: RESTful

---

# 3. 🧩 System Features

---

## 3.1 Category Management

### Description

Quản lý danh mục sản phẩm

### Functional Requirements

| ID    | Requirement             |
| ----- | ----------------------- |
| FR-01 | Tạo category            |
| FR-02 | Cập nhật category       |
| FR-03 | Xóa category            |
| FR-04 | Xem danh sách category  |
| FR-05 | Hỗ trợ category cha-con |

---

## 3.2 Product Management

### Description

Quản lý sản phẩm

| ID    | Requirement       |
| ----- | ----------------- |
| FR-06 | Tạo sản phẩm      |
| FR-07 | Cập nhật sản phẩm |
| FR-08 | Xóa sản phẩm      |
| FR-09 | Gán category      |
| FR-10 | Quản lý variant   |

---

## 3.3 Product Variant

| ID    | Requirement       |
| ----- | ----------------- |
| FR-11 | Tạo variant       |
| FR-12 | SKU phải unique   |
| FR-13 | Quản lý giá riêng |

---

## 3.4 Inventory Management

| ID    | Requirement            |
| ----- | ---------------------- |
| FR-14 | Theo dõi tồn kho       |
| FR-15 | Không cho phép tồn âm  |
| FR-16 | Cập nhật tồn khi order |

---

## 3.5 Order Management

| ID    | Requirement         |
| ----- | ------------------- |
| FR-17 | Tạo đơn hàng        |
| FR-18 | Xem đơn hàng        |
| FR-19 | Cập nhật trạng thái |
| FR-20 | Tracking đơn        |

---

## 3.6 Authentication & Authorization

| ID    | Requirement     |
| ----- | --------------- |
| FR-21 | Đăng ký         |
| FR-22 | Đăng nhập       |
| FR-23 | Phân quyền role |

---

# 4. 🔄 External Interface Requirements

---

## 4.1 User Interface

* Admin UI: Razor Pages
* Responsive design

---

## 4.2 API Interface

### Product API

| Method | Endpoint      |
| ------ | ------------- |
| GET    | /api/products |
| POST   | /api/products |

---

## 4.3 Database Interface

* SQL Server
* EF Core ORM

---

# 5. ⚙️ Non-Functional Requirements

---

## 5.1 Performance

* Response API < 200ms
* Load page < 2s

---

## 5.2 Scalability

* Hỗ trợ scale horizontal
* Tách service sau này

---

## 5.3 Security

* JWT Authentication
* Role-based authorization
* Hash password

---

## 5.4 Reliability

* Không mất dữ liệu khi lỗi
* Retry logic

---

## 5.5 Maintainability

* Clean Architecture
* SOLID principles

---

# 6. 🧠 Business Rules

---

## Product

* Phải thuộc category
* Có ít nhất 1 variant

---

## Inventory

```
Available = Quantity - ReservedQuantity
```

---

## Order Status Flow

```
Pending → Confirmed → Shipping → Delivered → Cancelled
```

---

# 7. 📊 Data Requirements

* Lưu trữ thông tin sản phẩm
* Lưu lịch sử đơn hàng
* Tracking trạng thái

---

# 8. 🚨 Constraints

* Sử dụng .NET
* Sử dụng SQL Server
* Áp dụng Clean Architecture

---

# 9. 🔮 Future Enhancements

* Payment Gateway (VNPay, Stripe)
* AI Recommendation
* Realtime tracking (SignalR)

---

# 10. 🧪 Acceptance Criteria

* CRUD hoạt động đúng
* Không lỗi khi concurrent order
* Inventory chính xác

---

# 11. 📅 Development Plan

| Phase   | Description        |
| ------- | ------------------ |
| Phase 1 | Product + Category |
| Phase 2 | Order + Inventory  |
| Phase 3 | UI + Optimization  |

---

# 12. ✅ Conclusion

Tài liệu SRS này:

* Định nghĩa rõ yêu cầu hệ thống
* Hỗ trợ dev & test
* Là nền tảng để phát triển scalable ecommerce system

---
