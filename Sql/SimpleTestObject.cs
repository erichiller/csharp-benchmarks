using System;
using System.ComponentModel.DataAnnotations;

using NodaTime;

using Npgsql;

using NpgsqlTypes;

namespace Benchmarks.Sql;

public record SimpleTestObject {
    [ Key ]
    public int Id { get; init; }

    public string  Name     { get; init; }
    public int[]   Integers { get; init; }
    public Instant Datetime { get; init; }

    private static readonly Instant _testTime = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
    private static          Random  _rand     = new ();

    public static Instant GetRandomInstant( Instant originTime = default, int floor = -365 ) {
        if ( originTime == default ) {
            originTime = _testTime;
        }

        return originTime.Plus( Duration.FromDays( _rand.Next( floor, floor + ( 365 * 2 ) ) ) );
    }
    
    public static SimpleTestObject GetNewObject( int id, int groups = 5 ) => new SimpleTestObject {
        Id       = id,
        Name     = $"Group{id%groups}",
        Integers = new[] { _rand.Next(), _rand.Next(), _rand.Next() },
        Datetime = GetRandomInstant()
    };

    public static string CreateSqlString = @"
        DROP TABLE IF EXISTS public.test_objects;
        CREATE TABLE public.test_objects
        (
            id integer NOT NULL,
            name text,
            integers int[],
            datetime timestamptz,
            CONSTRAINT pk_test_object_id PRIMARY KEY (id)
        )
            TABLESPACE pg_default;";

    //language=sql
    private static string _createPartitionTableSql = @"
        -- Table: public.test_object_partition_table
        DROP TABLE IF EXISTS public.test_object_partition_table;
        CREATE TABLE IF NOT EXISTS public.test_object_partition_table
        (
            id integer NOT NULL,
            name text,
            integers integer[],
            datetime timestamp with time zone NOT NULL
        ) PARTITION BY RANGE (id);


    CREATE INDEX IF NOT EXISTS test_object_partition_table_brin_idx
        ON public.test_object_partition_table USING brin (id) 
        WITH ( autosummarize = on )
    ;

        -- Partitions SQL
        CREATE TABLE public.part_1 PARTITION OF public.test_object_partition_table
            FOR VALUES FROM (0) TO (20000000);
        CREATE TABLE public.part_2 PARTITION OF public.test_object_partition_table
            FOR VALUES FROM (20000000) TO (40000000);
        CREATE TABLE public.part_3 PARTITION OF public.test_object_partition_table
            FOR VALUES FROM (40000000) TO (60000000);
        CREATE TABLE public.part_4 PARTITION OF public.test_object_partition_table
            FOR VALUES FROM (60000000) TO (80000000);
        CREATE TABLE public.part_5 PARTITION OF public.test_object_partition_table
            FOR VALUES FROM (80000000) TO (100000000);
        CREATE TABLE public.part_6 PARTITION OF public.test_object_partition_table
            FOR VALUES FROM (100000000) TO (120000000);
        CREATE TABLE public.part_default PARTITION OF public.test_object_partition_table DEFAULT ;
";

    public static void CreatePartitionTableBtreeIndex( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() {
            Connection = dbConnection,
            CommandText = @"
                DROP INDEX IF EXISTS public.test_object_partition_table_brin_idx;

                CREATE INDEX IF NOT EXISTS test_object_partition_table_btree_idx
	                ON public.test_object_partition_table USING btree (id) ;"
        };
        cmd.ExecuteNonQuery();
    }
    
    public static void CreatePartitionTableBrinIndex( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() {
            Connection = dbConnection,
            CommandText = @"
                DROP INDEX IF EXISTS public.test_object_partition_table_btree_idx;

                CREATE INDEX IF NOT EXISTS test_object_partition_table_brin_idx
                    ON public.test_object_partition_table USING brin (id)
                    WITH ( autosummarize = on );
                -- REINDEX INDEX my_index;
                " 
        };
        cmd.ExecuteNonQuery();
    }

    // /// <summary>
    // /// Return Fully Qualified Table Name
    // /// </summary>
    // /// <param name="sql"></param>
    // /// <returns></returns>
    // private string getTableNameFromCreateTableSql( string sql ) {
    //     var matches = Regex.Match( sql, @"CREATE TABLE [A-Z ]* (?<schemaName>[a-z_]*)\.(?<tableName>""?[a-z_ ]+""?)" );
    //     return matches.Groups[ "schemaName" ].Value + "." + matches.Groups[ "tableName" ].Value;
    // }

    private static int getPartitionTableCount( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() {
            Connection = dbConnection,
            CommandText =
                @"SELECT count(*) FROM public.test_object_partition_table;"
        };
        using var reader = cmd.ExecuteReader();
        reader.Read();
        return reader.GetInt32( 0 );
    }

    private static bool IsPartitionTablePopulated( NpgsqlConnection dbConnection, int minimumCountThreshold = 100_000 ) =>
        getPartitionTableCount(dbConnection) > minimumCountThreshold;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbConnection"></param>
    /// <param name="createCount"></param>
    /// <param name="startId"></param>
    /// <returns>Returns rows created, which can then be added back to the caller's <c>_count</c></returns>
    /// <exception cref="Exception"></exception>
    public static int PopulatePartitionTable( NpgsqlConnection dbConnection, int startId, int createCount ) {
        int objectsPerCopy = 1000; /* 1000 objects per COPY */
        if ( createCount % objectsPerCopy != 0 ) {
            throw new ArgumentException( $"{nameof(createCount)} % {objectsPerCopy} must be 0. " );
        }

        int rowsCreated = 0;

        for ( int o = 0 ; o < ( createCount / objectsPerCopy ) ; o++ ) {
            using var writer = dbConnection.BeginBinaryImport( "COPY public.test_object_partition_table (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < objectsPerCopy ; i++, startId++ ) {
                SimpleTestObject insertObject = SimpleTestObject.GetNewObject( startId );
                writer.StartRow();
                writer.Write( insertObject.Id, NpgsqlDbType.Integer );
                writer.Write( insertObject.Name, NpgsqlDbType.Text );
                writer.Write( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                writer.Write( insertObject.Datetime, NpgsqlDbType.TimestampTz );
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
                        tablename  = 'test_object_partition_table'
                    );"
        };
        using var reader = cmd.ExecuteReader();
        reader.Read();
        return reader.GetBoolean( 0 );
    }

    public static int CreatePartitionTable( NpgsqlConnection dbConnection ) {
        using var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = SimpleTestObject._createPartitionTableSql };
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
    public static int CreateAndPopulateIfNotExists( NpgsqlConnection dbConnection, int startId, int createCount, int threshold = 10_000 ) {
        if ( !DoesPartitionTableExist( dbConnection ) ) {
            CreatePartitionTable( dbConnection );
        }

        if ( !IsPartitionTablePopulated( dbConnection, threshold ) ) {
            return PopulatePartitionTable( dbConnection, startId, createCount );
        }

        return getPartitionTableCount( dbConnection );
    }
}