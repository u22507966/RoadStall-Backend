# Stock Take History Implementation Guide

## ? What Has Been Created:

### 1. **Database Table: StockTakeHistory**
Stores daily snapshots of stock takes with:
- StockId, UserId, SnapshotDate
- OpeningStock, ClosingStock
- StockName (denormalized for performance)

### 2. **API Endpoints:**

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/StockTakeHistory/snapshot` | POST | Creates daily snapshot (called by Azure Function) |
| `/api/StockTakeHistory/by-date/{date}` | GET | Get history for specific date |
| `/api/StockTakeHistory/range?startDate=&endDate=` | GET | Get history for date range |
| `/api/StockTakeHistory/by-stock/{stockId}` | GET | Get history for specific stock |
| `/api/StockTakeHistory/dates` | GET | Get all available snapshot dates |
| `/api/StockTakeHistory/export/{date}` | GET | Get data formatted for Excel export |
| `/api/StockTakeHistory/cleanup?daysToKeep=90` | DELETE | Delete old records |

---

## ?? Implementation Steps:

### Step 1: Add Migration (REQUIRED)

Run in Package Manager Console:
```powershell
Add-Migration AddStockTakeHistory
Update-Database
```

This creates the `StockTakeHistory` table in your database.

---

### Step 2: Test the Snapshot Endpoint

**Via Swagger:**
1. Go to: `https://localhost:7224/swagger` (local) or Azure URL
2. Find `POST /api/StockTakeHistory/snapshot`
3. Click "Try it out" ? Execute
4. Should return:
```json
{
  "message": "Daily snapshot created successfully",
  "date": "2026-02-24",
  "count": 15
}
```

**Via cURL:**
```bash
curl -X POST https://roadstall-g5eqf4gsbcdscqer.westeurope-01.azurewebsites.net/api/StockTakeHistory/snapshot
```

---

### Step 3: Deploy Updated API to Azure

```powershell
dotnet publish -c Release -o ./publish
```

Then upload to Azure App Service.

---

### Step 4: Create Azure Function

See `AZURE_FUNCTION_GUIDE.md` for detailed instructions.

**Quick Version:**
1. Create Function App in Azure Portal
2. Add Timer Trigger Function
3. Schedule: `0 0 0 * * *` (midnight daily)
4. Code calls your API endpoint

---

## ?? How It Works:

### Daily Flow:
```
12:00 AM
   ?
Azure Function Triggers
   ?
Calls: POST /api/StockTakeHistory/snapshot
   ?
API copies all current StockTakes ? StockTakeHistory
   ?
Returns success message
```

### Excel Export Flow (Frontend):
```
1. User selects date range
2. Frontend calls: GET /api/StockTakeHistory/range?startDate=2026-02-01&endDate=2026-02-28
3. Frontend receives JSON data
4. Frontend uses a library (e.g., xlsx, ExcelJS) to generate Excel file
5. User downloads Excel
```

---

## ?? Angular Frontend Example:

### Service:
```typescript
export class HistoryService {
  private apiUrl = 'https://your-api.azurewebsites.net/api/StockTakeHistory';

  constructor(private http: HttpClient) {}

  getAvailableDates(): Observable<Date[]> {
    return this.http.get<Date[]>(`${this.apiUrl}/dates`);
  }

  getHistoryByDate(date: string): Observable<StockTakeHistoryDto[]> {
    return this.http.get<StockTakeHistoryDto[]>(`${this.apiUrl}/by-date/${date}`);
  }

  getHistoryRange(startDate: string, endDate: string): Observable<StockTakeHistoryDto[]> {
    return this.http.get<StockTakeHistoryDto[]>(
      `${this.apiUrl}/range?startDate=${startDate}&endDate=${endDate}`
    );
  }

  getExportData(date: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/export/${date}`);
  }
}
```

### Component:
```typescript
export class HistoryComponent {
  availableDates: Date[] = [];
  historyData: StockTakeHistoryDto[] = [];

  constructor(private historyService: HistoryService) {}

  ngOnInit() {
    this.loadAvailableDates();
  }

  loadAvailableDates() {
    this.historyService.getAvailableDates().subscribe(dates => {
      this.availableDates = dates;
    });
  }

  loadHistory(date: string) {
    this.historyService.getHistoryByDate(date).subscribe(data => {
      this.historyData = data;
    });
  }

  exportToExcel(date: string) {
    this.historyService.getExportData(date).subscribe(response => {
      // Use xlsx library to create Excel file
      const worksheet = XLSX.utils.json_to_sheet(response.data);
      const workbook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(workbook, worksheet, 'Stock History');
      XLSX.writeFile(workbook, `stock-history-${date}.xlsx`);
    });
  }
}
```

### Install Excel Library:
```bash
npm install xlsx
npm install --save-dev @types/xlsx
```

---

## ?? Testing Checklist:

- [ ] Run migration: `Add-Migration AddStockTakeHistory`
- [ ] Update database: `Update-Database`
- [ ] Test snapshot endpoint in Swagger
- [ ] Verify data in database (check `StockTakeHistory` table)
- [ ] Deploy to Azure
- [ ] Create Azure Function
- [ ] Test Azure Function manually
- [ ] Wait for scheduled run (or trigger manually)
- [ ] Check function logs in Azure Portal

---

## ?? Database Schema:

```sql
CREATE TABLE StockTakeHistory (
    Id INT PRIMARY KEY IDENTITY,
    StockId INT NOT NULL,
    UserId INT NOT NULL,
    SnapshotDate DATETIME2 NOT NULL,
    OpeningStock INT NOT NULL,
    ClosingStock INT NOT NULL,
    StockName NVARCHAR(MAX) NOT NULL,
    
    CONSTRAINT FK_StockTakeHistory_Stock FOREIGN KEY (StockId) REFERENCES Stock(Id),
    CONSTRAINT FK_StockTakeHistory_User FOREIGN KEY (UserId) REFERENCES [User](Id)
);

CREATE INDEX IX_StockTakeHistory_SnapshotDate ON StockTakeHistory(SnapshotDate);
CREATE INDEX IX_StockTakeHistory_StockId ON StockTakeHistory(StockId);
```

---

## ?? Important Notes:

1. **Snapshot runs at midnight** - Make sure stock takes are finalized before then
2. **One snapshot per day** - Calling snapshot multiple times on same day will fail (protection against duplicates)
3. **StockName is denormalized** - Stored in history so it doesn't change if stock name is updated later
4. **Cleanup endpoint available** - Delete old records to manage database size

---

## ?? You're Done!

Your history system is now ready. The Azure Function will automatically create daily snapshots, and users can view/export historical data through your API endpoints.

