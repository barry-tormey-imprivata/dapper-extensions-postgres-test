using System;
using System.Net;
using System.Net.Sockets;

namespace DapperExtensions.IntegrationTest.Setup;

/// <summary>
/// Generic configuration used to establish a connection string to the database hosted in a Docker container
/// </summary>
public static class PostgresConfiguration
{
    private static string? _password;

    private static string? _hostPort;

    public static string? Password => GetRandomPassword();

    private static string Server => "127.0.0.1";

    public static string DatabasePort => "5432";

    public static string HostPort { get; } = GetFreeTcpPort();

    private static string UserId => "postgres";

    private static string Database => "postgres";

    public static string GetConnectionString(bool omitDatabase = false)
    {
        return
            $"Server={Server};Port={HostPort};{(omitDatabase ? string.Empty : $"Database={Database};")}User Id={UserId};Password={Password};";
    }

    private static string GetRandomPassword()
    {
        if (string.IsNullOrEmpty(_password))
        {
            _password = Guid.NewGuid().ToString();
        }

        return _password;
    }

    private static string GetFreeTcpPort()
    {
        if (!string.IsNullOrEmpty(_hostPort))
        {
            return _hostPort;
        }

        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        _hostPort = port.ToString();

        return _hostPort;
    }
}