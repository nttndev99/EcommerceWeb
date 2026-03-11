
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
          var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                if (await db.Categories.IgnoreQueryFilters().AnyAsync())
                {
                    logger.LogInformation("Database already seeded — skipping.");
                    return;
                }

                logger.LogInformation("Seeding database …");

                await using var tx = await db.Database.BeginTransactionAsync();

                try
                {
                    var cats = await SeedCategoriesAsync(db);
                    var prods = await SeedProductsAsync(db, cats);

                    await SeedImagesAsync(db, prods);
                    await SeedVariantsAsync(db, prods);

                    await db.SaveChangesAsync();

                    await SeedInventoryAsync(db, prods);

                    await db.SaveChangesAsync();

                    await tx.CommitAsync();

                    logger.LogInformation("Seeding complete.");
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    logger.LogError(ex, "Seeding failed.");
                    throw;
                }
            });
        }

        // CATEGORIES ────────────────────────────────────────────────────────────────

        private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(EcommerceDbContext db)
        {
            var t = DateTime.UtcNow;
            var electronics = C("Electronics", "electronics", null, "Consumer electronics & gadgets", 1, t);
            var fashion = C("Fashion", "fashion", null, "Clothing, footwear & accessories", 2, t);
            var home = C("Home & Garden", "home-garden", null, "Furniture, décor & garden", 3, t);
            var sports = C("Sports", "sports", null, "Equipment, apparel & outdoor", 4, t);
            var beauty = C("Beauty", "beauty", null, "Skincare, makeup & personal care", 5, t);
            var gaming = C("Gaming", "gaming", null, "Consoles, games & accessories", 6, t);
            db.Categories.AddRange(electronics, fashion, home, sports, beauty, gaming);
            await db.SaveChangesAsync();

            var smartphones = C("Smartphones", "smartphones", electronics.Id, "Latest mobile phones", 1, t);
            var laptops = C("Laptops", "laptops", electronics.Id, "Notebooks & ultrabooks", 2, t);
            var headphones = C("Headphones", "headphones", electronics.Id, "Over-ear, in-ear, TWS", 3, t);
            var tablets = C("Tablets", "tablets", electronics.Id, "iPads & Android tablets", 4, t);
            var cameras = C("Cameras", "cameras", electronics.Id, "DSLR, mirrorless & action cams", 5, t);
            var smartwatches = C("Smartwatches", "smartwatches", electronics.Id, "Fitness trackers & smart watches", 6, t);
            var menClothing = C("Men's Clothing", "mens-clothing", fashion.Id, "Tops, bottoms & suits for men", 1, t);
            var womenCloth = C("Women's Clothing", "womens-clothing", fashion.Id, "Dresses, tops & bottoms for women", 2, t);
            var shoes = C("Shoes", "shoes", fashion.Id, "Sneakers, boots & formal", 3, t);
            var bags = C("Bags", "bags", fashion.Id, "Backpacks, handbags & wallets", 4, t);
            var furniture = C("Furniture", "furniture", home.Id, "Sofas, beds, tables & chairs", 1, t);
            var kitchen = C("Kitchenware", "kitchenware", home.Id, "Cookware & kitchen tools", 2, t);
            var lighting = C("Lighting", "lighting", home.Id, "Lamps & strip lights", 3, t);
            var fitness = C("Fitness Equipment", "fitness-equipment", sports.Id, "Gym & home workout gear", 1, t);
            var outdoor = C("Outdoor Sports", "outdoor-sports", sports.Id, "Camping, hiking & cycling", 2, t);
            var skincare = C("Skincare", "skincare", beauty.Id, "Moisturisers, serums & sunscreens", 1, t);
            var makeup = C("Makeup", "makeup", beauty.Id, "Foundation, lips & eye makeup", 2, t);
            var haircare = C("Hair Care", "hair-care", beauty.Id, "Shampoos, conditioners & tools", 3, t);
            var consoles = C("Consoles", "consoles", gaming.Id, "PlayStation, Xbox & Nintendo", 1, t);
            var gameAccess = C("Gaming Accessories", "gaming-accessories", gaming.Id, "Controllers, headsets & keyboards", 2, t);
            db.Categories.AddRange(
                smartphones, laptops, headphones, tablets, cameras, smartwatches,
                menClothing, womenCloth, shoes, bags, furniture, kitchen, lighting,
                fitness, outdoor, skincare, makeup, haircare, consoles, gameAccess);
            await db.SaveChangesAsync();

            return new Dictionary<string, Category>
            {
                ["electronics"] = electronics,
                ["fashion"] = fashion,
                ["home"] = home,
                ["sports"] = sports,
                ["beauty"] = beauty,
                ["gaming"] = gaming,
                ["smartphones"] = smartphones,
                ["laptops"] = laptops,
                ["headphones"] = headphones,
                ["tablets"] = tablets,
                ["cameras"] = cameras,
                ["smartwatches"] = smartwatches,
                ["menClothing"] = menClothing,
                ["womenCloth"] = womenCloth,
                ["shoes"] = shoes,
                ["bags"] = bags,
                ["furniture"] = furniture,
                ["kitchen"] = kitchen,
                ["lighting"] = lighting,
                ["fitness"] = fitness,
                ["outdoor"] = outdoor,
                ["skincare"] = skincare,
                ["makeup"] = makeup,
                ["haircare"] = haircare,
                ["consoles"] = consoles,
                ["gameAccess"] = gameAccess,
            };
        }

        // PRODUCTS ──────────────────────────────────────────────────────────────────

        private static async Task<List<Product>> SeedProductsAsync(
            EcommerceDbContext db, Dictionary<string, Category> cats)
        {
            var t = DateTime.UtcNow;
            var list = new List<Product>
        {
            // SMARTPHONES
            P("iPhone 16 Pro Max","iphone-16-pro-max",cats["smartphones"].Id,
              1299.99m,1199.99m,"IPH16PM",   Active,true,"Apple",
              "The most powerful iPhone with A18 Pro chip and titanium design.",
              "A18 Pro · 48 MP ProRAW · Action Button · USB-C","apple,iphone,5g",t.AddDays(-30)),
            P("Samsung Galaxy S25 Ultra","samsung-s25-ultra",cats["smartphones"].Id,
              1199.99m,null,"SGS25U",        Active,true,"Samsung",
              "Galaxy flagship with built-in S Pen and Galaxy AI.",
              "Snapdragon 8 Elite · 200 MP · S Pen · 45 W","samsung,galaxy,s-pen,5g",t.AddDays(-22)),
            P("Google Pixel 9 Pro","google-pixel-9-pro",cats["smartphones"].Id,
              999.00m,899.00m,"GPX9P",       Active,false,"Google",
              "Pure Android with class-leading computational photography.",
              "Tensor G4 · 50 MP · Magic Eraser · 7 yrs updates","google,pixel,android",t.AddDays(-15)),
            P("OnePlus 13","oneplus-13",cats["smartphones"].Id,
              799.00m,null,"OP13",           Active,false,"OnePlus",
              "Flagship killer with Hasselblad-tuned cameras and 100 W charging.",
              "Snapdragon 8 Elite · Hasselblad · 100 W","oneplus,flagship",t.AddDays(-10)),
            P("iPhone 15","iphone-15",cats["smartphones"].Id,
              699.00m,549.00m,"IPH15",       Inactive,false,"Apple",
              "Previous-gen iPhone with USB-C and Dynamic Island.",
              "A16 Bionic · 48 MP · Dynamic Island","apple,iphone",t.AddDays(-90)),
            P("Xiaomi 14 Ultra","xiaomi-14-ultra",cats["smartphones"].Id,
              1099.00m,null,"XMI14U",        Active,false,"Xiaomi",
              "Leica-partnered quad-camera flagship phone.",
              "Snapdragon 8 Gen 3 · Leica quad cam · 90 W wireless","xiaomi,leica",t.AddDays(-8)),
            // LAPTOPS
            P("MacBook Pro 14\" M4","macbook-pro-14-m4",cats["laptops"].Id,
              1999.00m,null,"MBP14M4",       Active,true,"Apple",
              "Pro-grade laptop powered by M4 for creators and developers.",
              "M4 Pro · 18 h battery · Liquid Retina XDR · 1 TB","apple,macbook,m4",t.AddDays(-25)),
            P("Dell XPS 15","dell-xps-15",cats["laptops"].Id,
              1799.00m,1599.00m,"DXP15",     Active,false,"Dell",
              "Thin-and-light powerhouse with a stunning OLED display.",
              "Intel Core i9 · RTX 4070 · 3.5 K OLED","dell,xps,oled",t.AddDays(-18)),
            P("ASUS ROG Zephyrus G14","asus-rog-g14",cats["laptops"].Id,
              1499.00m,1349.00m,"ARG14",     Active,false,"ASUS",
              "Ultra-slim gaming laptop with RX 7900S and 240 Hz display.",
              "Ryzen 9 · RX 7900S · 240 Hz · 14\"","asus,rog,gaming",t.AddDays(-12)),
            P("Microsoft Surface Pro 11","surface-pro-11",cats["laptops"].Id,
              1599.00m,null,"MSP11",         Draft,false,"Microsoft",
              "Versatile 2-in-1 with Snapdragon X Elite and Copilot+.",
              "Snapdragon X Elite · 13\" OLED · Copilot+ PC","microsoft,surface,2-in-1",t.AddDays(-5)),
            P("Lenovo ThinkPad X1 Carbon","thinkpad-x1-carbon",cats["laptops"].Id,
              1649.00m,1449.00m,"TPXC",      Active,false,"Lenovo",
              "Ultimate business laptop — thin, light, legendary keyboard.",
              "Intel Ultra 7 · 14\" IPS · 57 Wh · MIL-SPEC","lenovo,thinkpad,business",t.AddDays(-20)),
            // HEADPHONES
            P("Sony WH-1000XM6","sony-wh-1000xm6",cats["headphones"].Id,
              399.99m,349.99m,"SWXM6",       Active,true,"Sony",
              "Industry-leading ANC with 40 h battery and Speak-to-Chat.",
              "QN2e chip · LDAC · Multipoint · 40 h ANC","sony,anc,wireless",t.AddDays(-22)),
            P("Apple AirPods Pro 3","airpods-pro-3",cats["headphones"].Id,
              279.00m,null,"APP3",           Active,false,"Apple",
              "Next-gen ANC earbuds with H2 chip and Personalised Spatial Audio.",
              "H2 chip · ANC · 35 h total · USB-C","apple,airpods,tws",t.AddDays(-8)),
            P("Bose QuietComfort 45","bose-qc45",cats["headphones"].Id,
              329.00m,279.00m,"BQC45",       Active,false,"Bose",
              "Legendary Bose comfort with class-leading noise rejection.",
              "22 h ANC · EQ via app · Foldable","bose,anc,wireless",t.AddDays(-40)),
            P("Sennheiser Momentum 4","sennheiser-momentum-4",cats["headphones"].Id,
              349.00m,null,"SMT4",           Active,false,"Sennheiser",
              "60 h audiophile headphones with adaptive ANC.",
              "60 h ANC · Transparency · aptX Adaptive","sennheiser,audiophile",t.AddDays(-14)),
            // TABLETS
            P("iPad Pro 13\" M4","ipad-pro-13-m4",cats["tablets"].Id,
              1299.00m,null,"IPP13M4",       Active,true,"Apple",
              "Thinnest Apple product with Ultra Retina XDR and M4 chip.",
              "M4 chip · Ultra Retina XDR · Apple Pencil Pro","apple,ipad,m4",t.AddDays(-35)),
            P("Samsung Galaxy Tab S10+","samsung-tab-s10-plus",cats["tablets"].Id,
              999.00m,899.00m,"SGT10P",      Active,false,"Samsung",
              "Top Android tablet with Dynamic AMOLED and S Pen.",
              "Snapdragon 8 Gen 3 · 12.4\" AMOLED · S Pen","samsung,android,tablet",t.AddDays(-28)),
            // SMARTWATCHES
            P("Apple Watch Series 10","apple-watch-series-10",cats["smartwatches"].Id,
              499.00m,null,"AWS10",          Active,false,"Apple",
              "Thinnest Apple Watch with crash detection and sleep apnea alerts.",
              "S10 chip · ECG · Sleep Apnea · 18 h","apple,watch,health",t.AddDays(-16)),
            P("Samsung Galaxy Watch 7","samsung-galaxy-watch-7",cats["smartwatches"].Id,
              329.00m,279.00m,"SGW7",        Active,false,"Samsung",
              "Advanced health monitoring with BioActive Sensor.",
              "Exynos W1000 · BioActive · 40 h battery","samsung,health,watch",t.AddDays(-20)),
            // FASHION
            P("Levi's 501 Original Jeans","levis-501-jeans",cats["menClothing"].Id,
              89.99m,69.99m,"LV501",         Active,false,"Levi's",
              "The definitive straight-leg jeans since 1873.",
              "100% cotton · Button fly · Straight fit","levis,jeans,denim",t.AddDays(-45)),
            P("Ralph Lauren Polo Shirt","ralph-lauren-polo",cats["menClothing"].Id,
              98.00m,null,"RLP",             Active,false,"Ralph Lauren",
              "Iconic polo shirt in 100% piqué cotton.",
              "100% cotton · Custom fit · Embroidered pony","polo,casual,classic",t.AddDays(-38)),
            P("Nike Air Max 270","nike-air-max-270",cats["shoes"].Id,
              150.00m,null,"NAM270",         Active,false,"Nike",
              "Iconic lifestyle shoe with the tallest Air unit heel.",
              "Max Air 270 · Mesh upper · Foam midsole","nike,air-max,sneakers",t.AddDays(-50)),
            P("Adidas Ultraboost 24","adidas-ultraboost-24",cats["shoes"].Id,
              190.00m,160.00m,"AUB24",       Active,true,"Adidas",
              "Fastest Ultraboost yet — BOOST midsole + Continental rubber.",
              "BOOST midsole · Continental grip · Primeknit+","adidas,running,boost",t.AddDays(-38)),
            P("New Balance 1080 v13","nb-1080-v13",cats["shoes"].Id,
              165.00m,null,"NB1080V13",      Active,false,"New Balance",
              "Maximum-cushion daily trainer for long runs.",
              "Fresh Foam X · Engineered mesh · 10 mm drop","new-balance,running",t.AddDays(-22)),
            // HOME
            P("Ergonomic Mesh Chair Pro","ergonomic-mesh-chair",cats["furniture"].Id,
              499.00m,399.00m,"EMC-PRO",     Active,false,null,
              "All-day comfort with lumbar support and breathable mesh back.",
              "4D armrests · Lumbar · 150 kg · Mesh back","chair,office,ergonomic",t.AddDays(-60)),
            P("Solid Oak Desk 160 cm","oak-desk-160",cats["furniture"].Id,
              699.00m,null,"OAK-DESK",       Active,false,null,
              "Minimalist Scandinavian desk in solid oak for home offices.",
              "Solid oak · Steel legs · Cable channel","desk,oak,office",t.AddDays(-55)),
            P("Velvet Accent Chair","velvet-accent-chair",cats["furniture"].Id,
              389.00m,299.00m,"VAC",         Active,false,null,
              "Mid-century modern accent chair in premium velvet.",
              "Solid wood legs · Foam cushion · 4 colours","chair,velvet,living-room",t.AddDays(-42)),
            P("KitchenAid Stand Mixer","kitchenaid-stand-mixer",cats["kitchen"].Id,
              499.00m,429.00m,"KA-MIXER",    Active,false,"KitchenAid",
              "Iconic 5-qt tilt-head stand mixer.",
              "5 qt bowl · 10 speeds · 59 attachments","kitchenaid,baking,mixer",t.AddDays(-33)),
            P("Instant Pot Duo 7-in-1","instant-pot-duo-7in1",cats["kitchen"].Id,
              99.99m,79.99m,"IP-DUO",        Active,false,"Instant Pot",
              "7-in-1 pressure cooker for fast healthy meals.",
              "7-in-1 · 6 qt · 14 programs","instant-pot,pressure-cooker",t.AddDays(-28)),
            // FITNESS
            P("Adjustable Dumbbell Set","adj-dumbbell-set",cats["fitness"].Id,
              299.00m,249.00m,"ADS-52",      Active,false,null,
              "Space-saving dial-select dumbbells replacing 15 fixed pairs.",
              "5–52.5 lb · Quick-select dial · Includes tray","dumbbells,fitness,gym",t.AddDays(-33)),
            P("Yoga Mat Premium 6 mm","yoga-mat-6mm",cats["fitness"].Id,
              49.99m,null,"YMP-6",           Active,false,null,
              "Non-slip 6 mm TPE yoga mat with alignment print.",
              "6 mm TPE · Non-slip · 183×61 cm","yoga,mat,fitness",t.AddDays(-20)),
            P("Resistance Band Set","resistance-band-set",cats["fitness"].Id,
              34.99m,27.99m,"RBS-5",         Active,false,null,
              "Set of 5 resistance bands from light to extra heavy.",
              "5 levels · Natural latex · Carry bag","bands,fitness,stretching",t.AddDays(-14)),
            // SKINCARE
            P("Vitamin C Brightening Serum","vitamin-c-serum",cats["skincare"].Id,
              45.00m,35.00m,"VCS-30",        Active,false,"The Ordinary",
              "10% pure Vitamin C with ferulic acid for radiant skin.",
              "10% L-Ascorbic Acid · Ferulic acid · 30 ml","serum,vitamin-c,brightening",t.AddDays(-14)),
            P("SPF 50+ Sunscreen Fluid","spf50-sunscreen",cats["skincare"].Id,
              28.00m,null,"SPF50-100",       Active,false,"La Roche-Posay",
              "Lightweight non-greasy daily mineral sunscreen.",
              "SPF50+ PA++++ · Mineral filters · 100 ml","sunscreen,spf,daily",t.AddDays(-9)),
            P("Hyaluronic Moisturiser","ha-moisturiser",cats["skincare"].Id,
              39.00m,29.00m,"HAM-50",        Active,false,"CeraVe",
              "48-hour hydration with hyaluronic acid and ceramides.",
              "3× HA · Ceramides · 48 h hydration · 50 ml","moisturiser,ha,ceramides",t.AddDays(-18)),
            // GAMING
            P("PlayStation 5 Pro","ps5-pro",cats["consoles"].Id,
              699.99m,null,"PS5P",           Active,true,"Sony",
              "Next-gen PlayStation with 8 K gaming and 30% faster GPU.",
              "AMD Zen 2+ · 60 CU GPU · 2 TB SSD · 8 K","sony,ps5,console",t.AddDays(-12)),
            P("Xbox Series X","xbox-series-x",cats["consoles"].Id,
              499.99m,null,"XBSX",           Active,false,"Microsoft",
              "Most powerful Xbox ever — true 4 K at 120 fps.",
              "AMD Zen 2 · 12 TFLOPS · 1 TB SSD","microsoft,xbox,4k",t.AddDays(-45)),
            P("Nintendo Switch OLED","nintendo-switch-oled",cats["consoles"].Id,
              349.99m,319.99m,"NSW-OLED",    Active,false,"Nintendo",
              "Versatile hybrid console with vivid 7\" OLED screen.",
              "7\" OLED · 64 GB · TV/Tabletop/Handheld","nintendo,switch,handheld",t.AddDays(-60)),
        };
            db.Products.AddRange(list);
            await db.SaveChangesAsync();
            return list;
        }

        // IMAGES ────────────────────────────────────────────────────────────────────

        private static async Task SeedImagesAsync(EcommerceDbContext db, List<Product> products)
        {
            var seeds = new Dictionary<string, string[]>
            {
                ["iphone-16-pro-max"] = ["iph16a", "iph16b", "iph16c"],
                ["samsung-s25-ultra"] = ["s25a", "s25b"],
                ["google-pixel-9-pro"] = ["px9a", "px9b"],
                ["oneplus-13"] = ["op13a"],
                ["iphone-15"] = ["iph15a", "iph15b"],
                ["xiaomi-14-ultra"] = ["xmi14a"],
                ["macbook-pro-14-m4"] = ["mbp14a", "mbp14b", "mbp14c"],
                ["dell-xps-15"] = ["dxp15a", "dxp15b"],
                ["asus-rog-g14"] = ["arog14a", "arog14b"],
                ["thinkpad-x1-carbon"] = ["tpx1a"],
                ["sony-wh-1000xm6"] = ["swxm6a", "swxm6b"],
                ["airpods-pro-3"] = ["app3a"],
                ["bose-qc45"] = ["bqc45a", "bqc45b"],
                ["sennheiser-momentum-4"] = ["smt4a"],
                ["ipad-pro-13-m4"] = ["ipp13a", "ipp13b"],
                ["samsung-tab-s10-plus"] = ["sgt10a", "sgt10b"],
                ["apple-watch-series-10"] = ["aws10a"],
                ["samsung-galaxy-watch-7"] = ["sgw7a"],
                ["levis-501-jeans"] = ["lv501a", "lv501b", "lv501c"],
                ["ralph-lauren-polo"] = ["rlpa"],
                ["nike-air-max-270"] = ["nam270a", "nam270b"],
                ["adidas-ultraboost-24"] = ["aub24a", "aub24b"],
                ["nb-1080-v13"] = ["nb1080a"],
                ["ergonomic-mesh-chair"] = ["emca", "emcb"],
                ["oak-desk-160"] = ["oaka"],
                ["velvet-accent-chair"] = ["vaca", "vacb"],
                ["kitchenaid-stand-mixer"] = ["kama", "kamb"],
                ["instant-pot-duo-7in1"] = ["ipda"],
                ["adj-dumbbell-set"] = ["adsa", "adsb"],
                ["yoga-mat-6mm"] = ["ympa"],
                ["resistance-band-set"] = ["rbsa"],
                ["vitamin-c-serum"] = ["vcsa"],
                ["spf50-sunscreen"] = ["spfa"],
                ["ha-moisturiser"] = ["hama"],
                ["ps5-pro"] = ["ps5a", "ps5b"],
                ["xbox-series-x"] = ["xbsxa"],
                ["nintendo-switch-oled"] = ["nswa", "nswb"],
            };
            var images = new List<ProductImage>();
            foreach (var p in products)
            {
                if (!seeds.TryGetValue(p.Slug, out var ss)) continue;
                for (int i = 0; i < ss.Length; i++)
                    images.Add(new ProductImage
                    {
                        ProductId = p.Id,
                        ImageUrl = $"https://picsum.photos/seed/{ss[i]}/600/600",
                        AltText = $"{p.Name} {i + 1}",
                        IsPrimary = i == 0,
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow
                    });
            }
            db.ProductImages.AddRange(images);
            await db.SaveChangesAsync();
        }

        // VARIANTS ──────────────────────────────────────────────────────────────────

        private static async Task SeedVariantsAsync(EcommerceDbContext db, List<Product> products)
        {
            var t = DateTime.UtcNow;
            var map = products.ToDictionary(p => p.Slug!);
            var vl = new List<ProductVariant>();

            void V(string slug, params (string n, string sku, decimal price, string? col, string? sz)[] vs)
            {
                if (!map.TryGetValue(slug, out var p)) return;
                for (int i = 0; i < vs.Length; i++)
                {
                    var v = vs[i];
                    vl.Add(new ProductVariant
                    {
                        ProductId = p.Id,
                        Name = v.n,
                        SKU = v.sku,
                        Price = v.price,
                        Color = v.col,
                        Size = v.sz,
                        IsActive = true,
                        DisplayOrder = i,
                        CreatedAt = t
                    });
                }
            }

            // Smartphones — storage
            V("iphone-16-pro-max",
                ("128 GB / Natural Titanium", "IPH16PM-128-NAT", 1099.99m, "#d4c5af", "128 GB"),
                ("256 GB / Black Titanium", "IPH16PM-256-BLK", 1199.99m, "#2c2c2e", "256 GB"),
                ("256 GB / White Titanium", "IPH16PM-256-WHT", 1199.99m, "#f5f5f0", "256 GB"),
                ("512 GB / Desert Titanium", "IPH16PM-512-DST", 1399.99m, "#c9b99a", "512 GB"),
                ("1 TB / Black Titanium", "IPH16PM-1TB-BLK", 1599.99m, "#2c2c2e", "1 TB"));
            V("samsung-s25-ultra",
                ("256 GB / Titanium Black", "SGS25U-256-BLK", 1199.99m, "#1a1a1a", "256 GB"),
                ("512 GB / Titanium Silver", "SGS25U-512-SLV", 1349.99m, "#c0c0c0", "512 GB"),
                ("1 TB / Titanium Gray", "SGS25U-1TB-GRY", 1549.99m, "#808080", "1 TB"));
            V("google-pixel-9-pro",
                ("128 GB / Obsidian", "GPX9P-128-OBS", 899.00m, "#2d2d2d", "128 GB"),
                ("256 GB / Porcelain", "GPX9P-256-POR", 999.00m, "#f0ede8", "256 GB"),
                ("512 GB / Hazel", "GPX9P-512-HAZ", 1099.00m, "#7a7d6e", "512 GB"));
            // Laptops — spec tiers
            V("macbook-pro-14-m4",
                ("M4 / 16 GB / 512 GB", "MBP14-M4-16-512", 1999.00m, "#e8e8e8", "512 GB"),
                ("M4 Pro / 24 GB / 1 TB", "MBP14-M4P-24-1T", 2399.00m, "#e8e8e8", "1 TB"),
                ("M4 Max / 36 GB / 1 TB", "MBP14-M4X-36-1T", 3199.00m, "#2d2d2d", "1 TB"));
            V("dell-xps-15",
                ("i7 / 16 GB / 512 GB", "DXP15-16-512", 1599.00m, "#1c1c1c", "512 GB"),
                ("i9 / 32 GB / 1 TB", "DXP15-32-1TB", 1799.00m, "#1c1c1c", "1 TB"));
            V("asus-rog-g14",
                ("16 GB / 512 GB", "ARG14-16-512", 1349.00m, "#1a1a1a", "512 GB"),
                ("32 GB / 1 TB", "ARG14-32-1TB", 1499.00m, "#1a1a1a", "1 TB"));
            // Headphones — colour
            V("sony-wh-1000xm6",
                ("Midnight Black", "SWXM6-BLK", 399.99m, "#1a1a1a", null),
                ("Platinum Silver", "SWXM6-SLV", 399.99m, "#c0c0c0", null),
                ("Sage Green", "SWXM6-GRN", 399.99m, "#8fbc8f", null));
            V("bose-qc45",
                ("White Smoke", "BQC45-WHT", 329.00m, "#f5f5f0", null),
                ("Midnight Blue", "BQC45-BLU", 329.00m, "#191970", null));
            // Jeans — W/L
            V("levis-501-jeans",
                ("W28/L30 Dark Wash", "LV501-28-30-D", 89.99m, "#1a237e", "W28/L30"),
                ("W30/L32 Dark Wash", "LV501-30-32-D", 89.99m, "#1a237e", "W30/L32"),
                ("W32/L32 Med Wash", "LV501-32-32-M", 89.99m, "#3949ab", "W32/L32"),
                ("W34/L34 Dark Wash", "LV501-34-34-D", 89.99m, "#1a237e", "W34/L34"),
                ("W36/L34 Light Wash", "LV501-36-34-L", 89.99m, "#9fa8da", "W36/L34"));
            // Polo — colour + size
            V("ralph-lauren-polo",
                ("White / S", "RLP-WHT-S", 98.00m, "#ffffff", "S"),
                ("White / M", "RLP-WHT-M", 98.00m, "#ffffff", "M"),
                ("White / L", "RLP-WHT-L", 98.00m, "#ffffff", "L"),
                ("Navy / M", "RLP-NAV-M", 98.00m, "#001f5b", "M"),
                ("Navy / L", "RLP-NAV-L", 98.00m, "#001f5b", "L"),
                ("Red / M", "RLP-RED-M", 98.00m, "#b71c1c", "M"),
                ("Red / L", "RLP-RED-L", 98.00m, "#b71c1c", "L"));
            // Shoes — US size + colour
            V("nike-air-max-270",
                ("US 7 / White", "NAM270-7-W", 150.00m, "#f5f5f5", "US 7"),
                ("US 8 / White", "NAM270-8-W", 150.00m, "#f5f5f5", "US 8"),
                ("US 9 / White", "NAM270-9-W", 150.00m, "#f5f5f5", "US 9"),
                ("US 9 / Black", "NAM270-9-B", 150.00m, "#212121", "US 9"),
                ("US 10 / White", "NAM270-10-W", 150.00m, "#f5f5f5", "US 10"),
                ("US 10 / Black", "NAM270-10-B", 150.00m, "#212121", "US 10"),
                ("US 11 / Black", "NAM270-11-B", 150.00m, "#212121", "US 11"));
            V("adidas-ultraboost-24",
                ("US 7 / Black", "AUB24-7-BK", 190.00m, "#212121", "US 7"),
                ("US 8 / Black", "AUB24-8-BK", 190.00m, "#212121", "US 8"),
                ("US 9 / Black", "AUB24-9-BK", 190.00m, "#212121", "US 9"),
                ("US 9 / Cloud White", "AUB24-9-WH", 190.00m, "#fafafa", "US 9"),
                ("US 10 / Black", "AUB24-10-BK", 190.00m, "#212121", "US 10"),
                ("US 10 / Cloud White", "AUB24-10-WH", 190.00m, "#fafafa", "US 10"),
                ("US 11 / Cloud White", "AUB24-11-WH", 190.00m, "#fafafa", "US 11"));
            V("nb-1080-v13",
                ("US 8 / Black", "NB1080-8-BK", 165.00m, "#212121", "US 8"),
                ("US 9 / Black", "NB1080-9-BK", 165.00m, "#212121", "US 9"),
                ("US 10 / Black", "NB1080-10-BK", 165.00m, "#212121", "US 10"),
                ("US 10 / White", "NB1080-10-WH", 165.00m, "#fafafa", "US 10"),
                ("US 11 / Black", "NB1080-11-BK", 165.00m, "#212121", "US 11"));
            // Velvet chair colours
            V("velvet-accent-chair",
                ("Dusty Pink", "VAC-PINK", 389.00m, "#c48b9f", null),
                ("Forest Green", "VAC-GRN", 389.00m, "#2e7d32", null),
                ("Navy Blue", "VAC-NAV", 389.00m, "#1a237e", null),
                ("Charcoal", "VAC-CHR", 389.00m, "#424242", null));
            // Yoga mat colours
            V("yoga-mat-6mm",
                ("Purple", "YMP-PUR", 49.99m, "#9c27b0", null),
                ("Sage Green", "YMP-GRN", 49.99m, "#4caf50", null),
                ("Slate Gray", "YMP-GRY", 49.99m, "#607d8b", null),
                ("Burnt Orange", "YMP-ORG", 49.99m, "#bf360c", null));
            // Nintendo colours
            V("nintendo-switch-oled",
                ("White", "NSW-OLED-WHT", 349.99m, "#f5f5f5", null),
                ("Neon Red/Blue", "NSW-OLED-NRB", 349.99m, "#e53935", null));

            db.ProductVariants.AddRange(vl);
        }

        // INVENTORY ─────────────────────────────────────────────────────────────────

        private static async Task SeedInventoryAsync(EcommerceDbContext db, List<Product> products)
        {
            var t = DateTime.UtcNow;
            var variants = await db.ProductVariants.IgnoreQueryFilters().ToListAsync();
            var byProd = variants.GroupBy(v => v.ProductId).ToDictionary(g => g.Key, g => g.ToList());
            var inv = new List<Inventory>();

            foreach (var p in products)
            {
                if (byProd.TryGetValue(p.Id, out var pvs))
                    foreach (var pv in pvs)
                        inv.Add(Inv(p.Id, pv.Id, StockQty(p.Slug!, pv.Name), StockRes(p.Slug!), 3, t));
                else
                    inv.Add(Inv(p.Id, null, StockQty(p.Slug!, null), StockRes(p.Slug!), 5, t));
            }
            db.Inventories.AddRange(inv);
        }

        // HELPERS ───────────────────────────────────────────────────────────────────

        private static Category C(string name, string slug, int? pid, string desc, int ord, DateTime t, bool active = true)
            => new() { Name = name, Slug = slug, ParentId = pid, Description = desc, DisplayOrder = ord, IsActive = active, CreatedAt = t };

        private static Product P(string name, string slug, int catId,
            decimal bp, decimal? sp, string sku, ProductStatus st, bool feat, string? brand,
            string desc, string shortDesc, string tags, DateTime created)
            => new()
            {
                Name = name,
                Slug = slug,
                CategoryId = catId,
                BasePrice = bp,
                SalePrice = sp,
                SKU = sku,
                Status = st,
                IsFeatured = feat,
                Brand = brand,
                Description = desc,
                ShortDescription = shortDesc,
                Tags = tags,
                CreatedAt = created,
                UpdatedAt = created
            };

        private static Inventory Inv(int pid, int? vid, int qty, int res, int thr, DateTime t)
            => new()
            {
                ProductId = pid,
                ProductVariantId = vid,
                Quantity = qty,
                ReservedQuantity = res,
                LowStockThreshold = thr,
                WarehouseLocation = Locs[Rnd.Next(Locs.Length)],
                LastStockUpdate = t.AddDays(-Rnd.Next(1, 14)),
                CreatedAt = t
            };

        private static int StockQty(string slug, string? variantName) => slug switch
        {
            "iphone-15" => 0,
            "surface-pro-11" => 2,
            "airpods-pro-3" => 3,
            "spf50-sunscreen" => 4,
            "resistance-band-set" => 1,
            _ when variantName?.Contains("1 TB") == true => 5,
            _ when variantName?.Contains("Desert") == true => 0,
            _ => Rnd.Next(12, 80)
        };
        private static int StockRes(string slug) => slug switch
        {
            "iphone-15" => 0,
            "surface-pro-11" => 0,
            _ => Rnd.Next(0, 5)
        };

        private static readonly Random Rnd = new(42);
        private static readonly string[] Locs = ["A-01", "A-02", "A-03", "B-01", "B-02", "C-01", "C-02", "D-01", "D-02"];
        private static ProductStatus Active => ProductStatus.Active;
        private static ProductStatus Inactive => ProductStatus.Inactive;
        private static ProductStatus Draft => ProductStatus.Draft;
    }
}