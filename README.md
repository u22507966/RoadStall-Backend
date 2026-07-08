# RoadStall Backend API

A comprehensive backend API for managing a roadside farmstall business - built with ASP.NET Core 8.0. This API handles inventory management, sales tracking, user authentication, and real-time push notifications for stock updates.

## What The API Does

This is the backend for a roadside stall management system. It handles:

- **Stock Management** - Track inventory, quantities, and prices
- **Sales Recording** - Record and track daily sales
- **Stock Takes** - Daily opening/closing stock counts with historical snapshots
- **User Authentication** - Login/register system with role-based access
- **Push Notifications** - Real-time web push notifications for stock requests and updates
- **Export Functionality** - Generate daily reports and export data

## Tech Stack

- **.NET 8.0** (LTS)
- **Entity Framework Core** - Database ORM
- **SQL Server** - Local development
- **SQLite** - Production/Azure deployment option
- **WebPush** - Web push notifications (VAPID protocol)
- **Swagger/OpenAPI** - API documentation

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (for local development)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/u22507966/RoadStall-Backend.git
   cd RoadStall-Backend
   ```

2. **Set up your configuration**
   
   Copy the example configuration file:
   ```bash
   cp appsettings.Example.json appsettings.json
   ```

3. **Configure your database**
   
   Open `appsettings.json` and update the connection string to match your SQL Server instance:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=RoadStallDB;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

4. **Generate VAPID keys for push notifications**
   
   You'll need VAPID keys to enable web push notifications. Generate them using the online tool:
   - Go to: https://vapidkeys.com/
   - Click "Generate VAPID Keys"
   - Copy the public and private keys
   
   Add them to your `appsettings.json`:
   ```json
   {
     "VapidSettings": {
       "PublicKey": "YOUR_GENERATED_PUBLIC_KEY",
       "PrivateKey": "YOUR_GENERATED_PRIVATE_KEY",
       "Subject": "mailto:your-email@example.com"
     }
   }
   ```

5. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Project Structure

```
RoadStallAPI/
├── Controllers/          # API endpoints
│   ├── AuthController.cs
│   ├── PushSubscriptionsController.cs
│   ├── SalesController.cs
│   ├── StocksController.cs
│   ├── StockTakesController.cs
│   └── StockTakeHistoryController.cs
├── Models/              # Data models
│   ├── DTOs/           # Data transfer objects
│   ├── Stock.cs
│   ├── Sale.cs
│   ├── User.cs
│   └── ...
├── Services/           # Business logic
│   └── AuthService.cs
├── Migrations/         # EF Core migrations
└── Program.cs         # App configuration
```

## Key Features

### Authentication
- User registration with automatic password hashing
- Login with token generation
- Role-based access control (Admin/User)
- New users start as inactive and require admin approval

### Push Notifications
- Subscribe to push notifications
- Send notifications to all users
- Send targeted notifications to admins only (for stock requests)
- Automatic cleanup of invalid subscriptions

### Stock Management
- CRUD operations for stock items
- Track quantities and prices
- Record stock changes with user attribution

### Sales Tracking
- Record individual sales
- Group sales by transaction
- Track sales history

### Daily Stock Takes
- Record opening and closing stock
- Automatic calculation of stock sold
- Historical snapshots for reporting
- Export data for specific dates

## API Endpoints

### Authentication
- `POST /api/Auth/register` - Register a new user
- `POST /api/Auth/login` - Login and get a token

### Stock
- `GET /api/Stocks` - Get all stock items
- `GET /api/Stocks/{id}` - Get a specific stock item
- `POST /api/Stocks` - Create a new stock item
- `PUT /api/Stocks/{id}` - Update a stock item
- `DELETE /api/Stocks/{id}` - Delete a stock item

### Sales
- `GET /api/Sales` - Get all sales
- `POST /api/Sales` - Record a new sale

### Stock Takes
- `GET /api/StockTakes` - Get all stock takes
- `POST /api/StockTakes` - Record a stock take

### Push Notifications
- `POST /api/PushSubscriptions/subscribe` - Subscribe to notifications
- `POST /api/PushSubscriptions/sendToAll` - Send notification to all users
- `POST /api/PushSubscriptions/sendStockRequest` - Send to admins only
- `DELETE /api/PushSubscriptions/deleteSubscription/{id}` - Unsubscribe

## Deployment

### Azure App Service

This app is configured for easy deployment to Azure App Service.

## Configuration

All sensitive configuration is stored in `appsettings.json` which is not committed to the repository.

Required configuration:
- Database connection string
- VAPID keys for push notifications
- CORS allowed origins (for your frontend URLs)

## Security Notes

⚠️ **Important**: This project is in active development, however some security features are not implemeneted yet and are mainly for demonstration/portfolio purposes. The authentication system uses:
- SHA256 for password hashing (basic, not production-grade)
- Simple Base64 token generation (not JWT)

## License

This project is open source and available for educational/portfolio purposes.

## Contact

Created by Ryan Gilbert

---
