using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace Silo;

public static class HostHelper
{
    public static void OpenBrowser(this Uri uri)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true, CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", uri.ToString());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", uri.ToString());
            }
        }
        catch (Exception exp)
        {
            Console.Error.WriteLine(exp.Message);
        }
    }

    public static int GetId(string sqlConn, int basePort)
    {
        var port = GetInstanceId();
        if (port >= 0)
            return port - basePort;

        var reservedPorts = GetReservedPorts(sqlConn);
        port = GetFreePort(reservedPorts, basePort, 10000);
        if (port >= 0)
            return port - basePort;

        return -1;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
    }

    private static int GetInstanceId()
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Trim(' ', '-').Equals("instanceid", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(args[i + 1], out var instanceId))
            {
                return instanceId;
            }
        }

        return -1;
    }

    private static HashSet<int> GetReservedPorts(string connectionString)
    {
        var query =
            $@"SELECT [Port] FROM [Orleans].[dbo].[OrleansMembershipTable] " +
            $@"WHERE HostName = '{Environment.MachineName}' and Status = 3;"; // 3 is 'Active' status

        var reservedPorts = new HashSet<int>();
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var cmd = new SqlCommand(query, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            reservedPorts.Add(reader.GetInt32("Port"));
        }

        return reservedPorts;
    }

    private static int GetFreePort(HashSet<int> reservedPorts, int basePort, int count = 1000)
    {
        for (int i = basePort; i < basePort + count; i++)
        {
            if (reservedPorts.Contains(i) == false && CheckAvailableServerPort(i)) // && IsPortAvailable(i) 
                return i;
        }

        return -1;
    }

    private static bool IsPortAvailable(int port)
    {
        IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
        try
        {
            TcpListener tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Start();
            tcpListener.Stop();
            return true;
        }
        catch (SocketException ex)
        {
            Console.Error.WriteLine(ex);
        }

        return false;
    }

    private static bool CheckAvailableServerPort(int port)
    {
        // Evaluate current system tcp connections. This is the same information provided
        // by the netstat command line application, just in .Net strongly-typed object
        // form.  We will look through the list, and if our port we would like to use
        // in our TcpClient is occupied, we will set isAvailable to false.
        IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();
        //TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

        foreach (IPEndPoint endpoint in tcpConnInfoArray)
        {
            if (endpoint.Port == port)
            {
                return false;
            }
        }

        return true;
     }

    public static IPAddress GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}
