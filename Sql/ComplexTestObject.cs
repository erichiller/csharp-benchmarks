using System;
using System.ComponentModel.DataAnnotations;

using NodaTime;

namespace Benchmarks.Sql;

public record ComplexTestObject {
    public ComplexTestObject( int id, string textCol, Instant tzCol, int intCol, decimal decCol1, decimal decCol2, float floatCol, string[] textArrayCol, int[] intArrayCol ) {
        Id           = id;
        TextCol      = textCol;
        TzCol        = tzCol;
        IntCol       = intCol;
        DecCol1      = decCol1;
        DecCol2      = decCol2;
        FloatCol     = floatCol;
        TextArrayCol = textArrayCol;
        IntArrayCol  = intArrayCol;
    }

    [ Key ]
    public int Id { get; init; }

    public string   TextCol       { get; init; }
    public Instant  TzCol     { get; init; }
    public int      IntCol      { get; init; }
    public Decimal  DecCol1      { get; init; }
    public Decimal  DecCol2      { get; init; }
    public float    FloatCol        { get; init; }
    public string[] TextArrayCol  { get; init; }
    public int[]    IntArrayCol { get; init; }

    public static string CreateSqlString = @"
        DROP TABLE IF EXISTS public.complex_test_objects;
        CREATE TABLE public.complex_test_objects
        (
            id integer NOT NULL,
            text_col text NOT NULL,
            tz_col timestamptz NOT NULL,
            int_col int NOT NULL,
            dec_col1 decimal NOT NULL,
            dec_col2 decimal NOT NULL,
            text_array_col text[],
            int_array_col int[],
            CONSTRAINT pk_complex_test_object_id PRIMARY KEY (id)
        )
            TABLESPACE pg_default;
			ALTER TABLE IF EXISTS public.test_objects_complex OWNER TO ""mkmrk-config-tests"";
            ";

   //  public static string CreateSqlString = @"
   //      DROP TABLE IF EXISTS public.complex_test_objects;
   //      CREATE TABLE public.complex_test_objects
   //      (
   //          id integer NOT NULL,
   //          text_col text NOT NULL,
   //          tz_col timestamptz NOT NULL,
   //          int_col int NOT NULL,
   //          dec_col1 decimal NOT NULL,
   //          dec_col2 decimal NOT NULL,
   //          float_col float NOT NULL,
   //          text_array_col text[],
   //          int_array_col int[],
   //          CONSTRAINT pk_complex_test_object_id PRIMARY KEY (id)
   //      )
   //          TABLESPACE pg_default;
			// ALTER TABLE IF EXISTS public.test_objects_complex OWNER TO ""mkmrk-config-tests"";
   //          ";
}