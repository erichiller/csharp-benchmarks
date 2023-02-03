using System.Diagnostics;

using Benchmarks.LanguageTests;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace LanguageTests;

public class CompilerConditionalTests : TestBase<CompilerConditionalTests> {

    /// <inheritdoc />
    public CompilerConditionalTests(ITestOutputHelper? output, ILogger? logger = null) : base(output, logger) { }

    private const  string _defaultStringValue = "THIS IS UNCHANGED";
    private static string _stringToSet         = _defaultStringValue;
    
    [ Fact ]
    public void CompiledOutMethodShouldNotPerformLambdaProvidedResultsInArgument( ) {
        // ReSharper disable once RedundantAssignment
        string[] strings = new string[]{ "one", "two", "three" };
        logger( String.Join( ',', strings ) );
        _stringToSet.Should().Be( _defaultStringValue );
        logger( String.Join( ',', strings.Select( s => s[^1] ) ) );
        _stringToSet.Should().Be( _defaultStringValue );
    }

    [ Conditional( "THIS_IS_NOT_SET" ) ]
    private void logger( string logMessage ) {
        _stringToSet = logMessage;
    }
    
}