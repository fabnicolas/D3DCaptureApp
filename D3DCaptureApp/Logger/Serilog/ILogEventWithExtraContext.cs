using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Linq;

namespace SerilogLoggerSystem {
    class ILogEventWithExtraContext:ILogEventEnricher {
        public void Enrich(LogEvent logEvent,ILogEventPropertyFactory propertyFactory) {
            var skip = 3;
            while(true) {
                var stack = new StackFrame(skip);
                if(stack.GetMethod()==null) {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller",new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();
                if(method.DeclaringType.Assembly!=typeof(Log).Assembly) {
                    var caller = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ",method.GetParameters().Select(pi => pi.ParameterType.FullName))})";
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller",new ScalarValue(caller)));
                }

                skip++;
            }
        }
    }
}
