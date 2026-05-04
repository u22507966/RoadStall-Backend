# Azure Function - Daily Stock Take Snapshot

## Create Azure Function

1. **In Azure Portal:**
   - Create a new Function App
   - Runtime: .NET 8
   - Operating System: Windows or Linux

2. **Create Function Code:**

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace StockTakeSnapshotFunction
{
    public class DailySnapshotFunction
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        // Runs every day at midnight (0 0 0 * * *)
        // Or test with every minute: 0 */1 * * * *
        [FunctionName("DailyStockTakeSnapshot")]
        public async Task Run(
            [TimerTrigger("0 0 0 * * *")] TimerInfo myTimer,
            ILogger log)
        {
            log.LogInformation($"Daily Stock Take Snapshot executed at: {DateTime.Now}");

            try
            {
                // Your API endpoint
                string apiUrl = "https://roadstall-g5eqf4gsbcdscqer.westeurope-01.azurewebsites.net/api/StockTakeHistory/snapshot";
                
                // Make POST request
                var response = await httpClient.PostAsync(apiUrl, null);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    log.LogInformation($"Snapshot created successfully: {content}");
                }
                else
                {
                    log.LogError($"Failed to create snapshot. Status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    log.LogError($"Error details: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Exception creating snapshot: {ex.Message}");
                throw;
            }
        }
    }
}
```

## Alternative: Simple HTTP Trigger (for testing)

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace StockTakeSnapshotFunction
{
    public class ManualSnapshotFunction
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        [FunctionName("ManualSnapshot")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Manual snapshot triggered");

            try
            {
                string apiUrl = "https://roadstall-g5eqf4gsbcdscqer.westeurope-01.azurewebsites.net/api/StockTakeHistory/snapshot";
                
                var response = await httpClient.PostAsync(apiUrl, null);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new OkObjectResult(new { message = "Snapshot created", details = content });
                }
                else
                {
                    return new BadRequestObjectResult(new { message = "Failed", error = content });
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
```

## Deployment Steps

### Option 1: Deploy via Visual Studio
1. Right-click Function project ? Publish
2. Select Azure Function App
3. Deploy

### Option 2: Deploy via Azure Portal
1. Go to your Function App
2. Functions ? Create
3. Choose "Timer trigger"
4. Schedule: `0 0 0 * * *`
5. Paste the code

### Option 3: Deploy via VS Code
1. Install Azure Functions extension
2. Right-click function ? Deploy to Function App

## Testing

### Test the API Endpoint Manually:
```bash
curl -X POST https://roadstall-g5eqf4gsbcdscqer.westeurope-01.azurewebsites.net/api/StockTakeHistory/snapshot
```

### Test in Swagger:
1. Go to: https://roadstall-g5eqf4gsbcdscqer.westeurope-01.azurewebsites.net/swagger
2. Find `POST /api/StockTakeHistory/snapshot`
3. Click "Try it out" ? Execute

## Timer Schedule Examples

```
0 0 0 * * *     - Every day at midnight
0 0 2 * * *     - Every day at 2 AM
0 */5 * * * *   - Every 5 minutes (testing)
0 0 0 * * 0     - Every Sunday at midnight
0 0 0 1 * *     - First day of every month
```

## Monitor Function

1. Azure Portal ? Function App ? Monitor
2. View logs in real-time
3. Check Application Insights for errors

