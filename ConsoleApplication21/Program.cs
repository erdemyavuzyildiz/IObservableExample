using System;
using System.Collections.Generic;

namespace ConsoleApplication21
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var locationReporter = new LocationReporter("user1");


			var locationTracker = new LocationTracker();
			var disposable= locationReporter.Subscribe(locationTracker);

		}
	}

	public class SportNewsSubject : IObservable<SportNews>
	{
		private readonly List<IObserver<SportNews>> observers = new List<IObserver<SportNews>>();

		public IDisposable Subscribe(IObserver<SportNews> observer)
		{
			observers.Add(observer);
			return null;
		}

		//SomethingHappened
		public void Broadcast()
		{
			var sportNews = new SportNews();

			observers.ForEach(y => y.OnNext(sportNews));
			observers.ForEach(y => y.OnCompleted());
		}
	}

	public class SportNewsObserver : IObserver<SportNews>
	{
		public void OnNext(SportNews value)
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}

		public void OnCompleted()
		{
			throw new NotImplementedException();
		}
	}

	public class SportNews
	{
	}

	public struct Location
{
   double lat, lon;

   public Location(double latitude, double longitude)
   {
      this.lat = latitude;
      this.lon = longitude;
   }

   public double Latitude
   { get { return this.lat; } }

   public double Longitude
   { get { return this.lon; } }
}


public class LocationTracker : IObservable<Location>
{
   public LocationTracker()
   {
      observers = new List<IObserver<Location>>();
   }

   private List<IObserver<Location>> observers;

   public IDisposable Subscribe(IObserver<Location> observer) 
   {
      if (! observers.Contains(observer)) 
         observers.Add(observer);
      return new Unsubscriber(observers, observer);
   }

   private class Unsubscriber : IDisposable
   {
      private List<IObserver<Location>>_observers;
      private IObserver<Location> _observer;

      public Unsubscriber(List<IObserver<Location>> observers, IObserver<Location> observer)
      {
         this._observers = observers;
         this._observer = observer;
      }

      public void Dispose()
      {
         if (_observer != null && _observers.Contains(_observer))
            _observers.Remove(_observer);
      }
   }

   public void TrackLocation(Nullable<Location> loc)
   {
      foreach (var observer in observers) {
         if (! loc.HasValue)
            observer.OnError(new LocationUnknownException());
         else
            observer.OnNext(loc.Value);
      }
   }

   public void EndTransmission()
   {
      foreach (var observer in observers.ToArray())
         if (observers.Contains(observer))
            observer.OnCompleted();

      observers.Clear();
   }
}

public class LocationUnknownException : Exception
{
   internal LocationUnknownException() 
   { }
}


public class LocationReporter : IObserver<Location>
{
   private IDisposable unsubscriber;
   private string instName;

   public LocationReporter(string name)
   {
      this.instName = name;
   }

   public string Name
   {  get{ return this.instName; } }

   public virtual void Subscribe(IObservable<Location> provider)
   {
      if (provider != null) 
         unsubscriber = provider.Subscribe(this);
   }

   public virtual void OnCompleted()
   {
      Console.WriteLine("The Location Tracker has completed transmitting data to {0}.", this.Name);
      this.Unsubscribe();
   }

   public virtual void OnError(Exception e)
   {
      Console.WriteLine("{0}: The location cannot be determined.", this.Name);
   }

   public virtual void OnNext(Location value)
   {
      Console.WriteLine("{2}: The current location is {0}, {1}", value.Latitude, value.Longitude, this.Name);
   }

   public virtual void Unsubscribe()
   {
      unsubscriber.Dispose();
   }
}
}