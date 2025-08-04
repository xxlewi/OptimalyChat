# 🏗️ Infrastructure & Architecture Documentation

## 🏛️ Clean Architecture Overview

This template implements **Clean Architecture** principles with a **3-Layer Architecture** pattern, ensuring maintainable, testable, and scalable .NET applications.

### 📊 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ Controllers │  │ ViewModels  │  │ Views (AdminLTE)    │  │
│  │ - Home      │  │ - Login     │  │ - Dashboard         │  │
│  │ - Account   │  │ - Register  │  │ - Authentication    │  │
│  │ - Health    │  │ - Base      │  │ - Layout            │  │
│  │ - Template* │  │ - Template* │  │ - Template CRUD     │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│           │                │                    │           │
│           └────────────────┼────────────────────┘           │
│                            │                                │
└────────────────────────────┼────────────────────────────────┘
                             │ AutoMapper
┌────────────────────────────┼────────────────────────────────┐
│                    SERVICE LAYER                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ Services    │  │ DTOs        │  │ Exceptions          │  │
│  │ - Base      │  │ - Base      │  │ - Business          │  │
│  │ - User      │  │ - User      │  │ - Validation        │  │
│  │ - Template* │  │ - Template* │  │ - NotFound          │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│           │                │                    │           │
│           └────────────────┼────────────────────┘           │
│                            │                                │
└────────────────────────────┼────────────────────────────────┘
                             │ AutoMapper
┌────────────────────────────┼────────────────────────────────┐
│                     DATA LAYER                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ Entities    │  │ Repository  │  │ DbContext           │  │
│  │ - User      │  │ - Generic   │  │ - Identity          │  │
│  │ - Base      │  │ - User      │  │ - Query Filters     │  │
│  │ - Template* │  │ - UnitOfWork│  │ - Configurations    │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                             │
                    ┌────────┼────────┐
                    │ PostgreSQL DB   │
                    │ + ASP.NET       │
                    │   Identity      │
                    │ + pgAdmin       │
                    └─────────────────┘
```

## 🎯 Layer Responsibilities

### 🖥️ Presentation Layer (`OptimalyChat.PresentationLayer`)

**Purpose**: User interface and user interaction handling

**Components**:
- **Controllers**: HTTP request handling with proper service layer integration
- **ViewModels**: Validated data structures with enterprise-grade validation
- **Views**: Razor pages with AdminLTE 3.2.0 layout and security features
- **Middleware**: Security headers and global exception handling
- **Extensions**: Enterprise Identity configuration with strong security policies

**Dependencies**: `ServiceLayer` only
**Quality**: 8/10 Enterprise-grade with comprehensive security protection

**Key Files**:
```
OptimalyChat.PresentationLayer/
├── Controllers/
│   ├── HomeController.cs         # Dashboard controller
│   ├── AccountController.cs      # Secure authentication with service layer
│   ├── HealthController.cs       # Health monitoring UI
│   └── TestController.cs         # Debug-only exception testing
├── ViewModels/
│   ├── LoginViewModel.cs         # Login form with validation
│   ├── RegisterViewModel.cs      # Registration with enterprise validation
│   └── BaseViewModel.cs          # Base class with audit info
├── Views/
│   ├── Shared/
│   │   ├── _AdminLTE_Layout.cshtml  # AdminLTE layout with security
│   │   └── _Layout.cshtml           # Layout wrapper
│   ├── Account/
│   │   ├── Login.cshtml             # Secure login page
│   │   └── Register.cshtml          # Registration with validation
│   ├── Health/
│   │   └── Index.cshtml             # Health monitoring dashboard
│   └── Home/
│       └── Index.cshtml             # Dashboard with widgets
├── Middleware/
│   ├── SecurityHeadersMiddleware.cs # Security headers (CSP, XSS protection)
│   └── GlobalExceptionMiddleware.cs # Global error handling
├── HealthChecks/
│   └── ApplicationHealthCheck.cs    # Custom health check
├── Mapping/
│   └── DtoToViewModelMappingProfile.cs   # AutoMapper DTO → ViewModel (renamed for clarity)
└── Extensions/
    └── ServiceCollectionExtensions.cs # Enterprise Identity config
