// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerConvertTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _typeMapping = new Dictionary<string, string>
        {
            [nameof(Convert.ToByte)] = "tinyint",
            [nameof(Convert.ToDecimal)] = "decimal(18, 2)",
            [nameof(Convert.ToDouble)] = "float",
            [nameof(Convert.ToInt16)] = "smallint",
            [nameof(Convert.ToInt32)] = "int",
            [nameof(Convert.ToInt64)] = "bigint",
            [nameof(Convert.ToString)] = "nvarchar(max)"
        };

        private static readonly List<Type> _supportedTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(DateTime),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        private static readonly IEnumerable<MethodInfo> _supportedMethods
            = _typeMapping.Keys
                .SelectMany(
                    t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                        .Where(
                            m => m.GetParameters().Length == 1
                                 && _supportedTypes.Contains(m.GetParameters().First().ParameterType)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Translate(
            MethodCallExpression methodCallExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => _supportedMethods.Contains(methodCallExpression.Method)
                ? new SqlFunctionExpression(
                    "CONVERT",
                    methodCallExpression.Type,
                    new[]
                    {
                        new SqlFragmentExpression(
                            _typeMapping[methodCallExpression.Method.Name]),
                        methodCallExpression.Arguments[0]
                    })
                : null;
    }
}
