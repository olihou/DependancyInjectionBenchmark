using System;
using Microsoft.Extensions.Logging;

namespace DependancyInjectionBenchmark.Model
{
    public class ValueProvider : IValueProvider
    {
        private readonly ILogger logger;

        public ValueProvider(ILogger<ValueProvider> logger) => this.logger = logger;

        public int GetValue(int val)
        {
            return val;
        }
    }
}
