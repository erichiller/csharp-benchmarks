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
        07:16:06.716 -05:00 [INF] 21 >> Inline x3 took: "00:00:03.0001159"
        07:16:07.743 -05:00 [INF] 20 >> Await After Task start x3 took: "00:00:01.0149079"
        07:16:08.746 -05:00 [INF] 20 >> WaitAll with inline tasks x3 took: "00:00:01.0027456"
        07:16:09.745 -05:00 [INF] 20 >> WaitAll After Task start x3 took: "00:00:00.9986269"
     */
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



    }
}