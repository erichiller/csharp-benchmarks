using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using NodaTime;

using Npgsql;

using NpgsqlTypes;

namespace Benchmarks.Sql;

public record TestObjectPartitionTableB {
    public Instant EventTimestamp { get; init; } // TODO: set as [Key] for EF Core?

    public string  EventKey      { get; init; } // TODO: set as [Key] for EF Core?
    public decimal NumericValue  { get; init; }
    public Int16   SmallintValue { get; init; }

    private static readonly Instant _testTime = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
    private static          Random  _rand     = new ();

    public static Instant GetRandomInstant( Instant originTime = default, int floor = -365 ) {
        if ( originTime == default ) {
            originTime = _testTime;
        }

        return originTime.Plus( Duration.FromDays( _rand.Next( floor, floor + ( 365 * 2 ) ) ) );
    }

    public static TestObjectPartitionTableB GetNextNewObject( ) {
        currentEventTimestamp += Duration.FromMilliseconds( _rand.Next( targetMillisecondInterval - millisecondDurationVariance, targetMillisecondInterval + millisecondDurationVariance ) );
        return new TestObjectPartitionTableB {
            EventTimestamp = currentEventTimestamp,
            EventKey       = $"Group{currentCount % GROUPS}",
            NumericValue   = ( decimal )_rand.NextDouble() * _rand.Next(),
            SmallintValue  = ( Int16 )_rand.Next( Int16.MaxValue )
        };
    }

    public static   int     preLoadCount;
    public static   Instant startDatetime               = default;
    public static   Instant currentEventTimestamp       = startDatetime == default ? Instant.FromDateTimeUtc( DateTime.UtcNow ) : startDatetime;
    public const    int     GROUPS                      = 5;
    public const    int     dayCount                    = 90;
    static          int     eventsPerDay                = preLoadCount / dayCount;
    static readonly int     targetMillisecondInterval   = 86_400_000   / eventsPerDay;
    static readonly int     millisecondDurationVariance = ( int )( targetMillisecondInterval * 0.5 );
    public static   int     currentCount                = 0;

    public static TestObjectPartitionTableB[] GetNewObjects( int count ) {
        TestObjectPartitionTableB[] createdObjects = new TestObjectPartitionTableB[ count ];
        for ( ; currentCount < count ; currentCount++ ) { }

        return createdObjects;
    }

    public static string CreateSqlString = @"
        DROP TABLE IF EXISTS public.test_objects_b;
        CREATE TABLE public.test_objects_b
        (
            event_timestamp timestamp with time zone NOT NULL,
            event_key text NOT NULL,
            numeric_value numeric NOT NULL,
            smallint_value smallint NOT NULL,
            CONSTRAINT pk_test_object_id PRIMARY KEY (event_timestamp, event_key)
        )
            TABLESPACE pg_default;";

    //language=sql
    private static string _createPartitionTableSql = @"
        -- Table: public.partition_table_b
        DROP TABLE IF EXISTS public.partition_table_b;
        CREATE TABLE IF NOT EXISTS public.partition_table_b
        (
            event_timestamp timestamp with time zone NOT NULL,
            event_key text NOT NULL,
            numeric_value numeric NOT NULL,
            smallint_value smallint NOT NULL
        ) PARTITION BY RANGE (event_key);


        CREATE INDEX IF NOT EXISTS partition_table_b_brin_idx
            ON public.partition_table_b USING brin (event_timestamp) 
            WITH ( autosummarize = on );
        
