//
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Couchbase.TestUtilities
{
    public class CouchbaseTestHelpers : TestHelpers
    {
        protected CouchbaseTestHelpers()
        {
        }

        public static CouchbaseTestHelpers Instance { get; } = new CouchbaseTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
        {
            return services.AddEntityFrameworkCouchbase();
        }

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCouchbase(
                TestEnvironment.ClientConfiguration,
                TestEnvironment.Authenticator,
                "eftest");
        }
    }
}
