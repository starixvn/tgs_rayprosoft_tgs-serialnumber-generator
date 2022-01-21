using BenchmarkDotNet.Running;
using SerialNumberGenerator.Benchmark;

_ = BenchmarkRunner.Run<GenerateTest>();
