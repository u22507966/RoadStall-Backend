# Swagger API Usage Guide

## How to Add Data Using Swagger

After restarting your application, Swagger will now show simplified JSON schemas without all the nested navigation properties.

### Creating a New Stock

**Endpoint:** `POST /api/Stocks`

**Simple JSON:**
```json
{
  "stockName": "Litchi 2KG",
  "quantity": 0,
  "price": 15
}
```

**Notes:**
- Don't include `id` - it's auto-generated
- Don't include `stockTakes`, `stockChanges`, or `sales` - they're navigation properties

---

### Creating a New User

**Endpoint:** `POST /api/Users`

**Simple JSON:**
```json
{
  "username": "john_doe",
  "password": "password123",
  "email": "john@example.com",
  "phone": "0123456789"
}
```

**Notes:**
- `phone` is optional (can be null or omitted)

---

### Creating a New StockTake

**Endpoint:** `POST /api/StockTakes`

**Simple JSON:**
```json
{
  "userId": 1,
  "stockId": 1,
  "date": "2026-01-29T14:00:00Z",
  "openingStock": 100,
  "closingStock": 95
}
```

**Notes:**
- `userId` and `stockId` must reference existing records
- `id` is auto-generated

---

### Creating a New StockChange

**Endpoint:** `POST /api/StockChanges`

**Simple JSON:**
```json
{
  "stockId": 1,
  "userId": 1,
  "changeType": "Stock Received",
  "quantity": 50
}
```

**Notes:**
- `changeType` should be either "Stock Received" or "Stock Removed"
- `changeDate` is set automatically to current time
- The stock quantity will be updated automatically

---

### Creating a New Sale

**Endpoint:** `POST /api/Sales`

**Simple JSON:**
```json
{
  "saleGroup": 1,
  "stockId": 1,
  "quantitySold": 5,
  "totalPrice": 75
}
```

**Notes:**
- `totalPrice` is the unit price (will be multiplied by `quantitySold` automatically)
- `date` is set automatically to current time
- The stock quantity will be decreased automatically

---

## Common Errors and Solutions

### Error: "The [field] field is required"
**Solution:** Make sure all required fields have values. Check that:
- String fields like `stockName`, `username` are not empty
- Numeric fields like `price`, `quantity` have valid numbers

### Error: "',' is an invalid start of a value"
**Solution:** You left a field empty. Example of wrong JSON:
```json
{
  "quantity": ,  ? WRONG - no value
}
```

Correct:
```json
{
  "quantity": 0  ? CORRECT - has a value
}
```

### Error: 400 Bad Request with "stock field is required"
**Solution:** Don't include navigation properties in your POST request. Use only the simple fields shown in this guide.

---

## Testing Order

If starting with an empty database, create records in this order:

1. **Users first** (referenced by StockTakes and StockChanges)
2. **Stocks second** (referenced by StockTakes, StockChanges, and Sales)
3. **StockTakes** (needs existing User and Stock)
4. **StockChanges** (needs existing User and Stock)
5. **Sales** (needs existing Stock)

---

## Quick Reference: Required vs Optional Fields

### Stock
- ? Required: `stockName`, `price`
- ?? Optional: `quantity` (defaults to 0)

### User
- ? Required: `username`, `password`, `email`
- ?? Optional: `phone`

### StockTake
- ? Required: `userId`, `stockId`, `date`, `openingStock`, `closingStock`

### StockChange
- ? Required: `stockId`, `userId`, `changeType`, `quantity`
- ?? Auto-set: `changeDate`

### Sale
- ? Required: `saleGroup`, `stockId`, `quantitySold`, `totalPrice`
- ?? Auto-set: `date`
