using System.Threading.Channels;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Benchmarks.LanguageTests; 

public class ChannelTests  : TestBase<ChannelTests> {

    /// <inheritdoc />
    public ChannelTests(ITestOutputHelper? output, ILogger? logger = null) : base(output, logger) { }

    [ Fact ]
    public async Task MarkingChannelCompleteShouldAllowReaderToReadAllWritten( ) {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<int>();

        const int itemsToWrite = 10;
        for ( int i = 0 ; i < itemsToWrite ; i++ ) {
            channel.Writer.TryWrite( i );
        }
        channel.Writer.Complete();

        int readCount = 0;
        await foreach(var channelValue in channel.Reader.ReadAllAsync() ) {
            channelValue.Should().Be( readCount );
            readCount++;
        }
        readCount.Should().Be( itemsToWrite );
    }
    
    [ Fact ]
    public async Task MarkingChannelCompleteShouldAllowReaderInVariableToReadAllWritten( ) {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<int>();

        var       reader       = channel.Reader;
        var       writer       = channel.Writer;
        const int itemsToWrite = 10;
        for ( int i = 0 ; i < itemsToWrite ; i++ ) {
            writer.TryWrite( i );
        }
        writer.Complete();

        int readCount = 0;
        await foreach(var channelValue in reader.ReadAllAsync() ) {
            channelValue.Should().Be( readCount );
            readCount++;
        }
        readCount.Should().Be( itemsToWrite );
    }
    
    [ Fact ]
    public void MarkingChannelComplete_ShouldAllowReader_InVariable_UsingTryRead_ToReadAllWritten( ) {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<int>();

        var       reader       = channel.Reader;
        var       writer       = channel.Writer;
        const int itemsToWrite = 10;
        for ( int i = 0 ; i < itemsToWrite ; i++ ) {
            writer.TryWrite( i );
        }
        writer.Complete();

        int readCount = 0;
        while( reader.TryRead( out int channelValue )){
            channelValue.Should().Be( readCount );
            readCount++;
        }
        readCount.Should().Be( itemsToWrite );
    }
    
    [ Fact ]
    public async Task MarkingChannelComplete_ShouldAllowReader_InVariable_Wait_UsingTryRead_ToReadAllWritten( ) {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<int>();

        var       reader       = channel.Reader;
        var       writer       = channel.Writer;
        const int itemsToWrite = 10;
        for ( int i = 0 ; i < itemsToWrite ; i++ ) {
            writer.TryWrite( i );
        }
        writer.Complete();

        int readCount = 0;
        await reader.WaitToReadAsync();
        while( reader.TryRead( out int channelValue )){
            channelValue.Should().Be( readCount );
            readCount++;
        }
        readCount.Should().Be( itemsToWrite );
    }
    
    [ Fact ]
    public async Task MarkingChannelComplete_ShouldAllowReader_InVariable_SeparateWriterThread_UsingTryRead_ToReadAllWritten( ) {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<int>();

        var       reader       = channel.Reader;
        const int itemsToWrite = 10;

        var task = Task.Run( ( ) => writerTask( channel.Writer ) );
        static void writerTask( ChannelWriter<int> writer ) {
            for ( int i = 0 ; i < itemsToWrite ; i++ ) {
                writer.TryWrite( i );
            }
            writer.Complete();
        }

        Thread.Sleep( 10 );

        int readCount = 0;
        await reader.WaitToReadAsync();
        while( reader.TryRead( out int channelValue )){
            channelValue.Should().Be( readCount );
            readCount++;
        }
        await task;
        readCount.Should().Be( itemsToWrite );
    }
    
}