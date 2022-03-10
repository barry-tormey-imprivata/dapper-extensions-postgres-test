using System;
using DapperExtensions.IntegrationTest.Entities;
using DapperExtensions.IntegrationTest.Setup;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Npgsql;
using Xunit;

namespace DapperExtensions.IntegrationTest;

/// <summary>
/// Tests CRUD operations with the default <see cref="AutoClassMapper{T}" />
/// </summary>
[Collection("db")]
public class AutoClassMapperCrudTests
{
    public AutoClassMapperCrudTests()
    {
        DapperExtensions.SqlDialect = new PostgreSqlDialect();
        DapperExtensions.DefaultMapper = typeof(AutoClassMapper<>);
    }

    [Fact]
    public long Create()
    {
        const string title = "Foo";
        const string author = "Bar";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var book = new Book { Title = title, Author = author };
        var id = (long)cn.Insert(book);

        Assert.NotEqual(0, id);

        cn.Close();

        return id;
    }

    [Fact]
    public void Read()
    {
        const string title = "Bar";
        const string author = "Baz";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var book1 = new Book { Title = title, Author = author };
        var book1Id = (long)cn.Insert(book1);

        var book2 = new Book { Title = title, Author = author };
        var book2Id = (long)cn.Insert(book2);

        Assert.NotEqual(0, book1.Id);
        Assert.NotEqual(0, book2.Id);
        Assert.NotEqual(book1Id, book2Id);

        var book1Response = cn.Get<Book>(book1Id);

        Assert.Equal(book1Id, book1Response.Id);
        Assert.Equal(book1.Title, book1Response.Title);
        Assert.Equal(book1.Author, book1Response.Author);

        var book2Response = cn.Get<Book>(book1Id);

        Assert.Equal(book2Id, book2Response.Id);
        Assert.Equal(book2.Title, book2Response.Title);
        Assert.Equal(book2.Author, book2Response.Author);

        cn.Close();
    }

    [Fact]
    public void Update()
    {
        const string title = "Raz";
        const string author = "Daz";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var book1 = new Book { Title = title, Author = author };
        book1.Id = (long)cn.Insert(book1);

        var book2 = new Book { Title = title, Author = author };
        book2.Id = (long)cn.Insert(book2);

        Assert.NotEqual(0, book1.Id);
        Assert.NotEqual(0, book2.Id);
        Assert.NotEqual(book1.Id, book2.Id);

        book1.Title = Guid.NewGuid().ToString();
        book1.Author = Guid.NewGuid().ToString();

        var book1Updated = cn.Update(book1);

        Assert.True(book1Updated);

        book2.Title = Guid.NewGuid().ToString();
        book2.Author = Guid.NewGuid().ToString();

        var book2Updated = cn.Update(book2);

        Assert.True(book2Updated);

        cn.Close();
    }

    [Fact]
    public void Delete()
    {
        const string title = "Maz";
        const string author = "Kaz";

        using var cn = new NpgsqlConnection(PostgresConfiguration.GetConnectionString());
        cn.Open();

        var book1 = new Book { Title = title, Author = author };
        book1.Id = (long)cn.Insert(book1);

        var book2 = new Book { Title = title, Author = author };
        book2.Id = (long)cn.Insert(book2);

        Assert.NotEqual(0, book1.Id);
        Assert.NotEqual(0, book2.Id);
        Assert.NotEqual(book1.Id, book2.Id);

        var book1Deleted = cn.Delete(book1);

        Assert.True(book1Deleted);

        var book2Deleted = cn.Delete(book2);

        Assert.True(book2Deleted);

        cn.Close();
    }
}