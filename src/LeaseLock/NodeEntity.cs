using System;
using Microsoft.WindowsAzure.Storage.Table;

class NodeEntity : TableEntity
{
    public NodeEntity(string resourceId, string discriminator)
    {
        PartitionKey = resourceId;
        RowKey = resourceId;
        MasterNodeId = discriminator;
    }

    public NodeEntity()
    {
    }

    public string ResourceId => PartitionKey;
    public string MasterNodeId { get; set; }
    public DateTime LeaseExpiration { get; set; }
}
