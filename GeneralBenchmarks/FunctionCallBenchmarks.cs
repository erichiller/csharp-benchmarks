using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using Benchmarks.Common;

namespace Benchmarks.General;

[ Config( typeof(BenchmarkConfig) ) ]
[ GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory ) ]
public class FunctionCallBenchmarks {
    private const int _numCalls = 20_000_000;

    private readonly IntermediateObject  _intermediateObject = new ();
    private          IntermediateObject? _nullableIntermediateObject;
    private          IntermediateObject? _nullableIntermediateObjectNotNull = new ();


    [ IterationSetup ]
    public void Setup( ) {
        FuncCalls.Counter           = 0;
        _nullableIntermediateObject = null;
    }

    /*
     * Synchronous
     */

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark( Baseline = true ) ]
    public string Direct_Sync_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = FuncCalls.Remove();
        }
        return result;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public string Direct_Sync_WithParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = FuncCalls.Remove( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public string Intermediate_Sync_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = _intermediateObject.Remove( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public string Intermediate_Sync_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = _intermediateObject.Remove();
        }
        return result;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public string Nullable_Intermediate_Sync_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = ( _nullableIntermediateObject ??= new () ).Remove( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public string Nullable_Intermediate_Sync_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = ( _nullableIntermediateObject ??= new () ).Remove();
        }
        return result;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public string Nullable_NotNull_Intermediate_Sync_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = ( _nullableIntermediateObjectNotNull ??= new () ).Remove( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public string Nullable_NotNull_Intermediate_Sync_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = ( _nullableIntermediateObjectNotNull ??= new () ).Remove();
        }
        return result;
    }

    /*
     * Async, Task
     */

    [ Benchmark(Baseline = true) ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Direct_Async_Task_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await FuncCalls.RemoveTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Direct_Async_Task_WithParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await FuncCalls.RemoveTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Intermediate_Async_Task_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await _intermediateObject.RemoveTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Intermediate_Async_Task_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await _intermediateObject.RemoveTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Nullable_Intermediate_Async_Task_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObject ??= new () ).RemoveTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Nullable_Intermediate_Async_Task_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObject ??= new () ).RemoveTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Nullable_NotNull_Intermediate_Async_Task_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObjectNotNull ??= new () ).RemoveTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Task" ) ]
    public async Task<string> Nullable_NotNull_Intermediate_Async_Task_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObjectNotNull ??= new () ).RemoveTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    /*
     * Async, ValueTask
     */

    [ Benchmark(Baseline = true) ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Direct_Async_ValueTask_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await FuncCalls.RemoveValueTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Direct_Async_ValueTask_WithParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await FuncCalls.RemoveValueTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Intermediate_Async_ValueTask_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await _intermediateObject.RemoveValueTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Intermediate_Async_ValueTask_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await _intermediateObject.RemoveValueTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Nullable_Intermediate_Async_ValueTask_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObject ??= new () ).RemoveValueTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Nullable_Intermediate_Async_ValueTask_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObject ??= new () ).RemoveValueTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Nullable_NotNull_Intermediate_Async_ValueTask_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObjectNotNull ??= new () ).RemoveValueTaskAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Nullable_NotNull_Intermediate_Async_ValueTask_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObjectNotNull ??= new () ).RemoveValueTaskAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Nullable_NotNull_Intermediate_Inline_Async_ValueTask_NoParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObjectNotNull ??= new () ).RemoveValueTaskInlineAsync();
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ValueTask" ) ]
    public async ValueTask<string> Nullable_NotNull_Intermediate_Inline_Async_ValueTask_WithEndParams_ReferenceReturn( ) {
        string result = String.Empty;
        for ( FuncCalls.Counter = 0 ; FuncCalls.Counter < _numCalls ; FuncCalls.Counter++ ) {
            result = await ( _nullableIntermediateObjectNotNull ??= new () ).RemoveValueTaskInlineAsync( 5, FuncCalls.Counter % 5 );
        }
        return result;
    }
}

public class IntermediateObject {
    public string Remove( )                      => FuncCalls.Remove();
    public string Remove( int start, int count ) => FuncCalls.Remove( start, count );

    internal Task<string> RemoveTaskAsync( )                      => FuncCalls.RemoveTaskAsync();
    internal Task<string> RemoveTaskAsync( int start, int count ) => FuncCalls.RemoveTaskAsync( start, count );

    internal ValueTask<string> RemoveValueTaskAsync( )                      => FuncCalls.RemoveValueTaskAsync();
    internal ValueTask<string> RemoveValueTaskAsync( int start, int count ) => FuncCalls.RemoveValueTaskAsync( start, count );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<string> RemoveValueTaskInlineAsync( )                      => FuncCalls.RemoveValueTaskAsync();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<string> RemoveValueTaskInlineAsync( int start, int count ) => FuncCalls.RemoveValueTaskAsync( start, count );
}

public static class FuncCalls {
    private const   string _testString = "This is a test string.";
    internal static int    Counter     = 0;

    internal static string Remove( )                      => _testString.Remove( 5, Counter++ % 5 );
    internal static string Remove( int start, int count ) => _testString.Remove( start, count );

    internal static Task<string> RemoveTaskAsync( )                      => Task.FromResult( _testString.Remove( 5, Counter++ % 5 ) );
    internal static Task<string> RemoveTaskAsync( int start, int count ) => Task.FromResult( _testString.Remove( start, count ) );

    internal static ValueTask<string> RemoveValueTaskAsync( )                      => ValueTask.FromResult( _testString.Remove( 5, Counter++ % 5 ) );
    internal static ValueTask<string> RemoveValueTaskAsync( int start, int count ) => ValueTask.FromResult( _testString.Remove( start, count ) );
}