using System.Net.Http.Json;
using System.Text.Json;
using ToDoApp.Shared.Common;

namespace ToDoApp.SharedUI.Services;

public class ApiService
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public ApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken token)
    {
        try
        {
            var response = await _client.GetAsync(endpoint, token);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>(_options, token);

                return ApiResponse<TResponse>.Success(data!, statusCode);
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(_options, token);

            return ApiResponse<TResponse>.Failure(title: problem?.Title ?? "Data Fetch Failed",
                                                  detail: problem?.Detail ?? "An error occurred while retrieving data from the server.", statusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network/Serialization Error during GET: {ex.Message}");

            return ApiResponse<TResponse>.Failure("Network Error", "Unable to communicate with the server.", 503);
        }
    }

    public async Task<ApiResponse<TResponse>> GetByIdAsync<TResponse>(string endpoint, CancellationToken token)
    {
        try
        {
            var response = await _client.GetAsync(endpoint, token);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>(_options, token);

                return ApiResponse<TResponse>.Success(data!, statusCode);
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(_options, token);

            return ApiResponse<TResponse>.Failure(title: problem?.Title ?? "Data Fetch Failed",
                                                  detail: problem?.Detail ?? "An error occurred while retrieving data from the server.", statusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network/Serialization Error during GET: {ex.Message}");

            return ApiResponse<TResponse>.Failure("Network Error", "Unable to communicate with the server.", 503);
        }
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint,
                                                                             TRequest request,
                                                                             CancellationToken token)
    {
        try
        {
            var response = await _client.PostAsJsonAsync(endpoint,
                                                         request,
                                                         options: _options,
                                                         cancellationToken: token);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>(_options, token);

                return ApiResponse<TResponse>.Success(data!, statusCode);
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(_options, token);

            return ApiResponse<TResponse>.Failure(title: problem?.Title ?? "Operation Failed",
                                                  detail: problem?.Detail ?? "Unknown error!",
                                                  statusCode,
                                                  errors: problem?.Errors);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network/Serialization Error: {ex.Message}");

            return ApiResponse<TResponse>.Failure("Network Error",
                                                  "Unable to communicate with the server.",
                                                  503);
        }
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint,
                                                                            TRequest request,
                                                                            CancellationToken token)
    {
        try
        {
            var response = await _client.PutAsJsonAsync(endpoint,
                                                        request,
                                                        options: _options,
                                                        cancellationToken: token);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>(_options, token);

                return ApiResponse<TResponse>.Success(data!);
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(_options, token);

            return ApiResponse<TResponse>.Failure(title: problem?.Title ?? "Operation Failed",
                                                  detail: problem?.Detail ?? "Unknown error!",
                                                  statusCode,
                                                  errors: problem?.Errors);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network/Serialization Error: {ex.Message}");

            return ApiResponse<TResponse>.Failure("Network Error",
                                                  "Unable to communicate with the server.",
                                                  503);
        }

    }

    public async Task<ApiResponse<TResponse>> PatchAsync<TResponse>(string endpoint,
                                                                    CancellationToken token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);

            var response = await _client.SendAsync(request, cancellationToken: token);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return ApiResponse<TResponse>.Success(default!);
                }

                var data = await response.Content.ReadFromJsonAsync<TResponse>(_options, token);
                return ApiResponse<TResponse>.Success(data!);
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(_options, token);

            return ApiResponse<TResponse>.Failure(title: problem?.Title ?? "Operation Failed",
                                                  detail: problem?.Detail ?? "Unknown error!",
                                                  statusCode,
                                                  errors: problem?.Errors);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network/Serialization Error: {ex.Message}");

            return ApiResponse<TResponse>.Failure("Network Error",
                                                  "Unable to communicate with the server.",
                                                  503);
        }

    }

    public async Task<ApiResponse<string>> DeleteAsync(string endpoint, CancellationToken token)
    {
        try
        {
            var response = await _client.DeleteAsync(endpoint, token);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var successMessage = await response.Content.ReadAsStringAsync(token);
                if (string.IsNullOrWhiteSpace(successMessage))
                {
                    successMessage = "Item deleted successfully.";
                }

                return ApiResponse<string>.Success(successMessage, statusCode);
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(_options, token);

            return ApiResponse<string>.Failure(title: problem?.Title ?? "Deletion Failed",
                                               detail: problem?.Detail ?? "The server rejected the deletion request.",
                                               statusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network/Serialization Error during DELETE: {ex.Message}");

            return ApiResponse<string>.Failure("Network Error", "Unable to communicate with the server.", 503);
        }
    }
}
