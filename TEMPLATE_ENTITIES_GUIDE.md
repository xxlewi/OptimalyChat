# 📋 Template Entities Guide

This guide shows how to create new entities using the Template CRUD pattern as a reference.

## 🎯 Template Entities Overview

The template includes complete CRUD examples:
- **TemplateProduct** - Main entity with complex business logic
- **TemplateCategory** - Lookup/reference entity 
- **Complete CRUD implementation** across all layers

## 🏗️ Architecture Pattern

### 1. Data Layer (`OptimalyChat.DataLayer`)
```
📁 Entities/
├── YourEntity.cs              # Entity with BaseEntity inheritance
└── YourLookupEntity.cs        # Lookup entities

📁 Configurations/
├── YourEntityConfiguration.cs # EF Core configuration
└── YourLookupConfiguration.cs # Lookup configuration
```

### 2. Service Layer (`OptimalyChat.ServiceLayer`)
```
📁 DTOs/
├── YourEntityDto.cs           # Data transfer objects

📁 Interfaces/
├── IYourEntityService.cs      # Service contracts

📁 Services/
├── YourEntityService.cs       # Business logic implementation
```

### 3. Presentation Layer (`OptimalyChat.PresentationLayer`)
```
📁 ViewModels/
├── YourEntityViewModel.cs     # UI-specific models

📁 Controllers/
├── YourEntitiesController.cs  # MVC controller

📁 Views/YourEntities/
├── Index.cshtml              # List view with search/filters
├── Details.cshtml            # Detail view
├── Create.cshtml             # Create form
├── Edit.cshtml               # Edit form
└── Delete.cshtml             # Delete confirmation
```

## 🚀 Step-by-Step Creation

### Step 1: Create Entity
```csharp
public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}
```

### Step 2: EF Configuration
```csharp
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
        
        builder.HasIndex(c => c.Email).IsUnique();
        
        // Seed data (use static dates!)
        builder.HasData(
            new Customer 
            { 
                Id = 1, 
                FirstName = "John", 
                LastName = "Doe",
                Email = "john@example.com",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
```

### Step 3: Add to DbContext
```csharp
// In ApplicationDbContext.cs
public DbSet<Customer> Customers { get; set; }
```

### Step 4: Create Migration
```bash
cd OptimalyChat.DataLayer
dotnet ef migrations add AddCustomer --startup-project ../OptimalyChat.PresentationLayer
dotnet ef database update --startup-project ../OptimalyChat.PresentationLayer
```

### Step 5: Create DTO
```csharp
public class CustomerDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
```

### Step 6: Service Interface & Implementation
```csharp
public interface ICustomerService : IBaseService<CustomerDto>
{
    Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}

public class CustomerService : BaseService<Customer, CustomerDto>, ICustomerService
{
    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper) 
        : base(unitOfWork, mapper) { }

    public override async Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken cancellationToken = default)
    {
        // Custom validation
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException(nameof(dto.Email), "Email is required");
            
        // Check for duplicates
        var existing = await GetByEmailAsync(dto.Email, cancellationToken);
        if (existing != null)
            throw new BusinessException($"Customer with email '{dto.Email}' already exists", "CUSTOMER_EXISTS");
            
        return await base.CreateAsync(dto, cancellationToken);
    }

    public async Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Customer>();
        var customers = await repository.FindAsync(c => c.Email == email, cancellationToken);
        var customer = customers.FirstOrDefault();
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }
}
```

### Step 7: Register Services
```csharp
// In ServiceLayer/Extensions/ServiceCollectionExtensions.cs
services.AddScoped<ICustomerService, CustomerService>();
```

### Step 8: AutoMapper Configuration
```csharp
// In ServiceLayer/Mapping/MappingProfile.cs
CreateMap<Customer, CustomerDto>().ReverseMap();

// In PresentationLayer/Mapping/ViewModelMappingProfile.cs  
CreateMap<CustomerDto, CustomerViewModel>().ReverseMap();
```

### Step 9: ViewModel
```csharp
public class CustomerViewModel : BaseViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
}
```

### Step 10: Controller
```csharp
[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;
    
    // Follow the TemplateProductsController pattern
    // - Index with search/filters/pagination
    // - Details, Create, Edit, Delete actions
    // - Proper error handling with try/catch
    // - ValidationException and BusinessException handling
}
```

