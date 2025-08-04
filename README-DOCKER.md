# Docker Setup Template

This project includes a **template-based** Docker Compose configuration that generates SQL Server setup dynamically based on your `Directory.Build.props` settings.

## Prerequisites

- Docker Desktop installed and running
- .NET 9 SDK
- PowerShell (Windows) or Bash with xmllint (Linux/macOS)

## Quick Start

1. **Generate Docker configuration:**
   
   **Windows (PowerShell):**
   ```powershell
   .\generate-docker-config.ps1
   ```
   
   **Linux/macOS (Bash):**
   ```bash
   ./generate-docker-config.sh
   ```

2. **Start the database:**
   ```bash
   docker-compose -f docker-compose.generated.yml up -d
   ```

3. **Update connection string in appsettings.json** (copy from script output)

4. **Apply Entity Framework migrations:**
   ```bash
   cd OptimalyChat.PresentationLayer
   dotnet ef database update
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

## How It Works

The template system uses placeholder tokens in configuration files that get replaced with actual values from `Directory.Build.props`:

### Template Files
- `docker-compose.yml` - Docker Compose template with `{{TOKENS}}`
- `init-db/01-init.sql` - SQL initialization script template

### Generated Files
- `docker-compose.generated.yml` - Ready-to-use Docker Compose file
- `init-db/01-init.generated.sql` - Database initialization script
- Connection string output for `appsettings.json`

## Configuration

All Docker settings are controlled via `Directory.Build.props`:

```xml
<PropertyGroup>
  <AppName>YourAppName</AppName>
  
  <!-- Docker Configuration -->
  <DockerSqlServerPort>1434</DockerSqlServerPort>
  <DockerDbName>$(AppName)_db</DockerDbName>
  <DockerDbUser>sa</DockerDbUser>
  <DockerDbPassword>$(AppName)2024!</DockerDbPassword>
</PropertyGroup>
```

## Template Tokens

The following tokens are replaced during generation:

| Token | Source | Description |
|-------|--------|-------------|
| `{{APP_NAME}}` | `<AppName>` | Application name |
| `optimalychat` | `<AppName>` (lowercase) | For container names |
| `{{DOCKER_SQLSERVER_PORT}}` | `<DockerSqlServerPort>` | SQL Server port |
| `{{DOCKER_DB_NAME}}` | `<DockerDbName>` | Database name |
| `{{DOCKER_DB_PASSWORD}}` | `<DockerDbPassword>` | SA password |

## Database Management

### Connect to SQL Server
Connection details are dynamically generated. After running the generation script, use:
- **Server:** localhost,{YOUR_PORT}
- **Authentication:** SQL Server Authentication  
- **User:** sa
- **Password:** {YOUR_PASSWORD}

### Reset Database
```bash
docker-compose -f docker-compose.generated.yml down -v
docker-compose -f docker-compose.generated.yml up -d
cd OptimalyChat.PresentationLayer
dotnet ef database update
```

## Project Forking

When forking this template:

1. **Update `Directory.Build.props`:**
   ```xml
   <AppName>MyNewProject</AppName>
   <DockerSqlServerPort>1435</DockerSqlServerPort> <!-- Avoid conflicts -->
   ```

2. **Regenerate Docker configuration:**
   ```bash
   ./generate-docker-config.sh  # or .ps1 on Windows
   ```

3. **Update connection string** in `appsettings.json` with the generated value

4. **Start your environment:**
   ```bash
   docker-compose -f docker-compose.generated.yml up -d
   ```

## Troubleshooting

### Generation Script Issues
```bash
# Check if Directory.Build.props exists and is valid XML
cat Directory.Build.props

# Linux/macOS: Install xmllint if needed
sudo apt-get install libxml2-utils  # Ubuntu/Debian
brew install libxml2                 # macOS
```

### Port Conflicts
Simply change `DockerSqlServerPort` in `Directory.Build.props` and regenerate:
```bash
./generate-docker-config.sh
```

### Container Issues
```bash
# View logs (use your generated file)
docker-compose -f docker-compose.generated.yml logs sqlserver

# Restart services
docker-compose -f docker-compose.generated.yml restart
```