        -- Partitions SQL
        CREATE TABLE public.part_default PARTITION OF public.partition_table_b DEFAULT ;
        ;" +
                                                     String.Join( String.Empty,
                                                                  Enumerable.Range( 0, GROUPS - 1 ).Select( groupCount => @$"
        
        CREATE TABLE public.part_1 PARTITION OF public.partition_table_b
            FOR VALUES IN ('Group{groupCount}');
         " ) );

    public static void CreatePartitionTableBtreeIndex( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() {
            Connection = dbConnection,
            CommandText = @"
                DROP INDEX IF EXISTS public.partition_table_b_brin_idx;

                CREATE INDEX IF NOT EXISTS partition_table_b_btree_idx
	                ON public.partition_table_b USING btree (event_timestamp, key) ;"
        };
        cmd.ExecuteNonQuery();
    }

    public static void CreatePartitionTableBrinIndex( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() {
            Connection = dbConnection,
            CommandText = @"
                DROP INDEX IF EXISTS public.partition_table_b_btree_idx;

                CREATE INDEX IF NOT EXISTS partition_table_b_brin_idx
                    ON public.partition_table_b USING brin (event_timestamp)
                    WITH ( autosummarize = on );
                -- REINDEX INDEX my_index;
                "
        };
        cmd.ExecuteNonQuery();
    }

    private static int getPartitionTableCount( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() {
            Connection = dbConnection,
            CommandText =
                @"SELECT count(*) FROM public.partition_table_b;"
        };
        using var reader = cmd.ExecuteReader();
        reader.Read();
        return reader.GetInt32( 0 );
    }

    private static bool IsPartitionTablePopulated( NpgsqlConnection dbConnection, int minimumCountThreshold = 100_000 ) =>
        getPartitionTableCount( dbConnection ) > minimumCountThreshold;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbConnection"></param>
    /// <param name="createCount"></param>
    /// <returns>Returns rows created, which can then be added back to the caller's <c>_count</c></returns>
    /// <exception cref="Exception"></exception>
    public static int PopulatePartitionTable( NpgsqlConnection dbConnection, int createCount ) {
        int objectsPerCopy = 1000; /* 1000 objects per COPY */
        if ( createCount % objectsPerCopy != 0 ) {
            throw new ArgumentException( $"{nameof(createCount)} % {objectsPerCopy} must be 0. " );
        }

        int rowsCreated = 0;

        var objects     = GetNewObjects( createCount );
        int objectCount = 0;

        for ( int o = 0 ; o < ( createCount / objectsPerCopy ) ; o++ ) {
            using var writer = dbConnection.BeginBinaryImport( "COPY public.partition_table_b (event_timestamp, event_key, numeric_value, smallint_value ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < objectsPerCopy ; i++, objectCount++ ) {
                writer.StartRow();
                writer.Write( objects[ objectCount ].EventTimestamp, NpgsqlDbType.TimestampTz );
                writer.Write( objects[ objectCount ].EventKey, NpgsqlDbType.Text );
                writer.Write( objects[ objectCount ].NumericValue, NpgsqlDbType.Numeric );
                writer.Write( objects[ objectCount ].SmallintValue, NpgsqlDbType.Smallint );
            }

            rowsCreated += ( int )writer.Complete();
        }

        return rowsCreated;
    }

    public static bool DoesPartitionTableExist( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() {
            Connection = dbConnection,
            CommandText =
                @"SELECT EXISTS (
                    SELECT FROM 
                        pg_tables
                    WHERE 
                        schemaname = 'public' AND 
                        tablename  = 'partition_table_b'
                    );"
        };
        using var reader = cmd.ExecuteReader();
        reader.Read();
        return reader.GetBoolean( 0 );
    }

    public static int CreatePartitionTable( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = TestObjectPartitionTableB._createPartitionTableSql };
        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///  
    /// </summary>
    /// <param name="dbConnection"></param>
    /// <param name="startId"></param>
    /// <param name="createCount"></param>
    /// <param name="threshold"></param>
    /// <returns>
    /// If not populated, returns rows created.
    /// Otherwise returns the count of rows which already exist.
    /// This number can then be added back to the caller's <c>_count</c>
    /// </returns>
    public static int CreateAndPopulateIfNotExists( NpgsqlConnection dbConnection, int createCount, int threshold = 10_000 ) {
        if ( !DoesPartitionTableExist( dbConnection ) ) {
            CreatePartitionTable( dbConnection );
        }

        if ( !IsPartitionTablePopulated( dbConnection, threshold ) ) {
            return PopulatePartitionTable( dbConnection, createCount );
        }

        return getPartitionTableCount( dbConnection );
    }
}