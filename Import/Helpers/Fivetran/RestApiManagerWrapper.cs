using FivetranClient;
using FivetranClient.Models;

namespace Import.Helpers.Fivetran;

public class RestApiManagerWrapper : IDisposable
{
    public RestApiManagerWrapper(RestApiManager restApiManager, string groupId)
    {
        RestApiManager = restApiManager;
        GroupId = groupId;
    }

    public RestApiManager RestApiManager { get; }
    // todo: GroupId seems to be only one reason for this class
    public string GroupId { get; }

    public void Dispose()
    {
        RestApiManager.Dispose();
        GC.SuppressFinalize(this);
    }

    public IAsyncEnumerable<Connector> GetConnectorsAsync(CancellationToken cancellationToken)
    {
        return RestApiManager.GetConnectorsAsync(GroupId, cancellationToken);
    }

    public async Task<DataSchemas?> GetConnectorSchemasAsync(string connectorId, CancellationToken none)
    {
        return await RestApiManager.GetConnectorSchemasAsync(connectorId, none);
    }
}