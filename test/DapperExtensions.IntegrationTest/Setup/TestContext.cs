using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Npgsql;
using Xunit;

namespace DapperExtensions.IntegrationTest.Setup;

public class TestContext : IAsyncLifetime
{
    private const string ContainerImageUri = "postgres";

    private readonly DockerClient _dockerClient;

    private string? _containerId;

    public TestContext()
    {
        _dockerClient = new DockerClientConfiguration(new Uri(DockerApiUri())).CreateClient();
    }

    /// <summary>
    /// Creates a Postgres Docker image and initializes the database tables
    /// </summary>
    /// <returns>Completed task when the container is started and the tables have been initialized</returns>
    public async Task InitializeAsync()
    {
        await PullImage();
        await StartContainer();

        await WaitForDatabase(TimeSpan.FromSeconds(30));

        await InitializePersonTable();
        await InitializeBookTable();
    }

    /// <summary>
    /// Attempts to kill the container created by the test and remove it
    /// </summary>
    /// <returns>Completed task once container has been killed and removed</returns>
    public async Task DisposeAsync()
    {
        if (_containerId != null)
        {
            await _dockerClient.Containers.KillContainerAsync(_containerId, new ContainerKillParameters());
            await _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters());
        }
    }

    /// <summary>
    /// Polls on database connection until the connection is established or a timeout occurs
    /// </summary>
    /// <param name="timeout">Maximum seconds to wait for database to become available</param>
    /// <returns></returns>
    /// <exception cref="NpgsqlException">Thrown if the database connection cannot be established before timeout</exception>
    private static async Task WaitForDatabase(TimeSpan timeout)
    {
        var startTime = DateTime.Now;
        var databaseAvailable = false;

        while (DateTime.Now < startTime.Add(timeout))
        {
            try
            {
                var connection = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
                await connection.OpenAsync();
                await connection.CloseAsync();

                databaseAvailable = true;

                Debug.WriteLine($"Database available after {(DateTime.Now - startTime).Seconds} seconds");

                break;
            }
            catch (NpgsqlException)
            {
                Debug.WriteLine($"Database unavailable after {(DateTime.Now - startTime).Seconds} seconds");
                await Task.Delay(1000);
            }
        }

        if (!databaseAvailable)
        {
            throw new NpgsqlException($"Failed to establish database connection after {timeout.Seconds} seconds");
        }
    }

    private static async Task InitializePersonTable()
    {
        var connection = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());

        await connection.OpenAsync();
        var sequenceCommand = new NpgsqlCommand("CREATE SEQUENCE person_id_seq " +
            "INCREMENT 1 " +
            "START 1 " +
            "MINVALUE 1 " +
            "MAXVALUE 2147483647 " +
            "CACHE 1; ", connection);
        await sequenceCommand.ExecuteNonQueryAsync();
        var command = new NpgsqlCommand("CREATE TABLE person (" +
            "id integer NOT NULL DEFAULT nextval('person_id_seq'::regclass)," +
            "first_name text," +
            "last_name text," +
            "active boolean DEFAULT true NOT NULL," +
            "date_created timestamp with time zone NOT NULL," +
            "CONSTRAINT \"PK_person\" PRIMARY KEY (id))", connection);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    private static async Task InitializeBookTable()
    {
        var connection = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());

        await connection.OpenAsync();
        var sequenceCommand = new NpgsqlCommand("CREATE SEQUENCE book_id_seq " +
            "INCREMENT 1 " +
            "START 1 " +
            "MINVALUE 1 " +
            "MAXVALUE 2147483647 " +
            "CACHE 1; ", connection);
        await sequenceCommand.ExecuteNonQueryAsync();
        var command = new NpgsqlCommand("CREATE TABLE book (" +
            "id integer NOT NULL DEFAULT nextval('book_id_seq'::regclass)," +
            "author text," +
            "title text," +
            "CONSTRAINT \"PK_book\" PRIMARY KEY (id))", connection);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    private async Task PullImage()
    {
        await _dockerClient.Images
            .CreateImageAsync(new ImagesCreateParameters
                {
                    FromImage = ContainerImageUri,
                    Tag = "latest"
                },
                new AuthConfig(),
                new Progress<JSONMessage>());
    }

    private async Task StartContainer()
    {
        var response = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = ContainerImageUri,
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                {
                    PostgresConfiguration.DatabasePort, default
                }
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        PostgresConfiguration.DatabasePort,
                        new List<PortBinding> { new() { HostPort = PostgresConfiguration.HostPort } }
                    }
                }
            },
            Env = new List<string> { $"POSTGRES_PASSWORD={PostgresConfiguration.Password}" }
        });

        _containerId = response.ID;

        await _dockerClient.Containers.StartContainerAsync(_containerId, null);
    }

    private string DockerApiUri()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
        {
            return "npipe://./pipe/docker_engine";
        }

        var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        if (isLinux)
        {
            return "unix:/var/run/docker.sock";
        }

        throw new Exception("Unable to determine what OS this is running on");
    }
}