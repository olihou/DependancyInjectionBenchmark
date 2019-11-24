using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DependancyInjectionBenchmark.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using StructureMap;

namespace DependancyInjectionBenchmark
{
    class Program
    {
        private static int ITERATION;
        static void Main(string[] args)
        {
            if (!int.TryParse(args.FirstOrDefault(), out int ITERATION))
                ITERATION = 5000000;

            Console.WriteLine($"Dependancy injection benchmark ({ITERATION} iterations)");
            Console.WriteLine("=====================================================");

            BenchNetCoreDI();

            BenchAutofac();

            BenchRaw();

            BenchSimpleInjector();

            BenchStructureMap();

            Console.WriteLine("=====================================================");
        }

        private static void BenchStructureMap()
        {
            var services = new ServiceCollection();

            services.AddScoped<IValueProvider, ValueProvider>();
            services.AddLogging();

            var container = new StructureMap.Container();
            container.Configure(config =>
            {
                config.Scan(_ =>
                {
                    _.AssemblyContainingType(typeof(Program));
                    _.WithDefaultConventions();
                });
                config.Populate(services);
            });

            Launch("StructureMap", (i) =>
            {
                var valueProvider = container.GetInstance<IValueProvider>();
                valueProvider.GetValue(i);
            });
        }

        private static void BenchSimpleInjector()
        {
            var services = new ServiceCollection();

            services.AddScoped<IValueProvider, ValueProvider>();
            services.AddLogging();

            var container = new SimpleInjector.Container();

            services.AddSimpleInjector(container);

            services.BuildServiceProvider().UseSimpleInjector(container);

            Launch("SimpleInjector (ThreadScoped)", (i) =>
            {
                using (var serviceScope = ThreadScopedLifestyle.BeginScope(container))
                {
                    var valueProvider = serviceScope.GetRequiredService<IValueProvider>();
                    valueProvider.GetValue(i);
                }
            });

            Launch("SimpleInjector (AsyncScopedLifestyle)", (i) =>
            {
                using (var serviceScope = AsyncScopedLifestyle.BeginScope(container))
                {
                    var valueProvider = serviceScope.GetRequiredService<IValueProvider>();
                    valueProvider.GetValue(i);
                }
            });
        }

        private static void BenchAutofac()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();
            builder.RegisterType<ValueProvider>().As<IValueProvider>();
            services.AddLogging();

            builder.Populate(services);
            var appContainer = builder.Build();
            var _serviceProvider = new AutofacServiceProvider(appContainer);

            Launch("Autofac", (i) =>
            {
                using (var serviceScope = _serviceProvider.CreateScope())
                {
                    var valueProvider = serviceScope.ServiceProvider.GetRequiredService<IValueProvider>();
                    valueProvider.GetValue(i);
                }
            });
        }

        private static void BenchNetCoreDI()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddScoped<IValueProvider, ValueProvider>()
                .BuildServiceProvider();

            Launch(".NET Core DI", (i) =>
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    var valueProvider = serviceScope.ServiceProvider.GetRequiredService<IValueProvider>();
                    valueProvider.GetValue(i);
                }
            });
        }

        private static void BenchRaw()
        {
            var factory = LoggerFactory.Create(p => p.AddConsole());

            Launch("Raw", (i) =>
            {
                new ValueProvider(factory.CreateLogger<ValueProvider>()).GetValue(i);
            });
        }

        private static void Launch(string DIName, Action<int> valueProvider)
        {
            Stopwatch sw = Stopwatch.StartNew();

            foreach (int i in Enumerable.Range(1, ITERATION))
            {
                valueProvider(i);
            }

            sw.Stop();
            Console.WriteLine("{1} : {0}", sw.Elapsed, DIName);
        }
    }
}
