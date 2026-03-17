using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Ecommerce.Domain.Enums;

namespace Ecommerce.MVC.TagHelpers;

// ═══════════════════════════════════════════════════════════════════════════
// 1. <add-to-cart> TAG HELPER
//
// Usage:
//   <add-to-cart product-id="@p.Id" return-url="@Context.Request.Path" />
//   <add-to-cart product-id="@p.Id" variant-id="@v.Id" label="Buy" size="lg" />
// ═══════════════════════════════════════════════════════════════════════════
[HtmlTargetElement("add-to-cart")]
public class AddToCartTagHelper : TagHelper
{
    [HtmlAttributeName("product-id")]
    public int ProductId { get; set; }

    [HtmlAttributeName("variant-id")]
    public int? VariantId { get; set; }

    [HtmlAttributeName("quantity")]
    public int Quantity { get; set; } = 1;

    [HtmlAttributeName("return-url")]
    public string? ReturnUrl { get; set; }

    [HtmlAttributeName("label")]
    public string Label { get; set; } = "Add to Cart";

    [HtmlAttributeName("size")]
    public string Size { get; set; } = "sm";   // sm | md | lg

    [HtmlAttributeName("variant")]
    public string Variant { get; set; } = "dark"; // Bootstrap color variant

    [HtmlAttributeName("icon")]
    public string Icon { get; set; } = "bi-cart-plus";

    [HtmlAttributeName("block")]
    public bool Block { get; set; } = false;

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "form";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("method", "post");
        output.Attributes.SetAttribute("action", "/Cart/Add");

        var btnClass = $"btn btn-{Variant} btn-{Size}{(Block ? " w-100" : "")}";

        var token = ViewContext.HttpContext.RequestServices
            .GetService(typeof(Microsoft.AspNetCore.Antiforgery.IAntiforgery))
            as Microsoft.AspNetCore.Antiforgery.IAntiforgery;
        var tokenValue = token?.GetAndStoreTokens(ViewContext.HttpContext).RequestToken ?? "";

        var html = $"""
            <input type="hidden" name="__RequestVerificationToken" value="{tokenValue}" />
            <input type="hidden" name="ProductId" value="{ProductId}" />
            <input type="hidden" name="Quantity"  value="{Quantity}" />
            """;

        if (VariantId.HasValue)
            html += $"""<input type="hidden" name="ProductVariantId" value="{VariantId}" />""";

        if (!string.IsNullOrEmpty(ReturnUrl))
            html += $"""<input type="hidden" name="returnUrl" value="{ReturnUrl}" />""";

        html += $"""
            <button type="submit" class="{btnClass}">
                <i class="bi {Icon} me-1"></i>{Label}
            </button>
            """;

        output.Content.SetHtmlContent(html);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 2. <price-display> TAG HELPER
//
// Usage:
//   <price-display base-price="@p.BasePrice" sale-price="@p.SalePrice" />
//   <price-display base-price="999000" sale-price="799000" size="lg" />
// ═══════════════════════════════════════════════════════════════════════════
[HtmlTargetElement("price-display")]
public class PriceDisplayTagHelper : TagHelper
{
    [HtmlAttributeName("base-price")]
    public decimal BasePrice { get; set; }

    [HtmlAttributeName("sale-price")]
    public decimal? SalePrice { get; set; }

    [HtmlAttributeName("size")]
    public string Size { get; set; } = "md"; // sm | md | lg

    [HtmlAttributeName("show-badge")]
    public bool ShowBadge { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName    = "div";
        output.TagMode    = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "price-display d-flex align-items-baseline flex-wrap gap-2");

        bool   hasSale = SalePrice.HasValue && SalePrice.Value < BasePrice;
        string fz      = Size switch { "lg" => "fs-3", "sm" => "small", _ => "fs-5" };

