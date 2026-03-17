using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class AppDbSeeder
{
    public static async Task SeedAsync(
        EcommerceDbContext db,
        ILogger logger,
        IServiceProvider services)
    {
        try
        {
            // ── Always run: Roles & Admin ─────────────────────────────────
            await SeedRolesAsync(services, logger);
            await SeedAdminUserAsync(services, logger);

            // ── Product data: only once ───────────────────────────────────
            if (await db.Categories.IgnoreQueryFilters().AnyAsync())
            {
                logger.LogInformation("Product data already seeded, skipping.");
                return;
            }

            // ─────────────────────────────────────────────────────────────
            // 1. ROOT CATEGORIES
            // ─────────────────────────────────────────────────────────────
            var electronics = new Category { Name = "Electronics",       Slug = "electronics",     Description = "Electronic devices and accessories",      ImageUrl = "https://placehold.co/600x400/3b82f6/ffffff?text=Electronics",   IsActive = true, DisplayOrder = 1 };
            var fashion     = new Category { Name = "Fashion",           Slug = "fashion",         Description = "Clothing, shoes, and accessories",         ImageUrl = "https://placehold.co/600x400/ec4899/ffffff?text=Fashion",       IsActive = true, DisplayOrder = 2 };
            var homeGarden  = new Category { Name = "Home & Garden",     Slug = "home-garden",     Description = "Furniture, décor, and garden tools",       ImageUrl = "https://placehold.co/600x400/22c55e/ffffff?text=Home",          IsActive = true, DisplayOrder = 3 };
            var sports      = new Category { Name = "Sports & Outdoors", Slug = "sports-outdoors", Description = "Sporting goods and outdoor equipment",     ImageUrl = "https://placehold.co/600x400/f97316/ffffff?text=Sports",        IsActive = true, DisplayOrder = 4 };
            var beauty      = new Category { Name = "Beauty & Health",   Slug = "beauty-health",   Description = "Skincare, makeup, and wellness products",  ImageUrl = "https://placehold.co/600x400/a855f7/ffffff?text=Beauty",        IsActive = true, DisplayOrder = 5 };

            await db.Categories.AddRangeAsync(electronics, fashion, homeGarden, sports, beauty);
            await db.SaveChangesAsync();

            // ─────────────────────────────────────────────────────────────
            // 2. SUB CATEGORIES
            // ─────────────────────────────────────────────────────────────
            var phones        = new Category { Name = "Phones",           Slug = "phones",          ParentId = electronics.Id, IsActive = true, DisplayOrder = 1 };
            var laptops       = new Category { Name = "Laptops",          Slug = "laptops",         ParentId = electronics.Id, IsActive = true, DisplayOrder = 2 };
            var tablets       = new Category { Name = "Tablets",          Slug = "tablets",         ParentId = electronics.Id, IsActive = true, DisplayOrder = 3 };
            var audio         = new Category { Name = "Audio",            Slug = "audio",           ParentId = electronics.Id, IsActive = true, DisplayOrder = 4 };
            var menClothing   = new Category { Name = "Men's Clothing",   Slug = "mens-clothing",   ParentId = fashion.Id,     IsActive = true, DisplayOrder = 1 };
            var womenClothing = new Category { Name = "Women's Clothing", Slug = "womens-clothing", ParentId = fashion.Id,     IsActive = true, DisplayOrder = 2 };
            var shoes         = new Category { Name = "Shoes",            Slug = "shoes",           ParentId = fashion.Id,     IsActive = true, DisplayOrder = 3 };
            var furniture     = new Category { Name = "Furniture",        Slug = "furniture",       ParentId = homeGarden.Id,  IsActive = true, DisplayOrder = 1 };
            var kitchenware   = new Category { Name = "Kitchenware",      Slug = "kitchenware",     ParentId = homeGarden.Id,  IsActive = true, DisplayOrder = 2 };
            var fitness       = new Category { Name = "Fitness",          Slug = "fitness",         ParentId = sports.Id,      IsActive = true, DisplayOrder = 1 };
            var outdoor       = new Category { Name = "Outdoor Gear",     Slug = "outdoor-gear",    ParentId = sports.Id,      IsActive = true, DisplayOrder = 2 };
            var skincare      = new Category { Name = "Skincare",         Slug = "skincare",        ParentId = beauty.Id,      IsActive = true, DisplayOrder = 1 };

            await db.Categories.AddRangeAsync(
                phones, laptops, tablets, audio,
                menClothing, womenClothing, shoes,
                furniture, kitchenware,
                fitness, outdoor, skincare);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded categories");

            // ─────────────────────────────────────────────────────────────
            // 3. PRODUCTS  (index 0-42)
            //    [0-4]   Phones        → HAS variants
            //    [5-9]   Laptops       → HAS variants
            //    [10-12] Tablets       → HAS variants
            //    [13-16] Audio         → HAS variants
            //    [17-20] Men Clothing  → HAS variants
            //    [21-24] Women Clothing→ HAS variants
            //    [25-27] Shoes         → HAS variants
            //    [28-30] Furniture     → NO variants (inventory per product)
            //    [31-33] Kitchenware   → NO variants
            //    [34-36] Fitness       → NO variants
            //    [37-39] Outdoor       → NO variants
            //    [40-42] Skincare      → NO variants
            // ─────────────────────────────────────────────────────────────
            var products = new List<Product>
            {
                // PHONES [0-4]
                new() { Name="iPhone 15 Pro",            Slug="iphone-15-pro",            SKU="APPL-IPH15P",   BasePrice=999.99m,  SalePrice=949.99m,  Status=ProductStatus.Active, IsFeatured=true,  CategoryId=phones.Id,        Brand="Apple",          Weight=0.187m, Tags="apple,iphone,5g",          Description="Apple iPhone 15 Pro with A17 Pro chip and titanium design.",           ShortDescription="Latest Apple flagship smartphone." },
                new() { Name="Samsung Galaxy S24",       Slug="samsung-galaxy-s24",       SKU="SAM-GS24",      BasePrice=799.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=true,  CategoryId=phones.Id,        Brand="Samsung",        Weight=0.167m, Tags="samsung,android,5g",       Description="Samsung Galaxy S24 with Snapdragon 8 Gen 3 and Galaxy AI.",             ShortDescription="Samsung flagship 2024." },
                new() { Name="Google Pixel 8 Pro",       Slug="google-pixel-8-pro",       SKU="GOOG-PX8P",     BasePrice=899.99m,  SalePrice=849.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=phones.Id,        Brand="Google",         Weight=0.213m, Tags="google,pixel,ai",          Description="Google Pixel 8 Pro with Tensor G3 chip and best-in-class camera.",     ShortDescription="Best AI camera smartphone." },
                new() { Name="OnePlus 12",               Slug="oneplus-12",               SKU="OP-12",         BasePrice=699.99m,  SalePrice=649.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=phones.Id,        Brand="OnePlus",        Weight=0.220m, Tags="oneplus,snapdragon",       Description="OnePlus 12 with Snapdragon 8 Gen 3 and 100W fast charging.",           ShortDescription="Flagship killer 2024." },
                new() { Name="Xiaomi 14 Ultra",          Slug="xiaomi-14-ultra",          SKU="XMI-14U",       BasePrice=1099.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=true,  CategoryId=phones.Id,        Brand="Xiaomi",         Weight=0.222m, Tags="xiaomi,leica,camera",      Description="Xiaomi 14 Ultra with Leica optics and Snapdragon 8 Gen 3.",            ShortDescription="Pro photography smartphone." },
                // LAPTOPS [5-9]
                new() { Name="MacBook Pro 14\"",         Slug="macbook-pro-14",           SKU="APPL-MBP14",    BasePrice=1999.99m, SalePrice=1899.99m, Status=ProductStatus.Active, IsFeatured=true,  CategoryId=laptops.Id,       Brand="Apple",          Weight=1.55m,  Tags="apple,macbook,m3",         Description="Apple MacBook Pro 14-inch with M3 chip and Liquid Retina XDR display.", ShortDescription="Pro laptop for creators." },
                new() { Name="Dell XPS 15",              Slug="dell-xps-15",              SKU="DELL-XPS15",    BasePrice=1799.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="Dell",           Weight=1.86m,  Tags="dell,oled,intel",          Description="Dell XPS 15 with Intel Core i9 and 3.5K OLED display.",               ShortDescription="Premium Windows laptop." },
                new() { Name="ASUS ROG Zephyrus G14",   Slug="asus-rog-zephyrus-g14",    SKU="ASUS-ROG-G14",  BasePrice=1499.99m, SalePrice=1399.99m, Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="ASUS",           Weight=1.65m,  Tags="asus,gaming,ryzen",        Description="ASUS ROG Zephyrus G14 gaming laptop with AMD Ryzen 9.",               ShortDescription="Compact gaming powerhouse." },
                new() { Name="Lenovo ThinkPad X1",       Slug="lenovo-thinkpad-x1",       SKU="LNV-TPX1C",     BasePrice=1599.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="Lenovo",         Weight=1.12m,  Tags="lenovo,thinkpad,business", Description="Lenovo ThinkPad X1 Carbon ultra-light business laptop.",               ShortDescription="Business ultrabook." },
                new() { Name="HP Spectre x360",          Slug="hp-spectre-x360",          SKU="HP-SPCX360",    BasePrice=1399.99m, SalePrice=1299.99m, Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="HP",             Weight=1.36m,  Tags="hp,spectre,2-in-1",        Description="HP Spectre x360 2-in-1 laptop with OLED touchscreen.",                ShortDescription="Premium 2-in-1 laptop." },
                // TABLETS [10-12]
                new() { Name="iPad Pro 12.9\"",          Slug="ipad-pro-12",              SKU="APPL-IPDP12",   BasePrice=1099.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=true,  CategoryId=tablets.Id,       Brand="Apple",          Weight=0.682m, Tags="apple,ipad,m2",            Description="Apple iPad Pro 12.9-inch with M2 chip and Liquid Retina XDR.",         ShortDescription="Most powerful iPad." },
                new() { Name="Samsung Galaxy Tab S9",    Slug="samsung-galaxy-tab-s9",    SKU="SAM-TABS9",     BasePrice=799.99m,  SalePrice=749.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=tablets.Id,       Brand="Samsung",        Weight=0.498m, Tags="samsung,android,amoled",   Description="Samsung Galaxy Tab S9 with Dynamic AMOLED 2X display.",                ShortDescription="Android flagship tablet." },
                new() { Name="Microsoft Surface Pro 9",  Slug="microsoft-surface-pro-9",  SKU="MSFT-SFP9",     BasePrice=1299.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=tablets.Id,       Brand="Microsoft",      Weight=0.879m, Tags="microsoft,surface,windows", Description="Microsoft Surface Pro 9 with Intel Core i7 and detachable keyboard.",  ShortDescription="Tablet meets laptop." },
                // AUDIO [13-16]
                new() { Name="Sony WH-1000XM5",          Slug="sony-wh-1000xm5",          SKU="SNY-WH1000XM5", BasePrice=349.99m,  SalePrice=279.99m,  Status=ProductStatus.Active, IsFeatured=true,  CategoryId=audio.Id,         Brand="Sony",           Weight=0.250m, Tags="sony,anc,wireless",        Description="Sony WH-1000XM5 wireless ANC headphones with 30hr battery.",           ShortDescription="Best ANC headphones." },
                new() { Name="Apple AirPods Pro 2",      Slug="apple-airpods-pro-2",       SKU="APPL-APP2",     BasePrice=249.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=audio.Id,         Brand="Apple",          Weight=0.051m, Tags="apple,airpods,anc",        Description="Apple AirPods Pro 2nd generation with H2 chip and Adaptive Audio.",    ShortDescription="Premium Apple earbuds." },
                new() { Name="Bose QuietComfort 45",     Slug="bose-quietcomfort-45",      SKU="BOSE-QC45",     BasePrice=329.99m,  SalePrice=299.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=audio.Id,         Brand="Bose",           Weight=0.238m, Tags="bose,anc,wireless",        Description="Bose QC45 wireless headphones with world-class noise cancellation.",   ShortDescription="Iconic Bose comfort." },
                new() { Name="JBL Charge 5",             Slug="jbl-charge-5",              SKU="JBL-CHG5",      BasePrice=179.99m,  SalePrice=149.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=audio.Id,         Brand="JBL",            Weight=0.960m, Tags="jbl,bluetooth,waterproof", Description="JBL Charge 5 portable Bluetooth speaker IP67 waterproof + powerbank.", ShortDescription="Waterproof Bluetooth speaker." },
                // MEN CLOTHING [17-20]
                new() { Name="Classic Men T-Shirt",      Slug="classic-men-tshirt",        SKU="FASH-TSH-001",  BasePrice=29.99m,   SalePrice=24.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",      Weight=0.20m,  Tags="tshirt,men,cotton",        Description="Comfortable 100% cotton everyday t-shirt, pre-shrunk fabric.",         ShortDescription="100% cotton casual tee." },
                new() { Name="Men Slim-Fit Chinos",      Slug="men-slim-fit-chinos",        SKU="FASH-CHN-001",  BasePrice=59.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",      Weight=0.45m,  Tags="chinos,men,pants",         Description="Stretch slim-fit chino pants, wrinkle-resistant cotton blend.",        ShortDescription="Smart casual chinos." },
                new() { Name="Men Denim Jacket",         Slug="men-denim-jacket",           SKU="FASH-DNM-001",  BasePrice=89.99m,   SalePrice=74.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",      Weight=0.85m,  Tags="denim,jacket,men",         Description="Classic denim jacket with button closure and chest pockets.",          ShortDescription="Timeless denim jacket." },
                new() { Name="Men Formal Oxford Shirt",  Slug="men-formal-oxford-shirt",    SKU="FASH-OXF-001",  BasePrice=49.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",      Weight=0.30m,  Tags="shirt,men,formal",         Description="Premium Oxford cotton formal shirt, wrinkle-resistant finish.",        ShortDescription="Classic Oxford shirt." },
                // WOMEN CLOTHING [21-24]
                new() { Name="Women Floral Dress",       Slug="women-floral-dress",         SKU="FASH-WD-001",   BasePrice=69.99m,   SalePrice=54.99m,   Status=ProductStatus.Active, IsFeatured=true,  CategoryId=womenClothing.Id, Brand="ElegantLine",    Weight=0.35m,  Tags="dress,women,floral",       Description="Light chiffon floral summer dress with v-neck and flowy silhouette.",  ShortDescription="Elegant summer dress." },
                new() { Name="Women Yoga Leggings",      Slug="women-yoga-leggings",         SKU="FASH-WL-001",   BasePrice=44.99m,   SalePrice=39.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=womenClothing.Id, Brand="ActiveGirl",     Weight=0.25m,  Tags="leggings,women,yoga",      Description="High-waist compression leggings for yoga and gym, 4-way stretch.",    ShortDescription="Flexible yoga leggings." },
                new() { Name="Women Wool Blazer",        Slug="women-wool-blazer",           SKU="FASH-WB-001",   BasePrice=119.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=womenClothing.Id, Brand="ElegantLine",    Weight=0.70m,  Tags="blazer,women,wool",        Description="Tailored single-button wool-blend blazer, fully lined.",              ShortDescription="Smart professional blazer." },
                new() { Name="Women Crop Hoodie",        Slug="women-crop-hoodie",           SKU="FASH-WH-001",   BasePrice=54.99m,   SalePrice=44.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=womenClothing.Id, Brand="ActiveGirl",     Weight=0.40m,  Tags="hoodie,women,crop",        Description="Soft fleece crop hoodie for casual everyday wear.",                    ShortDescription="Cozy fleece crop hoodie." },
                // SHOES [25-27]
                new() { Name="Nike Air Max 270",         Slug="nike-air-max-270",            SKU="NIKE-AM270",    BasePrice=149.99m,  SalePrice=129.99m,  Status=ProductStatus.Active, IsFeatured=true,  CategoryId=shoes.Id,         Brand="Nike",           Weight=0.31m,  Tags="nike,airmax,sneakers",     Description="Nike Air Max 270 with large Max Air unit for all-day comfort.",        ShortDescription="Iconic Air Max cushioning." },
                new() { Name="Adidas Ultraboost 23",     Slug="adidas-ultraboost-23",         SKU="ADI-UB23",      BasePrice=179.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=shoes.Id,         Brand="Adidas",         Weight=0.33m,  Tags="adidas,ultraboost,running", Description="Adidas Ultraboost 23 with Boost midsole and Primeknit+ upper.",       ShortDescription="Premium running shoes." },
                new() { Name="Converse Chuck Taylor",    Slug="converse-chuck-taylor",        SKU="CONV-CT-001",   BasePrice=64.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=shoes.Id,         Brand="Converse",       Weight=0.28m,  Tags="converse,casual,classic",  Description="Classic Converse Chuck Taylor All Star canvas sneaker.",              ShortDescription="Timeless canvas sneaker." },
                // FURNITURE [28-30] — NO variants
                new() { Name="Ergonomic Office Chair",   Slug="ergonomic-office-chair",      SKU="FURN-OC-001",   BasePrice=299.99m,  SalePrice=249.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=furniture.Id,     Brand="WorkPro",        Weight=14.0m,  Tags="chair,office,ergonomic",   Description="Full mesh ergonomic chair with lumbar support and adjustable armrests.", ShortDescription="Work comfortably all day." },
                new() { Name="Minimalist Desk",          Slug="minimalist-desk",              SKU="FURN-DK-001",   BasePrice=349.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=furniture.Id,     Brand="WorkPro",        Weight=28.0m,  Tags="desk,office,wood",         Description="Clean-line 140cm wooden desk with cable management tray.",            ShortDescription="Sleek home office desk." },
                new() { Name="3-Seater Sofa",            Slug="3-seater-sofa",                SKU="FURN-SF-001",   BasePrice=799.99m,  SalePrice=699.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=furniture.Id,     Brand="HomePlus",       Weight=45.0m,  Tags="sofa,living-room,fabric",  Description="Modern fabric 3-seater sofa with solid wood legs.",                   ShortDescription="Comfortable living room sofa." },
                // KITCHENWARE [31-33] — NO variants
                new() { Name="Instant Pot Duo 7-in-1",   Slug="instant-pot-duo-7in1",        SKU="KITCH-IP-001",  BasePrice=99.99m,   SalePrice=79.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=kitchenware.Id,   Brand="Instant Pot",    Weight=5.4m,   Tags="instant-pot,pressure-cooker", Description="7-in-1 multi-use programmable pressure cooker, 6 Qt.",              ShortDescription="The ultimate kitchen appliance." },
                new() { Name="Vitamix E310 Blender",     Slug="vitamix-e310-blender",         SKU="KITCH-VTX-001", BasePrice=349.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=kitchenware.Id,   Brand="Vitamix",        Weight=4.3m,   Tags="vitamix,blender,kitchen",  Description="Vitamix Explorian E310 blender with 5-year warranty.",                ShortDescription="Professional-grade blender." },
                new() { Name="Cast Iron Skillet 12\"",   Slug="cast-iron-skillet-12",         SKU="KITCH-CI-001",  BasePrice=44.99m,   SalePrice=39.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=kitchenware.Id,   Brand="Lodge",          Weight=3.6m,   Tags="cast-iron,skillet,lodge",  Description="Pre-seasoned 12-inch cast iron skillet, oven-safe to 500°F.",         ShortDescription="Classic cast iron skillet." },
                // FITNESS [34-36] — NO variants
                new() { Name="Adjustable Dumbbell Set",  Slug="adjustable-dumbbell-set",      SKU="FIT-DB-001",    BasePrice=349.99m,  SalePrice=299.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=fitness.Id,       Brand="Bowflex",        Weight=24.0m,  Tags="dumbbell,fitness,home-gym", Description="Adjustable dumbbell set 5-52.5 lbs per dumbbell, replaces 15 sets.", ShortDescription="Space-saving dumbbell set." },
                new() { Name="Yoga Mat Premium",         Slug="yoga-mat-premium",              SKU="FIT-YM-001",    BasePrice=49.99m,   SalePrice=39.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=fitness.Id,       Brand="Gaiam",          Weight=1.2m,   Tags="yoga,mat,fitness",         Description="Extra thick 6mm non-slip TPE yoga mat with carry strap.",             ShortDescription="Non-slip premium yoga mat." },
                new() { Name="Jump Rope Speed",          Slug="jump-rope-speed",               SKU="FIT-JR-001",    BasePrice=24.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=fitness.Id,       Brand="RogueFit",       Weight=0.15m,  Tags="jump-rope,cardio,speed",   Description="Lightweight aluminum speed jump rope with ball bearings.",            ShortDescription="Speed training jump rope." },
                // OUTDOOR [37-39] — NO variants
                new() { Name="North Face Tent 2P",       Slug="north-face-tent-2p",            SKU="OUT-TNT-001",   BasePrice=449.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=outdoor.Id,       Brand="The North Face", Weight=1.4m,   Tags="tent,camping,ultralight",  Description="2-person ultralight backpacking tent with rainfly, 3-season.",        ShortDescription="Ultralight camping tent." },
                new() { Name="Hydro Flask 32oz",         Slug="hydro-flask-32oz",               SKU="OUT-HF-001",    BasePrice=49.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=outdoor.Id,       Brand="Hydro Flask",    Weight=0.36m,  Tags="water-bottle,insulated",   Description="32 oz wide mouth insulated stainless steel water bottle, keeps cold 24hrs.", ShortDescription="Keep drinks cold 24hrs." },
                new() { Name="Osprey Atmos 65L",         Slug="osprey-atmos-65l",               SKU="OUT-BP-001",    BasePrice=299.99m,  SalePrice=269.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=outdoor.Id,       Brand="Osprey",         Weight=2.2m,   Tags="backpack,hiking,65l",      Description="Osprey Atmos AG 65L men's backpacking pack with Anti-Gravity suspension.", ShortDescription="Top-rated hiking backpack." },
                // SKINCARE [40-42] — NO variants
                new() { Name="Cetaphil Moisturizing Cream",  Slug="cetaphil-moisturizing-cream",  SKU="SKIN-CTF-001",  BasePrice=19.99m,  SalePrice=null,    Status=ProductStatus.Active, IsFeatured=false, CategoryId=skincare.Id,      Brand="Cetaphil",       Weight=0.60m,  Tags="cetaphil,moisturizer",     Description="Cetaphil moisturizing cream for dry, sensitive skin 550g tub.",       ShortDescription="Gentle daily moisturizer." },
                new() { Name="The Ordinary Niacinamide",     Slug="the-ordinary-niacinamide",     SKU="SKIN-ORD-001",  BasePrice=12.99m,  SalePrice=null,    Status=ProductStatus.Active, IsFeatured=false, CategoryId=skincare.Id,      Brand="The Ordinary",   Weight=0.05m,  Tags="niacinamide,serum",        Description="The Ordinary Niacinamide 10% + Zinc 1% serum 30ml.",                  ShortDescription="Pore-minimizing serum." },
                new() { Name="Neutrogena SPF 50",            Slug="neutrogena-spf50",             SKU="SKIN-NTG-001",  BasePrice=14.99m,  SalePrice=12.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=skincare.Id,      Brand="Neutrogena",     Weight=0.12m,  Tags="sunscreen,spf50",          Description="Neutrogena Ultra Sheer Dry-Touch SPF 50 sunscreen 88ml.",             ShortDescription="Lightweight SPF 50 protection." },
            };

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} products", products.Count);

            // ─────────────────────────────────────────────────────────────
            // 4. PRODUCT IMAGES  (3 ảnh per product)
            //    ảnh 1 = primary  (màu nền đặc trưng)
            //    ảnh 2 = side view
            //    ảnh 3 = detail / lifestyle
            // ─────────────────────────────────────────────────────────────
            // Màu nền đặc trưng theo category
            string ImgBg(Product prod)
            {
                if (prod.CategoryId == phones.Id || prod.CategoryId == tablets.Id)  return "1e293b/e2e8f0";
                if (prod.CategoryId == laptops.Id)                                  return "0f172a/f8fafc";
                if (prod.CategoryId == audio.Id)                                    return "1d4ed8/dbeafe";
                if (prod.CategoryId == menClothing.Id)                              return "374151/f3f4f6";
                if (prod.CategoryId == womenClothing.Id)                            return "be185d/fce7f3";
                if (prod.CategoryId == shoes.Id)                                    return "b45309/fef3c7";
                if (prod.CategoryId == furniture.Id)                                return "713f12/fef9c3";
                if (prod.CategoryId == kitchenware.Id)                              return "166534/dcfce7";
                if (prod.CategoryId == fitness.Id)                                  return "9a3412/ffedd5";
                if (prod.CategoryId == outdoor.Id)                                  return "14532d/f0fdf4";
                if (prod.CategoryId == skincare.Id)                                 return "6b21a8/f5f3ff";
                return "334155/f1f5f9";
            }

            var images = products.SelectMany(prod =>
            {
                var slug = Uri.EscapeDataString(prod.Name);
                var bg   = ImgBg(prod);
                return new[]
                {
                    new ProductImage { ProductId = prod.Id, ImageUrl = $"https://placehold.co/800x800/{bg}?text={slug}",           AltText = prod.Name,                    IsPrimary = true,  DisplayOrder = 1 },
                    new ProductImage { ProductId = prod.Id, ImageUrl = $"https://placehold.co/800x800/{bg}?text={slug}+Side",      AltText = prod.Name + " - Side View",   IsPrimary = false, DisplayOrder = 2 },
                    new ProductImage { ProductId = prod.Id, ImageUrl = $"https://placehold.co/800x800/{bg}?text={slug}+Detail",    AltText = prod.Name + " - Detail",      IsPrimary = false, DisplayOrder = 3 },
                };
            }).ToList();

            await db.ProductImages.AddRangeAsync(images);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} product images (3/product)", images.Count);

            // ─────────────────────────────────────────────────────────────
            // 5. VARIANTS
            // ─────────────────────────────────────────────────────────────
            var allVariants = new List<ProductVariant>();
            var p = products;

            // Color name → CSS hex mapping
            static string ToHex(string? color) => color?.ToLower().Trim() switch
            {
                // Blacks
                "black" or "onyx black" or "silky black" or "classic black"
                    or "black titanium" or "core black"                     => "#1c1c1e",
                // Whites
                "white" or "white titanium" or "white smoke" or "classic white"
                    or "cloud white" or "moonlight white"                   => "#f2f2f7",
                // Silvers / Grays
                "silver" or "natural titanium" or "platinum"                => "#c7c7cc",
                "gray" or "marble gray" or "graphite" or "eclipse gray"
                    or "space gray"                                         => "#636366",
                // Blues
                "blue" or "cobalt violet" or "poseidon blue"
                    or "floral blue" or "sapphire"                          => "#3b82f6",
                "light blue"                                                => "#93c5fd",
                "navy"                                                      => "#1e3a5f",
                // Reds / Pinks
                "red"                                                       => "#ef4444",
                "pink" or "floral pink"                                     => "#ec4899",
                // Greens
                "green" or "flowy emerald"                                  => "#10b981",
                "olive"                                                     => "#6b7c2e",
                // Yellows / Browns
                "floral yellow"                                             => "#facc15",
                "khaki"                                                     => "#c3a46b",
                "camel"                                                     => "#c19a6b",
                // Purples
                "lavender"                                                  => "#a78bfa",
                // Multi-color → split color
                "white/black"                                               => "#e5e7eb",
                "black/red"                                                 => "#7f1d1d",
                // Beige
                "beige"                                                     => "#f5f0e8",
                _                                                           => "#e5e7eb",
            };

            // Helper: color × size  →  SKU = {prefix}-C{ci}S{si}
            void AddCS(int productId, string prefix,
                    string[] colors, string[] sizes,
                    decimal price, decimal? sale = null, string? material = null)
            {
                for (int ci = 0; ci < colors.Length; ci++)
                    for (int si = 0; si < sizes.Length; si++)
                        allVariants.Add(new ProductVariant
                        {
                            ProductId    = productId,
                            Name         = $"{colors[ci]} / {sizes[si]}",
                            SKU          = $"{prefix}-C{ci}S{si}",
                            Price        = price,
                            SalePrice    = sale,
                            Color        = colors[ci],
                            ColorHex     = ToHex(colors[ci]),
                            Size         = sizes[si],
                            Material     = material,
                            IsActive     = true,
                            DisplayOrder = si + 1,
                        });
            }

            // Helper: color only  →  SKU = {prefix}-C{ci}
            void AddC(int productId, string prefix,
                      string[] colors, decimal price, decimal? sale = null)
            {
                for (int ci = 0; ci < colors.Length; ci++)
                    allVariants.Add(new ProductVariant
                    {
                        ProductId    = productId,
                        Name         = colors[ci],
                        SKU          = $"{prefix}-C{ci}",
                        Price        = price,
                        SalePrice    = sale,
                        Color        = colors[ci],
                        ColorHex     = ToHex(colors[ci]),
                        IsActive     = true,
                        DisplayOrder = ci + 1,
                    });
            }

            // Helper: named configs (laptops / storage tiers)
            void AddStorage(int productId, string prefix,
                            (string label, decimal price, decimal? sale)[] configs)
            {
                for (int i = 0; i < configs.Length; i++)
                    allVariants.Add(new ProductVariant
                    {
                        ProductId    = productId,
                        Name         = configs[i].label,
                        SKU          = $"{prefix}-V{i}",
                        Price        = configs[i].price,
                        SalePrice    = configs[i].sale,
                        Size         = configs[i].label,
                        IsActive     = true,
                        DisplayOrder = i + 1,
                    });
            }

            // ── PHONES [0-4] ──────────────────────────────────────────────
            AddCS(p[0].Id,  "IPH15P",  ["Black Titanium","White Titanium","Natural Titanium"], ["256GB","512GB"],   999.99m, 949.99m);
            AddCS(p[1].Id,  "GS24",    ["Onyx Black","Marble Gray","Cobalt Violet"],           ["256GB","512GB"],   799.99m);
            AddCS(p[2].Id,  "PX8P",    ["Obsidian","Porcelain"],                               ["128GB","256GB"],   899.99m, 849.99m);
            AddCS(p[3].Id,  "OP12",    ["Flowy Emerald","Silky Black"],                        ["256GB","512GB"],   699.99m, 649.99m);
            AddCS(p[4].Id,  "XMI14U",  ["White","Black"],                                      ["512GB","1TB"],    1099.99m);

            // ── LAPTOPS [5-9] ─────────────────────────────────────────────
            AddStorage(p[5].Id,  "MBP14",  [("16GB / 512GB SSD",1999.99m,1899.99m),("32GB / 1TB SSD",2499.99m,null),("36GB / 2TB SSD",3199.99m,null)]);
            AddStorage(p[6].Id,  "XPS15",  [("16GB / 512GB SSD",1799.99m,null),("32GB / 1TB SSD",2199.99m,null)]);
            AddStorage(p[7].Id,  "ROGG14", [("16GB / 512GB SSD",1499.99m,1399.99m),("32GB / 1TB SSD",1799.99m,null)]);
            AddStorage(p[8].Id,  "TPX1C",  [("16GB / 512GB SSD",1599.99m,null),("32GB / 1TB SSD",1999.99m,null)]);
            AddStorage(p[9].Id,  "HPX360", [("16GB / 512GB SSD",1399.99m,1299.99m),("16GB / 1TB SSD",1599.99m,null)]);

            // ── TABLETS [10-12] ───────────────────────────────────────────
            AddCS(p[10].Id, "IPDP12", ["Silver","Space Gray"],  ["256GB","512GB"], 1099.99m);
            AddCS(p[11].Id, "TABS9",  ["Beige","Graphite"],     ["128GB","256GB"],  799.99m, 749.99m);
            AddCS(p[12].Id, "SFP9",   ["Platinum","Sapphire"],  ["256GB","512GB"], 1299.99m);

            // ── AUDIO [13-16] ─────────────────────────────────────────────
            AddC(p[13].Id, "WH1000XM5", ["Black","Silver"],             349.99m, 279.99m);
            AddC(p[14].Id, "APP2",      ["White"],                       249.99m);
            AddC(p[15].Id, "QC45",      ["Black","White Smoke"],          329.99m, 299.99m);
            AddC(p[16].Id, "JBL5",      ["Black","Blue","Red"],           179.99m, 149.99m);

            // ── MEN CLOTHING [17-20] ──────────────────────────────────────
            string[] clothSizes  = ["S","M","L","XL"];
            string[] pantsWaists = ["W30","W32","W34","W36"];
            AddCS(p[17].Id, "TSH", ["Black","White","Navy"],       clothSizes,   29.99m, 24.99m, "Cotton");
            AddCS(p[18].Id, "CHN", ["Khaki","Navy","Olive"],       pantsWaists,  59.99m, material: "Stretch Cotton");
            AddCS(p[19].Id, "DNM", ["Blue","Black","Light Blue"],  clothSizes,   89.99m, 74.99m);
            AddCS(p[20].Id, "OXF", ["White","Light Blue","Pink"],  clothSizes,   49.99m);

            // ── WOMEN CLOTHING [21-24] ────────────────────────────────────
            string[] wSizes = ["XS","S","M","L"];
            AddCS(p[21].Id, "WD", ["Floral Blue","Floral Pink"],             wSizes,              69.99m, 54.99m);
            AddCS(p[22].Id, "WL", ["Black","Gray","Navy"],                   [..wSizes,"XL"],     44.99m, 39.99m, "Stretch Fabric");
            AddCS(p[23].Id, "WB", ["Black","Camel","Navy"],                  wSizes,             119.99m, material: "Wool Blend");
            AddCS(p[24].Id, "WH", ["Pink","Gray","Lavender"],                wSizes,              54.99m, 44.99m, "Fleece");

            // ── SHOES [25-27] ─────────────────────────────────────────────
            string[] shoeSizes = ["7","8","9","10","11","12"];
            AddCS(p[25].Id, "AM270", ["White/Black","Black/Red"],         shoeSizes, 149.99m, 129.99m);
            AddCS(p[26].Id, "UB23",  ["Core Black","Cloud White"],        shoeSizes, 179.99m);
            AddCS(p[27].Id, "CT",    ["White","Black","Navy"],             shoeSizes,  64.99m);

            // p[28-42]: Furniture / Kitchenware / Fitness / Outdoor / Skincare → NO variants

            await db.ProductVariants.AddRangeAsync(allVariants);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} variants", allVariants.Count);

            // ─────────────────────────────────────────────────────────────
            // 6. INVENTORIES
            //    Rule: has variants  → Inventory per variant  (VariantId NOT null)
            //          no  variants  → Inventory per product  (VariantId null)
            // ─────────────────────────────────────────────────────────────
            var inventories = new List<Inventory>();
            var rng = new Random(42); // seed cố định → data nhất quán mỗi lần chạy

            foreach (var v in allVariants)
            {
                var qty      = rng.Next(10, 150);
                var reserved = rng.Next(0, Math.Max(1, qty / 5));
                inventories.Add(new Inventory
                {
                    ProductId        = v.ProductId,
                    ProductVariantId = v.Id,
                    Quantity          = qty,
                    ReservedQuantity  = reserved,
                    LowStockThreshold = 10,
                    WarehouseLocation = $"WH-{(char)('A' + rng.Next(0, 5))}{rng.Next(1, 10):D2}",
                    LastStockUpdate   = DateTime.UtcNow,
                });
            }

            foreach (var prod in products.Skip(28)) // index 28-42: no-variant products
            {
                var qty      = rng.Next(5, 60);
                var reserved = rng.Next(0, Math.Max(1, qty / 5));
                inventories.Add(new Inventory
                {
                    ProductId        = prod.Id,
                    ProductVariantId = null,
                    Quantity          = qty,
                    ReservedQuantity  = reserved,
                    LowStockThreshold = 5,
                    WarehouseLocation = $"WH-{(char)('A' + rng.Next(0, 5))}{rng.Next(1, 10):D2}",
                    LastStockUpdate   = DateTime.UtcNow,
                });
            }

            await db.Inventories.AddRangeAsync(inventories);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} inventories", inventories.Count);

            // ─────────────────────────────────────────────────────────────
            // 7. SEED CUSTOMER USERS  (dùng cho Orders)
            // ─────────────────────────────────────────────────────────────
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            async Task<AppUser> EnsureUser(string email, string fullName, string password)
            {
                var existing = await userManager.FindByEmailAsync(email);
                if (existing is not null) return existing;

                var user = new AppUser
                {
                    FullName       = fullName,
                    UserName       = email,
                    Email          = email,
                    EmailConfirmed = true,
                    IsActive       = true,
                    CreatedAt      = DateTime.UtcNow,
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Customer");
                return user;
            }

            var alice = await EnsureUser("alice@example.com",   "Alice Nguyen",  "Customer@123");
            var bob   = await EnsureUser("bob@example.com",     "Bob Tran",      "Customer@123");
            var carol = await EnsureUser("carol@example.com",   "Carol Le",      "Customer@123");
            logger.LogInformation("Seeded customer users");

            // ─────────────────────────────────────────────────────────────
            // 8. ORDERS + ORDER ITEMS
            //    Mỗi order có 2-4 items, mix sản phẩm có/không variant
            //    OrderStatus / PaymentStatus đa dạng để test UI
            // ─────────────────────────────────────────────────────────────

            // Lấy snapshot variant đầu tiên của 1 product (với products có variant)
            ProductVariant? FirstVariant(Product prod)
                => allVariants.FirstOrDefault(v => v.ProductId == prod.Id);

            OrderItem MakeItem(Product prod, int qty, ProductVariant? variant = null)
            {
                var unitPrice = variant?.SalePrice ?? variant?.Price
                              ?? prod.SalePrice     ?? prod.BasePrice;
                return new OrderItem
                {
                    ProductId        = prod.Id,
                    ProductVariantId = variant?.Id,
                    ProductName      = prod.Name,
                    VariantName      = variant?.Name,
                    SKU              = variant?.SKU ?? prod.SKU,
                    ImageUrl         = $"https://placehold.co/200x200/{ImgBg(prod)}?text={Uri.EscapeDataString(prod.Name)}",
                    Quantity         = qty,
                    UnitPrice        = unitPrice,
                };
            }

            Order MakeOrder(string userId, string code, OrderStatus status, PaymentStatus pStatus,
                            PaymentMethod pMethod, string recipient, string phone,
                            string address, string ward, string district, string province,
                            decimal shipping, decimal discount, string? coupon,
                            DateTime? paidAt, DateTime? shippedAt, DateTime? deliveredAt,
                            string? note, List<OrderItem> items)
            {
                var sub   = items.Sum(i => i.UnitPrice * i.Quantity);
                var total = sub + shipping - discount;
                return new Order
                {
                    OrderCode     = code,
                    UserId        = userId,
                    OrderStatus   = status,
                    PaymentStatus = pStatus,
                    PaymentMethod = pMethod,
                    RecipientName  = recipient,
                    RecipientPhone = phone,
                    AddressLine    = address,
                    Ward           = ward,
                    District       = district,
                    Province       = province,
                    SubTotal       = sub,
                    ShippingFee    = shipping,
                    DiscountAmount = discount,
                    Total          = total,
                    CouponCode     = coupon,
                    PaidAt         = paidAt,
                    ShippedAt      = shippedAt,
                    DeliveredAt    = deliveredAt,
                    Note           = note,
                    Items          = items,
                };
            }

            var orders = new List<Order>
            {
                // ── Alice ─────────────────────────────────────────────────
                // Order 1: Delivered, paid via VNPay
                MakeOrder(alice.Id, "ORD-20240301-0001",
                    OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.Momo,
                    "Alice Nguyen", "0901234567",
                    "123 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City",
                    30_000m, 0m, null,
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-28), DateTime.UtcNow.AddDays(-25),
                    null,
                    [
                        MakeItem(p[0], 1, FirstVariant(p[0])),  // iPhone 15 Pro Black/256GB
                        MakeItem(p[13], 1, FirstVariant(p[13])), // Sony WH-1000XM5 Black
                    ]),

                // Order 2: Shipped, COD unpaid
                MakeOrder(alice.Id, "ORD-20240310-0002",
                    OrderStatus.Shipped, PaymentStatus.Unpaid, PaymentMethod.COD,
                    "Alice Nguyen", "0901234567",
                    "123 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City",
                    25_000m, 50_000m, "SAVE50K",
                    null, DateTime.UtcNow.AddDays(-3), null,
                    "Please call before delivery",
                    [
                        MakeItem(p[21], 2, FirstVariant(p[21])), // Women Floral Dress × 2
                        MakeItem(p[25], 1, FirstVariant(p[25])), // Nike Air Max 270
                    ]),

                // Order 3: Pending COD
                MakeOrder(alice.Id, "ORD-20240315-0003",
                    OrderStatus.Pending, PaymentStatus.Unpaid, PaymentMethod.COD,
                    "Alice Nguyen", "0901234567",
                    "123 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City",
                    20_000m, 0m, null,
                    null, null, null, null,
                    [
                        MakeItem(p[34], 1),  // Yoga Mat (no variant)
                        MakeItem(p[40], 2),  // Cetaphil (no variant)
                        MakeItem(p[42], 3),  // Neutrogena SPF50 (no variant)
                    ]),

                // ── Bob ───────────────────────────────────────────────────
                // Order 4: Delivered, bank transfer
                MakeOrder(bob.Id, "ORD-20240205-0004",
                    OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.BankTransfer,
                    "Bob Tran", "0912345678",
                    "45 Nguyen Hue", "Ben Thanh", "District 1", "Ho Chi Minh City",
                    50_000m, 100_000m, "TECH10",
                    DateTime.UtcNow.AddDays(-45), DateTime.UtcNow.AddDays(-42), DateTime.UtcNow.AddDays(-40),
                    null,
                    [
                        MakeItem(p[5], 1, FirstVariant(p[5])),   // MacBook Pro 16GB/512GB
                        MakeItem(p[14], 1, FirstVariant(p[14])), // AirPods Pro White
                    ]),

                // Order 5: Processing, Momo paid
                MakeOrder(bob.Id, "ORD-20240312-0005",
                    OrderStatus.Processing, PaymentStatus.Paid, PaymentMethod.Momo,
                    "Bob Tran", "0912345678",
                    "45 Nguyen Hue", "Ben Thanh", "District 1", "Ho Chi Minh City",
                    30_000m, 0m, null,
                    DateTime.UtcNow.AddDays(-5), null, null,
                    "Gift wrap please",
                    [
                        MakeItem(p[17], 3, FirstVariant(p[17])), // T-Shirt Black/S × 3
                        MakeItem(p[19], 1, FirstVariant(p[19])), // Denim Jacket Blue/S
                        MakeItem(p[26], 1, FirstVariant(p[26])), // Adidas Ultraboost
                    ]),

                // Order 6: Cancelled
                MakeOrder(bob.Id, "ORD-20240208-0006",
                    OrderStatus.Cancelled, PaymentStatus.Refunded, PaymentMethod.Momo,
                    "Bob Tran", "0912345678",
                    "45 Nguyen Hue", "Ben Thanh", "District 1", "Ho Chi Minh City",
                    0m, 0m, null,
                    DateTime.UtcNow.AddDays(-50), null, null,
                    null,
                    [
                        MakeItem(p[10], 1, FirstVariant(p[10])), // iPad Pro Silver/256GB
                    ]),

                // ── Carol ─────────────────────────────────────────────────
                // Order 7: Delivered, full furniture + kitchenware (no variants)
                MakeOrder(carol.Id, "ORD-20240110-0007",
                    OrderStatus.Delivered, PaymentStatus.Paid, PaymentMethod.BankTransfer,
                    "Carol Le", "0923456789",
                    "88 Tran Hung Dao", "Nguyen Cu Trinh", "District 5", "Ho Chi Minh City",
                    200_000m, 0m, null,
                    DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-56), DateTime.UtcNow.AddDays(-52),
                    null,
                    [
                        MakeItem(p[28], 1),  // Ergonomic Chair
                        MakeItem(p[29], 1),  // Minimalist Desk
                        MakeItem(p[31], 1),  // Instant Pot
                    ]),

                // Order 8: Confirmed, mix variant + no-variant
                MakeOrder(carol.Id, "ORD-20240314-0008",
                    OrderStatus.Processing, PaymentStatus.Paid, PaymentMethod.Momo,
                    "Carol Le", "0923456789",
                    "88 Tran Hung Dao", "Nguyen Cu Trinh", "District 5", "Ho Chi Minh City",
                    30_000m, 20_000m, "WELCOME20K",
                    DateTime.UtcNow.AddDays(-2), null, null,
                    null,
                    [
                        MakeItem(p[2],  1, FirstVariant(p[2])),  // Pixel 8 Pro Obsidian/128GB
                        MakeItem(p[22], 2, FirstVariant(p[22])), // Yoga Leggings Black/XS × 2
                        MakeItem(p[35], 1),                       // Yoga Mat (no variant)
                        MakeItem(p[41], 2),                       // The Ordinary Niacinamide × 2
                    ]),

                // Order 9: Pending, outdoor gear
                MakeOrder(carol.Id, "ORD-20240316-0009",
                    OrderStatus.Pending, PaymentStatus.Unpaid, PaymentMethod.COD,
                    "Carol Le", "0923456789",
                    "88 Tran Hung Dao", "Nguyen Cu Trinh", "District 5", "Ho Chi Minh City",
                    50_000m, 0m, null,
                    null, null, null,
                    "Leave at door if not home",
                    [
                        MakeItem(p[37], 1),  // North Face Tent
                        MakeItem(p[38], 2),  // Hydro Flask × 2
                        MakeItem(p[39], 1),  // Osprey Backpack
                    ]),
            };

            await db.Orders.AddRangeAsync(orders);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} orders with {Count2} order items",
                orders.Count, orders.Sum(o => o.Items.Count));

            logger.LogInformation("🎉 Database seeding completed!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // ROLES
    // ─────────────────────────────────────────────────────────────────────
    private static async Task SeedRolesAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Customer", "Manager" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Seeded role: {Role}", role);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // ADMIN USER
    // ─────────────────────────────────────────────────────────────────────
    private static async Task SeedAdminUserAsync(IServiceProvider services, ILogger logger)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        const string email    = "admin@ecommerce.com";
        const string password = "Admin@123";

        if (await userManager.FindByEmailAsync(email) is not null) return;

        var admin = new AppUser
        {
            FullName       = "System Admin",
            UserName       = email,
            Email          = email,
            EmailConfirmed = true,
            IsActive       = true,
            CreatedAt      = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, "Admin");
        logger.LogInformation("Seeded admin: {Email}", email);
    }
}