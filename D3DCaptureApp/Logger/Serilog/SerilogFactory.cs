using Serilog;
using Serilog.Core;

namespace SerilogLoggerSystem {
    class SerilogFactory {
        public static Logger GetLogger() {
            return new LoggerConfiguration()
                .Enrich.WithExtraContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [At: {Caller}{Exception}] ===> {Message}{NewLine}")
                .CreateLogger();
        }

        public static LoggerConfiguration GetLoggerBasicConfiguration() {
            return new LoggerConfiguration()
                .Enrich.WithExtraContext();
        }
    }

}