### Step 11: Views
Create AdminLTE views following the TemplateProducts pattern:
- **Index.cshtml** - Table with search, filters, pagination
- **Create.cshtml** - Form with validation
- **Edit.cshtml** - Edit form
- **Details.cshtml** - Read-only details
- **Delete.cshtml** - Confirmation page

### Step 12: Add to Navigation
```html
<!-- In _AdminLTE_Layout.cshtml -->
<li class="nav-item">
    <a asp-controller="Customers" asp-action="Index" class="nav-link">
        <i class="nav-icon fas fa-users"></i>
        <p>Customers</p>
    </a>
</li>
```

## 🔧 Key Patterns to Follow

### ✅ DO:
- Inherit from `BaseEntity` for audit fields
- Use `IBaseService<TDto>` pattern for consistent API
- Add proper validation in service layer
- Use AutoMapper for all mappings
- Follow AdminLTE styling in views
- Add comprehensive error handling
- Use `ConfigureAwait(false)` in async methods
- Support `CancellationToken` parameters

### ❌ DON'T:
- Skip business validation in service layer
- Put business logic in controllers
- Use `DateTime.Now` - always `DateTime.UtcNow`
- Use dynamic values in EF seed data
- Forget to add services to DI container
- Skip AutoMapper configuration

## 🗑️ Removing Template Entities

When ready for production:

1. **Remove template files:**
```bash
# Entities
rm OptimalyChat.DataLayer/Entities/TemplateProduct.cs
rm OptimalyChat.DataLayer/Entities/TemplateCategory.cs
rm OptimalyChat.DataLayer/Configurations/TemplateProductConfiguration.cs
rm OptimalyChat.DataLayer/Configurations/TemplateCategoryConfiguration.cs

# Service Layer
rm OptimalyChat.ServiceLayer/DTOs/TemplateProductDto.cs
rm OptimalyChat.ServiceLayer/DTOs/TemplateCategoryDto.cs
rm OptimalyChat.ServiceLayer/Interfaces/ITemplateProductService.cs
rm OptimalyChat.ServiceLayer/Interfaces/ITemplateCategoryService.cs
rm OptimalyChat.ServiceLayer/Services/TemplateProductService.cs
rm OptimalyChat.ServiceLayer/Services/TemplateCategoryService.cs

# Presentation Layer
rm OptimalyChat.PresentationLayer/ViewModels/TemplateProductViewModel.cs
rm OptimalyChat.PresentationLayer/Controllers/TemplateProductsController.cs
rm -rf OptimalyChat.PresentationLayer/Views/TemplateProducts/
```

2. **Remove from DbContext:**
```csharp
// Remove these lines from ApplicationDbContext.cs
public DbSet<TemplateProduct> TemplateProducts { get; set; }
public DbSet<TemplateCategory> TemplateCategories { get; set; }
```

3. **Remove from DI:**
```csharp
// Remove from ServiceCollectionExtensions.cs
services.AddScoped<ITemplateCategoryService, TemplateCategoryService>();
services.AddScoped<ITemplateProductService, TemplateProductService>();
```

4. **Remove from AutoMapper:**
```csharp
// Remove template mappings from both mapping profiles
```

5. **Remove from navigation:**
```html
<!-- Remove Template Products menu item -->
```

6. **Create migration:**
```bash
dotnet ef migrations add RemoveTemplateEntities --startup-project ../OptimalyChat.PresentationLayer
dotnet ef database update --startup-project ../OptimalyChat.PresentationLayer
```

## 🎯 Best Practices Summary

1. **Follow the 3-layer pattern** consistently
2. **Use proper error handling** with custom exceptions  
3. **Validate at service layer** not just UI
4. **Keep controllers thin** - delegate to services
5. **Use AutoMapper** for all object mapping
6. **Follow AdminLTE patterns** for consistent UI
7. **Add proper logging** for debugging
8. **Use async/await properly** with ConfigureAwait(false)
9. **Support cancellation tokens** for responsive UI
10. **Document your business rules** in code comments

This template provides a solid foundation for any CRUD entity in your application! 🚀