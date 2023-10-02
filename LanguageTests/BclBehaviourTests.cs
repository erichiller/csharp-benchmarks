using FluentAssertions;

using Xunit;

namespace Benchmarks.LanguageTests; 

public class BclBehaviourTests {
    

    [ Fact ]
    public void AddingToAnEnumerableDuringIterationCausesException( ) {
        List<string> list = new List<string> {
            "one",
            "two",
            "three"
        };
        using var enumerator = list.GetEnumerator();
        enumerator.Current.Should().BeNull();
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Should().Be( "one" );
        enumerator.MoveNext().Should().BeTrue();
        list.Add( "four" );
        enumerator.Current.Should().Be( "two" );
        enumerator.Invoking( static n => n.MoveNext() )
                  .Should().ThrowExactly<InvalidOperationException>();
    }
}