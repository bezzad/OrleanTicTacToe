using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Silo;

public static class HostHelper
{
    public static int GetInstanceId(this string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Trim(' ', '-').Equals("instanceid", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(args[i + 1], out var instanceId))
            {
                return instanceId;
            }
        }

        return 0;
    }

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

    public static int GetActiveSiloCount(string connectionString, string clusterId)
    {
        var query = $@"SELECT COUNT(*) FROM OrleansMembershipTable " +
                    $@"WHERE Status = 3 and DeploymentId = '{clusterId}'"; // 3 is 'Active' status

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var cmd = new SqlCommand(query, connection);
        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }
}
