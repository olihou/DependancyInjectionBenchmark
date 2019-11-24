using System;
namespace DependancyInjectionBenchmark.Model
{
    public interface IValueProvider
    {
        int GetValue(int val);
    }
}
