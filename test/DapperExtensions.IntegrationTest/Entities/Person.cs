using System;

namespace DapperExtensions.IntegrationTest.Entities;

public class Person
{
    public long Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool Active { get; set; }

    public DateTime DateCreated { get; set; }
}