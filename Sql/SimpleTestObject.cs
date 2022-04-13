using System;
using System.ComponentModel.DataAnnotations;

using NodaTime;

namespace Benchmarks.Sql;

public record SimpleTestObject {
    [ Key ]
    public int Id { get; init; }

    public string  Name     { get; init; }
    public int[]   Integers { get; init; }
    public Instant Datetime { get; init; }

    private static readonly Instant _testTime        = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
    private static          Random  _rand            = new ();

    public static Instant GetRandomInstant( Instant originTime = default, int floor = -365 ) {
        if ( originTime == default ) {
            originTime = _testTime;
        }
        return originTime.Plus( Duration.FromDays( _rand.Next( floor, floor + ( 365 * 2 ) ) ) );
    }

    public static SimpleTestObject GetNewObject( int id ) => new SimpleTestObject {
        Id       = id,
        Name     = "TestObject Name here",
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
    public static string CreatePartitionTable = @"
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
    --WITH ( a)
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
}