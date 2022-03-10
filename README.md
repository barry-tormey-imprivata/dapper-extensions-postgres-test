# Dapper Extensions (1.7.0) - Postgres Test

## Description

Provides integration tests that are used to test and validate [Dapper Extensions](https://github.com/tmsmith/Dapper-Extensions) v.1.7.0 against a PostgreSQL database using [Docker.NET](https://github.com/dotnet/Docker.DotNet).

### Technologies

* [Dapper Extensions](https://github.com/tmsmith/Dapper-Extensions) - 1.7.0
* [Docker.NET](https://github.com/dotnet/Docker.DotNet)
* [XUnit](https://xunit.net)

## Getting Started

The solution contains a single XUnit test project that can be ran in [Visual Studio 2022](https://visualstudio.microsoft.com/launch) or with the [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/).

### Prerequisites

This test is built on top of [.NET 6](https://devblogs.microsoft.com/dotnet/announcing-net-6/?WT.mc_id=dotnet-35129-website), which requires Visual Studio 2022 to run. The Postgres database is provided via a [Docker](https://docs.docker.com/get-docker) container using Docker.NET. **The test requires Docker to be installed to initialize the container.**

* [.NET 6 SDK for Windows x64](https://dotnet.microsoft.com/download/dotnet/6.0)
* [Visual Studio 2022 - Professional](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Professional&rel=17)
* [Docker](https://docs.docker.com/get-docker)

## Usage

Execute the tests using Visual Studio 2022 or .NET CLI.

### Visual Studio

1. Go to "Test"
1. Select "Run all tests" (CTRL+R, A)

### .NET CLI

```bash
dotnet test
```

## Observations

Using Dapper Extensions 1.7.0 with the `PostgreSqlDialect` results in an `InvalidOperationException` thrown by the `Get` and `GetAsync<T>` methods of Dapper Extensions.

```bash
Error Message:
   System.InvalidOperationException : Sequence contains more than one element
  Stack Trace:
     at System.Linq.ThrowHelper.ThrowMoreThanOneElementException()
   at System.Linq.Enumerable.TryGetSingle[TSource](IEnumerable`1 source, Boolean& found)
   at System.Linq.Enumerable.SingleOrDefault[TSource](IEnumerable`1 source)
   at DapperExtensions.DapperImplementor.InternalGet[T](IDbConnection connection, Object id, IDbTransaction transaction, Nullable`1 commandTimeout, IList`1 colsToSelect, IList`1 includedProperties)
   at System.Dynamic.UpdateDelegates.UpdateAndExecute7[T0,T1,T2,T3,T4,T5,T6,TRet](CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
   at DapperExtensions.DapperImplementor.Get[T](IDbConnection connection, Object id, IDbTransaction transaction, Nullable`1 commandTimeout, IList`1 includedProperties)
   at System.Dynamic.UpdateDelegates.UpdateAndExecute5[T0,T1,T2,T3,T4,TRet](CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
   at DapperExtensions.DapperExtensions.Get[T](IDbConnection connection, Object id, IDbTransaction transaction, Nullable`1 commandTimeout)
   at DapperExtensions.IntegrationTest.CrudTests.Read()
   ```
