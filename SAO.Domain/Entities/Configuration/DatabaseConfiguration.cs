using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Entities.Configuration
{
    public sealed class DatabaseConfiguration
    {
        
        public string ConnectionString { get; init; } = string.Empty;

        public string Provider { get; init; } = string.Empty;

        public int CommandTimeout { get; init; }

        public int MaxRetryCount { get; init; }

        public bool EnableSqlLogging { get; init; }

        public int MaxPoolSize { get; init; }

        public int MinPoolSize { get; init; }

        private DatabaseConfiguration() { }

        public static DatabaseConfiguration Create(
            string connectionString,
            string provider = "PostgreSQL")
        {
            return new DatabaseConfiguration
            {
                ConnectionString = connectionString,
                Provider = provider,
                CommandTimeout = 120,
                MaxRetryCount = 3,
                EnableSqlLogging = false,
                MaxPoolSize = 100,
                MinPoolSize = 10
            };
        }

        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                errorMessage = "ConnectionString no puede estar vacío";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Provider))
            {
                errorMessage = "Provider no puede estar vacío";
                return false;
            }

            var validProviders = new[] { "PostgreSQL", "SQLServer", "SQLite" };
            if (!validProviders.Contains(Provider, StringComparer.OrdinalIgnoreCase))
            {
                errorMessage = $"Provider debe ser uno de: {string.Join(", ", validProviders)}";
                return false;
            }

            if (CommandTimeout <= 0)
            {
                errorMessage = "CommandTimeout debe ser mayor a 0";
                return false;
            }

            if (MaxPoolSize < MinPoolSize)
            {
                errorMessage = "MaxPoolSize debe ser mayor o igual a MinPoolSize";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public string ToSafeString()
        {
            var builder = ConnectionString.Split(';')
                .Select(part =>
                {
                    if (part.Contains("Password", StringComparison.OrdinalIgnoreCase))
                        return "Password=***";
                    return part;
                });

            return string.Join(";", builder);
        }
    }
}
