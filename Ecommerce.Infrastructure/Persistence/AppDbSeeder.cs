
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Persistence
{
    public static class AppDbSeeder
    {


    public static async Task SeedAsync(EcommerceDbContext db, ILogger logger)
    {
        try
        {
            if (db.Categories.Any())
            {
                logger.LogInformation("⏭️  Database already seeded, skipping.");
                return;
            }

            // ── 1. CATEGORIES ────────────────────────────────────────────
            var electronics = new Category { Name = "Electronics",       Slug = "electronics",       Description = "Electronic devices and accessories",    ImageUrl = "https://placehold.co/400x300?text=Electronics",   ParentId = null, IsActive = true, DisplayOrder = 1 };
            var fashion     = new Category { Name = "Fashion",           Slug = "fashion",           Description = "Clothing, shoes, and accessories",       ImageUrl = "https://placehold.co/400x300?text=Fashion",       ParentId = null, IsActive = true, DisplayOrder = 2 };
            var homeGarden  = new Category { Name = "Home & Garden",     Slug = "home-garden",       Description = "Furniture, décor, and garden tools",     ImageUrl = "https://placehold.co/400x300?text=HomeGarden",    ParentId = null, IsActive = true, DisplayOrder = 3 };
            var sports      = new Category { Name = "Sports & Outdoors", Slug = "sports-outdoors",   Description = "Sporting goods and outdoor equipment",   ImageUrl = "https://placehold.co/400x300?text=Sports",        ParentId = null, IsActive = true, DisplayOrder = 4 };
            var beauty      = new Category { Name = "Beauty & Health",   Slug = "beauty-health",     Description = "Skincare, makeup, and wellness products", ImageUrl = "https://placehold.co/400x300?text=Beauty",       ParentId = null, IsActive = true, DisplayOrder = 5 };

            await db.Categories.AddRangeAsync(electronics, fashion, homeGarden, sports, beauty);
            await db.SaveChangesAsync();

            // Sub-categories
            var phones        = new Category { Name = "Phones",            Slug = "phones",            ParentId = electronics.Id, IsActive = true, DisplayOrder = 1 };
            var laptops       = new Category { Name = "Laptops",           Slug = "laptops",           ParentId = electronics.Id, IsActive = true, DisplayOrder = 2 };
            var tablets       = new Category { Name = "Tablets",           Slug = "tablets",           ParentId = electronics.Id, IsActive = true, DisplayOrder = 3 };
            var audio         = new Category { Name = "Audio",             Slug = "audio",             ParentId = electronics.Id, IsActive = true, DisplayOrder = 4 };
            var menClothing   = new Category { Name = "Men's Clothing",    Slug = "mens-clothing",     ParentId = fashion.Id,     IsActive = true, DisplayOrder = 1 };
            var womenClothing = new Category { Name = "Women's Clothing",  Slug = "womens-clothing",   ParentId = fashion.Id,     IsActive = true, DisplayOrder = 2 };
            var shoes         = new Category { Name = "Shoes",             Slug = "shoes",             ParentId = fashion.Id,     IsActive = true, DisplayOrder = 3 };
            var furniture     = new Category { Name = "Furniture",         Slug = "furniture",         ParentId = homeGarden.Id,  IsActive = true, DisplayOrder = 1 };
            var kitchenware   = new Category { Name = "Kitchenware",       Slug = "kitchenware",       ParentId = homeGarden.Id,  IsActive = true, DisplayOrder = 2 };
            var fitness       = new Category { Name = "Fitness",           Slug = "fitness",           ParentId = sports.Id,      IsActive = true, DisplayOrder = 1 };
            var outdoor       = new Category { Name = "Outdoor Gear",      Slug = "outdoor-gear",      ParentId = sports.Id,      IsActive = true, DisplayOrder = 2 };
            var skincare      = new Category { Name = "Skincare",          Slug = "skincare",          ParentId = beauty.Id,      IsActive = true, DisplayOrder = 1 };

            await db.Categories.AddRangeAsync(phones, laptops, tablets, audio, menClothing, womenClothing, shoes, furniture, kitchenware, fitness, outdoor, skincare);
            await db.SaveChangesAsync();

            logger.LogInformation("✅ Seeded Categories");

            // ── 2. PRODUCTS ──────────────────────────────────────────────
            var products = new List<Product>
            {
                // ── PHONES (5) ──
                new() { Name = "iPhone 15 Pro",           Slug = "iphone-15-pro",           Description = "Apple iPhone 15 Pro with A17 Pro chip.",               ShortDescription = "Latest Apple flagship.",           BasePrice = 999.99m,  SalePrice = 949.99m,  SKU = "APPL-IPH15P",    Status = ProductStatus.Active, IsFeatured = true,  CategoryId = phones.Id,        Brand = "Apple",    Weight = 0.187m, Tags = "apple,iphone,5g" },
                new() { Name = "Samsung Galaxy S24",      Slug = "samsung-galaxy-s24",      Description = "Samsung Galaxy S24 with Snapdragon 8 Gen 3.",           ShortDescription = "Samsung flagship 2024.",           BasePrice = 799.99m,  SalePrice = null,     SKU = "SAM-GS24",       Status = ProductStatus.Active, IsFeatured = true,  CategoryId = phones.Id,        Brand = "Samsung",  Weight = 0.167m, Tags = "samsung,android,5g" },
                new() { Name = "Google Pixel 8 Pro",      Slug = "google-pixel-8-pro",      Description = "Google Pixel 8 Pro with Tensor G3 chip and AI features.", ShortDescription = "Best AI camera smartphone.",      BasePrice = 899.99m,  SalePrice = 849.99m,  SKU = "GOOG-PX8P",      Status = ProductStatus.Active, IsFeatured = false, CategoryId = phones.Id,        Brand = "Google",   Weight = 0.213m, Tags = "google,pixel,ai,camera" },
                new() { Name = "OnePlus 12",              Slug = "oneplus-12",               Description = "OnePlus 12 with Snapdragon 8 Gen 3 and Hasselblad camera.", ShortDescription = "Flagship killer 2024.",         BasePrice = 699.99m,  SalePrice = 649.99m,  SKU = "OP-12",          Status = ProductStatus.Active, IsFeatured = false, CategoryId = phones.Id,        Brand = "OnePlus",  Weight = 0.220m, Tags = "oneplus,snapdragon,fast-charge" },
                new() { Name = "Xiaomi 14 Ultra",         Slug = "xiaomi-14-ultra",          Description = "Xiaomi 14 Ultra with Leica optics and Snapdragon 8 Gen 3.", ShortDescription = "Pro photography smartphone.",   BasePrice = 1099.99m, SalePrice = null,     SKU = "XMI-14U",        Status = ProductStatus.Active, IsFeatured = true,  CategoryId = phones.Id,        Brand = "Xiaomi",   Weight = 0.222m, Tags = "xiaomi,leica,camera,5g" },

                // ── LAPTOPS (5) ──
                new() { Name = "MacBook Pro 14\"",        Slug = "macbook-pro-14",           Description = "Apple MacBook Pro 14-inch with M3 chip.",               ShortDescription = "Pro laptop for creators.",         BasePrice = 1999.99m, SalePrice = 1899.99m, SKU = "APPL-MBP14",     Status = ProductStatus.Active, IsFeatured = true,  CategoryId = laptops.Id,       Brand = "Apple",    Weight = 1.55m,  Tags = "apple,macbook,m3" },
                new() { Name = "Dell XPS 15",             Slug = "dell-xps-15",              Description = "Dell XPS 15 with Intel Core i9 and OLED display.",       ShortDescription = "Premium Windows laptop.",          BasePrice = 1799.99m, SalePrice = null,     SKU = "DELL-XPS15",     Status = ProductStatus.Active, IsFeatured = false, CategoryId = laptops.Id,       Brand = "Dell",     Weight = 1.86m,  Tags = "dell,xps,oled,intel" },
                new() { Name = "ASUS ROG Zephyrus G14",  Slug = "asus-rog-zephyrus-g14",    Description = "ASUS ROG Zephyrus G14 gaming laptop with Ryzen 9.",       ShortDescription = "Compact gaming powerhouse.",       BasePrice = 1499.99m, SalePrice = 1399.99m, SKU = "ASUS-ROG-G14",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = laptops.Id,       Brand = "ASUS",     Weight = 1.65m,  Tags = "asus,rog,gaming,ryzen" },
                new() { Name = "Lenovo ThinkPad X1 Carbon", Slug = "lenovo-thinkpad-x1-carbon", Description = "Ultra-light business laptop with Intel Core i7.",   ShortDescription = "Business ultrabook.",              BasePrice = 1599.99m, SalePrice = null,     SKU = "LNV-TPX1C",      Status = ProductStatus.Active, IsFeatured = false, CategoryId = laptops.Id,       Brand = "Lenovo",   Weight = 1.12m,  Tags = "lenovo,thinkpad,business" },
                new() { Name = "HP Spectre x360",         Slug = "hp-spectre-x360",          Description = "HP Spectre x360 2-in-1 laptop with OLED touchscreen.",   ShortDescription = "Premium 2-in-1 laptop.",          BasePrice = 1399.99m, SalePrice = 1299.99m, SKU = "HP-SPCX360",     Status = ProductStatus.Active, IsFeatured = false, CategoryId = laptops.Id,       Brand = "HP",       Weight = 1.36m,  Tags = "hp,spectre,2-in-1,oled" },

                // ── TABLETS (3) ──
                new() { Name = "iPad Pro 12.9\"",         Slug = "ipad-pro-12",              Description = "Apple iPad Pro 12.9-inch with M2 chip and Liquid Retina XDR display.", ShortDescription = "Most powerful iPad.",  BasePrice = 1099.99m, SalePrice = null,     SKU = "APPL-IPDP12",    Status = ProductStatus.Active, IsFeatured = true,  CategoryId = tablets.Id,       Brand = "Apple",    Weight = 0.682m, Tags = "apple,ipad,m2" },
                new() { Name = "Samsung Galaxy Tab S9",   Slug = "samsung-galaxy-tab-s9",    Description = "Samsung Galaxy Tab S9 with Dynamic AMOLED display.",       ShortDescription = "Android flagship tablet.",         BasePrice = 799.99m,  SalePrice = 749.99m,  SKU = "SAM-TABS9",      Status = ProductStatus.Active, IsFeatured = false, CategoryId = tablets.Id,       Brand = "Samsung",  Weight = 0.498m, Tags = "samsung,android,amoled" },
                new() { Name = "Microsoft Surface Pro 9", Slug = "microsoft-surface-pro-9",  Description = "Microsoft Surface Pro 9 with Intel Core i7 and detachable keyboard.", ShortDescription = "Tablet meets laptop.",  BasePrice = 1299.99m, SalePrice = null,     SKU = "MSFT-SFP9",      Status = ProductStatus.Active, IsFeatured = false, CategoryId = tablets.Id,       Brand = "Microsoft", Weight = 0.879m, Tags = "microsoft,surface,windows" },

                // ── AUDIO (4) ──
                new() { Name = "Sony WH-1000XM5",         Slug = "sony-wh-1000xm5",          Description = "Sony WH-1000XM5 wireless noise-canceling headphones.",     ShortDescription = "Best ANC headphones.",             BasePrice = 349.99m,  SalePrice = 279.99m,  SKU = "SNY-WH1000XM5",  Status = ProductStatus.Active, IsFeatured = true,  CategoryId = audio.Id,         Brand = "Sony",     Weight = 0.250m, Tags = "sony,anc,wireless,headphones" },
                new() { Name = "Apple AirPods Pro 2",     Slug = "apple-airpods-pro-2",       Description = "Apple AirPods Pro 2nd generation with H2 chip.",            ShortDescription = "Premium Apple earbuds.",           BasePrice = 249.99m,  SalePrice = null,     SKU = "APPL-APP2",      Status = ProductStatus.Active, IsFeatured = false, CategoryId = audio.Id,         Brand = "Apple",    Weight = 0.051m, Tags = "apple,airpods,anc,earbuds" },
                new() { Name = "Bose QuietComfort 45",    Slug = "bose-quietcomfort-45",      Description = "Bose QC45 wireless headphones with world-class noise cancellation.", ShortDescription = "Iconic Bose comfort.",     BasePrice = 329.99m,  SalePrice = 299.99m,  SKU = "BOSE-QC45",      Status = ProductStatus.Active, IsFeatured = false, CategoryId = audio.Id,         Brand = "Bose",     Weight = 0.238m, Tags = "bose,anc,wireless" },
                new() { Name = "JBL Charge 5",            Slug = "jbl-charge-5",              Description = "JBL Charge 5 portable Bluetooth speaker with powerbank feature.", ShortDescription = "Waterproof Bluetooth speaker.", BasePrice = 179.99m, SalePrice = 149.99m,  SKU = "JBL-CHG5",       Status = ProductStatus.Active, IsFeatured = false, CategoryId = audio.Id,         Brand = "JBL",      Weight = 0.960m, Tags = "jbl,bluetooth,speaker,waterproof" },

                // ── MEN'S CLOTHING (4) ──
                new() { Name = "Classic Men T-Shirt",     Slug = "classic-men-tshirt",        Description = "Comfortable 100% cotton everyday t-shirt.",                ShortDescription = "100% cotton casual tee.",          BasePrice = 29.99m,   SalePrice = 24.99m,   SKU = "FASH-TSH-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = menClothing.Id,   Brand = "BasicWear", Weight = 0.20m, Tags = "tshirt,men,cotton" },
                new() { Name = "Men Slim-Fit Chinos",     Slug = "men-slim-fit-chinos",        Description = "Stretch slim-fit chino pants, wrinkle-resistant.",         ShortDescription = "Smart casual chinos.",             BasePrice = 59.99m,   SalePrice = null,     SKU = "FASH-CHN-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = menClothing.Id,   Brand = "BasicWear", Weight = 0.45m, Tags = "chinos,men,pants,slim" },
                new() { Name = "Men Denim Jacket",        Slug = "men-denim-jacket",           Description = "Classic denim jacket with button closure.",                ShortDescription = "Timeless denim jacket.",           BasePrice = 89.99m,   SalePrice = 74.99m,   SKU = "FASH-DNM-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = menClothing.Id,   Brand = "BasicWear", Weight = 0.85m, Tags = "denim,jacket,men" },
                new() { Name = "Men Formal Oxford Shirt", Slug = "men-formal-oxford-shirt",    Description = "Premium Oxford cotton formal shirt, wrinkle-resistant.",    ShortDescription = "Classic Oxford shirt.",            BasePrice = 49.99m,   SalePrice = null,     SKU = "FASH-OXF-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = menClothing.Id,   Brand = "BasicWear", Weight = 0.30m, Tags = "shirt,men,formal,oxford" },

                // ── WOMEN'S CLOTHING (4) ──
                new() { Name = "Women Floral Dress",      Slug = "women-floral-dress",         Description = "Light chiffon floral summer dress with v-neck.",           ShortDescription = "Elegant summer dress.",            BasePrice = 69.99m,   SalePrice = 54.99m,   SKU = "FASH-WD-001",    Status = ProductStatus.Active, IsFeatured = true,  CategoryId = womenClothing.Id, Brand = "ElegantLine", Weight = 0.35m, Tags = "dress,women,floral,summer" },
                new() { Name = "Women Yoga Leggings",     Slug = "women-yoga-leggings",         Description = "High-waist compression leggings for yoga and gym.",        ShortDescription = "Flexible yoga leggings.",          BasePrice = 44.99m,   SalePrice = 39.99m,   SKU = "FASH-WL-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = womenClothing.Id, Brand = "ActiveGirl",  Weight = 0.25m, Tags = "leggings,women,yoga,activewear" },
                new() { Name = "Women Wool Blazer",       Slug = "women-wool-blazer",           Description = "Tailored single-button wool-blend blazer.",                ShortDescription = "Smart professional blazer.",       BasePrice = 119.99m,  SalePrice = null,     SKU = "FASH-WB-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = womenClothing.Id, Brand = "ElegantLine", Weight = 0.70m, Tags = "blazer,women,wool,formal" },
                new() { Name = "Women Crop Hoodie",       Slug = "women-crop-hoodie",           Description = "Soft fleece crop hoodie for casual everyday wear.",         ShortDescription = "Cozy fleece crop hoodie.",         BasePrice = 54.99m,   SalePrice = 44.99m,   SKU = "FASH-WH-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = womenClothing.Id, Brand = "ActiveGirl",  Weight = 0.40m, Tags = "hoodie,women,crop,fleece" },

                // ── SHOES (3) ──
                new() { Name = "Nike Air Max 270",        Slug = "nike-air-max-270",            Description = "Nike Air Max 270 with large Max Air unit in the heel.",    ShortDescription = "Iconic Air Max cushioning.",       BasePrice = 149.99m,  SalePrice = 129.99m,  SKU = "NIKE-AM270",     Status = ProductStatus.Active, IsFeatured = true,  CategoryId = shoes.Id,         Brand = "Nike",     Weight = 0.31m,  Tags = "nike,airmax,sneakers,running" },
                new() { Name = "Adidas Ultraboost 23",    Slug = "adidas-ultraboost-23",         Description = "Adidas Ultraboost 23 with Boost midsole and Primeknit upper.", ShortDescription = "Premium running shoes.",        BasePrice = 179.99m,  SalePrice = null,     SKU = "ADI-UB23",       Status = ProductStatus.Active, IsFeatured = false, CategoryId = shoes.Id,         Brand = "Adidas",   Weight = 0.33m,  Tags = "adidas,ultraboost,running" },
                new() { Name = "Converse Chuck Taylor",   Slug = "converse-chuck-taylor",        Description = "Classic Converse Chuck Taylor All Star canvas sneaker.",   ShortDescription = "Timeless canvas sneaker.",         BasePrice = 64.99m,   SalePrice = null,     SKU = "CONV-CT-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = shoes.Id,         Brand = "Converse",  Weight = 0.28m, Tags = "converse,chucktaylor,casual" },

                // ── FURNITURE (3) ──
                new() { Name = "Ergonomic Office Chair",  Slug = "ergonomic-office-chair",      Description = "Full mesh ergonomic chair with lumbar support and armrests.", ShortDescription = "Work comfortably all day.",      BasePrice = 299.99m,  SalePrice = 249.99m,  SKU = "FURN-OC-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = furniture.Id,     Brand = "WorkPro",   Weight = 14.0m, Tags = "chair,office,ergonomic,mesh" },
                new() { Name = "Minimalist Desk",         Slug = "minimalist-desk",              Description = "Clean-line 140cm wooden desk with cable management.",       ShortDescription = "Sleek home office desk.",          BasePrice = 349.99m,  SalePrice = null,     SKU = "FURN-DK-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = furniture.Id,     Brand = "WorkPro",   Weight = 28.0m, Tags = "desk,office,wood,minimalist" },
                new() { Name = "3-Seater Sofa",           Slug = "3-seater-sofa",                Description = "Modern fabric 3-seater sofa with solid wood legs.",         ShortDescription = "Comfortable living room sofa.",    BasePrice = 799.99m,  SalePrice = 699.99m,  SKU = "FURN-SF-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = furniture.Id,     Brand = "HomePlus",  Weight = 45.0m, Tags = "sofa,living-room,fabric" },

                // ── KITCHENWARE (3) ──
                new() { Name = "Instant Pot Duo 7-in-1",  Slug = "instant-pot-duo-7in1",        Description = "7-in-1 multi-use programmable pressure cooker.",             ShortDescription = "The ultimate kitchen appliance.",  BasePrice = 99.99m,   SalePrice = 79.99m,   SKU = "KITCH-IP-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = kitchenware.Id,   Brand = "Instant Pot", Weight = 5.4m, Tags = "instant-pot,pressure-cooker,kitchen" },
                new() { Name = "Vitamix E310 Blender",    Slug = "vitamix-e310-blender",         Description = "Vitamix Explorian E310 with 5-year warranty.",              ShortDescription = "Professional-grade blender.",      BasePrice = 349.99m,  SalePrice = null,     SKU = "KITCH-VTX-001",  Status = ProductStatus.Active, IsFeatured = false, CategoryId = kitchenware.Id,   Brand = "Vitamix",   Weight = 4.3m,  Tags = "vitamix,blender,kitchen" },
                new() { Name = "Cast Iron Skillet 12\"",  Slug = "cast-iron-skillet-12",         Description = "Pre-seasoned 12-inch cast iron skillet, oven-safe to 500°F.", ShortDescription = "Classic cast iron skillet.",      BasePrice = 44.99m,   SalePrice = 39.99m,   SKU = "KITCH-CI-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = kitchenware.Id,   Brand = "Lodge",     Weight = 3.6m,  Tags = "cast-iron,skillet,lodge,cooking" },

                // ── FITNESS (3) ──
                new() { Name = "Adjustable Dumbbell Set", Slug = "adjustable-dumbbell-set",      Description = "Adjustable dumbbell set 5–52.5 lbs per dumbbell.",          ShortDescription = "Space-saving dumbbell set.",       BasePrice = 349.99m,  SalePrice = 299.99m,  SKU = "FIT-DB-001",     Status = ProductStatus.Active, IsFeatured = false, CategoryId = fitness.Id,       Brand = "Bowflex",   Weight = 24.0m, Tags = "dumbbell,fitness,home-gym" },
                new() { Name = "Yoga Mat Premium",        Slug = "yoga-mat-premium",             Description = "Extra thick 6mm non-slip TPE yoga mat with carry strap.",   ShortDescription = "Non-slip premium yoga mat.",       BasePrice = 49.99m,   SalePrice = 39.99m,   SKU = "FIT-YM-001",     Status = ProductStatus.Active, IsFeatured = false, CategoryId = fitness.Id,       Brand = "Gaiam",     Weight = 1.2m,  Tags = "yoga,mat,fitness,tpe" },
                new() { Name = "Jump Rope Speed",         Slug = "jump-rope-speed",              Description = "Lightweight aluminum speed jump rope with ball bearings.",   ShortDescription = "Speed training jump rope.",        BasePrice = 24.99m,   SalePrice = null,     SKU = "FIT-JR-001",     Status = ProductStatus.Active, IsFeatured = false, CategoryId = fitness.Id,       Brand = "RogueFit",  Weight = 0.15m, Tags = "jump-rope,cardio,speed" },

                // ── OUTDOOR (3) ──
                new() { Name = "North Face Tent 2-Person", Slug = "north-face-tent-2-person",   Description = "2-person ultralight backpacking tent with rainfly.",        ShortDescription = "Ultralight camping tent.",         BasePrice = 449.99m,  SalePrice = null,     SKU = "OUT-TNT-001",    Status = ProductStatus.Active, IsFeatured = false, CategoryId = outdoor.Id,       Brand = "The North Face", Weight = 1.4m, Tags = "tent,camping,outdoor,ultralight" },
                new() { Name = "Hydro Flask 32 oz",       Slug = "hydro-flask-32oz",             Description = "32 oz wide mouth insulated stainless steel water bottle.",  ShortDescription = "Keep drinks cold 24hrs.",          BasePrice = 49.99m,   SalePrice = null,     SKU = "OUT-HF-001",     Status = ProductStatus.Active, IsFeatured = false, CategoryId = outdoor.Id,       Brand = "Hydro Flask", Weight = 0.36m, Tags = "water-bottle,hydro-flask,insulated" },
                new() { Name = "Osprey Atmos 65 Backpack", Slug = "osprey-atmos-65-backpack",   Description = "Osprey Atmos AG 65L men's backpacking pack.",               ShortDescription = "Top-rated hiking backpack.",       BasePrice = 299.99m,  SalePrice = 269.99m,  SKU = "OUT-BP-001",     Status = ProductStatus.Active, IsFeatured = false, CategoryId = outdoor.Id,       Brand = "Osprey",    Weight = 2.2m,  Tags = "backpack,hiking,osprey,65l" },

                // ── SKINCARE (3) ──
                new() { Name = "Cetaphil Moisturizing Cream", Slug = "cetaphil-moisturizing-cream", Description = "Cetaphil moisturizing cream for dry, sensitive skin 550g.", ShortDescription = "Gentle daily moisturizer.",     BasePrice = 19.99m,   SalePrice = null,     SKU = "SKIN-CTF-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = skincare.Id,      Brand = "Cetaphil", Weight = 0.6m,   Tags = "cetaphil,moisturizer,skincare,sensitive" },
                new() { Name = "The Ordinary Niacinamide",    Slug = "the-ordinary-niacinamide",    Description = "The Ordinary Niacinamide 10% + Zinc 1% serum 30ml.",        ShortDescription = "Pore-minimizing serum.",          BasePrice = 12.99m,   SalePrice = null,     SKU = "SKIN-ORD-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = skincare.Id,      Brand = "The Ordinary", Weight = 0.05m, Tags = "niacinamide,serum,skincare" },
                new() { Name = "Neutrogena SPF 50 Sunscreen", Slug = "neutrogena-spf50-sunscreen",  Description = "Neutrogena Ultra Sheer Dry-Touch SPF 50 sunscreen 88ml.",   ShortDescription = "Lightweight SPF 50 protection.",  BasePrice = 14.99m,   SalePrice = 12.99m,   SKU = "SKIN-NTG-001",   Status = ProductStatus.Active, IsFeatured = false, CategoryId = skincare.Id,      Brand = "Neutrogena", Weight = 0.12m, Tags = "sunscreen,spf50,neutrogena" },
            };

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();

            logger.LogInformation("✅ Seeded {Count} Products", products.Count);

            // ── 3. PRODUCT IMAGES ────────────────────────────────────────
            var images = products.SelectMany(p => new[]
            {
                new ProductImage { ProductId = p.Id, ImageUrl = $"https://placehold.co/800x800?text={Uri.EscapeDataString(p.Name)}",   AltText = p.Name,           IsPrimary = true,  DisplayOrder = 1 },
                new ProductImage { ProductId = p.Id, ImageUrl = $"https://placehold.co/800x800?text={Uri.EscapeDataString(p.Name)}+2", AltText = p.Name + " alt",  IsPrimary = false, DisplayOrder = 2 },
            }).ToList();

            await db.ProductImages.AddRangeAsync(images);
            await db.SaveChangesAsync();

            logger.LogInformation("✅ Seeded ProductImages");

            // ── 4. PRODUCT VARIANTS ──────────────────────────────────────
            var allVariants = new List<ProductVariant>();

            void AddVariants(Product p, IEnumerable<ProductVariant> variants)
            {
                foreach (var v in variants) { v.ProductId = p.Id; allVariants.Add(v); }
            }

            var iphone15      = products[0];
            var galaxyS24     = products[1];
            var pixel8Pro     = products[2];
            var oneplus12     = products[3];
            var xiaomi14Ultra = products[4];
            var macbookPro    = products[5];
            var dellXps       = products[6];
            var rogZephyrus   = products[7];
            var thinkpad      = products[8];
            var hpSpectre     = products[9];
            var ipadPro       = products[10];
            var galaxyTabS9   = products[11];
            var surfacePro    = products[12];
            var sonyWH        = products[13];
            var airpodsPro    = products[14];
            var boseQC        = products[15];
            var jblCharge     = products[16];
            var tshirtMen     = products[17];
            var chinosMen     = products[18];
            var denimJacket   = products[19];
            var oxfordShirt   = products[20];
            var floralDress   = products[21];
            var yogaLeggings  = products[22];
            var woolBlazer    = products[23];
            var cropHoodie    = products[24];
            var nikeAirMax    = products[25];
            var adidasUB      = products[26];
            var converse      = products[27];
            // products[28..39] = furniture, kitchenware, fitness, outdoor, skincare (no variants needed)

            // Phones — Color + Storage
            AddVariants(iphone15, new[]
            {
                new ProductVariant { Name = "Black / 256GB",    SKU = "IPH15P-BLK-256", Price = 999.99m,  Color = "Black Titanium",  Size = "256GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Black / 512GB",    SKU = "IPH15P-BLK-512", Price = 1099.99m, Color = "Black Titanium",  Size = "512GB", IsActive = true, DisplayOrder = 2 },
                new ProductVariant { Name = "White / 256GB",    SKU = "IPH15P-WHT-256", Price = 999.99m,  Color = "White Titanium",  Size = "256GB", IsActive = true, DisplayOrder = 3 },
                new ProductVariant { Name = "Natural / 512GB",  SKU = "IPH15P-NAT-512", Price = 1099.99m, Color = "Natural Titanium",Size = "512GB", IsActive = true, DisplayOrder = 4 },
            });
            AddVariants(galaxyS24, new[]
            {
                new ProductVariant { Name = "Onyx Black / 256GB", SKU = "GS24-BLK-256", Price = 799.99m, Color = "Onyx Black",   Size = "256GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Marble Gray / 256GB",SKU = "GS24-GRY-256", Price = 799.99m, Color = "Marble Gray",  Size = "256GB", IsActive = true, DisplayOrder = 2 },
                new ProductVariant { Name = "Cobalt Violet / 512GB",SKU="GS24-VIO-512", Price = 899.99m, Color = "Cobalt Violet",Size = "512GB", IsActive = true, DisplayOrder = 3 },
            });
            AddVariants(pixel8Pro, new[]
            {
                new ProductVariant { Name = "Obsidian / 128GB", SKU = "PX8P-OBS-128", Price = 899.99m,  Color = "Obsidian", Size = "128GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Porcelain / 256GB",SKU = "PX8P-POR-256", Price = 999.99m,  Color = "Porcelain",Size = "256GB", IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(oneplus12, new[]
            {
                new ProductVariant { Name = "Flowy Emerald / 256GB", SKU = "OP12-GRN-256", Price = 699.99m, Color = "Flowy Emerald", Size = "256GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Silky Black / 512GB",   SKU = "OP12-BLK-512", Price = 799.99m, Color = "Silky Black",   Size = "512GB", IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(xiaomi14Ultra, new[]
            {
                new ProductVariant { Name = "White / 512GB",  SKU = "XMI14U-WHT-512", Price = 1099.99m, Color = "White",  Size = "512GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Black / 512GB",  SKU = "XMI14U-BLK-512", Price = 1099.99m, Color = "Black",  Size = "512GB", IsActive = true, DisplayOrder = 2 },
                new ProductVariant { Name = "Black / 1TB",    SKU = "XMI14U-BLK-1T",  Price = 1299.99m, Color = "Black",  Size = "1TB",   IsActive = true, DisplayOrder = 3 },
            });

            // Laptops — RAM + Storage
            AddVariants(macbookPro, new[]
            {
                new ProductVariant { Name = "16GB / 512GB", SKU = "MBP14-16-512", Price = 1999.99m, SalePrice = 1899.99m, Material = "16GB RAM", Size = "512GB SSD", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "32GB / 1TB",   SKU = "MBP14-32-1T",  Price = 2499.99m, Material = "32GB RAM", Size = "1TB SSD",   IsActive = true, DisplayOrder = 2 },
                new ProductVariant { Name = "36GB / 2TB",   SKU = "MBP14-36-2T",  Price = 3199.99m, Material = "36GB RAM", Size = "2TB SSD",   IsActive = true, DisplayOrder = 3 },
            });
            AddVariants(dellXps, new[]
            {
                new ProductVariant { Name = "16GB / 512GB", SKU = "XPS15-16-512", Price = 1799.99m, Material = "16GB RAM", Size = "512GB SSD", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "32GB / 1TB",   SKU = "XPS15-32-1T",  Price = 2199.99m, Material = "32GB RAM", Size = "1TB SSD",   IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(rogZephyrus, new[]
            {
                new ProductVariant { Name = "16GB / 512GB", SKU = "ROGG14-16-512", Price = 1499.99m, SalePrice = 1399.99m, Material = "16GB RAM", Size = "512GB SSD", Color = "Eclipse Gray", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "32GB / 1TB",   SKU = "ROGG14-32-1T",  Price = 1799.99m, Material = "32GB RAM", Size = "1TB SSD",   Color = "Moonlight White", IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(thinkpad, new[]
            {
                new ProductVariant { Name = "16GB / 512GB", SKU = "TPX1C-16-512", Price = 1599.99m, Material = "16GB RAM", Size = "512GB SSD", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "32GB / 1TB",   SKU = "TPX1C-32-1T",  Price = 1999.99m, Material = "32GB RAM", Size = "1TB SSD",   IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(hpSpectre, new[]
            {
                new ProductVariant { Name = "16GB / 512GB / Nightfall Black", SKU = "HPX360-16-512-BLK", Price = 1399.99m, SalePrice = 1299.99m, Color = "Nightfall Black", Material = "16GB RAM", Size = "512GB SSD", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "16GB / 1TB / Poseidon Blue",     SKU = "HPX360-16-1T-BLU",  Price = 1599.99m, Color = "Poseidon Blue",  Material = "16GB RAM", Size = "1TB SSD",   IsActive = true, DisplayOrder = 2 },
            });

            // Tablets — Storage + Color
            AddVariants(ipadPro, new[]
            {
                new ProductVariant { Name = "Silver / 256GB", SKU = "IPDP12-SLV-256", Price = 1099.99m, Color = "Silver",      Size = "256GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Space Gray / 512GB", SKU = "IPDP12-SPG-512", Price = 1299.99m, Color = "Space Gray", Size = "512GB", IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(galaxyTabS9, new[]
            {
                new ProductVariant { Name = "Beige / 128GB",  SKU = "TABS9-BEI-128", Price = 799.99m, SalePrice = 749.99m, Color = "Beige",     Size = "128GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Graphite / 256GB",SKU = "TABS9-GRF-256",Price = 949.99m, Color = "Graphite",   Size = "256GB", IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(surfacePro, new[]
            {
                new ProductVariant { Name = "Platinum / 256GB", SKU = "SFP9-PLT-256", Price = 1299.99m, Color = "Platinum",    Size = "256GB", IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Sapphire / 512GB", SKU = "SFP9-SAP-512", Price = 1599.99m, Color = "Sapphire",    Size = "512GB", IsActive = true, DisplayOrder = 2 },
            });

            // Audio — Color only
            AddVariants(sonyWH, new[]
            {
                new ProductVariant { Name = "Black",  SKU = "WH1000XM5-BLK", Price = 349.99m, SalePrice = 279.99m, Color = "Black",  IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Silver", SKU = "WH1000XM5-SLV", Price = 349.99m, SalePrice = 279.99m, Color = "Silver", IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(airpodsPro, new[]
            {
                new ProductVariant { Name = "White (USB-C)", SKU = "APP2-WHT", Price = 249.99m, Color = "White", IsActive = true, DisplayOrder = 1 },
            });
            AddVariants(boseQC, new[]
            {
                new ProductVariant { Name = "Black",        SKU = "QC45-BLK",  Price = 329.99m, SalePrice = 299.99m, Color = "Black",        IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "White Smoke",  SKU = "QC45-WHT",  Price = 329.99m, SalePrice = 299.99m, Color = "White Smoke",  IsActive = true, DisplayOrder = 2 },
            });
            AddVariants(jblCharge, new[]
            {
                new ProductVariant { Name = "Black",  SKU = "JBL-CHG5-BLK", Price = 179.99m, SalePrice = 149.99m, Color = "Black",  IsActive = true, DisplayOrder = 1 },
                new ProductVariant { Name = "Blue",   SKU = "JBL-CHG5-BLU", Price = 179.99m, SalePrice = 149.99m, Color = "Blue",   IsActive = true, DisplayOrder = 2 },
                new ProductVariant { Name = "Red",    SKU = "JBL-CHG5-RED", Price = 179.99m, SalePrice = 149.99m, Color = "Red",    IsActive = true, DisplayOrder = 3 },
            });

            // Fashion — Color + Size
            string[] shirtSizes  = ["S", "M", "L", "XL"];
            string[] pantsWaists = ["W30", "W32", "W34", "W36"];

            // Helper: dùng index C0/C1/C2 để tránh duplicate SKU khi color có prefix giống nhau
            static void AddColorSizeVariants(
                List<ProductVariant> list, Product product,
                string prefix, string[] colorList, string[] sizeList,
                decimal price, decimal? salePrice = null, string? material = null)
            {
                for (int ci = 0; ci < colorList.Length; ci++)
                    for (int si = 0; si < sizeList.Length; si++)
                        list.Add(new ProductVariant
                        {
                            ProductId    = product.Id,
                            Name         = $"{colorList[ci]} / {sizeList[si]}",
                            SKU          = $"{prefix}-C{ci}-{sizeList[si]}",   // ← C0/C1/C2 unique
                            Price        = price,
                            SalePrice    = salePrice,
                            Color        = colorList[ci],
                            Size         = sizeList[si],
                            Material     = material,
                            IsActive     = true,
                            DisplayOrder = si + 1
                        });
            }

            // Men's Clothing
            AddColorSizeVariants(allVariants, tshirtMen,   "TSH",  ["Black", "White", "Navy"],         shirtSizes,  29.99m, 24.99m, "Cotton");
            AddColorSizeVariants(allVariants, denimJacket, "DNM",  ["Blue", "Black", "Light Blue"],     shirtSizes,  89.99m, 74.99m);
            AddColorSizeVariants(allVariants, oxfordShirt, "OXF",  ["White", "Light Blue", "Pink"],     shirtSizes,  49.99m);

            // Chinos — Color × Waist
            AddColorSizeVariants(allVariants, chinosMen,   "CHN",  ["Khaki", "Navy", "Olive"],          pantsWaists, 59.99m, material: "Stretch Cotton");

            // Women's Clothing
            AddColorSizeVariants(allVariants, floralDress,   "WD",  ["Floral Blue", "Floral Pink", "Floral Yellow"], ["XS", "S", "M", "L"],       69.99m, 54.99m);
            AddColorSizeVariants(allVariants, yogaLeggings,  "WL",  ["Black", "Gray", "Navy"],                       ["XS", "S", "M", "L", "XL"], 44.99m, 39.99m, "Stretch Fabric");
            AddColorSizeVariants(allVariants, woolBlazer,    "WB",  ["Black", "Camel", "Navy"],                      ["XS", "S", "M", "L"],       119.99m, material: "Wool Blend");
            AddColorSizeVariants(allVariants, cropHoodie,    "WH",  ["Pink", "Gray", "Lavender"],                    ["XS", "S", "M", "L"],       54.99m, 44.99m, "Fleece");

            // Shoes — Color × US Size
            string[] usSizes = ["7", "8", "9", "10", "11", "12"];
            AddColorSizeVariants(allVariants, nikeAirMax, "AM270", ["White/Black", "Black/Red"],         usSizes, 149.99m, 129.99m);
            AddColorSizeVariants(allVariants, adidasUB,   "UB23",  ["Core Black", "Cloud White"],        usSizes, 179.99m);
            AddColorSizeVariants(allVariants, converse,   "CT",    ["Classic White", "Classic Black", "Navy"], usSizes, 64.99m);

            await db.ProductVariants.AddRangeAsync(allVariants);
            await db.SaveChangesAsync();

            logger.LogInformation("✅ Seeded {Count} ProductVariants", allVariants.Count);

            // ── 5. INVENTORIES ───────────────────────────────────────────
            var inventories = new List<Inventory>();

            // Products with variants → one Inventory per variant
            foreach (var variant in allVariants)
            {
                var qty = Random.Shared.Next(5, 120);
                var reserved = Random.Shared.Next(0, Math.Max(1, qty / 5));
                inventories.Add(new Inventory
                {
                    ProductId         = variant.ProductId,
                    ProductVariantId  = variant.Id,
                    Quantity          = qty,
                    ReservedQuantity  = reserved,
                    LowStockThreshold = 10,
                    WarehouseLocation = $"WH-{(char)('A' + Random.Shared.Next(0, 5))}{Random.Shared.Next(1, 10):D2}",
                    LastStockUpdate   = DateTime.UtcNow
                });
            }

            // Products WITHOUT variants (furniture, kitchenware, fitness, outdoor, skincare)
            // products index 28..39
            var noVariantProducts = products.Skip(28).ToList();
            foreach (var p in noVariantProducts)
            {
                var qty = Random.Shared.Next(5, 60);
                inventories.Add(new Inventory
                {
                    ProductId         = p.Id,
                    ProductVariantId  = null,
                    Quantity          = qty,
                    ReservedQuantity  = Random.Shared.Next(0, Math.Max(1, qty / 5)),
                    LowStockThreshold = 5,
                    WarehouseLocation = $"WH-{(char)('A' + Random.Shared.Next(0, 5))}{Random.Shared.Next(1, 10):D2}",
                    LastStockUpdate   = DateTime.UtcNow
                });
            }

            await db.Inventories.AddRangeAsync(inventories);
            await db.SaveChangesAsync();

            logger.LogInformation("✅ Seeded {Count} Inventories", inventories.Count);
            logger.LogInformation("🎉 Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during database seeding");
            throw;
        }
    }




    }
}