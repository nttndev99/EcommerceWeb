# HOME(Category, Index, ProductDetail, _ProductCard, _header) (Read - Add to cart)
- Home/Index (BestSeller, NewArrivals, Hotsale) => JS+ PartialView

# Cart - Checkout (Guest-Login) - 
- TagHelpers, CurrencyHelper

- ViewComponents/CartBadgeViewComponent.cs
ViewComponents/PaginationViewComponent
ViewComponents/ProductPriceViewComponent

- Views/Shared/Components/CartBadge/Default.cshtml
Views/Shared/Components/Pagination/Default.cshtml
Views/Shared/Components/ProductPrice/Default.cshtml

- CartService, EmailService(SendOrderConfirmationAsync)
- CartController, CheckoutController, OrderController

