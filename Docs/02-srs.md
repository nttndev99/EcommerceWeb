# Software Requirement Specification

## Product Perspective
The system is a standalone e-commerce application built with a service-oriented architecture. It integrates with third-party payment gateways and uses caching to ensure inventory consistency.

## Product Functions
- Product catalog management
- Shopping cart and checkout
- Online payment processing
- Order lifecycle management
- Real-time inventory management
- Reporting and notifications

## User Classes and Characteristics
- Customer: Purchases products and tracks orders
- Admin: Manages products, inventory, and orders
- System: Handles automation and background tasks

## Operating Environment
- Web browsers (Chrome, Edge, Firefox)
- Backend: ASP.NET Core 
- Database: MySQL
- Cache: Redis

## System Features and Requirements
### User Authentication
- Description: Users must authenticate to access protected features.
- Functional Requirements:
FR-1: System shall allow user registration
FR-2: System shall allow user login/logout
FR-3: System shall support role-based access control
### Product Management
- Description: Admin manages product catalog.
- Functional Requirements:
FR-4: Admin shall create, update, and delete products
FR-5: System shall support Stock Keeping Unit products
FR-6: System shall display product availability
### Inventory Management (Core)
- Description: System maintains accurate inventory in real time.
- Functional Requirements:
FR-7: System shall track inventory quantity per Stock Keeping Unit
FR-8: System shall reserve inventory during checkout
FR-9: System shall deduct inventory after successful payment
FR-10: System shall restore inventory if payment fails
FR-11: System shall trigger low-stock alerts
### Shopping Cart & Checkout
- Description: Customers add products to cart and complete checkout.
- Functional Requirements:
FR-12: System shall validate stock before checkout
FR-13: System shall prevent checkout if stock is insufficient
FR-14: System shall lock inventory during payment processing
### Order Management
- Description: System manages order lifecycle.
- Functional Requirements:
FR-15: System shall create an order with status "Pending Payment"
FR-16: System shall update order status after payment
FR-17: Admin shall cancel orders
FR-18: System shall rollback inventory on cancellation
### Payment Processing
- Description: System integrates with external payment gateways.
- Functional Requirements:
FR-19: System shall redirect users to payment gateway
FR-20: System shall handle payment success/failure callbacks
FR-21: System shall prevent duplicate payments
### Reporting & Notifications
- Description: System provides insights and alerts.
- Functional Requirements:
FR-22: System shall generate sales reports
FR-23: System shall display inventory reports
FR-24: System shall notify admin of low stock

## External Interface Requirements
- User Interfaces:
Responsive web UI
Admin dashboard
- Software Interfaces:
Payment gateways (Stripe, VNPay)
Email service (SMTP / SendGrid)
- Communication Interfaces:
HTTPS (REST APIs)

## Non-Functional Requirements
- Performance:
NFR-1: Inventory updates shall complete within 1 second
NFR-2: System shall support 100+ concurrent checkouts
- Security:
NFR-3: Passwords shall be encrypted
NFR-4: APIs shall require authentication
- Reliability:
NFR-5: Inventory data consistency ≥ 99.5%
- Scalability:
NFR-6: System shall support horizontal scaling
- Availability:
NFR-7: System uptime ≥ 99.9%

## Use Case Summary
- Place Order: Customer
- Process Payment: System
- Manage Inventory: Admin
- Cancel Order: Admin

## Future Enhancements
- Multi-warehouse support
- Supplier management
- AI-based demand forecasting
- Mobile applications

## Appendix
Order Status Flow:
Created → Pending Payment → Paid → Processing → Shipped → Completed / Cancelled