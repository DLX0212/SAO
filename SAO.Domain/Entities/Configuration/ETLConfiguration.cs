using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Entities.Configuration
{
    public sealed class ETLConfiguration
    {

        public bool RunOnStartup { get; init; }

        public int IntervalMinutes { get; init; }

        public int BatchSize { get; init; }

        public bool ParallelProcessing { get; init; }

        public int MaxRetries { get; init; }

        public int ExtractionTimeoutSeconds { get; init; }

        private ETLConfiguration() { }

        public static ETLConfiguration Default => new()
        {
            RunOnStartup = true,
            IntervalMinutes = 60,
            BatchSize = 1000,
            ParallelProcessing = true,
            MaxRetries = 3,
            ExtractionTimeoutSeconds = 300
        };

        public bool IsValid(out string errorMessage)
        {
            if (IntervalMinutes <= 0)
            {
                errorMessage = "IntervalMinutes debe ser mayor a 0";
                return false;
            }

            if (BatchSize <= 0 || BatchSize > 10000)
            {
                errorMessage = "BatchSize debe estar entre 1 y 10000";
                return false;
            }

            if (MaxRetries < 0 || MaxRetries > 10)
            {
                errorMessage = "MaxRetries debe estar entre 0 y 10";
                return false;
            }

            if (ExtractionTimeoutSeconds <= 0)
            {
                errorMessage = "ExtractionTimeoutSeconds debe ser mayor a 0";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public ETLConfiguration With(
            bool? runOnStartup = null,
            int? intervalMinutes = null,
            int? batchSize = null,
            bool? parallelProcessing = null,
            int? maxRetries = null,
            int? extractionTimeoutSeconds = null)
        {
            return new ETLConfiguration
            {
                RunOnStartup = runOnStartup ?? RunOnStartup,
                IntervalMinutes = intervalMinutes ?? IntervalMinutes,
                BatchSize = batchSize ?? BatchSize,
                ParallelProcessing = parallelProcessing ?? ParallelProcessing,
                MaxRetries = maxRetries ?? MaxRetries,
                ExtractionTimeoutSeconds = extractionTimeoutSeconds ?? ExtractionTimeoutSeconds
            };
        }
    }
}