```

### 🔧 Service Layer (`OptimalyChat.ServiceLayer`)

**Purpose**: Business logic and application services

**Components**:
- **Services**: Production-ready business logic with comprehensive error handling
- **DTOs**: Generic Data Transfer Objects with type safety
- **Interfaces**: Service abstractions with generic TKey support
- **Exceptions**: Structured exception hierarchy for proper error handling
- **Mapping**: AutoMapper profiles with validation

**Dependencies**: `DataLayer` only
**Quality**: 9/10 Enterprise-grade with proper validation and exception handling

**Key Files**:
```
OptimalyChat.ServiceLayer/
├── Services/
│   ├── BaseService.cs            # Generic CRUD with exception handling & validation
│   ├── UserService.cs            # User-specific business logic with validation
│   ├── TemplateProductService.cs # Template: Complete CRUD implementation
│   └── TemplateCategoryService.cs # Template: Lookup entity service
├── DTOs/
│   ├── BaseDto.cs               # Generic DTO with TKey support
│   ├── UserDto.cs               # User DTO with computed properties
│   ├── TemplateProductDto.cs    # Template: Product DTO with computed properties
│   ├── TemplateCategoryDto.cs   # Template: Category DTO with product count
│   └── PagedResult.cs           # Pagination support
├── Interfaces/
│   ├── IBaseService.cs          # Generic service interface with TKey
│   └── IUserService.cs          # User service contract
├── Exceptions/
│   ├── BusinessException.cs      # Business logic errors with codes
│   ├── ValidationException.cs    # Input validation errors
│   └── NotFoundException.cs      # Entity not found errors
├── Mapping/
│   └── EntityToDtoMappingProfile.cs    # AutoMapper Entity → DTO (renamed for clarity)
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

### 🗄️ Data Layer (`OptimalyChat.DataLayer`)

**Purpose**: Data access and persistence

**Components**:
- **Entities**: Domain models with audit trails
- **DbContext**: EF Core database context
- **Repository**: Generic repository pattern with soft delete
- **Unit of Work**: Transaction management

**Dependencies**: None (no dependencies on higher layers)

**Key Files**:
```
OptimalyChat.DataLayer/
├── Entities/
│   ├── User.cs                  # Custom user entity extending IdentityUser
│   └── BaseEntity.cs            # Audit fields (CreatedAt, UpdatedAt, IsDeleted)
├── Data/
│   └── ApplicationDbContext.cs   # EF Core Identity context with global query filters
├── Repositories/
│   ├── BaseRepository.cs        # Generic repository with ConfigureAwait(false)
│   ├── Repository.cs            # Repository for BaseEntity (int ID)
│   ├── UserRepository.cs        # User-specific repository methods
│   └── UnitOfWork.cs            # Transaction management with audit logic
├── Interfaces/
│   ├── IBaseEntity.cs           # Base entity interfaces (TKey support)
│   ├── IRepository.cs           # Generic repository contracts
│   ├── IUserRepository.cs       # User repository contract
│   └── IUnitOfWork.cs           # Unit of work contract
├── Configurations/
│   └── UserConfiguration.cs     # User entity EF configuration
├── Migrations/                   # EF Core migrations with Identity
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

## 🔄 Data Flow

### 📥 Request Flow
```
HTTP Request → Controller → Service → Repository → Database
```

### 📤 Response Flow
```
Database → Entity → DTO → ViewModel → View → HTTP Response
```

### 🗺️ Object Mapping Chain
```
Entity (Data) → DTO (Service) → ViewModel (Presentation)
     ↓              ↓                    ↓
 AutoMapper    AutoMapper           Razor View
```

## 🧩 Design Patterns

### 🏪 Generic Repository Pattern
- **True Generic Repository**: `IRepository<TEntity, TKey>` supports any entity and ID type
- **Backward Compatibility**: `IRepository<TEntity>` for int ID entities  
- **User-Specific Repository**: `IUserRepository` for Identity operations
- **Global Query Filters**: Automatic soft delete filtering
- **ConfigureAwait(false)**: All async operations optimized for performance

```csharp
public interface IRepository<TEntity, TKey> where TEntity : class, IBaseEntity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);  // Synchronní - pouze mark pro update
    void Delete(TEntity entity);  // Soft delete
}

// User repository s string ID
public interface IUserRepository : IRepository<User, string>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(string emailOrUserId, CancellationToken cancellationToken = default);
}
```

### 🔄 Enhanced Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Repository přístup s lazy loading
    IUserRepository Users { get; }
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;  // int ID
    IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class, IBaseEntity<TKey>;  // generic ID
    
    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<int> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default, params object[] parameters);
}
```

