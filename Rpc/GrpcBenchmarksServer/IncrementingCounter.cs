namespace Benchmarks.Rpc.GrpcBenchmarksServer;

public class IncrementingCounter {
    
    public void Increment(int amount)
    {
        Count += amount;
    }

    public int Count { get; private set; }
}