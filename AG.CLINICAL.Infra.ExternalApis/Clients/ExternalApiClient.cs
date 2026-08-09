namespace AG.CLINICAL.Infra.ExternalApis.Clients;

public sealed class ExternalApiClient
{
    private readonly HttpClient _httpClient;

    public ExternalApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HttpClient HttpClient => _httpClient;
}