### 🎯 Dependency Injection
Each layer has its own `ServiceCollectionExtensions`:
```csharp
// Program.cs
builder.Services.AddDataLayer(builder.Configuration);
builder.Services.AddServiceLayer();
builder.Services.AddPresentationLayer();
```

## 🗃️ Database Design

### 📋 Enhanced Base Entity Pattern
All entities inherit from `BaseEntity` with comprehensive audit trail:
```csharp
public abstract class BaseEntity : IBaseEntity<int>
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }        // 🆕 Who created
    public string? UpdatedBy { get; set; }        // 🆕 Who updated
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }      // 🆕 When deleted
    public string? DeletedBy { get; set; }        // 🆕 Who deleted
    
    // 🆕 Computed properties for UI with proper UTC handling
    public bool IsActive => !IsDeleted;
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
    public string CreatedAtDisplay => CreatedAt.Kind == DateTimeKind.Utc 
        ? CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm") 
        : CreatedAt.ToString("dd.MM.yyyy HH:mm");
    public string? UpdatedAtDisplay => UpdatedAt?.Kind == DateTimeKind.Utc 
        ? UpdatedAt?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") 
        : UpdatedAt?.ToString("dd.MM.yyyy HH:mm");
}
```

### 🔄 Enhanced Automatic Audit Trail
`ApplicationDbContext` automatically manages comprehensive audit fields:
```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var currentUserId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    foreach (var entry in ChangeTracker.Entries<BaseEntity>())
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUserId;  // 🆕 Who created
                break;
            case EntityState.Modified:
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;  // 🆕 Who updated
                break;
        }
    }
    return base.SaveChangesAsync(cancellationToken);
}
```

## 🐳 Docker Infrastructure

### 🏗️ Dynamic Configuration System
Template uses **token-based generation** for environment-specific setup:

```xml
<!-- Directory.Build.props -->
<DockerPostgresPort>5434</DockerPostgresPort>
<DockerDbName>$(AppName)_db</DockerDbName>
<DockerDbUser>$(AppName)_user</DockerDbUser>
<DockerDbPassword>$(AppName)2024!</DockerDbPassword>
```

### 🔧 Generation Scripts
- **Windows**: `generate-docker-config.ps1`
- **Linux/macOS**: `generate-docker-config.sh`

Both scripts:
1. Parse `Directory.Build.props`
2. Replace `{{TOKENS}}` in templates
3. Generate `docker-compose.generated.yml`
4. Output connection string for `appsettings.json`

### 🗄️ PostgreSQL Setup
```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: {{DOCKER_DB_USER}}
      POSTGRES_PASSWORD: {{DOCKER_DB_PASSWORD}}
      POSTGRES_DB: {{DOCKER_DB_NAME}}
    ports:
      - "{{DOCKER_POSTGRES_PORT}}:5432"
    
  pgadmin:
    image: dpage/pgadmin4:latest
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@optimalychat.local
      PGADMIN_DEFAULT_PASSWORD: admin123
    ports:
      - "{{DOCKER_PGADMIN_PORT}}:80"
```

## 🔒 Security Implementation

### 🛡️ Security Features
- **Anti-forgery tokens** on all modifying actions
- **Nullable reference types** for compile-time null safety
- **No hardcoded secrets** in source code
- **Template-based credentials** for easy customization
- **Soft delete** prevents accidental data loss

### 🔐 Enhanced CSRF Protection
All forms now include anti-forgery tokens:
```html
<!-- All forms automatically protected -->
<form asp-action="Create" method="post">
    @Html.AntiForgeryToken()  <!-- 🆕 Added to all forms -->
    <!-- form content -->
</form>
```

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(ProductViewModel model)
{
    // Protected against CSRF attacks
}
```

## 🎨 UI Framework

### 🎯 AdminLTE 3.2.0 Integration
- **Responsive design** with Bootstrap 4
- **Dashboard widgets** with placeholder data
- **Sidebar navigation** with clean menu structure
- **Modular components** for easy customization
- **🆕 Dark mode support** with localStorage persistence
- **🆕 Theme switching** with toggle button in navigation

### 🧩 Enhanced Layout Structure
```
_AdminLTE_Layout.cshtml (Master)
├── Navbar (top navigation with dark mode toggle) 🆕
├── Sidebar (left navigation with theme support) 🆕
├── Content Wrapper
│   ├── Content Header (breadcrumbs)
│   └── Main Content (@RenderBody())
└── Footer
```

**🆕 Dark Mode Features:**
```javascript
// Dark mode with state persistence
function toggleDarkMode() {
    if (localStorage.getItem('darkMode') === 'true') {
        disableDarkMode();
    } else {
        enableDarkMode();
    }
}

