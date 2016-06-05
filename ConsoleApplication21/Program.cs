using System;
using System.Collections.Generic;

namespace ConsoleApplication21
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			// Define a provider and two observers.
			var provider = new LocationTracker();
			var reporter1 = new LocationReporter("FixedGPS");
			reporter1.Subscribe(provider);
			var reporter2 = new LocationReporter("MobileGPS");
			reporter2.Subscribe(provider);

			provider.TrackLocation(new Location(47.6456, -122.1312));
			reporter1.Unsubscribe();
			provider.TrackLocation(new Location(47.6677, -122.1199));
			provider.TrackLocation(null);
			provider.EndTransmission();
		}
	}

	public struct Location
	{
		public Location(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public double Latitude { get; }
		public double Longitude { get; }
	}

	public class LocationTracker : IObservable<Location>
	{
		private readonly List<IObserver<Location>> observers;

		public LocationTracker()
		{
			observers = new List<IObserver<Location>>();
		}

		public IDisposable Subscribe(IObserver<Location> observer)
		{
			if (!observers.Contains(observer))
				observers.Add(observer);
			return new Unsubscriber(observers, observer);
		}

		public void TrackLocation(Location? loc)
		{
			foreach (var observer in observers)
			{
				if (!loc.HasValue)
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

		private class Unsubscriber : IDisposable
		{
			private readonly IObserver<Location> _observer;
			private readonly List<IObserver<Location>> _observers;

			public Unsubscriber(List<IObserver<Location>> observers, IObserver<Location> observer)
			{
				_observers = observers;
				_observer = observer;
			}

			public void Dispose()
			{
				if (_observer != null && _observers.Contains(_observer))
					_observers.Remove(_observer);
			}
		}
	}

	public class LocationUnknownException : Exception
	{
		internal LocationUnknownException()
		{
		}
	}

	public class LocationReporter : IObserver<Location>
	{
		private IDisposable unsubscriber;

		public LocationReporter(string name)
		{
			Name = name;
		}

		public string Name { get; }

		public virtual void OnCompleted()
		{
			Console.WriteLine("The Location Tracker has completed transmitting data to {0}.", Name);
			Unsubscribe();
		}

		public virtual void OnError(Exception e)
		{
			Console.WriteLine("{0}: The location cannot be determined.", Name);
		}

		public virtual void OnNext(Location value)
		{
			Console.WriteLine("{2}: The current location is {0}, {1}", value.Latitude, value.Longitude, Name);
		}

		public virtual void Subscribe(IObservable<Location> provider)
		{
			if (provider != null)
				unsubscriber = provider.Subscribe(this);
		}

		public virtual void Unsubscribe()
		{
			unsubscriber.Dispose();
		}
	}
}