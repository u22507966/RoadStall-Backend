# RoadStall API - Deployment Guide

## Database Configuration

This application is configured to use:
- **SQL Server** for local development
- **SQLite** for Azure/Production deployment

### Local Development (SQL Server)

The application will automatically use SQL Server when running locally. Make sure:
1. SQL Server is running
2. Connection string in `appsettings.json` is correct
3. `UseSqlite` is set to `false` in `appsettings.json`

**Development Features:**
- ? HTTP is allowed (HTTPS redirection only in production)
- ? Swagger UI available at `/swagger`
- ? SQL Server migrations apply automatically on startup
- ? CORS configured for `http://localhost:4200` (Angular dev)

To apply migrations to SQL Server manually:
```bash
dotnet ef database update
```

To create a new migration:
```bash
dotnet ef migrations add MigrationName
```

### Azure/Production Deployment (SQLite)

The application will automatically use SQLite when deployed to Azure. The database file will be created automatically at `roadstall.db` in the application directory.

#### Steps to Deploy to Azure:

1. **Build the application for production:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Configure Azure App Service:**
   - Set environment variable: `ASPNETCORE_ENVIRONMENT=Production`
   - Or add Application Setting in Azure Portal: `UseSqlite=true`

3. **Deploy files:**
   - Upload all files from the `./publish` folder to Azure App Service
   - Or use Azure DevOps/GitHub Actions for CI/CD

4. **First Run:**
   - The SQLite database will be created automatically on first run
   - The database will be empty - you'll need to seed initial data if required

#### Important Notes:

- **SQLite Database Persistence**: The SQLite database file is stored in the application directory. On Azure App Service, this may not persist across restarts unless you use:
  - Azure File Share (mount persistent storage)
  - Azure Storage (store DB file externally)
  
- **Data Loss Risk**: Without persistent storage, the SQLite database will reset if the app restarts.

#### Recommended: Use Azure SQL Database for Production

For a production environment, consider using Azure SQL Database instead of SQLite:

1. Create an Azure SQL Database
2. Add connection string to Azure App Service Application Settings:
   ```
   ConnectionStrings__DefaultConnection=Server=tcp:your-server.database.windows.net,1433;Database=RoadStallDB;...
   ```
3. Set `UseSqlite=false` in Azure App Service Application Settings
4. Run migrations:
   ```bash
   dotnet ef database update --connection "your-azure-sql-connection-string"
   ```

## Environment Variables

- `UseSqlite`: Set to `true` for SQLite, `false` for SQL Server
- `ASPNETCORE_ENVIRONMENT`: Set to `Production` for production deployment

## Migrations

All SQL Server migrations are preserved in the `Migrations` folder:
- 20260106205303_Initial
- 20260106205904_Second
- 20260107074800_Third
- 20260108221206_Fourth
- 20260109093804_Fifth

SQLite uses `EnsureCreated()` which creates the database from the current model without migrations.

## CORS Configuration

The API allows requests from:
- http://localhost:4200 (Angular dev server)
- https://roadstallprototype.netlify.app (Production frontend)

## Testing

To test SQLite locally:
1. Stop the application
2. Set `"UseSqlite": true` in `appsettings.json`
3. Run the application
4. A `roadstall.db` file will be created in the project directory
5. Remember to set it back to `false` when done testing
