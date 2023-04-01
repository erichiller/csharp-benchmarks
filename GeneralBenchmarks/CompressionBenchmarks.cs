using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;

/* RESULTS :
 
# ZipArchiveMode = Create
 
|                Method | CompressionLevel | CompressionMethod |    Mean [ms] | Error [ms] | StdDev [ms] |  Median [ms] | Allocated [B] | Output file size (10MB log file input)
|---------------------- |----------------- |------------------ |-------------:|-----------:|------------:|-------------:|--------------:|------------
| CompressFileToArchive |          Fastest |            Brotli |     11.03 ms |   0.216 ms |    0.289 ms |     10.93 ms |        5893 B | 501.6 KB
| CompressFileToArchive |          Fastest |           Deflate |     29.97 ms |   0.318 ms |    0.557 ms |     29.83 ms |        6010 B | 607.2 KB
| CompressFileToArchive |          Optimal |            Brotli |     31.77 ms |   0.301 ms |    0.281 ms |     31.73 ms |        5980 B | 352.6 KB
| CompressFileToArchive |          Fastest |               Zip |     36.21 ms |   0.261 ms |    0.232 ms |     36.18 ms |        7093 B | 607.3 KB
| CompressFileToArchive |          Fastest |              Gzip |     36.28 ms |   0.241 ms |    0.225 ms |     36.21 ms |        6093 B | 607.2 KB
| CompressFileToArchive |          Optimal |           Deflate |     65.10 ms |   1.298 ms |    2.470 ms |     63.73 ms |        6101 B | 421.3 KB
| CompressFileToArchive |          Optimal |              Gzip |     70.37 ms |   0.361 ms |    0.320 ms |     70.22 ms |        6193 B | 421.3 KB
| CompressFileToArchive |          Optimal |               Zip |     70.58 ms |   0.342 ms |    0.303 ms |     70.60 ms |        7193 B | 421.5 KB
| CompressFileToArchive |     SmallestSize |           Deflate |    124.04 ms |   1.517 ms |    1.267 ms |    124.48 ms |        6381 B | 375.1 KB
| CompressFileToArchive |     SmallestSize |              Gzip |    130.18 ms |   1.742 ms |    1.629 ms |    129.70 ms |        6474 B | 375.2 KB
| CompressFileToArchive |     SmallestSize |               Zip |    130.79 ms |   0.773 ms |    0.723 ms |    130.77 ms |        7482 B | 375.3 KB
| CompressFileToArchive |     SmallestSize |            Brotli | 17,552.17 ms |  25.259 ms |   23.628 ms | 17,547.91 ms |        7760 B | 245.6 KB

 
# ZipArchiveMode = Update
 
|                Method | CompressionLevel | CompressionMethod |    Mean [ms] | Error [ms] | StdDev [ms] |  Median [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|---------------------- |----------------- |------------------ |-------------:|-----------:|------------:|-------------:|----------:|----------:|----------:|--------------:|
| CompressFileToArchive |          Fastest |               Zip |     47.98 ms |   0.553 ms |    0.490 ms |     48.03 ms | 1545.4545 | 1545.4545 | 1545.4545 |    33432568 B |
| CompressFileToArchive |          Optimal |               Zip |     83.39 ms |   1.588 ms |    1.560 ms |     83.02 ms | 1833.3333 | 1833.3333 | 1833.3333 |    33433060 B |
| CompressFileToArchive |     SmallestSize |               Zip |    143.42 ms |   1.304 ms |    1.220 ms |    143.26 ms | 1750.0000 | 1750.0000 | 1750.0000 |    33433138 B |

 */

[ Config( typeof(BenchmarkConfig) ) ]
[ SuppressMessage( "Performance", "CA1822:Mark members as static" ) ]
public sealed class CompressionBenchmarks {
    private readonly FileInfo                       _logFileInfo         = new FileInfo( "sampleLogFile.log" ); // 10MB log file input
    private readonly Dictionary<string, List<long>> _compressedFileSizes = new Dictionary<string, List<long>>();
    
    [ Params( CompressionLevel.Fastest, CompressionLevel.Optimal, CompressionLevel.SmallestSize ) ]
    public CompressionLevel CompressionLevel = CompressionLevel.Fastest;


