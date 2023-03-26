using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using NodaTime;

namespace Benchmarks.Sql;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum TestEnum {
    value1 = 0,
    value2 = 1,
    value3 = 2
}

public record EnumTestObject {
    [ Key ]
    public int Id { get; init; }

    public required string   Name     { get; init; }
    public required int[]    Integers { get; init; }
    public required Instant  Datetime { get; init; }
    public required TestEnum TestEnum { get; init; }

    private static readonly Instant _testTime = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
    private static readonly Random  _rand     = new ();

    
    // ReSharper disable once MemberCanBePrivate.Global
    public static Instant GetRandomInstant( Instant originTime = default, int floor = -365 ) {
        if ( originTime == default ) {
            originTime = _testTime;
        }

        return originTime.Plus( Duration.FromDays( _rand.Next( floor, floor + ( 365 * 2 ) ) ) );
    }
    
    public static EnumTestObject GetNewObject( int id, int groups = 5 ) => new EnumTestObject {
        Id       = id,
        Name     = $"Group{id%groups}",
        Integers = new[] { _rand.Next(), _rand.Next(), _rand.Next() },
        Datetime = GetRandomInstant(),
        TestEnum = (TestEnum) (id % 3)
    };

    [StringSyntax("sql")]
    public static string CreateSqlString = @"
        DROP TYPE IF EXISTS public.test_enum  CASCADE;
        CREATE TYPE public.test_enum AS ENUM
            ('value1', 'value2', 'value3');

        DROP TABLE IF EXISTS public.test_enum_objects;
        CREATE TABLE public.test_enum_objects
        (
            id integer NOT NULL,
            name text,
            integers int[],
            datetime timestamptz,
            test_enum test_enum,
            CONSTRAINT pk_test_enum_object_id PRIMARY KEY (id)
        )
            TABLESPACE pg_default;";
}