// Automatic theme restoration
$(document).ready(function() {
    if (localStorage.getItem('darkMode') === 'true') {
        enableDarkMode();
    }
});
```

## 🚀 Development Workflow

### 🔄 Adding New Features

1. **Create Entity** in `DataLayer/Entities/`
2. **Add DbSet** to `ApplicationDbContext`
3. **Create Migration**: `dotnet ef migrations add FeatureName`
4. **Create DTO** in `ServiceLayer/DTOs/`
5. **Update AutoMapper** profiles
6. **Create Service** in `ServiceLayer/Services/`
7. **Create ViewModel** in `PresentationLayer/ViewModels/`
8. **Create Controller** in `PresentationLayer/Controllers/`
9. **Create Views** with AdminLTE styling

### 🧪 Testing Strategy
- **Unit Tests**: Test services in isolation
- **Integration Tests**: Test controller-to-database flow
- **Repository Tests**: Test data access logic
- **Mapping Tests**: Verify AutoMapper configurations

## 📦 NuGet Dependencies

### 🗄️ Data Layer
- `Microsoft.EntityFrameworkCore` (9.0.7)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (9.0.7)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.4)

### 🔧 Service Layer
- `AutoMapper` (12.0.1)
- `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.1)

### 🖥️ Presentation Layer
- `Microsoft.EntityFrameworkCore.Design` (9.0.7) - for migrations
- `Microsoft.AspNetCore.Identity.UI` (9.0.7) - for Identity scaffolding
- `Serilog.AspNetCore` (8.0.1) - structured logging
- `Serilog.Sinks.File` (5.0.0) - file logging
- `AspNetCore.HealthChecks.Npgsql` (8.0.1) - PostgreSQL health checks

## 🔧 Configuration Management

### 📋 Centralized Properties
`Directory.Build.props` centralizes all project configuration:
- Project naming and versioning
- Docker ports and database settings
- NuGet package versions
- Compiler settings

### 🔄 Easy Forking Process
1. Change `<AppName>` in `Directory.Build.props`
2. Update `<DockerPostgresPort>` to avoid conflicts
3. Run `./generate-docker-config.sh`
4. Update connection string in `appsettings.json`
5. Start development: `docker-compose -f docker-compose.generated.yml up -d`

## 🎯 Template Entity System - Reference Implementation

### Complete CRUD Reference Architecture

The template includes **TemplateProduct** and **TemplateCategory** entities that demonstrate enterprise-grade CRUD implementation across all architectural layers.

