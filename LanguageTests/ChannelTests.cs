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
        await foreach(var channelValue in  channel.Reader.ReadAllAsync() ) {
            channelValue.Should().Be( readCount );
            readCount++;
        }
        readCount.Should().Be( itemsToWrite );
    }
    
}