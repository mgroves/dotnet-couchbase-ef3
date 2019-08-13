//
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Couchbase.Storage;

namespace Microsoft.EntityFrameworkCore.Couchbase.Query.Sql
{
    public interface ISqlGenerator
    {
        /// <summary>
        ///     Generates a SQL query for the given bucket name and parameter values.
        /// </summary>
        /// <param name="bucketName">The bucket name.</param>
        /// <param name="parameterValues"> The parameter values. </param>
        /// <returns>
        ///     The SQL query.
        /// </returns>
        CouchbaseSqlQuery GenerateSqlQuery(
            [NotNull] string bucketName,
            [NotNull] IReadOnlyDictionary<string, object> parameterValues);
    }
}
