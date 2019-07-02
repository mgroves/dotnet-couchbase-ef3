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
        private string _bucketName;
        private IBucket _bucket;
        private Cluster _cluster;

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
            _bucketName = options.BucketName;

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
            return Cluster.OpenBucket(_bucketName);
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
            return await Task.FromException<bool>(new NotImplementedException("CouchbaseClientWrapper::CreateDatabaseIfNotExistsOnceAsync"));
            // var response = await Client.Databases.CreateDatabaseIfNotExistsAsync(_databaseId, cancellationToken: cancellationToken);
            //
            // return response.StatusCode == HttpStatusCode.Created;
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
            var document = parameters.Document;

            var documentHasIdField = document["id"] != null;
            if (documentHasIdField)
            {
                var idelement = document["id"].Parent;
                idelement.Remove();
            }

            var result = await Bucket.InsertAsync(id, document);

            return result.Success;
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
            return await Task.FromException<bool>(new NotImplementedException("CouchbaseClientWrapper::ReplaceItemOnceAsync"));

            // using (var stream = new MemoryStream())
            // using (var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false))
            // using (var jsonWriter = new JsonTextWriter(writer))
            // {
            //     JsonSerializer.Create().Serialize(jsonWriter, parameters.Document);
            //     await jsonWriter.FlushAsync();
            //
            //     var items = Client.Databases[_databaseId].Containers[parameters.ContainerId].Items;
            //     using (var response = await items.ReplaceItemStreamAsync("0", parameters.ItemId, stream, null, cancellationToken))
            //     {
            //         return response.StatusCode == HttpStatusCode.OK;
            //     }
            // }
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
            _commandLogger.ExecutingSqlQuery(query);

            return new DocumentEnumerable(this, containerId, query);
        }

        public IAsyncEnumerable<JObject> ExecuteSqlQueryAsync(
            string containerId,
            [NotNull] CouchbaseSqlQuery query)
        {
            _commandLogger.ExecutingSqlQuery(query);

            return new DocumentAsyncEnumerable(this, containerId, query);
        }

        // private CouchbaseResultSetIterator CreateQuery(
        //     string containerId,
        //     CouchbaseSqlQuery query)
        // {
        //     var items = Client.Databases[_databaseId].Containers[containerId].Items;
        //     var queryDefinition = new CouchbaseSqlQueryDefinition(query.Query);
        //     foreach (var parameter in query.Parameters)
        //     {
        //         queryDefinition.UseParameter(parameter.Name, parameter.Value);
        //     }
        //
        //     return items.CreateItemQueryAsStream(queryDefinition, "0");
        // }

        private class DocumentEnumerable : IEnumerable<JObject>
        {
            private readonly CouchbaseClientWrapper _CouchbaseClient;
            private readonly string _containerId;
            private readonly CouchbaseSqlQuery _CouchbaseSqlQuery;

            public DocumentEnumerable(
                CouchbaseClientWrapper CouchbaseClient,
                string containerId,
                CouchbaseSqlQuery CouchbaseSqlQuery)
            {
                _CouchbaseClient = CouchbaseClient;
                _containerId = containerId;
                _CouchbaseSqlQuery = CouchbaseSqlQuery;
            }

            public IEnumerator<JObject> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class Enumerator : IEnumerator<JObject>
            {
//                private CouchbaseResultSetIterator _query;
                private Stream _responseStream;
                private StreamReader _reader;
                private JsonTextReader _jsonReader;
                private readonly CouchbaseClientWrapper _CouchbaseClient;
                private readonly string _containerId;
                private readonly CouchbaseSqlQuery _CouchbaseSqlQuery;
        
                public Enumerator(DocumentEnumerable documentEnumerable)
                {
                    _CouchbaseClient = documentEnumerable._CouchbaseClient;
                    _containerId = documentEnumerable._containerId;
                    _CouchbaseSqlQuery = documentEnumerable._CouchbaseSqlQuery;
                }
        
                public JObject Current { get; private set; }
        
                object IEnumerator.Current => Current;
        
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    throw new NotImplementedException("CouchbaseClientWrapper::Enumerator::MoveNext");
                    /*
                    if (_jsonReader == null)
                    {
                        if (_query == null)
                        {
                            _query = _CouchbaseClient.CreateQuery(_containerId, _CouchbaseSqlQuery);
                        }
        
                        if (!_query.HasMoreResults)
                        {
                            Current = default;
                            return false;
                        }
        
                        _responseStream = _query.FetchNextSetAsync().GetAwaiter().GetResult().Content;
                        _reader = new StreamReader(_responseStream);
                        _jsonReader = new JsonTextReader(_reader);
        
                        while (_jsonReader.Read())
                        {
                            if (_jsonReader.TokenType == JsonToken.StartObject)
                            {
                                while (_jsonReader.Read())
                                {
                                    if (_jsonReader.TokenType == JsonToken.StartArray)
                                    {
                                        goto ObjectFound;
                                    }
                                }
                            }
                        }
        
                        ObjectFound:
                        ;
                    }
        
                    while (_jsonReader.Read())
                    {
                        if (_jsonReader.TokenType == JsonToken.StartObject)
                        {
                            while (_jsonReader.Read())
                            {
                                if (_jsonReader.TokenType == JsonToken.StartObject)
                                {
                                    Current = new JsonSerializer().Deserialize<JObject>(_jsonReader);
                                    return true;
                                }
                            }
                        }
                    }
        
                    _jsonReader.Close();
                    _jsonReader = null;
                    _reader.Dispose();
                    _reader = null;
                    _responseStream.Dispose();
                    _responseStream = null;
        
                    return MoveNext();
                    */
                }
        
                public void Dispose()
                {
                    _jsonReader?.Close();
                    _jsonReader = null;
                    _reader?.Dispose();
                    _reader = null;
                    _responseStream?.Dispose();
                    _responseStream = null;
                }
        
                public void Reset() => throw new NotImplementedException();
            }
        }

        private class DocumentAsyncEnumerable : IAsyncEnumerable<JObject>
        {
            private readonly CouchbaseClientWrapper _CouchbaseClient;
            private readonly string _containerId;
            private readonly CouchbaseSqlQuery _CouchbaseSqlQuery;

            public DocumentAsyncEnumerable(
                CouchbaseClientWrapper CouchbaseClient,
                string containerId,
                CouchbaseSqlQuery CouchbaseSqlQuery)
            {
                _CouchbaseClient = CouchbaseClient;
                _containerId = containerId;
                _CouchbaseSqlQuery = CouchbaseSqlQuery;
            }

            public IAsyncEnumerator<JObject> GetEnumerator() => new AsyncEnumerator(this);

            private class AsyncEnumerator : IAsyncEnumerator<JObject>
            {
//                private CouchbaseResultSetIterator _query;
                private Stream _responseStream;
                private StreamReader _reader;
                private JsonTextReader _jsonReader;
                private readonly CouchbaseClientWrapper _CouchbaseClient;
                private readonly string _containerId;
                private readonly CouchbaseSqlQuery _CouchbaseSqlQuery;

                public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable)
                {
                    _CouchbaseClient = documentEnumerable._CouchbaseClient;
                    _containerId = documentEnumerable._containerId;
                    _CouchbaseSqlQuery = documentEnumerable._CouchbaseSqlQuery;
                }

                public JObject Current { get; private set; }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    return await Task.FromException<bool>(new NotImplementedException("CouchbaseClientWrapper::AsyncEnumerator::MoveNext"));

                    // cancellationToken.ThrowIfCancellationRequested();
                    //
                    // if (_jsonReader == null)
                    // {
                    //     if (_query == null)
                    //     {
                    //         _query = _CouchbaseClient.CreateQuery(_containerId, _CouchbaseSqlQuery);
                    //     }
                    //
                    //     if (!_query.HasMoreResults)
                    //     {
                    //         Current = default;
                    //         return false;
                    //     }
                    //
                    //     _responseStream = (await _query.FetchNextSetAsync(cancellationToken)).Content;
                    //     _reader = new StreamReader(_responseStream);
                    //     _jsonReader = new JsonTextReader(_reader);
                    //
                    //     while (_jsonReader.Read())
                    //     {
                    //         if (_jsonReader.TokenType == JsonToken.StartObject)
                    //         {
                    //             while (_jsonReader.Read())
                    //             {
                    //                 if (_jsonReader.TokenType == JsonToken.StartArray)
                    //                 {
                    //                     goto ObjectFound;
                    //                 }
                    //             }
                    //         }
                    //     }
                    //
                    //     ObjectFound:
                    //     ;
                    // }
                    //
                    // while (_jsonReader.Read())
                    // {
                    //     if (_jsonReader.TokenType == JsonToken.StartObject)
                    //     {
                    //         while (_jsonReader.Read())
                    //         {
                    //             if (_jsonReader.TokenType == JsonToken.StartObject)
                    //             {
                    //                 Current = new JsonSerializer().Deserialize<JObject>(_jsonReader);
                    //                 return true;
                    //             }
                    //         }
                    //     }
                    // }
                    //
                    // _jsonReader.Close();
                    // _jsonReader = null;
                    // _reader.Dispose();
                    // _reader = null;
                    // _responseStream.Dispose();
                    // _responseStream = null;
                    // return await MoveNext(cancellationToken);
                }

                public void Dispose()
                {
                    _jsonReader?.Close();
                    _jsonReader = null;
                    _reader?.Dispose();
                    _reader = null;
                    _responseStream?.Dispose();
                    _responseStream = null;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            _cluster.Dispose();
        }
    }
}
