using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Persistence;

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

            // ── 1. ROOT CATEGORIES ────────────────────────────────────────
            var electronics = new Category { Name = "Electronics",       Slug = "electronics",     IsActive = true, DisplayOrder = 1 };
            var fashion     = new Category { Name = "Fashion",           Slug = "fashion",         IsActive = true, DisplayOrder = 2 };
            var homeGarden  = new Category { Name = "Home & Garden",     Slug = "home-garden",     IsActive = true, DisplayOrder = 3 };
            var sports      = new Category { Name = "Sports & Outdoors", Slug = "sports-outdoors", IsActive = true, DisplayOrder = 4 };
            var beauty      = new Category { Name = "Beauty & Health",   Slug = "beauty-health",   IsActive = true, DisplayOrder = 5 };

            await db.Categories.AddRangeAsync(electronics, fashion, homeGarden, sports, beauty);
            await db.SaveChangesAsync();

            // ── 2. SUB CATEGORIES ─────────────────────────────────────────
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
                fitness, outdoor,
                skincare);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded categories");

            // ── 3. PRODUCTS ───────────────────────────────────────────────
            // Rule: ALL products below have variants → Inventory per variant only
            var products = new List<Product>
            {
                // PHONES
                new() { Name="iPhone 15 Pro",            Slug="iphone-15-pro",            SKU="APPL-IPH15P",  BasePrice=999.99m,  SalePrice=949.99m,  Status=ProductStatus.Active, IsFeatured=true,  CategoryId=phones.Id,        Brand="Apple",      Tags="apple,iphone,5g" },
                new() { Name="Samsung Galaxy S24",       Slug="samsung-galaxy-s24",       SKU="SAM-GS24",     BasePrice=799.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=true,  CategoryId=phones.Id,        Brand="Samsung",    Tags="samsung,android,5g" },
                new() { Name="Google Pixel 8 Pro",       Slug="google-pixel-8-pro",       SKU="GOOG-PX8P",    BasePrice=899.99m,  SalePrice=849.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=phones.Id,        Brand="Google",     Tags="google,pixel,ai" },
                new() { Name="OnePlus 12",               Slug="oneplus-12",               SKU="OP-12",        BasePrice=699.99m,  SalePrice=649.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=phones.Id,        Brand="OnePlus",    Tags="oneplus,5g" },
                new() { Name="Xiaomi 14 Ultra",          Slug="xiaomi-14-ultra",          SKU="XMI-14U",      BasePrice=1099.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=true,  CategoryId=phones.Id,        Brand="Xiaomi",     Tags="xiaomi,leica" },
                // LAPTOPS
                new() { Name="MacBook Pro 14\"",         Slug="macbook-pro-14",           SKU="APPL-MBP14",   BasePrice=1999.99m, SalePrice=1899.99m, Status=ProductStatus.Active, IsFeatured=true,  CategoryId=laptops.Id,       Brand="Apple",      Tags="apple,macbook,m3" },
                new() { Name="Dell XPS 15",              Slug="dell-xps-15",              SKU="DELL-XPS15",   BasePrice=1799.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="Dell",       Tags="dell,oled" },
                new() { Name="ASUS ROG Zephyrus G14",   Slug="asus-rog-zephyrus-g14",    SKU="ASUS-ROG-G14", BasePrice=1499.99m, SalePrice=1399.99m, Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="ASUS",       Tags="asus,gaming" },
                new() { Name="Lenovo ThinkPad X1",      Slug="lenovo-thinkpad-x1",       SKU="LNV-TPX1C",    BasePrice=1599.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="Lenovo",     Tags="lenovo,business" },
                new() { Name="HP Spectre x360",         Slug="hp-spectre-x360",          SKU="HP-SPCX360",   BasePrice=1399.99m, SalePrice=1299.99m, Status=ProductStatus.Active, IsFeatured=false, CategoryId=laptops.Id,       Brand="HP",         Tags="hp,2-in-1" },
                // TABLETS
                new() { Name="iPad Pro 12.9\"",         Slug="ipad-pro-12",              SKU="APPL-IPDP12",  BasePrice=1099.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=true,  CategoryId=tablets.Id,       Brand="Apple",      Tags="apple,ipad,m2" },
                new() { Name="Samsung Galaxy Tab S9",   Slug="samsung-galaxy-tab-s9",    SKU="SAM-TABS9",    BasePrice=799.99m,  SalePrice=749.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=tablets.Id,       Brand="Samsung",    Tags="samsung,android" },
                new() { Name="Microsoft Surface Pro 9", Slug="microsoft-surface-pro-9",  SKU="MSFT-SFP9",    BasePrice=1299.99m, SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=tablets.Id,       Brand="Microsoft",  Tags="microsoft,windows" },
                // AUDIO
                new() { Name="Sony WH-1000XM5",         Slug="sony-wh-1000xm5",          SKU="SNY-WH1000XM5",BasePrice=349.99m,  SalePrice=279.99m,  Status=ProductStatus.Active, IsFeatured=true,  CategoryId=audio.Id,         Brand="Sony",       Tags="sony,anc" },
                new() { Name="Apple AirPods Pro 2",     Slug="apple-airpods-pro-2",      SKU="APPL-APP2",    BasePrice=249.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=audio.Id,         Brand="Apple",      Tags="apple,airpods" },
                new() { Name="Bose QuietComfort 45",    Slug="bose-quietcomfort-45",     SKU="BOSE-QC45",    BasePrice=329.99m,  SalePrice=299.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=audio.Id,         Brand="Bose",       Tags="bose,anc" },
                new() { Name="JBL Charge 5",            Slug="jbl-charge-5",             SKU="JBL-CHG5",     BasePrice=179.99m,  SalePrice=149.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=audio.Id,         Brand="JBL",        Tags="jbl,bluetooth" },
                // MEN CLOTHING
                new() { Name="Classic Men T-Shirt",     Slug="classic-men-tshirt",       SKU="FASH-TSH-001", BasePrice=29.99m,   SalePrice=24.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",  Tags="tshirt,men" },
                new() { Name="Men Slim-Fit Chinos",     Slug="men-slim-fit-chinos",       SKU="FASH-CHN-001", BasePrice=59.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",  Tags="chinos,men" },
                new() { Name="Men Denim Jacket",        Slug="men-denim-jacket",          SKU="FASH-DNM-001", BasePrice=89.99m,   SalePrice=74.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",  Tags="denim,men" },
                new() { Name="Men Formal Oxford Shirt", Slug="men-formal-oxford-shirt",   SKU="FASH-OXF-001", BasePrice=49.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=menClothing.Id,   Brand="BasicWear",  Tags="shirt,men" },
                // WOMEN CLOTHING
                new() { Name="Women Floral Dress",      Slug="women-floral-dress",        SKU="FASH-WD-001",  BasePrice=69.99m,   SalePrice=54.99m,   Status=ProductStatus.Active, IsFeatured=true,  CategoryId=womenClothing.Id, Brand="ElegantLine",Tags="dress,women" },
                new() { Name="Women Yoga Leggings",     Slug="women-yoga-leggings",       SKU="FASH-WL-001",  BasePrice=44.99m,   SalePrice=39.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=womenClothing.Id, Brand="ActiveGirl", Tags="leggings,women" },
                new() { Name="Women Wool Blazer",       Slug="women-wool-blazer",         SKU="FASH-WB-001",  BasePrice=119.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=womenClothing.Id, Brand="ElegantLine",Tags="blazer,women" },
                new() { Name="Women Crop Hoodie",       Slug="women-crop-hoodie",         SKU="FASH-WH-001",  BasePrice=54.99m,   SalePrice=44.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=womenClothing.Id, Brand="ActiveGirl", Tags="hoodie,women" },
                // SHOES
                new() { Name="Nike Air Max 270",        Slug="nike-air-max-270",          SKU="NIKE-AM270",   BasePrice=149.99m,  SalePrice=129.99m,  Status=ProductStatus.Active, IsFeatured=true,  CategoryId=shoes.Id,         Brand="Nike",       Tags="nike,airmax" },
                new() { Name="Adidas Ultraboost 23",    Slug="adidas-ultraboost-23",      SKU="ADI-UB23",     BasePrice=179.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=shoes.Id,         Brand="Adidas",     Tags="adidas,running" },
                new() { Name="Converse Chuck Taylor",   Slug="converse-chuck-taylor",     SKU="CONV-CT-001",  BasePrice=64.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=shoes.Id,         Brand="Converse",   Tags="converse,casual" },
                // FURNITURE — no variants → inventory per product
                new() { Name="Ergonomic Office Chair",  Slug="ergonomic-office-chair",    SKU="FURN-OC-001",  BasePrice=299.99m,  SalePrice=249.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=furniture.Id,     Brand="WorkPro",    Tags="chair,ergonomic" },
                new() { Name="Minimalist Desk",         Slug="minimalist-desk",           SKU="FURN-DK-001",  BasePrice=349.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=furniture.Id,     Brand="WorkPro",    Tags="desk,office" },
                new() { Name="3-Seater Sofa",           Slug="3-seater-sofa",             SKU="FURN-SF-001",  BasePrice=799.99m,  SalePrice=699.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=furniture.Id,     Brand="HomePlus",   Tags="sofa,living-room" },
                // KITCHENWARE
                new() { Name="Instant Pot Duo 7-in-1",  Slug="instant-pot-duo-7in1",     SKU="KITCH-IP-001", BasePrice=99.99m,   SalePrice=79.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=kitchenware.Id,   Brand="Instant Pot",Tags="instant-pot" },
                new() { Name="Vitamix E310 Blender",    Slug="vitamix-e310-blender",      SKU="KITCH-VTX-001",BasePrice=349.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=kitchenware.Id,   Brand="Vitamix",    Tags="vitamix,blender" },
                new() { Name="Cast Iron Skillet 12\"",  Slug="cast-iron-skillet-12",      SKU="KITCH-CI-001", BasePrice=44.99m,   SalePrice=39.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=kitchenware.Id,   Brand="Lodge",      Tags="cast-iron,skillet" },
                // FITNESS
                new() { Name="Adjustable Dumbbell Set", Slug="adjustable-dumbbell-set",   SKU="FIT-DB-001",   BasePrice=349.99m,  SalePrice=299.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=fitness.Id,       Brand="Bowflex",    Tags="dumbbell,fitness" },
                new() { Name="Yoga Mat Premium",        Slug="yoga-mat-premium",          SKU="FIT-YM-001",   BasePrice=49.99m,   SalePrice=39.99m,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=fitness.Id,       Brand="Gaiam",      Tags="yoga,mat" },
                new() { Name="Jump Rope Speed",         Slug="jump-rope-speed",           SKU="FIT-JR-001",   BasePrice=24.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=fitness.Id,       Brand="RogueFit",   Tags="jump-rope,cardio" },
                // OUTDOOR
                new() { Name="North Face Tent 2P",      Slug="north-face-tent-2p",        SKU="OUT-TNT-001",  BasePrice=449.99m,  SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=outdoor.Id,       Brand="The North Face", Tags="tent,camping" },
                new() { Name="Hydro Flask 32oz",        Slug="hydro-flask-32oz",          SKU="OUT-HF-001",   BasePrice=49.99m,   SalePrice=null,     Status=ProductStatus.Active, IsFeatured=false, CategoryId=outdoor.Id,       Brand="Hydro Flask",Tags="water-bottle" },
                new() { Name="Osprey Atmos 65L",        Slug="osprey-atmos-65l",          SKU="OUT-BP-001",   BasePrice=299.99m,  SalePrice=269.99m,  Status=ProductStatus.Active, IsFeatured=false, CategoryId=outdoor.Id,       Brand="Osprey",     Tags="backpack,hiking" },
                // SKINCARE
                new() { Name="Cetaphil Moisturizing Cream", Slug="cetaphil-moisturizing-cream", SKU="SKIN-CTF-001", BasePrice=19.99m, SalePrice=null,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=skincare.Id,    Brand="Cetaphil",   Tags="moisturizer,skincare" },
                new() { Name="The Ordinary Niacinamide",    Slug="the-ordinary-niacinamide",    SKU="SKIN-ORD-001", BasePrice=12.99m, SalePrice=null,   Status=ProductStatus.Active, IsFeatured=false, CategoryId=skincare.Id,    Brand="The Ordinary",Tags="niacinamide,serum" },
                new() { Name="Neutrogena SPF 50",           Slug="neutrogena-spf50",            SKU="SKIN-NTG-001", BasePrice=14.99m, SalePrice=12.99m, Status=ProductStatus.Active, IsFeatured=false, CategoryId=skincare.Id,    Brand="Neutrogena", Tags="sunscreen,spf50" },
            };

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} products", products.Count);

            // ── 4. PRODUCT IMAGES ─────────────────────────────────────────
            var images = products.SelectMany(p => new[]
            {
                new ProductImage { ProductId = p.Id, ImageUrl = $"https://placehold.co/800x800?text={Uri.EscapeDataString(p.Name)}",    IsPrimary = true,  DisplayOrder = 1 },
                new ProductImage { ProductId = p.Id, ImageUrl = $"https://placehold.co/800x800?text={Uri.EscapeDataString(p.Name)}+2",  IsPrimary = false, DisplayOrder = 2 },
            });
            await db.ProductImages.AddRangeAsync(images);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded product images");

            // ── 5. VARIANTS ───────────────────────────────────────────────
            // Index mapping for readability
            var p = products;
            var allVariants = new List<ProductVariant>();

            // Helper: color×size
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
                            Size         = sizes[si],
                            Material     = material,
                            IsActive     = true,
                            DisplayOrder = si + 1,
                        });
            }

            // Helper: color only
            void AddC(int productId, string prefix, string[] colors, decimal price, decimal? sale = null)
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
                        IsActive     = true,
                        DisplayOrder = ci + 1,
                    });
            }

            // Helper: storage only (phones/tablets/laptops)
            void AddStorage(int productId, string prefix, (string label, decimal price, decimal? sale)[] configs)
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

            // ── PHONES ──
            // iPhone 15 Pro [0]: color × storage
            AddCS(p[0].Id,  "IPH15P",   ["Black Titanium","White Titanium","Natural Titanium"], ["256GB","512GB"],  999.99m, 949.99m);
            // Galaxy S24 [1]
            AddCS(p[1].Id,  "GS24",     ["Onyx Black","Marble Gray","Cobalt Violet"],           ["256GB","512GB"],  799.99m);
            // Pixel 8 Pro [2]
            AddCS(p[2].Id,  "PX8P",     ["Obsidian","Porcelain"],                               ["128GB","256GB"],  899.99m, 849.99m);
            // OnePlus 12 [3]
            AddCS(p[3].Id,  "OP12",     ["Flowy Emerald","Silky Black"],                        ["256GB","512GB"],  699.99m, 649.99m);
            // Xiaomi 14 Ultra [4]
            AddCS(p[4].Id,  "XMI14U",   ["White","Black"],                                      ["512GB","1TB"],   1099.99m);

            // ── LAPTOPS ──
            // MacBook Pro [5]: RAM+Storage
            AddStorage(p[5].Id, "MBP14", [("16GB/512GB",1999.99m,1899.99m),("32GB/1TB",2499.99m,null),("36GB/2TB",3199.99m,null)]);
            // Dell XPS [6]
            AddStorage(p[6].Id, "XPS15", [("16GB/512GB",1799.99m,null),("32GB/1TB",2199.99m,null)]);
            // ROG Zephyrus [7]
            AddStorage(p[7].Id, "ROGG14",[("16GB/512GB",1499.99m,1399.99m),("32GB/1TB",1799.99m,null)]);
            // ThinkPad [8]
            AddStorage(p[8].Id, "TPX1C", [("16GB/512GB",1599.99m,null),("32GB/1TB",1999.99m,null)]);
            // HP Spectre [9]
            AddStorage(p[9].Id, "HPX360",[("16GB/512GB",1399.99m,1299.99m),("16GB/1TB",1599.99m,null)]);

            // ── TABLETS ──
            AddCS(p[10].Id, "IPDP12",   ["Silver","Space Gray"],     ["256GB","512GB"], 1099.99m);
            AddCS(p[11].Id, "TABS9",    ["Beige","Graphite"],         ["128GB","256GB"],  799.99m, 749.99m);
            AddCS(p[12].Id, "SFP9",     ["Platinum","Sapphire"],      ["256GB","512GB"], 1299.99m);

            // ── AUDIO ──
            AddC(p[13].Id, "WH1000XM5", ["Black","Silver"],                349.99m, 279.99m);
            AddC(p[14].Id, "APP2",      ["White"],                          249.99m);
            AddC(p[15].Id, "QC45",      ["Black","White Smoke"],             329.99m, 299.99m);
            AddC(p[16].Id, "JBL5",      ["Black","Blue","Red"],              179.99m, 149.99m);

            // ── MEN CLOTHING ──
            string[] clothSizes  = ["S","M","L","XL"];
            string[] pantsWaists = ["W30","W32","W34","W36"];
            AddCS(p[17].Id, "TSH",  ["Black","White","Navy"],        clothSizes,   29.99m, 24.99m, "Cotton");
            AddCS(p[18].Id, "CHN",  ["Khaki","Navy","Olive"],        pantsWaists,  59.99m, material:"Stretch Cotton");
            AddCS(p[19].Id, "DNM",  ["Blue","Black","Light Blue"],   clothSizes,   89.99m, 74.99m);
            AddCS(p[20].Id, "OXF",  ["White","Light Blue","Pink"],   clothSizes,   49.99m);

            // ── WOMEN CLOTHING ──
            string[] wSizes = ["XS","S","M","L"];
            AddCS(p[21].Id, "WD",   ["Floral Blue","Floral Pink"],   wSizes,       69.99m, 54.99m);
            AddCS(p[22].Id, "WL",   ["Black","Gray","Navy"],         [..wSizes,"XL"], 44.99m, 39.99m, "Stretch Fabric");
            AddCS(p[23].Id, "WB",   ["Black","Camel","Navy"],        wSizes,      119.99m, material:"Wool Blend");
            AddCS(p[24].Id, "WH",   ["Pink","Gray","Lavender"],      wSizes,       54.99m, 44.99m, "Fleece");

            // ── SHOES ──
            string[] shoeSizes = ["7","8","9","10","11","12"];
            AddCS(p[25].Id, "AM270", ["White/Black","Black/Red"],    shoeSizes,   149.99m, 129.99m);
            AddCS(p[26].Id, "UB23",  ["Core Black","Cloud White"],   shoeSizes,   179.99m);
            AddCS(p[27].Id, "CT",    ["White","Black","Navy"],        shoeSizes,    64.99m);

            // ── FURNITURE, KITCHENWARE, FITNESS, OUTDOOR, SKINCARE ────────
            // No variants — inventory will be per product (ProductVariantId = null)
            // p[28..42] → no variants added

            await db.ProductVariants.AddRangeAsync(allVariants);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} variants", allVariants.Count);

            // ── 6. INVENTORIES ─────────────────────────────────────────────
            // Rule: if product has variants → inventory per variant (VariantId NOT null)
            //       if product has no variants → inventory per product (VariantId null)
            var inventories = new List<Inventory>();
            var rng = Random.Shared;

            // Products WITH variants (index 0–27)
            foreach (var v in allVariants)
            {
                var qty      = rng.Next(10, 150);
                var reserved = rng.Next(0, Math.Max(1, qty / 5));
                inventories.Add(new Inventory
                {
                    ProductId         = v.ProductId,
                    ProductVariantId  = v.Id,          // ← variant-level
                    Quantity          = qty,
                    ReservedQuantity  = reserved,
                    LowStockThreshold = 10,
                    WarehouseLocation = $"WH-{(char)('A' + rng.Next(0, 5))}{rng.Next(1, 10):D2}",
                    LastStockUpdate   = DateTime.UtcNow,
                });
            }

            // Products WITHOUT variants (index 28–42: furniture, kitchen, fitness, outdoor, skincare)
            foreach (var prod in products.Skip(28))
            {
                var qty      = rng.Next(5, 60);
                var reserved = rng.Next(0, Math.Max(1, qty / 5));
                inventories.Add(new Inventory
                {
                    ProductId         = prod.Id,
                    ProductVariantId  = null,           // ← product-level
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
            logger.LogInformation("Database seeding completed!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    // ─────────────────────────────────────────────
    // ROLES
    // ─────────────────────────────────────────────
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

    // ─────────────────────────────────────────────
    // ADMIN USER
    // ─────────────────────────────────────────────
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