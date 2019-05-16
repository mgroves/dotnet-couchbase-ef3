//
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Couchbase.Infrastructure;
using Microsoft.EntityFrameworkCore.Couchbase.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class CouchbaseContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseCouchbase<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] ClientConfiguration clientConfiguration,
            [NotNull] IAuthenticator authenticator,
            [NotNull] string bucketName,
            [CanBeNull] Action<CouchbaseContextOptionsBuilder> CouchbaseOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseCouchbase(
                (DbContextOptionsBuilder)optionsBuilder,
                clientConfiguration,
                authenticator,
                bucketName,
                CouchbaseOptionsAction);

        public static DbContextOptionsBuilder UseCouchbase(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] ClientConfiguration clientConfiguration,
            [NotNull] IAuthenticator authenticator,
            [NotNull] string bucketName,
            [CanBeNull] Action<CouchbaseContextOptionsBuilder> CouchbaseOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(clientConfiguration, nameof(clientConfiguration));
            Check.NotNull(authenticator, nameof(authenticator));
            Check.NotNull(bucketName, nameof(bucketName));
            Check.NotEmpty(bucketName, nameof(bucketName));

            var extension = optionsBuilder.Options.FindExtension<CouchbaseOptionsExtension>()
                            ?? new CouchbaseOptionsExtension();

            extension = extension
                .WithAuthenticator(authenticator)
                .WithBucketName(bucketName)
                .WithClientConfiguration(clientConfiguration);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            CouchbaseOptionsAction?.Invoke(new CouchbaseContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }
    }
}
