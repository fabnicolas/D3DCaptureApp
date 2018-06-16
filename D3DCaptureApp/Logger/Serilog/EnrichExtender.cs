using Serilog;
using Serilog.Configuration;

namespace SerilogLoggerSystem {
    static class EnrichExtender {
        public static LoggerConfiguration WithExtraContext(this LoggerEnrichmentConfiguration enrichmentConfiguration) {
            return enrichmentConfiguration.With<ILogEventWithExtraContext>();
        }
    }
}
