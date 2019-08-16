//
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.N1QL;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Couchbase.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Couchbase.Storage.Internal
{
    public class CouchbaseClientWrapper : IDisposable
    {
        private readonly IExecutionStrategyFactory _executionStrategyFactory;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;

        private static readonly string _userAgent = " Microsoft.EntityFrameworkCore.Couchbase/" + ProductInfo.GetVersion();
        public static readonly JsonSerializer Serializer = new JsonSerializer();
        private ClientConfiguration _clientConfiguration;
        private IAuthenticator _authenticator;
        private IBucket _bucket;
        private Cluster _cluster;

        public string BucketName { get; }

        static CouchbaseClientWrapper()
        {
            Serializer.Converters.Add(new ByteArrayConverter());
            Serializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        }

        public CouchbaseClientWrapper(
            [NotNull] IDbContextOptions dbContextOptions,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            var options = dbContextOptions.FindExtension<CouchbaseOptionsExtension>();

            _clientConfiguration = options.ClientConfiguration;
            _authenticator = options.Authenticator;
            BucketName = options.BucketName;

            _executionStrategyFactory = executionStrategyFactory;
            _commandLogger = commandLogger;
        }

        private IBucket Bucket =>
            _bucket
            ?? (_bucket = ConnectToCouchbaseBucket());

        private Cluster Cluster =>
            _cluster
            ?? (_cluster = ConnectToCouchbaseCluster());

        private Cluster ConnectToCouchbaseCluster()
        {
            var cluster = new Cluster(_clientConfiguration);
            cluster.Authenticate(_authenticator);
            return cluster;
        }

        private IBucket ConnectToCouchbaseBucket()
        {
            return Cluster.OpenBucket(BucketName);
        }

        public bool CreateDatabaseIfNotExists()
            => _executionStrategyFactory.Create().Execute(
                (object)null, CreateDatabaseIfNotExistsOnce, null);

        public bool CreateDatabaseIfNotExistsOnce(
            DbContext context,
            object state)
            => CreateDatabaseIfNotExistsOnceAsync(context, state).GetAwaiter().GetResult();

        public Task<bool> CreateDatabaseIfNotExistsAsync(
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (object)null, CreateDatabaseIfNotExistsOnceAsync, null, cancellationToken);

        public async Task<bool> CreateDatabaseIfNotExistsOnceAsync(
            DbContext _,
            object __,
            CancellationToken cancellationToken = default)
        {
            var manager = Cluster.CreateManager();

            var info = await manager.ClusterInfoAsync();
            if (!info.Success)
            {
                throw new Exception($"Error checking for existing bucket {BucketName}", info.Exception);
            }

            if (info.Value.BucketConfigs().All(p => p.Name != BucketName))
            {
                var createResult = await manager.CreateBucketAsync(BucketName);
                if (!createResult.Success)
                {
                    throw new Exception($"Error creating bucket {BucketName}", info.Exception);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeleteDatabase()
            => _executionStrategyFactory.Create().Execute((object)null, DeleteDatabaseOnce, null);

        public bool DeleteDatabaseOnce(
            DbContext context,
            object state)
            => DeleteDatabaseOnceAsync(context, state).GetAwaiter().GetResult();

        public Task<bool> DeleteDatabaseAsync(
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (object)null, DeleteDatabaseOnceAsync, null, cancellationToken);

        public async Task<bool> DeleteDatabaseOnceAsync(
            DbContext _,
            object __,
            CancellationToken cancellationToken = default)
        {
            return await Task.FromException<bool>(new NotImplementedException("CouchbaseClientWrapper::DeleteDatabaseOnceAsync"));

            // var response = await Client.Databases[_databaseId].DeleteAsync(cancellationToken: cancellationToken);
            //
            // return response.StatusCode == HttpStatusCode.NoContent;
        }

        public bool CreateContainerIfNotExists(
            string containerId,
            string partitionKey)
            => _executionStrategyFactory.Create().Execute(
                (containerId, partitionKey), CreateContainerIfNotExistsOnce, null);

        private bool CreateContainerIfNotExistsOnce(
            DbContext context,
            (string ContainerId, string PartitionKey) parameters)
            => CreateContainerIfNotExistsOnceAsync(context, parameters).GetAwaiter().GetResult();

        public Task<bool> CreateContainerIfNotExistsAsync(
            string containerId,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, partitionKey), CreateContainerIfNotExistsOnceAsync, null, cancellationToken);

        private async Task<bool> CreateContainerIfNotExistsOnceAsync(
            DbContext _,
            (string ContainerId, string PartitionKey) parameters,
            CancellationToken cancellationToken = default)
        {
            return await Task.FromException<bool>(new NotImplementedException("CouchbaseClientWrapper::CreateContainerIfNotExistsOnceAsync"));

            // var response = await Client.Databases[_databaseId].Containers
            //     .CreateContainerIfNotExistsAsync(
            //     new CouchbaseContainerSettings(parameters.ContainerId, "/" + parameters.PartitionKey), cancellationToken: cancellationToken);
            //
            // return response.StatusCode == HttpStatusCode.Created;
        }

        public bool CreateItem(
            string containerId,
            JToken document)
            => _executionStrategyFactory.Create().Execute(
                (containerId, document), CreateItemOnce, null);

        private bool CreateItemOnce(
            DbContext context,
            (string ContainerId, JToken Document) parameters)
            => CreateItemOnceAsync(context, parameters).GetAwaiter().GetResult();

        public Task<bool> CreateItemAsync(
            string containerId,
            JToken document,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, document), CreateItemOnceAsync, null, cancellationToken);

        private async Task<bool> CreateItemOnceAsync(
            DbContext _,
            (string ContainerId, JToken Document) parameters,
            CancellationToken cancellationToken = default)
        {
            var id = parameters.Document["id"].ToString();

            var document = StripIdFromDocument(parameters.Document);

            var result = await Bucket.InsertAsync(id, document);

            return result.Success;
        }

        private static JToken StripIdFromDocument(JToken document)
        {
            var documentHasIdField = document["id"] != null;
            if (documentHasIdField)
            {
                var idelement = document["id"].Parent;
                idelement.Remove();
            }

            return document;
        }

        public bool ReplaceItem(
            string collectionId,
            string documentId,
            JObject document)
            => _executionStrategyFactory.Create().Execute(
                (collectionId, documentId, document), ReplaceItemOnce, null);

        private bool ReplaceItemOnce(
            DbContext context,
            (string, string, JObject) parameters)
            => ReplaceItemOnceAsync(context, parameters).GetAwaiter().GetResult();

        public Task<bool> ReplaceItemAsync(
            string collectionId,
            string documentId,
            JObject document,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (collectionId, documentId, document), ReplaceItemOnceAsync, null, cancellationToken);

        private async Task<bool> ReplaceItemOnceAsync(
            DbContext _,
            (string ContainerId, string ItemId, JObject Document) parameters,
            CancellationToken cancellationToken = default)
        {
            var document = StripIdFromDocument(parameters.Document);

            var result = await _bucket.ReplaceAsync(parameters.ItemId, document);

            return result.Success;
        }

        public bool DeleteItem(
            string containerId,
            string documentId)
            => _executionStrategyFactory.Create().Execute(
                (containerId, documentId), DeleteItemOnce, null);

        public bool DeleteItemOnce(
            DbContext context,
            (string ContainerId, string DocumentId) parameters)
            => DeleteItemOnceAsync(context, parameters).GetAwaiter().GetResult();

        public Task<bool> DeleteItemAsync(
            string containerId,
            string documentId,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, documentId), DeleteItemOnceAsync, null, cancellationToken);

        public async Task<bool> DeleteItemOnceAsync(
            DbContext _,
            (string ContainerId, string DocumentId) parameters,
            CancellationToken cancellationToken = default)
        {

            var result = await _bucket.RemoveAsync(parameters.DocumentId);

            return result.Success;
        }

        public IEnumerable<JObject> ExecuteSqlQuery(
            string containerId,
            [NotNull] CouchbaseSqlQuery query)
        {
            return new DocumentEnumerable(this, containerId, query);
        }

        public IAsyncEnumerable<JObject> ExecuteSqlQueryAsync(
            string containerId,
            [NotNull] CouchbaseSqlQuery query)
        {
            return new DocumentAsyncEnumerable(this, containerId, query);
        }

        private IQueryRequest CreateQuery(
            string containerId,
            CouchbaseSqlQuery query)
        {
            var queryText = query.Query;

            _commandLogger.ExecutingSqlQuery(query);

            var request = new QueryRequest(queryText)
                .UseStreaming(true);

            foreach (var parameter in query.Parameters)
            {
                request.AddNamedParameter(parameter.Name, parameter.Value);
            }

            return request;
        }

        private class DocumentEnumerable : IEnumerable<JObject>
        {
            private readonly CouchbaseClientWrapper _couchbaseClient;
            private readonly string _containerId;
            private readonly CouchbaseSqlQuery _couchbaseSqlQuery;

            public DocumentEnumerable(
                CouchbaseClientWrapper couchbaseClient,
                string containerId,
                CouchbaseSqlQuery couchbaseSqlQuery)
            {
                _couchbaseClient = couchbaseClient;
                _containerId = containerId;
                _couchbaseSqlQuery = couchbaseSqlQuery;
            }

            public IEnumerator<JObject> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class Enumerator : IEnumerator<JObject>
            {
                private IQueryResult<JObject> _queryResult;
                private IEnumerator<JObject> _resultEnumerator;
                private readonly CouchbaseClientWrapper _couchbaseClient;
                private readonly string _containerId;
                private readonly CouchbaseSqlQuery _couchbaseSqlQuery;

                public Enumerator(DocumentEnumerable documentEnumerable)
                {
                    _couchbaseClient = documentEnumerable._couchbaseClient;
                    _containerId = documentEnumerable._containerId;
                    _couchbaseSqlQuery = documentEnumerable._couchbaseSqlQuery;
                }

                public JObject Current { get; private set; }

                object IEnumerator.Current => Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (_queryResult == null)
                    {
                        var query = _couchbaseClient.CreateQuery(_containerId, _couchbaseSqlQuery);

                        _queryResult = _couchbaseClient.Bucket.Query<JObject>(query);
                        _queryResult.EnsureSuccess();

                        _resultEnumerator = _queryResult.GetEnumerator();
                    }

                    var returnValue = _resultEnumerator.MoveNext();
                    Current = _resultEnumerator.Current;

                    if (!returnValue)
                    {
                        // Check for additional errors after the rows are returned
                        _queryResult.EnsureSuccess();
                    }

                    return returnValue;
                }

                public void Dispose()
                {
                    _resultEnumerator?.Dispose();
                    _resultEnumerator = null;
                    _queryResult?.Dispose();
                    _queryResult = null;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class DocumentAsyncEnumerable : IAsyncEnumerable<JObject>
        {
            private readonly CouchbaseClientWrapper _couchbaseClient;
            private readonly string _containerId;
            private readonly CouchbaseSqlQuery _couchbaseSqlQuery;

            public DocumentAsyncEnumerable(
                CouchbaseClientWrapper couchbaseClient,
                string containerId,
                CouchbaseSqlQuery couchbaseSqlQuery)
            {
                _couchbaseClient = couchbaseClient;
                _containerId = containerId;
                _couchbaseSqlQuery = couchbaseSqlQuery;
            }

            public IAsyncEnumerator<JObject> GetEnumerator() => new AsyncEnumerator(this);

            private class AsyncEnumerator : IAsyncEnumerator<JObject>
            {
                private IQueryResult<JObject> _queryResult;
                private IAsyncEnumerator<JObject> _resultEnumerator;
                private readonly CouchbaseClientWrapper _couchbaseClient;
                private readonly string _containerId;
                private readonly CouchbaseSqlQuery _couchbaseSqlQuery;

                public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable)
                {
                    _couchbaseClient = documentEnumerable._couchbaseClient;
                    _containerId = documentEnumerable._containerId;
                    _couchbaseSqlQuery = documentEnumerable._couchbaseSqlQuery;
                }

                public JObject Current { get; private set; }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_queryResult == null)
                    {
                        var query = _couchbaseClient.CreateQuery(_containerId, _couchbaseSqlQuery);

                        _queryResult = await _couchbaseClient.Bucket.QueryAsync<JObject>(query, cancellationToken);
                        _queryResult.EnsureSuccess();

                        // TODO: Use IAsyncEnumerable directly once Couchbase SDK support is available https://issues.couchbase.com/browse/NCBC-2020
                        _resultEnumerator = _queryResult.ToAsyncEnumerable().GetEnumerator();
                    }

                    var returnValue = await _resultEnumerator.MoveNext(cancellationToken);
                    if (returnValue)
                    {
                        Current = _resultEnumerator.Current;
                    }
                    else
                    {
                        Current = null;

                        // Check for additional errors after the rows are returned
                        _queryResult.EnsureSuccess();
                    }

                    return returnValue;
                }

                public void Dispose()
                {
                    _resultEnumerator?.Dispose();
                    _resultEnumerator = null;
                    _queryResult?.Dispose();
                    _queryResult = null;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            _cluster?.Dispose();
            _cluster = null;
        }
    }
}
