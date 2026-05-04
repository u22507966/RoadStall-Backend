# Create Azure Timer Function (Simple Method - No Local Code!)

This guide shows you how to create a timer function **directly in Azure Portal** to call your `GetExportData` endpoint at 11:50 PM daily.

## Prerequisites

? You must have an **Azure Function App** already created in Azure
- If you don't have one, I'll show you how to create it below

---

## Step 1: Create Azure Function App (If You Don't Have One)

1. Go to **Azure Portal** (https://portal.azure.com)
2. Click **"Create a resource"** (top left)
3. Search for **"Function App"** and click it
4. Click **"Create"**
5. Fill in the details:
   - **Subscription**: Your Azure subscription
   - **Resource Group**: Choose existing or create new (e.g., "RoadStall-RG")
   - **Function App name**: Something unique (e.g., "roadstall-functions")
   - **Runtime stack**: **.NET**
   - **Version**: **8 (LTS), isolated worker model**
   - **Region**: Choose closest to your users
   - **Operating System**: **Windows** (easier)
   - **Plan type**: **Consumption (Serverless)** (cheapest - only pay when it runs!)
6. Click **"Review + Create"**
7. Click **"Create"**
8. Wait for deployment (takes 1-2 minutes)

---

## Step 2: Create the Timer Function

### Option A: Using "Development Tools" Menu (Easiest)

1. Go to your **Function App** in Azure Portal
2. In the left menu, under **"Functions"**, click **"Functions"**
3. Click **"+ Create"** at the top
4. You should see a panel on the right side:
   - **Development environment**: Select **"Develop in portal"**
   - **Template**: Select **"Timer trigger"**
   - **New Function**: Name it **"DailyExportFunction"**
   - **Schedule**: Enter **`0 50 23 * * *`** (runs at 11:50 PM daily)
5. Click **"Create"**

### Option B: If You Don't See "Create" Button

Sometimes the UI is different. Try this:

1. Go to your **Function App** in Azure Portal
2. In the left menu, under **"Development Tools"**, click **"Advanced Tools"** or **"App Service Editor"**
3. OR look for **"Functions"** in the left menu and click **"+ Add"** or **"+ New Function"**

If you still don't see it:
- Make sure your Function App is **running** (not stopped)
- Check that you selected **".NET 8 isolated"** when creating the Function App
- You may need to enable **"In-portal editing"** in Configuration settings

---

## Step 3: Add the Code

1. After creating the function, you'll see a code editor
2. **Replace ALL the code** with this:

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class DailyExportFunction
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public DailyExportFunction(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<DailyExportFunction>();
        _httpClient = httpClientFactory.CreateClient();
    }

    [Function("DailyExportFunction")]
    public async Task Run([TimerTrigger("0 50 23 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Daily Export Function executed at: {DateTime.Now}");

        try
        {
            var apiUrl = Environment.GetEnvironmentVariable("ApiBaseUrl");
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("ApiBaseUrl not configured!");
                return;
            }

            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var url = $"{apiUrl}/api/StockTakeHistory/export/{today}";

            _logger.LogInformation($"Calling: {url}");

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Success! Retrieved export data for {today}");
            }
            else
            {
                _logger.LogWarning($"Failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
        }
    }
}
```

**IMPORTANT NOTE**: If Azure Portal doesn't support .NET 8 isolated code editing, you may need to use **Option 1 (Logic Apps)** instead, which I'll explain below.

---

## Step 4: Configure the API URL

1. In your Function App, go to **"Configuration"** (in the left menu under "Settings")
2. Under **"Application settings"**, click **"+ New application setting"**
3. Add:
   - **Name**: `ApiBaseUrl`
   - **Value**: Your API URL (e.g., `https://your-api.azurewebsites.net`)
   - **DO NOT include trailing slash!**
4. Click **"OK"**
5. Click **"Save"** at the top
6. Click **"Continue"** when prompted

---

## Step 5: Test the Function

1. Go back to your function (**Functions** ? **DailyExportFunction**)
2. Click **"Code + Test"** in the left menu
3. Click **"Test/Run"** at the top
4. Click **"Run"**
5. Check the **"Logs"** at the bottom to see if it worked

---

## Step 6: Monitor the Function

To see when the function runs and check logs:

1. In your function, click **"Monitor"** in the left menu
2. You'll see:
   - Execution history (when it ran)
   - Success/failure status
   - Logs for each execution

---

## ?? ALTERNATIVE: Logic Apps (Even Simpler!)

If the above is too complicated, try **Logic Apps** instead:

### Create a Logic App

1. Go to **Azure Portal**
2. Click **"Create a resource"**
3. Search for **"Logic App"** and click it
4. Click **"Create"**
5. Fill in:
   - **Subscription**: Your subscription
   - **Resource Group**: Same as your API
   - **Logic App name**: "roadstall-daily-export"
   - **Region**: Same as your API
   - **Plan type**: **Consumption** (cheapest)
6. Click **"Review + Create"** ? **"Create"**

### Build the Workflow

1. After deployment, go to your Logic App
2. Click **"Logic app designer"** in the left menu
3. Click **"+ New workflow"**
4. Name it **"DailyExport"**
5. Click **"Add a trigger"**
6. Search for **"Recurrence"** and select it
7. Set:
   - **Interval**: 1
   - **Frequency**: Day
   - Click **"Add new parameter"** ? Select **"At these hours"** and **"At these minutes"**
   - **At these hours**: 23
   - **At these minutes**: 50
8. Click **"+ New step"**
9. Search for **"HTTP"** and select it
10. Fill in:
    - **Method**: GET
    - **URI**: `https://your-api.azurewebsites.net/api/StockTakeHistory/export/@{formatDateTime(utcNow(), 'yyyy-MM-dd')}`
11. Click **"Save"** at the top

**Done!** The Logic App will now call your endpoint at 11:50 PM every day.

### Test Logic App

1. In the designer, click **"Run Trigger"** ? **"Run"**
2. Check the **"Overview"** page to see run history

---

## Which Option Should You Choose?

| Option | Difficulty | Cost | Best For |
|--------|-----------|------|----------|
| **Logic Apps** | ? Easiest | $ (slightly more) | Non-developers, visual workflow |
| **Azure Function (Portal)** | ?? Medium | $ (cheapest) | Some coding experience |
| **Azure Function (Local)** | ??? Advanced | $ (cheapest) | Professional development |

**My Recommendation**: Try **Logic Apps** first! It's visual, easy to understand, and requires zero code.

---

## Troubleshooting

### "I can't find the Create button for Functions"

- Your Function App might be using an older runtime
- Try creating a new Function App with **.NET 8 isolated worker model**
- OR use **Logic Apps** instead (easier!)

### "The function isn't running at the right time"

- Azure uses **UTC timezone** by default
- If you're in a different timezone, adjust the schedule:
  - Example: For EST (UTC-5), 11:50 PM EST = 4:50 AM UTC
  - Change schedule to: `0 50 4 * * *`

### "Can't connect to API"

- Make sure your API is **deployed and running** on Azure
- Check that `ApiBaseUrl` is set correctly (no trailing slash!)
- Test your API URL in a browser first

### "Still stuck?"

Use **Logic Apps** instead - it's much simpler and you can see exactly what's happening in the visual designer!

---

## Schedule Format Explained

`0 50 23 * * *` means:

| Position | Value | Meaning |
|----------|-------|---------|
| 1st | 0 | Second (always 0) |
| 2nd | 50 | Minute (50) |
| 3rd | 23 | Hour (11 PM in 24-hour format) |
| 4th | * | Every day of month |
| 5th | * | Every month |
| 6th | * | Every day of week |

Examples:
- `0 0 0 * * *` = Midnight every day
- `0 30 9 * * 1-5` = 9:30 AM, Monday-Friday only
- `0 0 */6 * * *` = Every 6 hours

---

## Next Steps

Once your function is working:
1. ? Check the logs to verify it's calling your API
2. ? Make sure your API endpoint is returning data
3. ? Set up **Application Insights** for better monitoring (optional)
4. ? Consider adding email notifications on failure (optional)

Good luck! ??
