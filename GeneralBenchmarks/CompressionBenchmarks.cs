#define TAR
#define ZIP
// #define TRACK_FILE_SIZE_STATS

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
#if TAR
using System;
using System.Formats.Tar;
#endif
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using Benchmarks.Common;

using Perfolizer.Metrology;

namespace Benchmarks.General;

/* RESULTS :

# ZipArchiveMode = Create

|                Method | CompressionLevel | CompressionMethod |    Mean [ms] | Error [ms] | StdDev [ms] |  Median [ms] | Allocated [B] | Output file size (10MB log file input)
|---------------------- |----------------- |------------------ |-------------:|-----------:|------------:|-------------:|--------------:|------------
| CreateNewArchive |          Fastest |            Brotli |     11.03 ms |   0.216 ms |    0.289 ms |     10.93 ms |        5893 B | 501.6 KB
| CreateNewArchive |          Fastest |           Deflate |     29.97 ms |   0.318 ms |    0.557 ms |     29.83 ms |        6010 B | 607.2 KB
| CreateNewArchive |          Optimal |            Brotli |     31.77 ms |   0.301 ms |    0.281 ms |     31.73 ms |        5980 B | 352.6 KB
| CreateNewArchive |          Fastest |               Zip |     36.21 ms |   0.261 ms |    0.232 ms |     36.18 ms |        7093 B | 607.3 KB
| CreateNewArchive |          Fastest |              Gzip |     36.28 ms |   0.241 ms |    0.225 ms |     36.21 ms |        6093 B | 607.2 KB
| CreateNewArchive |          Optimal |           Deflate |     65.10 ms |   1.298 ms |    2.470 ms |     63.73 ms |        6101 B | 421.3 KB
| CreateNewArchive |          Optimal |              Gzip |     70.37 ms |   0.361 ms |    0.320 ms |     70.22 ms |        6193 B | 421.3 KB
| CreateNewArchive |          Optimal |               Zip |     70.58 ms |   0.342 ms |    0.303 ms |     70.60 ms |        7193 B | 421.5 KB
| CreateNewArchive |     SmallestSize |           Deflate |    124.04 ms |   1.517 ms |    1.267 ms |    124.48 ms |        6381 B | 375.1 KB
| CreateNewArchive |     SmallestSize |              Gzip |    130.18 ms |   1.742 ms |    1.629 ms |    129.70 ms |        6474 B | 375.2 KB
| CreateNewArchive |     SmallestSize |               Zip |    130.79 ms |   0.773 ms |    0.723 ms |    130.77 ms |        7482 B | 375.3 KB
| CreateNewArchive |     SmallestSize |            Brotli | 17,552.17 ms |  25.259 ms |   23.628 ms | 17,547.91 ms |        7760 B | 245.6 KB


# ZipArchiveMode = Update

|                Method | CompressionLevel | CompressionMethod |    Mean [ms] | Error [ms] | StdDev [ms] |  Median [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|---------------------- |----------------- |------------------ |-------------:|-----------:|------------:|-------------:|----------:|----------:|----------:|--------------:|
| CreateNewArchive |          Fastest |               Zip |     47.98 ms |   0.553 ms |    0.490 ms |     48.03 ms | 1545.4545 | 1545.4545 | 1545.4545 |    33432568 B |
| CreateNewArchive |          Optimal |               Zip |     83.39 ms |   1.588 ms |    1.560 ms |     83.02 ms | 1833.3333 | 1833.3333 | 1833.3333 |    33433060 B |
| CreateNewArchive |     SmallestSize |               Zip |    143.42 ms |   1.304 ms |    1.220 ms |    143.26 ms | 1750.0000 | 1750.0000 | 1750.0000 |    33433138 B |

 */

[ Config( typeof(LocalConfig) ) ]
[ SuppressMessage( "Performance", "CA1822:Mark members as static" ) ]
public class CompressionBenchmarks {
    public const string LOG_FILE_DIRECTORY = "/home/eric/Downloads/compression_benchmarks";
    // public const string LOG_FILE_DIRECTORY = "data";
    private static readonly FileInfo[] _logFileInfo = [
        new FileInfo( $"{LOG_FILE_DIRECTORY}{Path.DirectorySeparatorChar}sampleLogFile_1.log" ), // 10MB log file input
        new FileInfo( $"{LOG_FILE_DIRECTORY}{Path.DirectorySeparatorChar}sampleLogFile_2.log" ), // 10MB log file input
        new FileInfo( $"{LOG_FILE_DIRECTORY}{Path.DirectorySeparatorChar}sampleLogFile_3.log" ), // 10MB log file input
    ];
#if TRACK_FILE_SIZE_STATS
    private readonly Dictionary<string, List<long>> _compressedFileSizes = new Dictionary<string, List<long>>();
#endif

