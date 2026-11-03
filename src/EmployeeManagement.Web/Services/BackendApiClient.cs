using System.Net.Http.Json;

namespace EmployeeManagement.Web.Services;

public sealed record ApiHealthResponse(string Status);

public sealed class BackendApiClient(HttpClient httpClient)
{
    public Task<ApiHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<ApiHealthResponse>("api/health", cancellationToken);
}
