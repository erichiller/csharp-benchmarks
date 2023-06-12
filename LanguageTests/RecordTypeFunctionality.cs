using FluentAssertions;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Benchmarks.LanguageTests;

public class RecordTypeFunctionality : TestBase<RecordTypeFunctionality> {
    /// <inheritdoc />
    public RecordTypeFunctionality( ITestOutputHelper? output, ILogger? logger = null ) : base( output, logger ) { }

    [ Fact ]
    public void RecordCastToInterfaceShouldMaintainImplementorProperties( ) {
        string             descriptionPropertyValue1 = "A description";
        string             descriptionPropertyValue2 = "edited description";
        string             descriptionPropertyValue3 = "second edited description";
        CommonProps        commonProps               = new () { Description = descriptionPropertyValue1 };
        IMutableHiddenInfo mutable                   = commonProps;
        mutable.Sequence.Should().BeNull();
        mutable.NextSequence();
        mutable.Sequence.Should().Be( 1 );
        ( mutable as CommonProps ).Should().BeOfType<CommonProps>()
                                  .Subject.Description.Should().Be( descriptionPropertyValue1 );
        mutable.NextSequence();
        commonProps = ( mutable as CommonProps ).Should().BeOfType<CommonProps>().Subject with { Description = descriptionPropertyValue2 };
        commonProps.Description.Should().Be( descriptionPropertyValue2 );
        ( commonProps as IMutableHiddenInfo ).Sequence.Should().Be( 2 );
        IMutableHiddenInfo mutable2 = commonProps with { Description = descriptionPropertyValue3 };
        ( mutable2 as CommonProps ).Should().BeOfType<CommonProps>()
                                   .Subject.Description.Should().Be( descriptionPropertyValue3 );
        mutable.ToString().Should().Be( $"CommonProps {{ Description = {descriptionPropertyValue1} }}" );
        commonProps.ToString().Should().Be( $"CommonProps {{ Description = {descriptionPropertyValue2} }}" );
        mutable2.ToString().Should().Be( $"CommonProps {{ Description = {descriptionPropertyValue3} }}" );
    }
    [ Fact ]
    public void RecordCastToInterfaceCopyAndReturnShouldMaintainImplementorProperties( ) {
        string             descriptionPropertyValue1 = "A description";
        CommonProps        commonProps               = new () { Description = descriptionPropertyValue1 };
        IMutableHiddenInfo mutable                   = commonProps;
        IMutableHiddenInfo mutable2                  = mutable.WithHiddenInfoReturnNew( 2, nameof(mutable2) );
        mutable2.Should().BeOfType<CommonProps>().Subject.Description.Should().Be( descriptionPropertyValue1 );
        mutable2.Id.Should().Be( 2 );
        mutable2.Name.Should().Be( nameof(mutable2) );
        mutable.Should().BeSameAs( commonProps );
        mutable.Should().NotBeSameAs( mutable2 );
        commonProps.Should().NotBeSameAs( mutable2 );
    }
}

file interface IHiddenInfo {
    public int?    Id       { get; }
    public string? Name     { get; }
    public int?    Sequence { get; }
}

file interface IMutableHiddenInfo
    : IHiddenInfo {
    internal void               NextSequence( );
    internal IMutableHiddenInfo NextSequenceReturnNew( );

    internal void WithHiddenInfo(
        int    Id,
        string Name
    );

    internal IMutableHiddenInfo WithHiddenInfoReturnNew(
        int    Id,
        string Name
    );
}

file record CommonProps : IMutableHiddenInfo {
    public required string Description { get; init; }

    int? IHiddenInfo.   Id       => _id;
    string? IHiddenInfo.Name     => _name;
    int? IHiddenInfo.   Sequence => _sequence;

    private int?    _id       = null;
    private string? _name     = null;
    private int?    _sequence = null;

    void IMutableHiddenInfo.NextSequence( ) {
        // ( this as IMutableHiddenInfo ).Id++;
        this._sequence ??= 0;
        this._sequence++;
    }

    IMutableHiddenInfo IMutableHiddenInfo.NextSequenceReturnNew( ) {
        return this with { _sequence = _sequence is null ? 0 : _sequence + 1 };
    }

    void IMutableHiddenInfo.WithHiddenInfo(
        int    id,
        string name
    ) {
        // IMutableHiddenInfo mutable = this;
        this._id   = id;
        this._name = name;
    }

    IMutableHiddenInfo IMutableHiddenInfo.WithHiddenInfoReturnNew(
        int    id,
        string name
    ) {
        return this with {
            _id = id,
            _name = name
        };
    }
}