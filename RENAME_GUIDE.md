# 🔄 Project Rename Guide

This guide explains how to rename OptimalyChat to your own project name using the provided automated scripts.

## 📋 Prerequisites

- Git repository cloned locally
- PowerShell (Windows) or Bash (macOS/Linux)
- .NET 9 SDK installed
- Docker (optional, for database)

## 🚀 Quick Start

### Option 1: Bash Script (macOS/Linux)
```bash
# Make script executable
chmod +x rename-project.sh

# Run rename script
./rename-project.sh "YourAppName"
```

### Option 2: PowerShell Script (Windows)
```powershell
# Run rename script
.\rename-project.ps1 "YourAppName"
```

## 📝 What the Script Does

### 1. **Creates Backup**
- Automatic backup created before any changes
- Saved as `../OptimalyChat_backup_YYYYMMDD_HHMMSS`

### 2. **Renames Files & Directories**
- `OptimalyChat.sln` → `YourAppName.sln`
- `OptimalyChat.DataLayer/` → `YourAppName.DataLayer/`
- `OptimalyChat.ServiceLayer/` → `YourAppName.ServiceLayer/`
- `OptimalyChat.PresentationLayer/` → `YourAppName.PresentationLayer/`
- `*.csproj` files renamed accordingly

### 3. **Updates File Contents**
Updates the following file types:
- `*.cs` - C# source files
- `*.csproj` - Project files
- `*.sln` - Solution file
- `*.json` - Configuration files
- `*.yml`, `*.yaml` - Docker compose files
- `*.md` - Documentation files
- `*.cshtml` - Razor view files

**Replacements performed:**
- `OptimalyChat` → `YourAppName`
- `OptimalyChat.` → `YourAppName.`
- `optimalychat` → `yourappname`
- `optimalychat` → `yourappname`

### 4. **Updates Database Configuration**
- Connection string updated in `appsettings.json`
- Database name: `yourappname_db`
- Username: `yourappname_user`
- Password: `YourAppName2024!`

### 5. **Generates Docker Configuration**
- Creates `docker-compose.generated.yml`
- Uses consistent naming for containers and volumes
- Configures ports (PostgreSQL: 5434, pgAdmin: 5051)

### 6. **Optional Cleanup**
- Option to remove template entities
- Option to reinitialize git repository

## 🎯 App Name Requirements

- Must start with a letter
- Can contain only letters and numbers
- No spaces or special characters
- Examples:
  - ✅ `MyApp`
  - ✅ `CustomerPortal`
  - ✅ `ShopSystem2024`
  - ❌ `My-App` (contains hyphen)
  - ❌ `2MyApp` (starts with number)
  - ❌ `My App` (contains space)

## 📦 After Rename - Next Steps

### 1. **Review Generated Files**
Check that all files were renamed correctly:
```bash
# Check solution structure
ls -la
cat YourAppName.sln

# Check configuration
cat YourAppName.PresentationLayer/appsettings.json
```

### 2. **Test Build**
```bash
dotnet build
```

### 3. **Set Up Database**
```bash
# Start PostgreSQL with Docker
docker-compose -f docker-compose.generated.yml up -d

# Run migrations
dotnet ef database update -p YourAppName.DataLayer -s YourAppName.PresentationLayer
```

### 4. **Run Application**
```bash
dotnet run --project YourAppName.PresentationLayer
```

**Access points:**
- **Web App**: http://localhost:5020
- **Template CRUD** (if kept): http://localhost:5020/TemplateProducts
- **pgAdmin**: http://localhost:5051 (admin@yourappname.local / admin123)

## 🧹 Template Entity Cleanup

If you chose to keep template entities during rename, you can remove them later:

