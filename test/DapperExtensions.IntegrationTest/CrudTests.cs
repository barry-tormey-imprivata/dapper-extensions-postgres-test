using System;
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
public class CrudTests
{
    public CrudTests()
    {
        DapperExtensions.SqlDialect = new PostgreSqlDialect();
        DapperExtensions.DefaultMapper = typeof(PostgresMapper<>);
    }

    [Fact]
    public void Create()
    {
        const string firstName = "Foo";
        const string lastName = "Bar";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var person = new Person { FirstName = firstName, LastName = lastName };
        var id = (long)cn.Insert(person);

        Assert.NotEqual(0, id);

        cn.Close();
    }

    [Fact]
    public void Read()
    {
        const string firstName = "Bar";
        const string lastName = "Baz";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var person1 = new Person { FirstName = firstName, LastName = lastName };
        var person1Id = (long)cn.Insert(person1);

        var person2 = new Person { FirstName = firstName, LastName = lastName };
        var person2Id = (long)cn.Insert(person2);

        Assert.NotEqual(0, person1.Id);
        Assert.NotEqual(0, person2.Id);
        Assert.NotEqual(person1Id, person2Id);

        var person1Response = cn.Get<Person>(person1Id);

        Assert.Equal(person1Id, person1Response.Id);
        Assert.Equal(person1.FirstName, person1Response.FirstName);
        Assert.Equal(person1.LastName, person1Response.LastName);

        var person2Response = cn.Get<Person>(person1Id);

        Assert.Equal(person2Id, person2Response.Id);
        Assert.Equal(person2.FirstName, person2Response.FirstName);
        Assert.Equal(person2.LastName, person2Response.LastName);

        cn.Close();
    }

    [Fact]
    public void Update()
    {
        const string firstName = "Raz";
        const string lastName = "Daz";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var person1 = new Person { FirstName = firstName, LastName = lastName };
        person1.Id = (long)cn.Insert(person1);

        var person2 = new Person { FirstName = firstName, LastName = lastName };
        person2.Id = (long)cn.Insert(person2);

        Assert.NotEqual(0, person1.Id);
        Assert.NotEqual(0, person2.Id);
        Assert.NotEqual(person1.Id, person2.Id);

        person1.FirstName = Guid.NewGuid().ToString();
        person1.LastName = Guid.NewGuid().ToString();

        var person1Updated = cn.Update(person1);

        Assert.True(person1Updated);

        person2.FirstName = Guid.NewGuid().ToString();
        person2.LastName = Guid.NewGuid().ToString();

        var person2Updated = cn.Update(person2);

        Assert.True(person2Updated);

        cn.Close();
    }

    [Fact]
    public void Delete()
    {
        const string firstName = "Maz";
        const string lastName = "Kaz";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var person1 = new Person { FirstName = firstName, LastName = lastName };
        person1.Id = (long)cn.Insert(person1);

        var person2 = new Person { FirstName = firstName, LastName = lastName };
        person2.Id = (long)cn.Insert(person2);

        Assert.NotEqual(0, person1.Id);
        Assert.NotEqual(0, person2.Id);
        Assert.NotEqual(person1.Id, person2.Id);

        var person1Deleted = cn.Delete(person1);

        Assert.True(person1Deleted);

        var person2Deleted = cn.Delete(person2);

        Assert.True(person2Deleted);

        cn.Close();
    }
}