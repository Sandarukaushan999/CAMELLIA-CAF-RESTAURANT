# 🚀 Quick Start Guide - Camellia POS System

## Step 1: Start the Backend API

1. Open a terminal/command prompt
2. Navigate to the backend directory:
   ```bash
   cd backend/CamelliaPOS.API
   ```
3. Run the API:
   ```bash
   dotnet run
   ```
4. The API will start on `https://localhost:7000` (check the console output for the exact URL)

## Step 2: Start the Frontend Application

1. Open a **new** terminal/command prompt
2. Navigate to the frontend directory:
   ```bash
   cd frontend/CamelliaPOS.WPF
   ```
3. **Important**: Update the API URL in `Services/ApiService.cs`:
   ```csharp
   BaseAddress = new Uri("https://localhost:7000/api/") // Match your backend URL
   ```
4. Build and run:
   ```bash
   dotnet build
   dotnet run
   ```

## Step 3: Login

Use the default credentials:
- **Username**: `camellia`
- **Password**: `camellia123`

## Step 4: Using the POS System

### Main Screen Layout

```
┌─────────────────────────────────────────────┐
│ CAMELLIA CAFÉ & RESTAURANT POS               │
├───────────────┬─────────────────┬───────────┤
│ CATEGORY BAR  │ MENU ITEMS GRID │ BILLING   │
│ (ICONS)       │ (IMAGES)        │ PANEL     │
└───────────────┴─────────────────┴───────────┘
```

### How to Process an Order

1. **Select Category**: Click on a category icon (🥤 Fruit Juices, ☕ Coffees, etc.)
2. **Add Items**: Click on menu items to add them to the cart
3. **Adjust Quantities**: Use +/- buttons in the cart
4. **Apply Discount/Tax**: Enter values in the billing panel
5. **Select Payment Method**: Choose Cash or Card
6. **Enter Paid Amount**: (For cash payments)
7. **Click PAY**: Process the order

### Features Available

- ✅ **Category Switching**: Click category icons to filter menu items
- ✅ **Cart Management**: Add, remove, adjust quantities
- ✅ **Payment Processing**: Cash and Card payments
- ✅ **Order History**: Orders are saved in the database
- ✅ **Happy Hour**: Automatic discounts during configured hours

## 🔧 Configuration

### Backend Configuration

Edit `backend/CamelliaPOS.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=camellia_pos.db"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong",
    "Issuer": "CamelliaPOS",
    "Audience": "CamelliaPOS"
  }
}
```

### Frontend Configuration

Edit `frontend/CamelliaPOS.WPF/Services/ApiService.cs`:

```csharp
BaseAddress = new Uri("https://localhost:7000/api/") // Your API URL
```

## 🖨️ Hardware Setup (Optional)

### Thermal Printer

1. Connect printer via USB/Serial
2. Note the COM port (e.g., COM1, COM2)
3. Update `HardwareService.cs` with correct port:
   ```csharp
   InitializePrinter("COM1", 9600);
   ```

### Barcode Scanner

USB HID barcode scanners work automatically - just scan items!

### Cash Drawer

Connected via printer - opens automatically on cash payment.

## 📊 Accessing Analytics

Analytics endpoints are available via API:
- Daily: `GET /api/analytics/daily?date=2024-01-15`
- Weekly: `GET /api/analytics/weekly?startDate=2024-01-01`
- Monthly: `GET /api/analytics/monthly?year=2024&month=1`
- Yearly: `GET /api/analytics/yearly?year=2024`
- Profit: `GET /api/analytics/profit?startDate=2024-01-01&endDate=2024-01-31`

## 🗄️ Database Location

The SQLite database is created automatically at:
- `backend/CamelliaPOS.API/camellia_pos.db`

## 🔄 Backups

Backups are created automatically daily in:
- `backend/CamelliaPOS.API/Backups/`

Manual backup:
```bash
POST /api/backup/create
```

## ❓ Troubleshooting

### API Not Starting
- Check if port 7000 is available
- Check firewall settings
- Verify .NET 8 SDK is installed

### Frontend Can't Connect
- Verify API is running
- Check API URL in `ApiService.cs`
- Check CORS settings in `Program.cs`

### Database Issues
- Delete `camellia_pos.db` to reset
- Database will be recreated on next run

### Login Fails
- Verify username: `camellia`
- Verify password: `camellia123`
- Check API is running

## 📞 Support

For issues or questions:
- 📞 Phone: 0710901871
- 📧 Email: voxosolution@gmail.com

---

**Happy Selling! ☕🍹**