    [ Params(
        CompressionLevel.Fastest
        , CompressionLevel.Optimal
        , CompressionLevel.SmallestSize
    ) ]
    public CompressionLevel CompressionLevel = CompressionLevel.Fastest;


    // [ Params(
    //     // CompressionMethod.Brotli,
    //     // CompressionMethod.Deflate,
    //     // CompressionMethod.Gzip,
    //     CompressionMethod.Zip
    // ) ]
    // [ SuppressMessage( "ReSharper", "MemberCanBeProtected.Global" ) ]
    // [ SuppressMessage( "ReSharper", "UnusedAutoPropertyAccessor.Global" ) ]
    // // ReSharper disable once MemberCanBePrivate.Global
    // // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // public CompressionMethod CompressionMethod { get; set; }

    // [ Params(1,2,3) ]
    // [ SuppressMessage( "ReSharper", "MemberCanBeProtected.Global" ) ]
    // [ SuppressMessage( "ReSharper", "UnusedAutoPropertyAccessor.Global" ) ]
    // // ReSharper disable once MemberCanBePrivate.Global
    // // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // public int FilesToCompress { get; set; }

    const int _minFiles = 1;
    // const int _maxFiles = 1;
    const int _maxFiles = 3;

#if ZIP
    public IEnumerable<object[]> ZipArgumentMatrix( ) {
        for ( int i = _minFiles ; i <= _maxFiles ; i++ ) {
            foreach ( var compressionMethod in new[] {
                         CompressionMethod.Brotli,
                         CompressionMethod.Deflate,
                         CompressionMethod.Gzip,
                         CompressionMethod.Zip
                     } ) {
                yield return [ i, compressionMethod ];
            }
        }
    }
#endif

#if TAR
    public IEnumerable<object[]> TarArgumentMatrix( ) {
        for ( int i = _minFiles ; i <= _maxFiles ; i++ ) {
            foreach ( var compressionMethod in new[] {
                         CompressionMethod.Brotli,
                         CompressionMethod.Deflate,
                         CompressionMethod.Gzip
                     } ) {
                yield return [ i, compressionMethod ];
            }
        }
    }
#endif


