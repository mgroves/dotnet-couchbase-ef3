# Couchbase Provider for Entity Framework Core 3 

Everything in this solution besides the Couchbase projects are a copy of the the [EntityFrameworkCore release/3.0-preview5 branch](https://github.com/aspnet/EntityFrameworkCore/tree/release/3.0-preview5). In the future, all the non-Couchbase specific projects will/should be replaced by NuGet packages.

Documentation on using EF Core is available at <https://docs.microsoft.com/ef/core/>.

## Couchbase Provider for Entity Framework Core 2

If you are using EF Core 2.x, go here instead: https://github.com/couchbaselabs/dotnet-couchbase-ef

## What is EF Core?

Entity Framework (EF) Core is a lightweight and extensible version of the popular Entity Framework data access technology.

EF Core is an object-relational mapper (O/RM) that enables .NET developers to work with a database using .NET objects. It eliminates the need for most of the data-access code that developers usually need to write.

## Building from source

To run a complete build on command line only, execute `build.cmd` or `build.sh` without arguments.
This will execute only the part of the build script that downloads and initializes a few required build tools and packages.

See [developer documentation](https://github.com/aspnet/EntityFrameworkCore/wiki/Getting-and-Building-the-Code) for more details.

## Contributor's Guide

The remaining work is currently divided among two GitHub projects.

* The first project is called ["Low Hanging Fruit"](https://github.com/mgroves/dotnet-couchbase-ef3/projects/1) and these represent relatively easy/small/isolated pieces of work.
* The second project is called ["Linq"](https://github.com/mgroves/dotnet-couchbase-ef3/projects/2) and it is strictly for tasks that are related to making EFCore 3 Couchbase Linq work.

If you see or can describe some piece of work that needs to be added, please create an issue and assign it to one of the above Projects.

If you would like to or are already working on an issue, please leave a comment in the issue! Note that a comment is not considered to be a promise of delivery, so don't let someone else's comment deter you from ALSO working on it if you want to!