    [ Params( CompressionMethod.Brotli, CompressionMethod.Deflate, CompressionMethod.Gzip, CompressionMethod.Zip ) ]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public CompressionMethod CompressionMethod { get; set; }

    [ Benchmark ]
    public void CompressFileToArchive( ) {
        FileInfo         outputCompressedFilePath = new FileInfo( System.IO.Path.Join( _logFileInfo.DirectoryName, $"sampleLogFile_{CompressionMethod}_{CompressionLevel}" + CompressionMethod.GetFileExtension() ) );
        using FileStream inputFileStream          = File.Open( _logFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read );
        using FileStream compressedFileStream     = File.Open( outputCompressedFilePath.FullName, FileMode.Create, FileAccess.ReadWrite );
        // System.Console.WriteLine($"writing to {outputCompressedFilePath.FullName}");
        if ( CompressionMethod == CompressionMethod.Zip ) {
            using ZipArchive archive     = new ZipArchive( compressedFileStream, ZipArchiveMode.Create );
            ZipArchiveEntry  entry       = archive.CreateEntry( _logFileInfo.Name, CompressionLevel );
            using var        entryStream = entry.Open();
            inputFileStream.CopyTo( entryStream );
            entryStream.Flush();
        } else {
            using Stream compressor = this.CompressionMethod switch {
                                          CompressionMethod.Brotli  => new BrotliStream( compressedFileStream, CompressionLevel ),
                                          CompressionMethod.Gzip    => new GZipStream( compressedFileStream, CompressionLevel ),
                                          CompressionMethod.Deflate => new DeflateStream( compressedFileStream, CompressionLevel ),
                                          _                         => throw new System.ComponentModel.InvalidEnumArgumentException( nameof(CompressionLevel), ( int )CompressionLevel, typeof(CompressionMethod) )
                                      };
            inputFileStream.CopyTo( compressor );
            compressor.Flush();
        }
        if ( !_compressedFileSizes.ContainsKey( outputCompressedFilePath.FullName ) ) {
            _compressedFileSizes[ outputCompressedFilePath.FullName ] = new List<long>();
        }
        _compressedFileSizes[ outputCompressedFilePath.FullName ].Add( outputCompressedFilePath.Length );
        // System.Console.WriteLine( outputCompressedFilePath.FullName + " length is " + outputCompressedFilePath.Length );
        // compressedFileStream.Flush();
    }

    [ GlobalCleanup ]
    public void GlobalCleanup( ) {
        foreach ( var (path, listOfSizes) in _compressedFileSizes ) {
            System.Console.WriteLine( $"path: {path}"                                  +
                                      $"\n\t Min:     {listOfSizes.Min( x => x )}"     +
                                      $"\n\t Average: {listOfSizes.Average( x => x )}" +
                                      $"\n\t Max:     {listOfSizes.Max( x => x )}" );
            new FileInfo( path ).CopyTo( "/home/eric/Downloads/" + System.IO.Path.GetFileName( path ), true );
        }
    }
}

/// <summary>
/// The compression method.
/// </summary>
public enum CompressionMethod {
    /// <inheritdoc cref="System.IO.Compression.BrotliStream"/>
    Brotli,
    /// <inheritdoc cref="System.IO.Compression.DeflateStream"/>
    Deflate,
    /// <inheritdoc cref="System.IO.Compression.ZipArchiveEntry"/>
    /// <remarks>This is the default compression for <see cref="ZipArchive"/></remarks>
    Zip,
    /// <inheritdoc cref="System.IO.Compression.GZipStream"/>
    Gzip
}

internal static class CompressionMethodExtensions {
    /// <summary>
    /// The extension to use for the compressed file <b>within the <see cref="ZipArchive"/></b>
    /// </summary>
    public static string GetFileExtension( this CompressionMethod compressionMethod ) =>
        compressionMethod switch {
            CompressionMethod.Brotli  => ".br",
            CompressionMethod.Deflate => ".dfl",
            CompressionMethod.Gzip    => ".gz",
            CompressionMethod.Zip     => ".zip", // Zip Entries don't need to be decompressed beyond what is automatically performed.
            _                         => throw new InvalidEnumArgumentException( $"{compressionMethod} is not a valid compression method", ( int )compressionMethod, typeof(CompressionMethod) )
        };
}