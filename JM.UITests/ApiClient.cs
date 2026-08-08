namespace JM.UITests;

/// <summary>
/// Lightweight wrapper around <see cref="HttpClient"/> for API integration tests,
/// pre-configured with the API base address. Authentication bearer setup can be
/// layered on top per-test as needed.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri(TestSettings.ApiBaseAddress);
    }

    public HttpResponseMessage Get(string path) => _http.GetAsync(path).Result;
    public HttpResponseMessage Post(string path, HttpContent content) => _http.PostAsync(path, content).Result;
    public HttpResponseMessage Put(string path, HttpContent content) => _http.PutAsync(path, content).Result;
    public HttpResponseMessage Delete(string path) => _http.DeleteAsync(path).Result;
}
