# ☕🍹 Camellia Café & Restaurant POS System

A professional, touch-first Point of Sale (POS) system built with .NET 8, featuring offline-first architecture, JWT authentication, inventory management with approval workflow, analytics, and hardware integration.

## 🚀 Features

### Core Features
- ✅ **Touch-First UI** - Large buttons, eye-catchy design optimized for touch screens
- ✅ **JWT Authentication** - Secure login with role-based access control
- ✅ **Fast Billing** - Ultra-fast order processing optimized for juice bar speed
- ✅ **Inventory Management** - Full inventory control with approval workflow
- ✅ **Analytics & Reports** - Daily, weekly, monthly, yearly sales and profit tracking
- ✅ **Offline-First** - SQLite database works without internet
- ✅ **Happy Hour Pricing** - Time-based automatic discounts
- ✅ **Combo Items** - Pre-configured combo deals (juice + snack)
- ✅ **Waste Tracking** - Track spoilage and waste for accurate profit calculation
- ✅ **Hardware Ready** - Barcode scanner, thermal printer, cash drawer support

### User Roles
- **Admin** - Full access to all features
- **Manager** - Analytics + Approval access
- **Cashier** - Billing only
- **Inventory** - Add/Edit items (pending approval)

## 🛠️ Technology Stack

### Backend
- **ASP.NET Core 8.0** Web API
- **SQLite** Database (Offline-first)
- **Entity Framework Core** ORM
- **JWT Bearer** Authentication
- **BCrypt** Password Hashing

### Frontend
- **WPF (.NET 8)** Windows Desktop Application
- **MVVM Architecture** (CommunityToolkit.Mvvm)
- **Material Design** UI Components
- **Touch-Optimized** Layout

## 📋 Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Windows 10/11 (for WPF application)

## 🔧 Installation & Setup

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend/CamelliaPOS.API
```

2. Restore packages:
```bash
dotnet restore
```

3. Run the API:
```bash
dotnet run
```

The API will start on `https://localhost:7000` (or `http://localhost:5000`)

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend/CamelliaPOS.WPF
```

2. Update API URL in `Services/ApiService.cs`:
```csharp
BaseAddress = new Uri("https://localhost:7000/api/") // Update with your API URL
```

3. Build and run:
```bash
dotnet build
dotnet run
```

## 🔐 Default Login Credentials

```
Username: camellia
Password: camellia123
```

## 📁 Project Structure

```
CAMELLIA CAFÉ & RESTAURANT/
├── backend/
│   └── CamelliaPOS.API/
│       ├── Controllers/      # API Controllers
│       ├── Data/             # DbContext & Database
│       ├── DTOs/             # Data Transfer Objects
│       ├── Models/           # Entity Models
│       ├── Services/         # Business Logic Services
│       └── Program.cs        # Application Entry Point
│
└── frontend/
    └── CamelliaPOS.WPF/
        ├── Models/           # Data Models
        ├── ViewModels/       # MVVM ViewModels
        ├── Views/            # XAML Views
        ├── Services/         # API Service Client
        └── Converters/       # Value Converters
```

## 🎯 API Endpoints

### Authentication
- `POST /api/auth/login` - User login

### Categories
- `GET /api/categories` - Get all categories

### Menu Items
- `GET /api/menuitems/category/{id}` - Get items by category
- `GET /api/menuitems/{id}` - Get item by ID
- `GET /api/menuitems/barcode/{barcode}` - Get item by barcode
- `POST /api/menuitems` - Create menu item (Admin/Inventory)
- `PUT /api/menuitems/{id}` - Update menu item (Admin/Inventory)
- `GET /api/menuitems/pending` - Get pending items (Admin/Manager)
- `POST /api/menuitems/{id}/approve` - Approve item (Admin/Manager)
- `POST /api/menuitems/{id}/reject` - Reject item (Admin/Manager)

### Orders
- `POST /api/orders` - Create new order
- `GET /api/orders/{id}` - Get order details

### Analytics
- `GET /api/analytics/daily?date={date}` - Daily sales report
- `GET /api/analytics/weekly?startDate={date}` - Weekly sales report
- `GET /api/analytics/monthly?year={year}&month={month}` - Monthly sales report
- `GET /api/analytics/yearly?year={year}` - Yearly sales report
- `GET /api/analytics/profit?startDate={date}&endDate={date}` - Profit analysis

### Waste Tracking
- `POST /api/waste` - Record waste (Admin/Manager/Inventory)
- `GET /api/waste?startDate={date}&endDate={date}` - Get waste records

## 🖨️ Hardware Integration

### Thermal Printer (ESC/POS)
- Receipt printing support
- Cash drawer control via printer

### Barcode Scanner
- USB HID barcode scanner support
- Automatic item lookup

### Cash Drawer
- Opens automatically on cash payment
- Connected via receipt printer (RJ11)

## 📊 Features in Detail

### Approval Workflow
1. Inventory user adds/edits menu item
2. Status changes to **PENDING**
3. Admin/Manager reviews and approves/rejects
4. Billing blocked until all pending items are approved

### Happy Hour Pricing
- Configure time-based discounts
- Automatically applied during specified hours
- Shown on bill and logged in analytics

### Combo Items
- Create combo deals (e.g., Mango Juice + Samosa)
- Treated as single item for faster billing
- Better analytics tracking

## 🔒 Security

- JWT token-based authentication
- Password hashing with BCrypt
- Role-based authorization
- Secure API endpoints

## 📝 Receipt Format

```
CAMELLIA CAFE & RESTAURANT
--------------------------------
Item            Qty   Price
--------------------------------
Orange Juice     2    600.00
Chicken Roll     1    350.00
--------------------------------
Subtotal:             950.00
Discount:              50.00
TOTAL:                900.00
Paid:               1000.00
Balance:              100.00
--------------------------------
Thank You ❤️

System by VOXOsolution
📞 0710901871
📧 voxosolution@gmail.com

© VOXOsolution
```

## 🚧 Future Enhancements

- [ ] Customer display support
- [ ] Daily auto backup implementation
- [ ] Cloud sync option
- [ ] Multi-location support
- [ ] Advanced reporting with charts
- [ ] Mobile app integration

## 👨‍💻 Development

### Running Tests
```bash
# Backend
cd backend/CamelliaPOS.API
dotnet test

# Frontend
cd frontend/CamelliaPOS.WPF
dotnet test
```

### Database Migrations
```bash
cd backend/CamelliaPOS.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 📞 Support

**VOXOsolution**
- 📞 Phone: 0710901871
- 📧 Email: voxosolution@gmail.com

## 📄 License

Copyright © VOXOsolution. All rights reserved.

---

**Built with ❤️ for Camellia Café & Restaurant**

