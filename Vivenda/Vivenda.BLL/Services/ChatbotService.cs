using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vivenda.BLL.DTOs;
using Vivenda.DAL.Data;
using Vivenda.DAL.Entities;

namespace Vivenda.BLL.Services;

public class ChatbotService : IChatbotService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public ChatbotService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _httpClient = new HttpClient();
        _apiKey = configuration["Groq:ApiKey"] ?? "";
    }

    public async Task<ChatbotResponseDto> ProcessMessageAsync(string message, string? context = null)
    {
        try
        {
            // Step 1: Retrieve relevant data from database (RAG - Retrieval)
            var propertyContext = await RetrieveRelevantDataAsync(message);
            
            // Step 2: Build the prompt with context
            var systemPrompt = BuildSystemPrompt(propertyContext);
            
            // Step 3: Call Groq LLM API (RAG - Generation)
            var llmResponse = await CallGroqApiAsync(systemPrompt, message);
            
            // Step 4: Extract property suggestions if mentioned
            var suggestedProperties = await ExtractSuggestedPropertiesAsync(message, propertyContext);

            return new ChatbotResponseDto
            {
                Response = llmResponse,
                SuggestedProperties = suggestedProperties,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new ChatbotResponseDto
            {
                Response = $"I apologize, but I encountered an error processing your request. Please try again. Error: {ex.Message}",
                Success = false
            };
        }
    }

    private async Task<string> RetrieveRelevantDataAsync(string query)
    {
        var sb = new StringBuilder();
        var lowerQuery = query.ToLower();

        // Get property statistics
        var totalProperties = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Active);
        var forSale = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Active && p.ListingType == ListingType.Sale);
        var forRent = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Active && p.ListingType == ListingType.Rent);
        
        // SQLite doesn't support Average on decimal, so we fetch and calculate client-side
        var activePrices = await _context.Properties
            .Where(p => p.Status == PropertyStatus.Active)
            .Select(p => p.Price)
            .ToListAsync();
        var avgPrice = activePrices.Any() ? activePrices.Average(p => (double)p) : 0;

        sb.AppendLine("=== VIVENDA DATABASE CONTEXT ===");
        sb.AppendLine($"\nOVERALL STATISTICS:");
        sb.AppendLine($"- Total Active Properties: {totalProperties}");
        sb.AppendLine($"- Properties For Sale: {forSale}");
        sb.AppendLine($"- Properties For Rent: {forRent}");
        sb.AppendLine($"- Average Price: ${avgPrice:N0}");

        // Get categories
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .Select(c => c.Name)
            .ToListAsync();
        sb.AppendLine($"\nAVAILABLE CATEGORIES: {string.Join(", ", categories)}");

        // Get amenities
        var amenities = await _context.Amenities
            .Where(a => a.IsActive)
            .Select(a => a.Name)
            .ToListAsync();
        sb.AppendLine($"\nAVAILABLE AMENITIES: {string.Join(", ", amenities)}");

        // Get relevant properties based on query keywords
        var properties = await GetRelevantPropertiesAsync(lowerQuery);
        
        if (properties.Any())
        {
            sb.AppendLine($"\n=== RELEVANT PROPERTIES ({properties.Count} found) ===");
            foreach (var prop in properties.Take(10))
            {
                sb.AppendLine($"\n[Property ID: {prop.Id}]");
                sb.AppendLine($"- Title: {prop.Title}");
                sb.AppendLine($"- Price: ${prop.Price:N0}");
                sb.AppendLine($"- Type: {prop.PropertyType} ({prop.ListingType})");
                sb.AppendLine($"- Location: {prop.Address}, {prop.City}, {prop.State}");
                sb.AppendLine($"- Bedrooms: {prop.Bedrooms}, Bathrooms: {prop.Bathrooms}");
                sb.AppendLine($"- Size: {prop.SquareFeet:N0} sqft");
                sb.AppendLine($"- Category: {prop.Category?.Name ?? "N/A"}");
                sb.AppendLine($"- Featured: {(prop.IsFeatured ? "Yes" : "No")}");
                if (!string.IsNullOrEmpty(prop.Description))
                {
                    var desc = prop.Description.Length > 200 
                        ? prop.Description.Substring(0, 200) + "..." 
                        : prop.Description;
                    sb.AppendLine($"- Description: {desc}");
                }
                var propAmenities = prop.PropertyAmenities?.Select(pa => pa.Amenity?.Name).Where(n => n != null);
                if (propAmenities?.Any() == true)
                {
                    sb.AppendLine($"- Amenities: {string.Join(", ", propAmenities)}");
                }
            }
        }

        // Get price ranges (using already fetched prices)
        var minPrice = activePrices.Any() ? activePrices.Min() : 0;
        var maxPrice = activePrices.Any() ? activePrices.Max() : 0;
        
        sb.AppendLine($"\nPRICE RANGE: ${minPrice:N0} - ${maxPrice:N0}");

        // Get locations
        var cities = await _context.Properties
            .Where(p => p.Status == PropertyStatus.Active)
            .Select(p => p.City)
            .Distinct()
            .ToListAsync();
        sb.AppendLine($"\nAVAILABLE CITIES: {string.Join(", ", cities)}");

        return sb.ToString();
    }

    private async Task<List<Property>> GetRelevantPropertiesAsync(string query)
    {
        var baseQuery = _context.Properties
            .Include(p => p.Category)
            .Include(p => p.User)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .Where(p => p.Status == PropertyStatus.Active);

        // Apply filters based on query keywords
        if (query.Contains("sale") || query.Contains("buy"))
        {
            baseQuery = baseQuery.Where(p => p.ListingType == ListingType.Sale);
        }
        else if (query.Contains("rent"))
        {
            baseQuery = baseQuery.Where(p => p.ListingType == ListingType.Rent);
        }

        if (query.Contains("featured") || query.Contains("best") || query.Contains("top"))
        {
            baseQuery = baseQuery.Where(p => p.IsFeatured);
        }

        // Property type filters
        if (query.Contains("house") || query.Contains("home"))
        {
            baseQuery = baseQuery.Where(p => p.PropertyType == PropertyType.House);
        }
        else if (query.Contains("apartment") || query.Contains("apt"))
        {
            baseQuery = baseQuery.Where(p => p.PropertyType == PropertyType.Apartment);
        }
        else if (query.Contains("condo"))
        {
            baseQuery = baseQuery.Where(p => p.PropertyType == PropertyType.Condo);
        }
        else if (query.Contains("villa"))
        {
            baseQuery = baseQuery.Where(p => p.PropertyType == PropertyType.Villa);
        }

        // Bedroom filters
        for (int i = 1; i <= 10; i++)
        {
            if (query.Contains($"{i} bed") || query.Contains($"{i}bed") || query.Contains($"{i} bedroom"))
            {
                baseQuery = baseQuery.Where(p => p.Bedrooms == i);
                break;
            }
        }

        // Price filters (simple detection)
        if (query.Contains("cheap") || query.Contains("affordable") || query.Contains("budget"))
        {
            baseQuery = baseQuery.OrderBy(p => p.Price);
        }
        else if (query.Contains("luxury") || query.Contains("expensive") || query.Contains("premium"))
        {
            baseQuery = baseQuery.OrderByDescending(p => p.Price);
        }
        else
        {
            baseQuery = baseQuery.OrderByDescending(p => p.CreatedAt);
        }

        return await baseQuery.Take(10).ToListAsync();
    }

    private string BuildSystemPrompt(string databaseContext)
    {
        return $@"You are a concise Vivenda real estate assistant. Help users find properties quickly.

STRICT RULES:
1. Keep responses SHORT (2-4 sentences max).
2. For property searches, ALWAYS provide a link to the filtered results page.
3. Use these URL formats for filtered search links:
   - By type: /Properties?PropertyType=House or Apartment or Condo or Villa or Commercial
   - By listing: /Properties?ListingType=Sale or Rent
   - By price: /Properties?MaxPrice=500000 or MinPrice=200000
   - By bedrooms: /Properties?MinBedrooms=3
   - By city: /Properties?City=Miami
   - Combined: /Properties?PropertyType=House&MaxPrice=500000&ListingType=Sale
4. After the search link, mention 1-2 top picks with individual links: [View](/Properties/Details/ID)
5. Format prices with $ and commas.

EXAMPLE RESPONSES:
- ""Found 3 houses under $500k! [View All Results](/Properties?PropertyType=House&MaxPrice=500000&ListingType=Sale)

  Top pick: **Cozy Suburban House** - $450,000 - [View](/Properties/Details/2)""

- ""Here are apartments for rent: [Browse All](/Properties?PropertyType=Apartment&ListingType=Rent)

  Best value: **Downtown 1BR** - $1,800/mo - [View](/Properties/Details/8)""

- ""Looking in Miami? [See Miami Properties](/Properties?City=Miami)""

{databaseContext}

Always provide the search page link FIRST, then mention top picks!";
    }

    private async Task<string> CallGroqApiAsync(string systemPrompt, string userMessage)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return "I apologize, but the chatbot is not properly configured. Please contact the administrator.";
        }

        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            temperature = 0.7,
            max_tokens = 400
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync(_apiUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Groq API error: {response.StatusCode} - {responseContent}");
        }

        using var doc = JsonDocument.Parse(responseContent);
        var messageContent = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return messageContent ?? "I couldn't generate a response. Please try again.";
    }

    private async Task<List<PropertyDto>> ExtractSuggestedPropertiesAsync(string query, string context)
    {
        var lowerQuery = query.ToLower();
        
        // Get properties that match the query for display
        var properties = await GetRelevantPropertiesAsync(lowerQuery);
        
        return properties.Take(5).Select(p => new PropertyDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Price = p.Price,
            Address = p.Address,
            City = p.City,
            State = p.State,
            ZipCode = p.ZipCode,
            Country = p.Country,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            SquareFeet = p.SquareFeet,
            PropertyType = p.PropertyType.ToString(),
            ListingType = p.ListingType.ToString(),
            Status = p.Status.ToString(),
            MainImageUrl = p.MainImageUrl,
            IsFeatured = p.IsFeatured,
            CategoryName = p.Category?.Name,
            AgentName = $"{p.User?.FirstName} {p.User?.LastName}".Trim(),
            AgentEmail = p.User?.Email,
            AgentPhone = p.User?.PhoneNumber,
            CreatedAt = p.CreatedAt,
            Amenities = p.PropertyAmenities?.Select(pa => pa.Amenity?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
        }).ToList();
    }
}