    /*
     *
     *
     *
     */

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        if ( new DirectoryInfo( CompressionBenchmarks.LOG_FILE_DIRECTORY ) is { Exists: false } directory ) {
            directory.Create();
        }
    }

    // [ IterationSetup( Target = nameof(UpdateArchive) ) ]
    // public void IterationSetup( ) {
    //     foreach( )
    //     if ( getOutputCompressedFilePath( nameof(UpdateArchive) ) is { Exists: true } filePath ) {
    //         filePath.Delete();
    //     }
    // }

    public static FileInfo GetOutputCompressedFilePath( string directoryName, string method, CompressionMethod compressionMethod, CompressionLevel compressionLevel, ArchiveType archiveType, int compressedFileCount ) =>
        new FileInfo( System.IO.Path.Join( directoryName, $"sampleLogFile_{method}_{compressionMethod}_{compressionLevel}_{compressedFileCount}files" +
                                                          method switch {
                                                              nameof(CompressTar) => archiveType.GetFileExtension() + compressionMethod.GetFileExtension(),
                                                              _                   => archiveType.GetFileExtension()
                                                          } ) );


    /*
     *
     */

    [ Benchmark ]
    [ Arguments( CompressionMethod.Brotli ) ]
    [ Arguments( CompressionMethod.Deflate ) ]
    [ Arguments( CompressionMethod.Gzip ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    public void CompressOnly( CompressionMethod CompressionMethod ) {
        int FileCount = 1;
        FileInfo outputCompressedFilePath = GetOutputCompressedFilePath(
            directoryName: LOG_FILE_DIRECTORY,
            method: nameof(CompressOnly),
            compressionMethod: CompressionMethod,
            compressedFileCount: FileCount,
            archiveType: ArchiveType.None,
            compressionLevel: this.CompressionLevel );
        int              i                    = 0;
        FileInfo         logFileInfo          = _logFileInfo[ i ];
        using FileStream inputFileStream      = File.Open( logFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read );
        using FileStream outputCompressedFile = File.Open( outputCompressedFilePath.FullName, FileMode.Create, FileAccess.Write );
        using Stream compressor = CompressionMethod switch {
                                      CompressionMethod.Brotli  => new BrotliStream( outputCompressedFile, this.CompressionLevel ),
                                      CompressionMethod.Gzip    => new GZipStream( outputCompressedFile, this.CompressionLevel ),
                                      CompressionMethod.Deflate => new DeflateStream( outputCompressedFile, this.CompressionLevel ),
                                      _                         => throw new System.ComponentModel.InvalidEnumArgumentException( nameof(this.CompressionLevel), ( int )this.CompressionLevel, typeof(CompressionMethod) )
                                  };
        inputFileStream.CopyTo( compressor );
    }

    [ Benchmark ]
    [ ArgumentsSource( nameof(ZipArgumentMatrix) ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    public void CreateNewArchive( int FileCount, CompressionMethod CompressionMethod ) {
        FileInfo outputCompressedFilePath = GetOutputCompressedFilePath(
            directoryName: LOG_FILE_DIRECTORY,
            method: nameof(CreateNewArchive),
            compressionMethod: CompressionMethod,
            compressedFileCount: FileCount,
            archiveType: ArchiveType.Zip,
            compressionLevel: this.CompressionLevel );
        if ( outputCompressedFilePath.Exists ) {
            outputCompressedFilePath.Delete();
        }
        using FileStream compressedFileStream = File.Open( outputCompressedFilePath.FullName, FileMode.Create, FileAccess.Write );
        using ZipArchive archive              = new ZipArchive( compressedFileStream, ZipArchiveMode.Create );
        for ( int i = 0 ; i < FileCount ; i++ ) {
            FileInfo         logFileInfo     = _logFileInfo[ i ];
            Stream           entry           = archive.CreateEntry( logFileInfo.Name + CompressionMethod.GetFileExtension(), CompressionMethod == CompressionMethod.Zip ? this.CompressionLevel : CompressionLevel.NoCompression ).Open();
            using FileStream inputFileStream = File.Open( logFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read );
            using Stream compressor = CompressionMethod switch {
                                          CompressionMethod.Zip     => entry,
                                          CompressionMethod.Brotli  => new BrotliStream( entry, this.CompressionLevel ),
                                          CompressionMethod.Gzip    => new GZipStream( entry, this.CompressionLevel ),
                                          CompressionMethod.Deflate => new DeflateStream( entry, this.CompressionLevel ),
                                          _                         => throw new System.ComponentModel.InvalidEnumArgumentException( nameof(this.CompressionLevel), ( int )this.CompressionLevel, typeof(CompressionMethod) )
                                      };
            inputFileStream.CopyTo( compressor );
        }
#if TRACK_FILE_SIZE_STATS
        outputCompressedFilePath.Refresh();
        if ( !_compressedFileSizes.ContainsKey( outputCompressedFilePath.FullName ) ) {
            _compressedFileSizes[ outputCompressedFilePath.FullName ] = new List<long>();
        }
        _compressedFileSizes[ outputCompressedFilePath.FullName ].Add( outputCompressedFilePath.Length );
#endif
    }

#if ZIP
    [ Benchmark ]
    [ ArgumentsSource( nameof(ZipArgumentMatrix) ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    public void UpdateZip( int FileCount, CompressionMethod CompressionMethod ) {
        FileInfo outputCompressedFilePath = GetOutputCompressedFilePath(
            directoryName: LOG_FILE_DIRECTORY,
            method: nameof(UpdateZip),
            compressionMethod: CompressionMethod,
            compressedFileCount: FileCount,
            archiveType: ArchiveType.Zip,
            compressionLevel: this.CompressionLevel );
        if ( outputCompressedFilePath.Exists ) {
            outputCompressedFilePath.Delete();
        }
        for ( int i = 0 ; i < FileCount ; i++ ) {
            FileInfo         logFileInfo          = _logFileInfo[ i ];
            using FileStream compressedFileStream = File.Open( outputCompressedFilePath.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite );
            using ZipArchive archive              = new ZipArchive( compressedFileStream, ZipArchiveMode.Update ); // this has to be ZipArchiveMode.Update otherwise it won't write to an existing directory.
            Stream           entry                = archive.CreateEntry( logFileInfo.Name + CompressionMethod.GetFileExtension(), CompressionMethod == CompressionMethod.Zip ? this.CompressionLevel : CompressionLevel.NoCompression ).Open();
            using FileStream inputFileStream      = File.Open( logFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read );
            using Stream compressor = CompressionMethod switch {
                                          CompressionMethod.Zip     => entry,
                                          CompressionMethod.Brotli  => new BrotliStream( entry, this.CompressionLevel ),
                                          CompressionMethod.Gzip    => new GZipStream( entry, this.CompressionLevel ),
                                          CompressionMethod.Deflate => new DeflateStream( entry, this.CompressionLevel ),
                                          _                         => throw new System.ComponentModel.InvalidEnumArgumentException( nameof(this.CompressionLevel), ( int )this.CompressionLevel, typeof(CompressionMethod) )
                                      };
            inputFileStream.CopyTo( compressor );
        }
#if TRACK_FILE_SIZE_STATS
        outputCompressedFilePath.Refresh();
        if ( !_compressedFileSizes.ContainsKey( outputCompressedFilePath.FullName ) ) {
            _compressedFileSizes[ outputCompressedFilePath.FullName ] = new List<long>();
        }
        _compressedFileSizes[ outputCompressedFilePath.FullName ].Add( outputCompressedFilePath.Length );
#endif
    }
#endif

#if TAR
    [ Benchmark ]
    [ ArgumentsSource( nameof(TarArgumentMatrix) ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    public void UpdateTar( int FileCount, CompressionMethod CompressionMethod ) {
        if ( FileCount <= 0 ) {
            throw new ArgumentException( null, nameof(FileCount) );
        }
        FileInfo outputCompressedFilePath = GetOutputCompressedFilePath(
            directoryName: LOG_FILE_DIRECTORY,
            method: nameof(UpdateTar),
            compressionMethod: CompressionMethod,
            compressedFileCount: FileCount,
            archiveType: ArchiveType.Tar,
            compressionLevel: this.CompressionLevel );

        if ( outputCompressedFilePath.Exists ) {
            outputCompressedFilePath.Delete();
        }
        for ( int entryCounter = 0 ; entryCounter < FileCount ; entryCounter++ ) {
            using FileStream stream = outputCompressedFilePath.Open( FileMode.OpenOrCreate, FileAccess.Write );
            if ( entryCounter > 0 ) {
                // stream = outputCompressedFilePath.Open( FileMode.Open, FileAccess.Write );
                stream.Seek( -1024, SeekOrigin.End );
            }
            using TarWriter writer          = new TarWriter( stream, TarEntryFormat.Pax );
            TarEntry        entry           = new PaxTarEntry( TarEntryType.RegularFile, _logFileInfo[ entryCounter ].Name + CompressionMethod.GetFileExtension() );
            using var       inputFileStream = _logFileInfo[ entryCounter ].OpenRead();
            using var       ms              = new MemoryStream();
            using Stream compressor = CompressionMethod switch {
                                          // CompressionMethod.Zip     => new ,
                                          CompressionMethod.Brotli  => new BrotliStream( ms, this.CompressionLevel, leaveOpen: true ),
                                          CompressionMethod.Gzip    => new GZipStream( ms, this.CompressionLevel, leaveOpen: true ),
                                          CompressionMethod.Deflate => new DeflateStream( ms, this.CompressionLevel, leaveOpen: true ),
                                          _                         => throw new System.ComponentModel.InvalidEnumArgumentException( nameof(General.CompressionMethod), ( int )CompressionMethod, typeof(CompressionMethod) )
                                      };
            inputFileStream.CopyTo( compressor );
            compressor.Close();
            entry.DataStream = ms;
            ms.Seek( 0, SeekOrigin.Begin );
            writer.WriteEntry( entry );
        }
#if TRACK_FILE_SIZE_STATS
        outputCompressedFilePath.Refresh();
        if ( !_compressedFileSizes.ContainsKey( outputCompressedFilePath.FullName ) ) {
            _compressedFileSizes[ outputCompressedFilePath.FullName ] = new List<long>();
        }
        _compressedFileSizes[ outputCompressedFilePath.FullName ].Add( outputCompressedFilePath.Length );
#endif
    }

    [ Benchmark ]
    [ ArgumentsSource( nameof(TarArgumentMatrix) ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    public void CompressTar( int FileCount, CompressionMethod CompressionMethod ) {
        if ( FileCount <= 0 ) {
            throw new ArgumentException( null, nameof(FileCount) );
        }
        FileInfo outputCompressedFilePath = GetOutputCompressedFilePath(
            directoryName: LOG_FILE_DIRECTORY,
            method: nameof(CompressTar),
            compressionMethod: CompressionMethod,
            compressedFileCount: FileCount,
            archiveType: ArchiveType.Tar,
            compressionLevel: this.CompressionLevel );
        if ( outputCompressedFilePath.Exists ) {
            outputCompressedFilePath.Delete();
        }
        {
            MemoryStream archiveStream = new MemoryStream();
            using ( TarWriter writer = new TarWriter( archiveStream, TarEntryFormat.Pax, leaveOpen: true ) ) {
                for ( int entryCounter = 0 ; entryCounter < FileCount ; entryCounter++ ) {
                    writer.WriteEntry( _logFileInfo[ entryCounter ].FullName, _logFileInfo[ entryCounter ].Name );
                }
            }
            using FileStream compressedFileStream = outputCompressedFilePath.Create();
            archiveStream.Seek( 0, SeekOrigin.Begin );
            using Stream compressor = CompressionMethod switch {
                                          CompressionMethod.Brotli  => new BrotliStream( compressedFileStream, this.CompressionLevel, leaveOpen: true ),
                                          CompressionMethod.Gzip    => new GZipStream( compressedFileStream, this.CompressionLevel, leaveOpen: true ),
                                          CompressionMethod.Deflate => new DeflateStream( compressedFileStream, this.CompressionLevel, leaveOpen: true ),
                                          _                         => throw new System.ComponentModel.InvalidEnumArgumentException( nameof(General.CompressionMethod), ( int )CompressionMethod, typeof(CompressionMethod) )
                                      };
            archiveStream.CopyTo( compressor );
        }
#if TRACK_FILE_SIZE_STATS
        outputCompressedFilePath.Refresh();
        if ( !_compressedFileSizes.ContainsKey( outputCompressedFilePath.FullName ) ) {
            _compressedFileSizes[ outputCompressedFilePath.FullName ] = new List<long>();
        }
        _compressedFileSizes[ outputCompressedFilePath.FullName ].Add( outputCompressedFilePath.Length );
#endif
    }
#endif

//
#if TRACK_FILE_SIZE_STATS
    [ GlobalCleanup ]
    public void GlobalCleanup( ) {
        foreach ( var (path, listOfSizes) in _compressedFileSizes ) {
            System.Console.WriteLine( $"""
                                       path: {path}
                                       	 Min:     {listOfSizes.Min( x => x )}
                                       	 Average: {listOfSizes.Average( x => x )}
                                       	 Max:     {listOfSizes.Max( x => x )}
                                       """ );
            new FileInfo( path ).CopyTo( "/home/eric/Downloads/" + System.IO.Path.GetFileName( path ), true );
        }
    }
#endif
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

public enum ArchiveType {
    None,
    Zip,
    Tar,
}

internal static class CompressionMethodExtensions {
    /// <summary>
    /// The extension to use for the compressed file <b>within the <see cref="ZipArchive"/></b>
    /// </summary>
    public static string GetFileExtension( this CompressionMethod compressionMethod, ArchiveType archiveType ) =>
        archiveType switch {
            ArchiveType.Zip => compressionMethod switch {
                                   CompressionMethod.Brotli  => ".br",
                                   CompressionMethod.Deflate => ".dfl",
                                   CompressionMethod.Gzip    => ".gz",
                                   CompressionMethod.Zip     => ".zip", // Zip Entries don't need to be decompressed beyond what is automatically performed.
                                   _                         => throw new InvalidEnumArgumentException( $"{compressionMethod} is not a valid compression method", ( int )compressionMethod, typeof(CompressionMethod) )
                               },
            ArchiveType.Tar => compressionMethod switch {
                                   CompressionMethod.Brotli  => ".tar.br",
                                   CompressionMethod.Deflate => ".tar.dfl",
                                   CompressionMethod.Gzip    => ".tar.gz",
                                   // CompressionMethod.Zip     => ".tar.zip",
                                   _ => throw new InvalidEnumArgumentException( $"{compressionMethod} is not a valid compression method", ( int )compressionMethod, typeof(CompressionMethod) )
                               },
            _ => throw new InvalidEnumArgumentException( $"{archiveType} is not a valid archive type", ( int )archiveType, typeof(ArchiveType) )
        };

    /// <summary>
    /// The extension to use for the compressed file <b>within the <see cref="ZipArchive"/></b>
    /// </summary>
    public static string GetFileExtension( this CompressionMethod compressionMethod ) =>
        compressionMethod switch {
            CompressionMethod.Brotli  => ".br",
            CompressionMethod.Deflate => ".dfl",
            CompressionMethod.Gzip    => ".gz",
            CompressionMethod.Zip     => "", //"".zip", // Zip Entries don't need to be decompressed beyond what is automatically performed.
            _                         => throw new InvalidEnumArgumentException( $"{compressionMethod} is not a valid compression method", ( int )compressionMethod, typeof(CompressionMethod) )
        };

    /// <summary>
    /// The extension to use for the archive file.
    /// </summary>
    public static string GetFileExtension( this ArchiveType archiveType ) =>
        archiveType switch {
            ArchiveType.Zip  => ".zip",
            ArchiveType.Tar  => ".tar",
            ArchiveType.None => String.Empty,
            _                => throw new InvalidEnumArgumentException( $"{archiveType} is not a valid archive type", ( int )archiveType, typeof(ArchiveType) )
        };
}

public class FileSizeOutputColumn : IColumn {
    public string Id         { get; }
    public string ColumnName { get; }

    public FileSizeOutputColumn( string columnName = "Output Size" ) {
        ColumnName = columnName;
        Id         = nameof(FileSizeOutputColumn) + "." + ColumnName;
    }

    public bool IsDefault( Summary summary, BenchmarkCase benchmarkCase ) => false;

    public string GetValue( Summary summary, BenchmarkCase benchmarkCase ) {
        var compressionLevel    = ( CompressionLevel )benchmarkCase.Parameters[ nameof(CompressionBenchmarks.CompressionLevel) ];
        var compressionMethod   = ( CompressionMethod )benchmarkCase.Parameters[ "CompressionMethod" ];
        var compressedFileCount = ( ( int? )benchmarkCase.Parameters.Items.SingleOrDefault( p => p.Name == "FileCount" )?.Value ) ?? 1;
        var methodName          = benchmarkCase.Descriptor.WorkloadMethodDisplayInfo;
        var archiveType = methodName switch {
                              nameof(CompressionBenchmarks.CompressOnly) => ArchiveType.None,
#if TAR
                              nameof(CompressionBenchmarks.UpdateTar)   => ArchiveType.Tar,
                              nameof(CompressionBenchmarks.CompressTar) => ArchiveType.Tar,
#endif
#if ZIP
                              nameof(CompressionBenchmarks.UpdateZip)        => ArchiveType.Zip,
                              nameof(CompressionBenchmarks.CreateNewArchive) => ArchiveType.Zip,
#endif
                              _ => throw new InvalidEnumArgumentException()
                          };
        FileInfo outputCompressedFile = CompressionBenchmarks.GetOutputCompressedFilePath(
            directoryName: CompressionBenchmarks.LOG_FILE_DIRECTORY,
            method: methodName,
            compressionMethod: compressionMethod,
            compressionLevel: compressionLevel,
            compressedFileCount: compressedFileCount,
            archiveType: archiveType
        );


        string? discoveredOutputFilePath = new DirectoryInfo( System.IO.Path.IsPathRooted( CompressionBenchmarks.LOG_FILE_DIRECTORY )
                                                                  ? CompressionBenchmarks.LOG_FILE_DIRECTORY
                                                                  : "." )
                                           .GetFiles( searchPattern: outputCompressedFile.Name, searchOption: SearchOption.AllDirectories )
                                           .MaxBy( f => f.LastWriteTime )?.FullName;
        FileInfo? discoveredOutputFile = discoveredOutputFilePath is { } ? new FileInfo( discoveredOutputFilePath ) : null;

        string outputString = (
            discoveredOutputFile is { Exists: true }
                ? ( discoveredOutputFile.Length / ( float )( summary.Style.SizeUnit?.BaseUnits ?? 1 ) ).ToString( "0.00" )
                : "0" ) + ' ' + summary.Style.SizeUnit?.FullName;
//         Console.WriteLine( $"""
//                             ---------------------------------------- START ----------------------------------------
//                             FolderInfo                           : {benchmarkCase.FolderInfo}
//                             Descriptor FolderInfo                : {benchmarkCase.Descriptor.FolderInfo}
//                             DisplayInfo                          : {benchmarkCase.DisplayInfo}
//                             Descriptor DisplayInfo               : {benchmarkCase.Descriptor.DisplayInfo}
//                             Descriptor WorkloadMethodDisplayInfo : {benchmarkCase.Descriptor.WorkloadMethodDisplayInfo}
//                             benchmarkCase.Parameters.PrintInfo   : {benchmarkCase.Parameters.PrintInfo}
//                             compressionLevel                     : {compressionLevel}
//                             compressionMethod                    : {compressionMethod}
//                             fileName                             : {outputCompressedFile.Name}
//                             fileName                             : {outputCompressedFile.FullName}
//                             fileName                             : {discoveredOutputFile?.FullName}
//                             fileSize                             : {( discoveredOutputFile is { Exists: true } ? discoveredOutputFile.Length : null ):N0}
//                             RELEASE directory children           : {String.Join( ", ", directory?.GetDirectories().Select( d => d.Name ) ?? [ "NONE" ] )}
//                             {nameof(discoveredOutputFilePath)}   : {discoveredOutputFilePath}
//
//                             HostEnvironmentInfo  : {summary.HostEnvironmentInfo.Configuration}
//                             DotNetSdkVersion     : {summary.HostEnvironmentInfo.DotNetSdkVersion}
//                             LogFilePath          : {summary.LogFilePath}
//                             ResultsDirectoryPath : {summary.ResultsDirectoryPath}
//
//                             job Id: {benchmarkCase.Job.Id}
//                             job FolderInfo: {benchmarkCase.Job.FolderInfo}
//                             job ResolvedId: {benchmarkCase.Job.ResolvedId}
//                             job DisplayInfo: {benchmarkCase.Job.DisplayInfo}
//                             
//                             outputString: {outputString}
//
//                             ----------------------------------------  END  ----------------------------------------
//                             """ );
        return outputString;
    }

    public          bool           IsAvailable( Summary summary )                                               => true;
    public          bool           AlwaysShow                                                                   => true;
    public          ColumnCategory Category                                                                     => ColumnCategory.Metric;
    public          int            PriorityInCategory                                                           => 0;
    public          bool           IsNumeric                                                                    => true;
    public          UnitType       UnitType                                                                     => UnitType.Size;
    public          string         Legend                                                                       => $"Compressed output file size";
    public          string         GetValue( Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style ) => GetValue( summary, benchmarkCase );
    public override string         ToString( ) => ColumnName;
}

public class LocalConfig : BenchmarkConfig {
    public LocalConfig( ) {
        AddColumn( new FileSizeOutputColumn() );
        HideColumns( Column.Job, Column.InvocationCount, Column.UnrollFactor );

        WithSummaryStyle( new SummaryStyle(
                              // cultureInfo: CultureInfo.CurrentCulture,
                              cultureInfo: CultureInfo.CreateSpecificCulture( "en-US" ),
                              sizeUnit: SizeUnit.KB,
                              printUnitsInHeader: true,
                              timeUnit: null!,
                              printUnitsInContent: true,
                              maxParameterColumnWidth: 20,
                              printZeroValuesInContent: false,
                              ratioStyle: RatioStyle.Value
                          ) );
        AddExporter( CsvExporter.Default ); // It says "Already Present" ; TODO: re-enable
        // this.KeepBenchmarkFiles( false );
    }
}