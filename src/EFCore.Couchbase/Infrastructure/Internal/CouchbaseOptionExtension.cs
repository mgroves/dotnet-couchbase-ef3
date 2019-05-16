//
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Couchbase.Infrastructure.Internal
{
    public class CouchbaseOptionsExtension : IDbContextOptionsExtension
    {
        private ClientConfiguration _clientConfiguration;
        private IAuthenticator _authenticator;
        private string _bucketName;
        private string _logFragment;

        public CouchbaseOptionsExtension()
        {
        }

        protected CouchbaseOptionsExtension(CouchbaseOptionsExtension copyFrom)
        {
            _clientConfiguration = copyFrom.ClientConfiguration;
            _authenticator = copyFrom.Authenticator;
            _bucketName = copyFrom.BucketName;
        }

        public virtual ClientConfiguration ClientConfiguration => _clientConfiguration;

        public virtual CouchbaseOptionsExtension WithClientConfiguration(ClientConfiguration clientConfiguration)
        {
            var clone = Clone();

            clone._clientConfiguration = clientConfiguration;

            return clone;
        }

        public virtual IAuthenticator Authenticator => _authenticator;

        public virtual CouchbaseOptionsExtension WithAuthenticator(IAuthenticator authenticator)
        {
            var clone = Clone();

            clone._authenticator = authenticator;

            return clone;
        }

        public virtual string BucketName => _bucketName;

        public virtual CouchbaseOptionsExtension WithBucketName(string bucketName)
        {
            var clone = Clone();

            clone._bucketName = bucketName;

            return clone;
        }

        protected virtual CouchbaseOptionsExtension Clone() => new CouchbaseOptionsExtension(this);

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkCouchbase();

            return true;
        }

        public long GetServiceProviderHashCode()
        {
            return 0;
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public virtual void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Couchbase"] = "1";
        }

        public string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append("ClientConfiguration=").Append("TODO?").Append(' ');

                    builder.Append("Authenticator=").Append("TODO?").Append(' ');

                    builder.Append("BucketName=").Append(_bucketName).Append(' ');

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