        if (hasSale)
        {
            int disc = (int)Math.Round((BasePrice - SalePrice!.Value) / BasePrice * 100);
            var html = $"""
                <span class="{fz} fw-bold text-danger">{SalePrice.Value:N0} ₫</span>
                <span class="text-muted text-decoration-line-through small">{BasePrice:N0} ₫</span>
                """;
            if (ShowBadge)
                html += $"""<span class="badge bg-danger rounded-pill" style="font-size:.65rem">-{disc}%</span>""";
            output.Content.SetHtmlContent(html);
        }
        else
        {
            output.Content.SetHtmlContent(
                $"""<span class="{fz} fw-bold">{BasePrice:N0} ₫</span>""");
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 3. <status-badge> TAG HELPER
//
// Usage:
//   <status-badge order-status="@order.OrderStatus" />
//   <status-badge product-status="@p.Status" />
//   <status-badge payment-status="@order.PaymentStatus" />
//   <status-badge value="Active" />
// ═══════════════════════════════════════════════════════════════════════════
[HtmlTargetElement("status-badge")]
public class StatusBadgeTagHelper : TagHelper
{
    [HtmlAttributeName("order-status")]
    public OrderStatus? OrderStatus { get; set; }

    [HtmlAttributeName("product-status")]
    public ProductStatus? ProductStatus { get; set; }

    [HtmlAttributeName("payment-status")]
    public PaymentStatus? PaymentStatus { get; set; }

    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    [HtmlAttributeName("pill")]
    public bool Pill { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        var (text, cssClass) = ResolveStyle();
        var pillClass = Pill ? " rounded-pill" : "";
        output.Attributes.SetAttribute("class", $"badge {cssClass}{pillClass}");
        output.Content.SetContent(text);
    }

    private (string text, string css) ResolveStyle()
    {
        if (OrderStatus.HasValue) return OrderStatus.Value switch
        {
            Ecommerce.Domain.Enums.OrderStatus.Pending    => ("Pending",    "bg-warning text-dark"),
            Ecommerce.Domain.Enums.OrderStatus.Processing => ("Processing", "bg-primary"),
            Ecommerce.Domain.Enums.OrderStatus.Shipped    => ("Shipped",    "bg-info text-dark"),
            Ecommerce.Domain.Enums.OrderStatus.Delivered  => ("Delivered",  "bg-success"),
            Ecommerce.Domain.Enums.OrderStatus.Cancelled  => ("Cancelled",  "bg-danger"),
            _ => (OrderStatus.Value.ToString(), "bg-secondary"),
        };

        if (ProductStatus.HasValue) return ProductStatus.Value switch
        {
            Ecommerce.Domain.Enums.ProductStatus.Active   => ("Active",   "bg-success"),
            Ecommerce.Domain.Enums.ProductStatus.Draft    => ("Draft",    "bg-secondary"),
            Ecommerce.Domain.Enums.ProductStatus.OutOfStock => ("Archived", "bg-warning text-dark"),
            _ => (ProductStatus.Value.ToString(), "bg-secondary"),
        };

        if (PaymentStatus.HasValue) return PaymentStatus.Value switch
        {
            Ecommerce.Domain.Enums.PaymentStatus.Unpaid  => ("Unpaid",   "bg-warning text-dark"),
            Ecommerce.Domain.Enums.PaymentStatus.Paid     => ("Paid",     "bg-success"),
            Ecommerce.Domain.Enums.PaymentStatus.Refunded => ("Refunded", "bg-info text-dark"),
            Ecommerce.Domain.Enums.PaymentStatus.Failed   => ("Failed",   "bg-danger"),
            _ => (PaymentStatus.Value.ToString(), "bg-secondary"),
        };

        // Fallback string value
        return (Value ?? "Unknown") switch
        {
            "Active" or "active"       => (Value!, "bg-success"),
            "Inactive" or "inactive"   => (Value!, "bg-secondary"),
            "Locked" or "locked"       => (Value!, "bg-danger"),
            "Pending" or "pending"     => (Value!, "bg-warning text-dark"),
            _ => (Value ?? "Unknown", "bg-light text-dark border"),
        };
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 4. <pagination> TAG HELPER
//
// Usage:
//   <pagination current-page="@Model.PageNumber"
//               total-pages="@Model.TotalPages"
//               asp-action="Index"
//               asp-controller="Products"
//               route-data='@new Dictionary<string,string>{{"search","phone"}}' />
// ═══════════════════════════════════════════════════════════════════════════
[HtmlTargetElement("pagination")]
public class PaginationTagHelper : TagHelper
{
    [HtmlAttributeName("current-page")]
    public int CurrentPage { get; set; } = 1;

    [HtmlAttributeName("total-pages")]
    public int TotalPages { get; set; } = 1;

    [HtmlAttributeName("asp-action")]
    public string Action { get; set; } = "Index";

    [HtmlAttributeName("asp-controller")]
    public string? Controller { get; set; }

    [HtmlAttributeName("route-data")]
    public Dictionary<string, string>? RouteData { get; set; }

    [HtmlAttributeName("size")]
    public string Size { get; set; } = "sm"; // sm | md | lg

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (TotalPages <= 1)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("aria-label", "Pagination");

        var sizeClass = Size == "lg" ? "" : Size == "sm" ? " pagination-sm" : "";
        var sb        = new System.Text.StringBuilder();
        sb.Append($"""<ul class="pagination{sizeClass} mb-0 gap-1">""");

        // Prev
        sb.Append(PageItem(CurrentPage - 1, """<i class="bi bi-chevron-left"></i>""",
            CurrentPage <= 1, "Previous"));

        // Page numbers — show window of 5
        int start = Math.Max(1, CurrentPage - 2);
        int end   = Math.Min(TotalPages, CurrentPage + 2);

        if (start > 1)
        {
            sb.Append(PageItem(1, "1"));
            if (start > 2) sb.Append("""<li class="page-item disabled"><span class="page-link">…</span></li>""");
        }

        for (int i = start; i <= end; i++)
            sb.Append(PageItem(i, i.ToString(), false, null, i == CurrentPage));

        if (end < TotalPages)
        {
            if (end < TotalPages - 1)
                sb.Append("""<li class="page-item disabled"><span class="page-link">…</span></li>""");
            sb.Append(PageItem(TotalPages, TotalPages.ToString()));
        }

        // Next
        sb.Append(PageItem(CurrentPage + 1, """<i class="bi bi-chevron-right"></i>""",
            CurrentPage >= TotalPages, "Next"));

        sb.Append("</ul>");
        output.Content.SetHtmlContent(sb.ToString());
    }

    private string PageItem(int page, string label,
        bool disabled = false, string? ariaLabel = null, bool active = false)
    {
        var activeClass   = active   ? " active"   : "";
        var disabledClass = disabled ? " disabled" : "";
        var url = BuildUrl(page);
        var aria = ariaLabel != null ? $""" aria-label="{ariaLabel}" """ : "";

        return $"""
            <li class="page-item{activeClass}{disabledClass}">
                <a class="page-link rounded"{aria} href="{url}">{label}</a>
            </li>
            """;
    }

    private string BuildUrl(int page)
    {
        var ctx    = ViewContext.HttpContext;
        var query  = System.Web.HttpUtility.ParseQueryString(ctx.Request.QueryString.ToString());
        query["pageNumber"] = page.ToString();

        if (RouteData != null)
            foreach (var kv in RouteData)
                query[kv.Key] = kv.Value;

        var path = Controller != null
            ? $"/{Controller}/{Action}"
            : ctx.Request.Path.ToString();

        return $"{path}?{query}";
    }
}