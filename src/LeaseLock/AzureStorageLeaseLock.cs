using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public class AzureStorageLeaseLock : ILeaseLock
{
    readonly TimeSpan LeaseDuration;
    readonly string ResourceId;
    readonly string InstanceId;
    readonly CloudTable Table;
    NodeEntity entity;

    public AzureStorageLeaseLock(
        string resourceId,
        string discriminator,
        TimeSpan leaseDuration,
        string tableName,
        string connectionString
        )
    {
        LeaseDuration = leaseDuration;
        ResourceId = resourceId;
        InstanceId = discriminator;

        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        Table = tableClient.GetTableReference(tableName);
        Table.CreateIfNotExistsAsync();
    }

    private async Task<bool> Fetch()
    {
        TableOperation retrieveOperation = TableOperation.Retrieve<NodeEntity>(ResourceId, ResourceId);
        var result = await Table.ExecuteAsync(retrieveOperation);
        entity = (NodeEntity)result.Result;
        return entity != null;
    }

    private Task Create()
    {
        entity = new NodeEntity(ResourceId, ResourceId)
        {
            LeaseExpiration = DateTime.UtcNow + LeaseDuration,
        };
        return Table.ExecuteAsync(TableOperation.Insert(entity));
    }

    public async Task<bool> TryClaim()
    {
        try
        {
            if (HasLease() || await Fetch())
            {
                var isOwner = entity.MasterNodeId == InstanceId;
                var isExpired = entity.LeaseExpiration < DateTime.UtcNow;
                if (!isOwner && !isExpired) return false;
                await Update();
            }
            else
            {
                await Create();
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            entity = null;
            return false;
        }
    }

    public bool HasLease()
    {
        return entity != null && entity.MasterNodeId == InstanceId && entity.LeaseExpiration > DateTime.UtcNow; ;
    }

    private Task Update()
    {
        var leaseExpiration = DateTime.UtcNow + LeaseDuration;
        entity.MasterNodeId = InstanceId;
        entity.LeaseExpiration = leaseExpiration;
        return Table.ExecuteAsync(TableOperation.Merge(entity));
    }

    public Task Release()
    {
        if (!HasLease()) return Task.FromResult(0);
        var instance = entity;
        entity = null;
        instance.LeaseExpiration = DateTime.UtcNow;
        return Table.ExecuteAsync(TableOperation.Delete(instance));
    }

    public void Dispose()
    {
        Release();
    }
}
