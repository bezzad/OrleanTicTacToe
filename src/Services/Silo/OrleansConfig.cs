using System.Net;
using System.Net.NetworkInformation;

namespace Silo;

/// <summary>
/// Contains properties utilized for configuration Orleans
/// Clients and Cluster Nodes.
/// </summary>
public class OrleansConfig
{
    /// <summary>
    /// The IP addresses that will be utilized in the cluster.
    /// First IP address is the host address.
    /// </summary>
    public NetworkInterfaceType NetworkInterfaceType { get; set; }

    /// <summary>
    /// The port used for Client to Server communication.
    /// </summary>
    public int GatewayPort { get; set; }

    /// <summary>
    /// The port for Silo to Silo communication
    /// </summary>
    public int SiloPort { get; set; }

    /// <summary>
    /// (ClusterId + ServiceId) are used for cluster membership
    /// </summary>
    public string ClusterId { get; set; }

    /// <summary>
    /// ServiceId is used for storage
    /// </summary>
    public string ServiceId { get; set; }

    /// <summary>
    /// Use ADO.Net for clustering with SQL Server.
    /// </summary>
    public string AdoNetClusteringConnectionString { get; set; }

    public IPAddress SiloAddress { get; set; }
}
