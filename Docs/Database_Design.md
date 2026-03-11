# Database Design

## Main Tables

Categories
Products
ProductVariants
VariantImage
Inventories
OrderItems
Orders
OrderTracking
Customers

## Relationships

- Categories 1 - N Products
- Product 1 - N ProductVariants
- ProductVariants 1 - 1 Inventories
- ProductVariants 1 - N VariantImage
- ProductVariants 1 - N OrderItems
- Orders 1 - N OrderItems
- Orders 1 - N OrderTracking
- Customers 1 - N Orders
