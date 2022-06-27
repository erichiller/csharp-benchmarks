using System.Diagnostics;

using Benchmarks.LanguageTests;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace LanguageTests;

public class AsyncTests : TestBase<AsyncTests> {

    /// <inheritdoc />
    public AsyncTests(ITestOutputHelper? output, ILogger? logger = null) : base(output, logger) { }

    private static async Task doSomeWork( ) => await Task.Delay( 1000 );
    
    /* Results
        Inline x3 took: "00:00:03.0001159"
        Await After Task start x3 took: "00:00:01.0149079"
        WaitAll with inline tasks x3 took: "00:00:01.0027456"
        WaitAll After Task start x3 took: "00:00:00.9986269"
        WhenAll with inline tasks x3 took: "00:00:00.9976685"
     */
    // see: https://alt-dev.com.au/why-you-may-be-doing-async-wrong-in-c
    [ Fact ]
    public async Task InlineAsyncShouldBeSlowerThanSplit( ) {

        Stopwatch stopwatch = new Stopwatch();
        
        stopwatch.Restart();
        await doSomeWork();
        await doSomeWork();
        await doSomeWork();
        this._logger.LogInformation( "Inline x3 took: {Stopwatch}", stopwatch.Elapsed );
        
        stopwatch.Restart();
        var t1 = doSomeWork();
        var t2 = doSomeWork();
        var t3 = doSomeWork();
        await t1;
        await t2;
        await t3;
        this._logger.LogInformation( "Await After Task start x3 took: {Stopwatch}", stopwatch.Elapsed );
        
        
        stopwatch.Restart();
        Task.WaitAll( doSomeWork(), doSomeWork(), doSomeWork() );
        this._logger.LogInformation( "WaitAll with inline tasks x3 took: {Stopwatch}", stopwatch.Elapsed );
        
        stopwatch.Restart();
        var t1B = doSomeWork();
        var t2B = doSomeWork();
        var t3B = doSomeWork();
        Task.WaitAll( t1B, t2B, t3B );
        this._logger.LogInformation( "WaitAll After Task start x3 took: {Stopwatch}", stopwatch.Elapsed );
        
        stopwatch.Restart();
        await Task.WhenAll( doSomeWork(), doSomeWork(), doSomeWork() );
        this._logger.LogInformation( "WhenAll with inline tasks x3 took: {Stopwatch}", stopwatch.Elapsed );



    }
}