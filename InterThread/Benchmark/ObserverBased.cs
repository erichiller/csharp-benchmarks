using System;
using System.Collections.Generic;

namespace Benchmarks.InterThread.Benchmark;

/*
 * https://docs.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern
 */

public record DataGeneratorMessage {
    public int    Id         { get; set; }
    public string Property_1 { get; set; }
}

public class DataGenerator : IObservable<DataGeneratorMessage> {
    private List<IObserver<DataGeneratorMessage>> observers = new();
    // private List<DataGeneratorMessage>            messages;

    /// <inheritdoc />
    public IDisposable Subscribe( IObserver<DataGeneratorMessage> observer ) {
        // Check whether observer is already registered. If not, add it
        if ( !observers.Contains( observer ) ) {
            observers.Add( observer );
        }

        return new Unsubscriber<DataGeneratorMessage>( observers, observer );
    }

    public void AddMessage( int id ) {
        var message = new DataGeneratorMessage() { Property_1 = "some name", Id = id };
        foreach ( var observer in observers ) {
            observer.OnNext( message );
        }
    }
}

public class Subscriber : IObserver<DataGeneratorMessage> {
    // private List<string> flightInfos = new List<message>();
    private IDisposable cancellation;


    public virtual void Subscribe( DataGenerator provider ) {
        cancellation = provider.Subscribe( this );
    }

    public virtual void Unsubscribe( ) {
        cancellation.Dispose();
        // flightInfos.Clear();
    }


    /// <inheritdoc />
    public void OnCompleted( ) {
        // flightInfos.Clear();
    }

    // No implementation needed: Method is not called by the BaggageHandler class.
    public virtual void OnError( Exception e ) {
        // No implementation.
    }

    /// <inheritdoc />
    public void OnNext( DataGeneratorMessage message ) {
        Console.WriteLine( $"{nameof(Subscriber)}.{nameof(OnNext)}( {message} ) called" );


        // bool updated = false;
        //
        // // Flight has unloaded its baggage; remove from the monitor.
        // if ( message.Carousel == 0 ) {
        //     var    flightsToRemove = new List<string>();
        //     string flightNo        = String.Format( "{0,5}", message.FlightNumber );
        //
        //     foreach ( var flightInfo in flightInfos ) {
        //         if ( flightInfo.Substring( 21, 5 ).Equals( flightNo ) ) {
        //             flightsToRemove.Add( flightInfo );
        //             updated = true;
        //         }
        //     }
        //
        //     foreach ( var flightToRemove in flightsToRemove )
        //         flightInfos.Remove( flightToRemove );
        //
        //     flightsToRemove.Clear();
        // } else {
        //     // Add flight if it does not exist in the collection.
        //     string flightInfo = String.Format( fmt, message.From, message.FlightNumber, message.Carousel );
        //     if ( !flightInfos.Contains( flightInfo ) ) {
        //         flightInfos.Add( flightInfo );
        //         updated = true;
        //     }
        // }
        //
        // if ( updated ) {
        //     flightInfos.Sort();
        //     Console.WriteLine( "Arrivals information from {0}", this.name );
        //     foreach ( var flightInfo in flightInfos )
        //         Console.WriteLine( flightInfo );
        //
        //     Console.WriteLine();
        // }
    }
}

internal class Unsubscriber<DataGeneratorMessage> : IDisposable {
    private List<IObserver<DataGeneratorMessage>> _observers;
    private IObserver<DataGeneratorMessage>       _observer;

    internal Unsubscriber( List<IObserver<DataGeneratorMessage>> observers, IObserver<DataGeneratorMessage> observer ) {
        this._observers = observers;
        this._observer  = observer;
    }

    public void Dispose( ) {
        if ( _observers.Contains( _observer ) )
            _observers.Remove( _observer );
    }
}