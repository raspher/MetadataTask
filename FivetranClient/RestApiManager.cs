using System.Net;
using FivetranClient.Fetchers;
using FivetranClient.Infrastructure;
using FivetranClient.Models;

namespace FivetranClient;

public class RestApiManager : IDisposable
{
    private readonly PaginatedFetcher _paginatedFetcher;
    private readonly NonPaginatedFetcher _nonPaginatedFetcher;
    // Indicates whether this instance owns the HttpClient and should dispose it.
    private readonly HttpClient? _createdClient;

    private static readonly Uri ApiBaseUrl = new("https://api.fivetran.com/v1/");

    public RestApiManager(string apiKey, string apiSecret, TimeSpan timeout)
    {
        var client = new FivetranHttpClient(ApiBaseUrl, apiKey, apiSecret, timeout);
        var requestHandler = new HttpRequestHandler(client);
        _paginatedFetcher = new PaginatedFetcher(requestHandler);
        _nonPaginatedFetcher = new NonPaginatedFetcher(requestHandler);
        _createdClient = client;
    }

    public IAsyncEnumerable<Group> GetGroupsAsync(CancellationToken cancellationToken)
    {
        const string endpointPath = "groups";
        return _paginatedFetcher.FetchItemsAsync<Group>(endpointPath, cancellationToken);
    }

    public IAsyncEnumerable<Connector> GetConnectorsAsync(string groupId, CancellationToken cancellationToken)
    {
        var endpointPath = $"groups/{WebUtility.UrlEncode(groupId)}/connectors";
        return _paginatedFetcher.FetchItemsAsync<Connector>(endpointPath, cancellationToken);
    }

    public async Task<DataSchemas?> GetConnectorSchemasAsync(
        string connectorId,
        CancellationToken cancellationToken)
    {
        var endpointPath = $"connectors/{WebUtility.UrlEncode(connectorId)}/schemas";
        return await _nonPaginatedFetcher.FetchAsync<DataSchemas>(endpointPath, cancellationToken);
    }

    public void Dispose()
    {
        _createdClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}