using System;
using System.Threading.Tasks;
using DapperExtensions.IntegrationTest.Entities;
using DapperExtensions.IntegrationTest.Mappers;
using DapperExtensions.IntegrationTest.Setup;
using DapperExtensions.Sql;
using Npgsql;
using Xunit;

namespace DapperExtensions.IntegrationTest;

/// <summary>
/// Tests async CRUD operations with a custom <see cref="PostgresMapper{T}" /> class mapper
/// </summary>
[Collection("db")]
public class CrudAsyncTests
{
    public CrudAsyncTests()
    {
        DapperAsyncExtensions.SqlDialect = new PostgreSqlDialect();
        DapperAsyncExtensions.DefaultMapper = typeof(PostgresMapper<>);
    }

    [Fact]
    public async Task Create()
    {
        const string firstName = "Foo";
        const string lastName = "Bar";

        await using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        await cn.OpenAsync();

        var person = new Person { FirstName = firstName, LastName = lastName };
        var id = (long)await cn.InsertAsync(person);

        Assert.NotEqual(0, id);

        await cn.CloseAsync();
    }

    [Fact]
    public async Task Read()
    {
        const string firstName = "Bar";
        const string lastName = "Baz";

        await using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        await cn.OpenAsync();

        var person1 = new Person { FirstName = firstName, LastName = lastName };
        var person1Id = (long)await cn.InsertAsync(person1);

        var person2 = new Person { FirstName = firstName, LastName = lastName };
        var person2Id = (long)await cn.InsertAsync(person2);

        Assert.NotEqual(0, person1.Id);
        Assert.NotEqual(0, person2.Id);
        Assert.NotEqual(person1Id, person2Id);

        var person1Response = await cn.GetAsync<Person>(person1Id);

        Assert.Equal(person1Id, person1Response.Id);
        Assert.Equal(person1.FirstName, person1Response.FirstName);
        Assert.Equal(person1.LastName, person1Response.LastName);

        var person2Response = await cn.GetAsync<Person>(person1Id);

        Assert.Equal(person2Id, person2Response.Id);
        Assert.Equal(person2.FirstName, person2Response.FirstName);
        Assert.Equal(person2.LastName, person2Response.LastName);

        await cn.CloseAsync();
    }

    [Fact]
    public async Task Update()
    {
        const string firstName = "Raz";
        const string lastName = "Daz";

        await using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        await cn.OpenAsync();

        var person1 = new Person { FirstName = firstName, LastName = lastName };
        person1.Id = (long)await cn.InsertAsync(person1);

        var person2 = new Person { FirstName = firstName, LastName = lastName };
        person2.Id = (long)await cn.InsertAsync(person2);

        Assert.NotEqual(0, person1.Id);
        Assert.NotEqual(0, person2.Id);
        Assert.NotEqual(person1.Id, person2.Id);

        person1.FirstName = Guid.NewGuid().ToString();
        person1.LastName = Guid.NewGuid().ToString();

        var person1Updated = await cn.UpdateAsync(person1);

        Assert.True(person1Updated);

        person2.FirstName = Guid.NewGuid().ToString();
        person2.LastName = Guid.NewGuid().ToString();

        var person2Updated = await cn.UpdateAsync(person2);

        Assert.True(person2Updated);

        await cn.CloseAsync();
    }

    [Fact]
    public async Task Delete()
    {
        const string firstName = "Maz";
        const string lastName = "Kaz";

        await using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        await cn.OpenAsync();

        var person1 = new Person { FirstName = firstName, LastName = lastName };
        person1.Id = (long)await cn.InsertAsync(person1);

        var person2 = new Person { FirstName = firstName, LastName = lastName };
        person2.Id = (long)await cn.InsertAsync(person2);

        Assert.NotEqual(0, person1.Id);
        Assert.NotEqual(0, person2.Id);
        Assert.NotEqual(person1.Id, person2.Id);

        var person1Deleted = await cn.DeleteAsync(person1);

        Assert.True(person1Deleted);

        var person2Deleted = await cn.DeleteAsync(person2);

        Assert.True(person2Deleted);

        await cn.CloseAsync();
    }
}