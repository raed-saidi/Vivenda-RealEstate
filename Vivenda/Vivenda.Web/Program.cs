using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vivenda.BLL.Services;
using Vivenda.DAL.Data;
using Vivenda.DAL.Entities;
using Vivenda.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Register services
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    await context.Database.MigrateAsync();
    
    // Create roles
    string[] roles = { "Admin", "Agent", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Create admin user
    var adminEmail = "admin@vivenda.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true
        };
        
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Create demo agent
    var agentEmail = "agent@vivenda.com";
    var agent = await userManager.FindByEmailAsync(agentEmail);
    if (agent == null)
    {
        agent = new User
        {
            UserName = agentEmail,
            Email = agentEmail,
            FirstName = "Raed",
            LastName = "Saidi",
            PhoneNumber = "+216 99 584 320",
            EmailConfirmed = true,
            IsActive = true
        };
        await userManager.CreateAsync(agent, "Agent123!");
        await userManager.AddToRoleAsync(agent, "Agent");
    }
    else
    {
        agent = await userManager.FindByEmailAsync(agentEmail);
    }

    // Seed Demo Properties
    if (!context.Properties.Any())
    {
        // Ensure categories exist
        var residentialCat = context.Categories.FirstOrDefault(c => c.Name == "Residential");
        if (residentialCat == null)
        {
            residentialCat = new Category { Name = "Residential", Description = "Homes and apartments for living", IconClass = "fas fa-home", IsActive = true };
            context.Categories.Add(residentialCat);
        }

        var commercialCat = context.Categories.FirstOrDefault(c => c.Name == "Commercial");
        if (commercialCat == null)
        {
            commercialCat = new Category { Name = "Commercial", Description = "Office spaces and retail", IconClass = "fas fa-building", IsActive = true };
            context.Categories.Add(commercialCat);
        }

        var luxuryCat = context.Categories.FirstOrDefault(c => c.Name == "Luxury");
        if (luxuryCat == null)
        {
            luxuryCat = new Category { Name = "Luxury", Description = "High-end premium properties", IconClass = "fas fa-gem", IsActive = true };
            context.Categories.Add(luxuryCat);
        }

        var vacationCat = context.Categories.FirstOrDefault(c => c.Name == "Vacation");
        if (vacationCat == null)
        {
            vacationCat = new Category { Name = "Vacation", Description = "Holiday homes and rentals", IconClass = "fas fa-umbrella-beach", IsActive = true };
            context.Categories.Add(vacationCat);
        }

        // Ensure amenities exist
        var amenityNames = new[] { "Swimming Pool", "Gym", "Parking", "Garden", "Air Conditioning", "Security", "Balcony", "Fireplace" };
        var amenityIcons = new[] { "fas fa-swimming-pool", "fas fa-dumbbell", "fas fa-parking", "fas fa-leaf", "fas fa-snowflake", "fas fa-shield-alt", "fas fa-door-open", "fas fa-fire" };
        
        for (int i = 0; i < amenityNames.Length; i++)
        {
            if (!context.Amenities.Any(a => a.Name == amenityNames[i]))
            {
                context.Amenities.Add(new Amenity { Name = amenityNames[i], IconClass = amenityIcons[i], IsActive = true });
            }
        }

        await context.SaveChangesAsync();

      var demoProperties = new List<Property>
{
    // === LUXURY VILLAS FOR SALE ===
    new Property
    {
        Title = "Villa Luxueuse avec Piscine à Gammarth",
        Description = "Magnifique villa moderne avec piscine privée, jardin méditerranéen et vue panoramique sur la mer. Architecture contemporaine avec des matériaux nobles, terrasse spacieuse et garage pour 3 voitures. Quartier résidentiel haut de gamme de Gammarth.",
        Price = 1850000, // TND
        Address = "Route de la Corniche",
        City = "Gammarth",
        State = "Tunis",
        ZipCode = "2078",
        Country = "Tunisia",
        Bedrooms = 6,
        Bathrooms = 4,
        SquareFeet = 4500,
        YearBuilt = 2021,
        PropertyType = PropertyType.Villa,
        ListingType = ListingType.Sale,
        Status = PropertyStatus.Active,
        IsFeatured = true,
        MainImageUrl = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800",
        UserId = agent!.Id,
        CategoryId = luxuryCat.Id
    },

    new Property
    {
        Title = "Villa avec Vue Mer à Sidi Bou Saïd",
        Description = "Villa traditionnelle tunisienne rénovée avec charme authentique. Cour intérieure, terrasse avec vue mer, architecture typique de Sidi Bou Saïd avec ses murs blancs et volets bleus. Proche de la marina et des cafés traditionnels.",
        Price = 1420000,
        Address = "Rue Sidi Chabaane",
        City = "Sidi Bou Saïd",
        State = "Tunis",
        ZipCode = "2026",
        Country = "Tunisia",
        Bedrooms = 5,
        Bathrooms = 3,
        SquareFeet = 3200,
        YearBuilt = 2018,
        PropertyType = PropertyType.Villa,
        ListingType = ListingType.Sale,
        Status = PropertyStatus.Active,
        IsFeatured = true,
        MainImageUrl = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800",
        UserId = agent.Id,
        CategoryId = luxuryCat.Id
    },

    // === FAMILY HOUSES FOR SALE ===
    new Property
    {
        Title = "Maison Familiale à Ariana Ville",
        Description = "Belle maison individuelle de 4 chambres avec jardin arboré et parking privé. Salon spacieux, cuisine équipée moderne, terrasse couverte idéale pour les repas en famille. Quartier calme proche écoles et commerces.",
        Price = 620000,
        Address = "Rue des Oliviers",
        City = "Ariana",
        State = "Tunis",
        ZipCode = "2080",
        Country = "Tunisia",
        Bedrooms = 4,
        Bathrooms = 3,
        SquareFeet = 2800,
        YearBuilt = 2017,
        PropertyType = PropertyType.House,
        ListingType = ListingType.Sale,
        Status = PropertyStatus.Active,
        IsFeatured = true,
        MainImageUrl = "https://i.pinimg.com/736x/e4/d5/1e/e4d51e41c20c616a434bfa574ac3ca7f.jpg",
        UserId = agent.Id,
        CategoryId = residentialCat.Id
    },

    new Property
    {
        Title = "Dar Traditionnel au Cœur de Nabeul",
        Description = "Maison traditionnelle tunisienne 'Dar' avec patio central, fontaine et décoration en zellige authentique. 3 chambres, salon traditionnel, cuisine moderne. Parfait mélange entre tradition et confort moderne.",
        Price = 380000,
        Address = "Medina de Nabeul",
        City = "Nabeul",
        State = "Nabeul",
        ZipCode = "8000",
        Country = "Tunisia",
        Bedrooms = 3,
        Bathrooms = 2,
        SquareFeet = 2100,
        YearBuilt = 2019,
        PropertyType = PropertyType.House,
        ListingType = ListingType.Sale,
        Status = PropertyStatus.Active,
        IsFeatured = false,
        MainImageUrl = "https://images.unsplash.com/photo-1600047509807-ba8f99d2cdde?w=800",
        UserId = agent.Id,
        CategoryId = residentialCat.Id
    },

    // === MODERN APARTMENTS FOR SALE ===
    new Property
    {
        Title = "Appartement Moderne au Lac de Tunis",
        Description = "Superbe appartement de 3 pièces au cœur du Lac de Tunis. Vue dégagée, balcon spacieux, finitions haut de gamme. Immeuble neuf avec ascenseur, parking souterrain et gardiennage 24h/24.",
        Price = 485000,
        Address = "Les Berges du Lac",
        City = "Tunis",
        State = "Tunis",
        ZipCode = "1053",
        Country = "Tunisia",
        Bedrooms = 2,
        Bathrooms = 2,
        SquareFeet = 1400,
        YearBuilt = 2022,
        PropertyType = PropertyType.Apartment,
        ListingType = ListingType.Sale,
        Status = PropertyStatus.Active,
        IsFeatured = true,
        MainImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
        UserId = agent.Id,
        CategoryId = residentialCat.Id
    },

    new Property
    {
        Title = "Studio Moderne à Sousse Centre",
        Description = "Studio élégant entièrement meublé en centre-ville de Sousse. Idéal investissement locatif ou pied-à-terre. Proche médina, plage et commerces. Parfait pour jeunes professionnels.",
        Price = 185000,
        Address = "Avenue Habib Bourguiba",
        City = "Sousse",
        State = "Sousse",
        ZipCode = "4000",
        Country = "Tunisia",
        Bedrooms = 1,
        Bathrooms = 1,
        SquareFeet = 650,
        YearBuilt = 2020,
        PropertyType = PropertyType.Apartment,
        ListingType = ListingType.Sale,
        Status = PropertyStatus.Active,
        IsFeatured = false,
        MainImageUrl = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800",
        UserId = agent.Id,
        CategoryId = residentialCat.Id
    },

    // === COASTAL PROPERTIES FOR SALE ===
    new Property
    {
        Title = "Villa de Vacances à Hammamet Sud",
        Description = "Charmante villa de vacances à 200m de la plage d'Hammamet. Piscine privée, jardin tropical, terrasse avec pergola. Architecture méditerranéenne, 4 chambres climatisées. Zone touristique calme.",
        Price = 890000,
        Address = "Zone Touristique Hammamet Sud",
        City = "Hammamet",
        State = "Nabeul",
        ZipCode = "8050",
        Country = "Tunisia",
        Bedrooms = 4,
        Bathrooms = 3,
        SquareFeet = 3000,
        YearBuilt = 2019,
        PropertyType = PropertyType.Villa,
        ListingType = ListingType.Sale,
        Status = PropertyStatus.Active,
        IsFeatured = true,
        MainImageUrl = "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
        UserId = agent.Id,
        CategoryId = vacationCat.Id
    },

    // === RENTAL PROPERTIES ===
    new Property
    {
        Title = "Appartement Meublé à La Marsa",
        Description = "Joli appartement 2 pièces entièrement meublé à La Marsa. Balcon avec vue, proche plage et restaurants. Idéal pour expatriés ou location saisonnière. Quartier résidentiel prisé.",
        Price = 1650, // Monthly rent in TND
        Address = "Rue du Parc",
        City = "La Marsa",
        State = "Tunis",
        ZipCode = "2070",
        Country = "Tunisia",
        Bedrooms = 2,
        Bathrooms = 1,
        SquareFeet = 950,
        YearBuilt = 2018,
        PropertyType = PropertyType.Apartment,
        ListingType = ListingType.Rent,
        Status = PropertyStatus.Active,
        IsFeatured = false,
        MainImageUrl = "https://images.unsplash.com/photo-1493809842364-78817add7ffb?w=800",
        UserId = agent.Id,
        CategoryId = residentialCat.Id
    },

    new Property
    {
        Title = "Villa avec Piscine à Monastir",
        Description = "Villa moderne avec piscine chauffée, jardin paysagé et garage. 5 chambres, salon double, cuisine américaine équipée. Proche marina de Monastir et aéroport. Location longue durée.",
        Price = 2800,
        Address = "Route de l'Aéroport",
        City = "Monastir",
        State = "Monastir",
        ZipCode = "5000",
        Country = "Tunisia",
        Bedrooms = 5,
        Bathrooms = 4,
        SquareFeet = 3500,
        YearBuilt = 2020,
        PropertyType = PropertyType.Villa,
        ListingType = ListingType.Rent,
        Status = PropertyStatus.Active,
        IsFeatured = true,
        MainImageUrl = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800",
        UserId = agent.Id,
        CategoryId = luxuryCat.Id
    },

    // === COMMERCIAL PROPERTIES ===
    new Property
    {
        Title = "Local Commercial Avenue Bourguiba",
        Description = "Excellent local commercial en plein centre de Tunis sur l'Avenue Habib Bourguiba. Vitrine sur rue passante, ideal pour boutique, showroom ou bureau. Très bon investissement locatif.",
        Price = 3200,
        Address = "Avenue Habib Bourguiba",
        City = "Tunis",
        State = "Tunis",
        ZipCode = "1001",
        Country = "Tunisia",
        Bedrooms = 0,
        Bathrooms = 2,
        SquareFeet = 1200,
        YearBuilt = 2021,
        PropertyType = PropertyType.Commercial,
        ListingType = ListingType.Rent,
        Status = PropertyStatus.Active,
        IsFeatured = false,
        MainImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=800",
        UserId = agent.Id,
        CategoryId = commercialCat.Id
    },

    new Property
    {
        Title = "Bureau Moderne à Sfax Centre",
        Description = "Espace bureau moderne et lumineux au centre de Sfax. Open space, salles de réunion, parking privé. Immeuble professionnel avec réception et sécurité. Parfait pour entreprises en croissance.",
        Price = 2400,
        Address = "Rue de la République",
        City = "Sfax",
        State = "Sfax",
        ZipCode = "3000",
        Country = "Tunisia",
        Bedrooms = 0,
        Bathrooms = 2,
        SquareFeet = 1800,
        YearBuilt = 2019,
        PropertyType = PropertyType.Commercial,
        ListingType = ListingType.Rent,
        Status = PropertyStatus.Active,
        IsFeatured = false,
        MainImageUrl = "https://images.unsplash.com/photo-1497366811353-6870744d04b2?w=800",
        UserId = agent.Id,
        CategoryId = commercialCat.Id
    }
};


        context.Properties.AddRange(demoProperties);
        await context.SaveChangesAsync();

        // Add amenities to properties
        var pool = context.Amenities.First(a => a.Name == "Swimming Pool");
        var gym = context.Amenities.First(a => a.Name == "Gym");
        var parking = context.Amenities.First(a => a.Name == "Parking");
        var garden = context.Amenities.First(a => a.Name == "Garden");
        var ac = context.Amenities.First(a => a.Name == "Air Conditioning");
        var security = context.Amenities.First(a => a.Name == "Security");
        var balcony = context.Amenities.First(a => a.Name == "Balcony");

        var props = context.Properties.ToList();
        var propertyAmenities = new List<PropertyAmenity>();

        // Add amenities to luxury properties
        foreach (var prop in props.Where(p => p.Price > 1000000 || p.PropertyType == PropertyType.Villa))
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = pool.Id });
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = gym.Id });
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = security.Id });
        }

        // Add common amenities to all properties
        foreach (var prop in props)
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = parking.Id });
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = ac.Id });
        }

        // Add garden to houses
        foreach (var prop in props.Where(p => p.PropertyType == PropertyType.House))
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = garden.Id });
        }

        // Add balcony to apartments/condos
        foreach (var prop in props.Where(p => p.PropertyType == PropertyType.Apartment || p.PropertyType == PropertyType.Condo))
        {
            propertyAmenities.Add(new PropertyAmenity { PropertyId = prop.Id, AmenityId = balcony.Id });
        }

        context.PropertyAmenities.AddRange(propertyAmenities);
        await context.SaveChangesAsync();
    }
    
    // Seed default site settings
    if (!context.SiteSettings.Any())
    {
        context.SiteSettings.AddRange(
            new SiteSettings { Key = "site_name", Value = "Vivenda", Description = "Website name", Category = "General" },
            new SiteSettings { Key = "site_description", Value = "Find your dream property", Description = "Website description", Category = "General" },
            new SiteSettings { Key = "contact_email", Value = "contact@vivenda.com", Description = "Contact email address", Category = "General" },
            new SiteSettings { Key = "contact_phone", Value = "+1 234 567 890", Description = "Contact phone number", Category = "General" },
            new SiteSettings { Key = "contact_address", Value = "123 Main St, City, Country", Description = "Office address", Category = "General" },
            new SiteSettings { Key = "facebook_url", Value = "https://facebook.com", Description = "Facebook page URL", Category = "Social" },
            new SiteSettings { Key = "twitter_url", Value = "https://twitter.com", Description = "Twitter profile URL", Category = "Social" },
            new SiteSettings { Key = "instagram_url", Value = "https://instagram.com", Description = "Instagram profile URL", Category = "Social" },
            new SiteSettings { Key = "meta_keywords", Value = "vivenda, real estate, property, homes, apartments", Description = "SEO keywords", Category = "SEO" },
            new SiteSettings { Key = "google_analytics_id", Value = "", Description = "Google Analytics tracking ID", Category = "SEO" }
        );
        await context.SaveChangesAsync();
    }
}

app.Run();

