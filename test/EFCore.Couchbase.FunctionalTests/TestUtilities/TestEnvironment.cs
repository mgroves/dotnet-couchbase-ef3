//
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.Couchbase.TestUtilities
{
    public static class TestEnvironment
    {
        public static IConfiguration Config { get; }

        static TestEnvironment()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile("config.test.json", optional: true)
                .AddEnvironmentVariables();

            Config = configBuilder.Build()
                .GetSection("Test:Couchbase");
        }

        public static ClientConfiguration ClientConfiguration
        {
            get
            {
                return new ClientConfiguration
                {
                    Servers = new List<Uri> { new Uri(Config["Server"]) }
                };
            }
        }

        public static IAuthenticator Authenticator
        {
            get
            {
                var username = Config["Username"];
                var password = Config["Password"];
                return new PasswordAuthenticator(username, password);
            }
        }
    }
}
