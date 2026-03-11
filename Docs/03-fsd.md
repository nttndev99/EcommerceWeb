# Functional Specification Document

## Scope
All functional flows, rules, validations, and system behaviors related to:
- Product management
- Inventory management (core focus)
- Cart, checkout, and payment
- Order lifecycle
- Admin operations

## System Overview
### High-Level Flow
- Customer browses products
- Customer adds items to cart
- System validates and reserves inventory
- Customer completes payment
- System confirms order and deducts inventory
- Admin processes and fulfills order
### User Roles
- Customer:	End user purchasing products
- Admin: Manages products, inventory, and orders
- System: Automated background processing


## Functional Specifications
### Authentication & Authorization
Description: Controls access to the system based on user roles.
Functional Behavior:
- Users must log in to access protected features
- Admin and Customer roles are enforced via RBAC
Business Rules: 
- One user can have only one primary role
- Unauthorized access is denied
### Product Management
Description: Admin manages product catalog and Stock Keeping Unit.
Functional Behavior:
- Admin can create, edit, delete products
- Each product has one or more Stock Keeping Unit
- Product availability is derived from inventory
Validations:
- Stock Keeping Unit must be unique
- Price must be greater than 0
### Inventory Management (Core Module)
Description: Ensures real-time and consistent inventory control.
Inventory States:
- Available
- Reserved
- Out of Stock
Functional Behavior:
- Inventory is checked when adding to cart
- Inventory is reserved at checkout
- Inventory is deducted after payment success
- Inventory is released if payment fails or times out
Business Rules:
- One Stock Keeping Unit maps to one inventory record
- Reserved inventory expires after configurable timeout
- Inventory quantity cannot be negative
### Cart Management
Description: Temporary storage for customer-selected items.
Functional Behavior:
- Add item to cart
- Update quantity
- Remove item from cart
Validations
- Quantity must not exceed available inventory
- Out-of-stock items cannot be added
### Checkout & Payment
Description: Handles order creation and payment processing.
Functional Behavior:
- System validates inventory before checkout
- System creates order with status "Pending Payment"
- System locks inventory during payment
- System processes payment callback
Business Rules:
- Order is confirmed only after successful payment
- Duplicate payment attempts are blocked
### Order Management
Order Status Flow:
Created → Pending Payment → Paid → Processing → Shipped → Completed / Cancelled
Functional Behavior:
- System updates order status automatically
- Admin can cancel orders before shipping
- Inventory is restored on cancellation
### Reporting & Notifications
Description: Provides visibility into system operations.
Functional Behavior:
- Daily sales report generation
- Inventory movement logs
- Low-stock notifications to admin
### Error Handling & Edge Cases
- Scenario: System Behavior
- Payment failure: Release reserved inventory
- Concurrent checkout: First successful reservation wins
- Inventory mismatch: Transaction rollback
- Session timeout: Cart is cleared
### Data Flow (Logical)
Product → Inventory
Cart → Checkout
Checkout → Payment Gateway
Payment Callback → Order + Inventory update
### Assumptions & Constraints
- Single warehouse
- Online payment only
- Moderate traffic scale
### Out of Scope
- Multi-vendor marketplace
- Multi-warehouse inventory
- Advanced promotions
### Future Enhancements 
- Multi-warehouse support
- Supplier management
- AI demand forecasting