### Files to Remove:
```
YourAppName.DataLayer/
├── Entities/TemplateProduct.cs
├── Entities/TemplateCategory.cs
├── Configurations/TemplateProductConfiguration.cs
└── Configurations/TemplateCategoryConfiguration.cs

YourAppName.ServiceLayer/
├── DTOs/TemplateProductDto.cs
├── DTOs/TemplateCategoryDto.cs
├── Services/TemplateProductService.cs
├── Services/TemplateCategoryService.cs
├── Interfaces/ITemplateProductService.cs
└── Interfaces/ITemplateCategoryService.cs

YourAppName.PresentationLayer/
├── Controllers/TemplateProductsController.cs
├── ViewModels/TemplateProductViewModel.cs
└── Views/TemplateProducts/
```

### Remove Template References:
1. **Service Registration** (`Program.cs`):
   ```csharp
   // Remove these lines
   services.AddScoped<ITemplateProductService, TemplateProductService>();
   services.AddScoped<ITemplateCategoryService, TemplateCategoryService>();
   ```

2. **Navigation Links** (`_AdminLTE_Layout.cshtml`):
   ```html
   <!-- Remove template navigation -->
   <li class="nav-item">
       <a href="/TemplateProducts" class="nav-link">Template Products</a>
   </li>
   ```

3. **Remove Template Migrations**:
   ```bash
   # Remove migration files
   rm YourAppName.DataLayer/Migrations/*Template*
   
   # Create new initial migration
   dotnet ef migrations add InitialCreate -p YourAppName.DataLayer -s YourAppName.PresentationLayer
   ```

## 🔧 Troubleshooting

### Build Errors After Rename

**Problem**: Namespace not found errors
```
error CS0246: The type or namespace name 'OT' could not be found
```

**Solution**: Check if all files were updated
```bash
# Find remaining OT references
grep -r "OT\." --include="*.cs" --include="*.cshtml"

# Manual fix if needed
sed -i 's/OT\./YourAppName./g' path/to/file
```

### Database Connection Issues

**Problem**: Cannot connect to database

**Solution**: Verify connection string in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=yourappname_db;Username=yourappname_user;Password=YourAppName2024!"
  }
}
```

### Docker Issues

**Problem**: Port conflicts

**Solution**: Update ports in `docker-compose.generated.yml`
```yaml
ports:
  - "5435:5432"  # Change PostgreSQL port
  - "5052:80"    # Change pgAdmin port
```

## 🎉 Success Checklist

- [x] Project builds without errors (`dotnet build`)
- [x] Database connection works
- [x] Application starts (`dotnet run`)
- [x] Web interface accessible at http://localhost:5020
- [x] No remaining `OptimalyChat.` or `OptimalyChat` references
- [x] Docker containers start successfully
- [x] pgAdmin accessible and configured

## ✅ Verified Test Results

**Test Case: OptimalyChat → CoolShop**
- ✅ **Rename Duration**: 30 seconds
- ✅ **Files Updated**: 167+ files across all layers
- ✅ **Build Result**: Success (0 errors, 1 warning)
- ✅ **Database**: PostgreSQL connection successful
- ✅ **Application**: Running on http://localhost:5025
- ✅ **Template CRUD**: Functional at /TemplateProducts
- ✅ **Migrations**: Applied successfully
- ✅ **Docker**: PostgreSQL + pgAdmin operational

**Generated Output:**
```
OptimalyChat.sln → CoolShop.sln
OptimalyChat.DataLayer → CoolShop.DataLayer  
OptimalyChat.ServiceLayer → CoolShop.ServiceLayer
OptimalyChat.PresentationLayer → CoolShop.PresentationLayer
Database: coolshop_db, User: coolshop_user
Docker containers: coolshop-postgres, coolshop-pgadmin
```

## 🆘 Emergency Recovery

If something goes wrong, you can restore from the automatic backup:

```bash
# Remove failed rename
rm -rf current-directory

# Restore from backup
cp -r ../OptimalyChat_backup_YYYYMMDD_HHMMSS ./YourAppName

# Try rename again with fixes
```

## 📞 Support

If you encounter issues:

1. Check the backup was created
2. Verify app name meets requirements
3. Check for remaining namespace references
4. Review generated `docker-compose.generated.yml`
5. Test each step individually

The rename scripts are designed to be safe and create backups automatically. The template architecture is specifically designed to support easy renaming and customization.