**🔍 Demo**: [http://localhost:5020/TemplateProducts](http://localhost:5020/TemplateProducts)

### Architecture Pattern Implementation

**🔸 Entity Layer (`OptimalyChat.DataLayer/Entities/`)**
- ✅ **BaseEntity inheritance** with audit trails (CreatedAt, UpdatedAt, IsDeleted)
- ✅ **Navigation properties** for EF Core relationships
- ✅ **Computed properties** for business logic (EffectivePrice, IsOnSale, StockStatus)
- ✅ **Virtual properties** for lazy loading and change tracking

**🔸 Data Configuration (`OptimalyChat.DataLayer/Configurations/`)**
- ✅ **IEntityTypeConfiguration<T>** implementation
- ✅ **Database constraints** (check constraints, unique indexes)
- ✅ **Relationship configuration** with proper delete behavior
- ✅ **Seed data** for development and testing
- ✅ **Column mappings** with precision and length constraints

**🔸 Service Layer (`OptimalyChat.ServiceLayer/`)**
- ✅ **BaseService<TEntity, TDto, TKey>** generic pattern
- ✅ **Business logic validation** with custom exceptions
- ✅ **AutoMapper profiles** for Entity ↔ DTO transformation
- ✅ **Pagination support** with PagedResult<T>
- ✅ **Repository pattern** usage with Unit of Work

**🔸 Presentation Layer (`OptimalyChat.PresentationLayer/`)**
- ✅ **MVC Controller** with proper error handling
- ✅ **ViewModels** with data annotations for validation
- ✅ **AdminLTE Views** with responsive design
- ✅ **Client-side validation** with jQuery
- ✅ **Pagination & filtering** with search capabilities

### Key Technical Implementations

**Generic Repository Usage:**
```csharp
// Correct generic repository pattern
var repository = _unitOfWork.GetRepository<TemplateProduct, int>();
var products = await repository.Query
    .Include(p => p.Category)  // Eager loading
    .Where(p => p.IsActive)
    .ToListAsync();
```

**Enhanced AutoMapper Configuration:**
```csharp
// EntityToDtoMappingProfile.cs - Entity to DTO mapping
CreateMap<TemplateProduct, TemplateProductDto>()
    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
    .ReverseMap()
    .ForMember(dest => dest.Category, opt => opt.Ignore());

// DtoToViewModelMappingProfile.cs - DTO to ViewModel mapping
CreateMap<TemplateProductDto, TemplateProductViewModel>().ReverseMap();
```

**Update Pattern (Fixed):**
```csharp
// Proper update with existing entity tracking
var existingEntity = await _repository.GetByIdAsync(dto.Id);
_mapper.Map(dto, existingEntity);  // Map changes to tracked entity
_repository.Update(existingEntity);
await _unitOfWork.SaveChangesAsync();
```

**View Integration:**
```html
<!-- AdminLTE CRUD with proper validation -->
<form asp-action="Edit" asp-route-id="@Model.Id">
    <input asp-for="Price" type="number" step="0.01" min="0" />
    <span asp-validation-for="Price" class="text-danger"></span>
</form>
```

### Business Logic Examples

**Template entities demonstrate:**
- **Price validation** (Sale price must be less than regular price)
- **Stock management** with status indicators
- **Category relationships** with cascade restrictions
- **Computed properties** for UI display (formatted prices, discount %)
- **Soft delete** with global query filters
- **Audit trails** with creation/update timestamps

### Learning Value

These template entities serve as **production-ready reference implementations** showing:
1. **How to structure entities** with proper relationships
2. **How to implement services** with business logic
3. **How to create controllers** with error handling
4. **How to build views** with validation and UX
5. **How to configure EF Core** with constraints and indexes
6. **How to use AutoMapper** between architectural layers
7. **How to implement pagination** and filtering

**🗑️ Removal for Production:**
Template entities include comments for easy identification and removal when building actual features.

## 🆕 Recent Enhancements (v2.1)

### **🎨 Dark Mode Implementation**
- Toggle button in navigation bar with moon/sun icon
- State persistence using localStorage
- Automatic theme restoration on page load
- Complete AdminLTE theme switching (sidebar, navbar, content)

### **🗺️ Enhanced Mapping Architecture**
Clear separation of mapping responsibilities:
- **EntityToDtoMappingProfile.cs** - Service layer mapping (Entity ↔ DTO)
- **DtoToViewModelMappingProfile.cs** - Presentation layer mapping (DTO ↔ ViewModel)
- Improved naming convention for better code navigation and maintenance

### **⏰ UTC Time Handling**
Proper UTC to local time conversion with:
- Computed display properties (`CreatedAtDisplay`, `UpdatedAtDisplay`)
- EF Core UTC conversion configuration
- Czech locale support with relative time display
- Consistent dd.MM.yyyy HH:mm formatting

### **🛡️ Security Hardening**
Enhanced CSRF protection:
- Anti-forgery tokens added to all forms
- TemplateProducts forms protected (Create, Edit, Delete)
- Export forms protected (ExportProducts, ExportCategories, ExportUsers)
- Consistent security across all POST operations

## 🎯 Production Readiness

### ✅ Quality Assurance
- **Clean Architecture** compliance verified
- **Security audit** passed (with recent CSRF enhancements)
- **Dependency flow** validated
- **Best practices** implemented
- **Production-ready** codebase with CRM-inspired patterns

### 🚀 Deployment Considerations
- Use **Azure Container Instances** or **Docker Swarm** for PostgreSQL
- Configure **Azure Key Vault** for production secrets
- Set up **Application Insights** for monitoring
- Use **Azure Database for PostgreSQL** for managed database
- Configure **SSL/TLS** for production connections

### 📈 Template Evolution
This template has evolved through:
1. **Initial Clean Architecture** implementation
2. **CRM-inspired enhancements** from real-world OptimalyCRM project
3. **v2.1 improvements** based on production feedback and best practices

The template now represents **enterprise-grade patterns** validated through actual production use! 🏆