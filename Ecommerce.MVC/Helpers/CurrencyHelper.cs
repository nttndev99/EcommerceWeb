using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Helpers;

public static class CurrencyHelper
{
    // ─────────────────────────────────────────────
    // @Html.Currency(999000)
    // → "999,000 ₫"
    // ─────────────────────────────────────────────
    public static IHtmlContent Currency(this IHtmlHelper html, decimal amount)
        => new HtmlString($"{amount:N0} ₫");

    // ─────────────────────────────────────────────
    // @Html.PriceDisplay(basePrice, salePrice)
    // → "<span>799,000 ₫</span> <del>999,000 ₫</del> <badge>-20%</badge>"
    // ─────────────────────────────────────────────
    public static IHtmlContent PriceDisplay(
        this IHtmlHelper html,
        decimal basePrice,
        decimal? salePrice = null,
        bool showBadge = true)
    {
        bool   hasSale = salePrice.HasValue && salePrice.Value < basePrice;

        if (!hasSale)
            return new HtmlString($"""<span class="fw-bold">{basePrice:N0} ₫</span>""");

        int    disc = (int)Math.Round((basePrice - salePrice!.Value) / basePrice * 100);
        var    badge = showBadge
            ? $"""<span class="badge bg-danger rounded-pill ms-1" style="font-size:.65rem">-{disc}%</span>"""
            : "";

        return new HtmlString($"""
            <span class="fw-bold text-danger">{salePrice.Value:N0} ₫</span>
            <span class="text-muted text-decoration-line-through small ms-1">{basePrice:N0} ₫</span>
            {badge}
            """);
    }

    // ─────────────────────────────────────────────
    // @Html.Discount(basePrice, salePrice)
    // → "-20%"
    // ─────────────────────────────────────────────
    public static string Discount(this IHtmlHelper html, decimal basePrice, decimal salePrice)
    {
        if (basePrice <= 0) return "0%";
        int disc = (int)Math.Round((basePrice - salePrice) / basePrice * 100);
        return $"-{disc}%";
    }
}