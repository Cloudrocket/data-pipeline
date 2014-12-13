﻿namespace Microsoft.Practices.DataPipeline.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Practices.DataPipeline.Logging;

    public class UnknownTypeMessageHandler : IMessageHandler
    {
        private static readonly ILogger Logger = LoggerFactory
            .GetLogger("Dispatcher.Processor");

        private readonly IPoisonMessageHandler _handler;

        public UnknownTypeMessageHandler()
        {
            _handler = (IPoisonMessageHandler)DependencyResolverFactory
                .GetResolver()
                .GetService(typeof(IPoisonMessageHandler));
        }

        public string Name
        {
            get { return "UnknownTypeMessageHandler"; }
        }

        public TimeSpan Timeout
        {
            get { return TimeSpan.FromSeconds(5); }
        }

        public async Task ExecuteAsync(
            ProcessingContext context,
            byte[] payload,
            IDictionary<string, string> properties)
        {
            var headers = properties
                .Select(p => string.Format("{0}:{1}\n", p.Key, p.Value))
                .ToArray();
                
            Logger.Warning(
                "No handler was found for {0}. The event contained the following properties:\n {1}",
                context,
                string.Join(Environment.NewLine, headers));

            await _handler
                .PublishAsync(FailureMode.UnknownPayload, context, payload, properties)
                .ConfigureAwait(false);
        }
    }
}