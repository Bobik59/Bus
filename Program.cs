namespace Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    class Passenger
    {
        public int DesiredBusNumber { get; }

        public Passenger(int busNumber)
        {
            DesiredBusNumber = busNumber;
        }
    }

    class Bus
    {
        public int Number { get; }
        public int Capacity { get; }
        private List<Passenger> _onBoard;

        public Bus(int number, int capacity)
        {
            Number = number;
            Capacity = capacity;
            _onBoard = new List<Passenger>();
        }

        public void BoardPassengers(List<Passenger> passengers)
        {
            foreach (var passenger in passengers.Take(Capacity - _onBoard.Count))
            {
                _onBoard.Add(passenger);
            }
        }

        public List<Passenger> DropOffPassengers()
        {
            var droppedOff = new List<Passenger>(_onBoard);
            _onBoard.Clear();
            return droppedOff;
        }

        public int FreeSeats => Capacity - _onBoard.Count;
    }

    class BusStop
    {
        private Queue<Passenger> _waitingPassengers = new Queue<Passenger>();
        private object _lock = new object();
        public void AddPassengers(List<Passenger> passengers)
        {
            lock (_lock)
            {
                foreach (var passenger in passengers)
                {
                    _waitingPassengers.Enqueue(passenger);
                }
                Console.WriteLine($"На остановке добавлено {passengers.Count} пассажиров. Всего ожидающих: {_waitingPassengers.Count}");
            }
        }

        public List<Passenger> BoardPassengers(Bus bus)
        {
            lock (_lock)
            {
                var boardingPassengers = new List<Passenger>();
                int seatsAvailable = bus.FreeSeats; 

                while (_waitingPassengers.Count > 0 && seatsAvailable > 0)
                {
                    var passenger = _waitingPassengers.Peek();

                    if (passenger.DesiredBusNumber == bus.Number)
                    {
                        boardingPassengers.Add(_waitingPassengers.Dequeue());
                        seatsAvailable--;
                    }
                    else
                    {
                        break;
                    }
                }

                Console.WriteLine($"Автобус {bus.Number} забрал {boardingPassengers.Count} пассажиров.");
                return boardingPassengers;
            }
        }
        public void PrintWaitingPassengers()
        {
            lock (_lock)
            {
                Console.WriteLine($"Всего ожидающих пассажиров: {_waitingPassengers.Count}");
            }
        }
    }

    class Program
    {
        static BusStop busStop = new BusStop();
        static Random random = new Random();

        static void GeneratePassengers()
        {
            while (true)
            {
                var newPassengers = new List<Passenger>();
                int count = random.Next(1, 6); // Генерация 1-5 пассажиров
                for (int i = 0; i < count; i++)
                {
                    int busNumber = random.Next(1, 4); // Ожидание автобусов 1-3
                    newPassengers.Add(new Passenger(busNumber));
                }

                busStop.AddPassengers(newPassengers);
                Thread.Sleep(random.Next(1000, 3000)); // Задержка 1-3 секунды
            }
        }

        static void BusSimulation(Bus bus)
        {
            while (true)
            {
                Console.WriteLine($"Автобус {bus.Number} прибыл на остановку.");
                var droppedOff = bus.DropOffPassengers();
                Console.WriteLine($"Автобус {bus.Number} высадил {droppedOff.Count} пассажиров.");

                var newPassengers = busStop.BoardPassengers(bus);
                bus.BoardPassengers(newPassengers);

                busStop.PrintWaitingPassengers();
                Thread.Sleep(random.Next(5000, 10000)); // Задержка 5-10 секунд
            }
        }

        static void Main(string[] args)
        {
            Thread passengerGenerator = new Thread(GeneratePassengers);
            passengerGenerator.Start();

            List<Thread> busThreads = new List<Thread>();
            for (int i = 1; i <= 3; i++) // Автобусы с номерами 1-3
            {
                var bus = new Bus(i, random.Next(5, 10)); // Вместимость 5-10 мест
                var busThread = new Thread(() => BusSimulation(bus));
                busThreads.Add(busThread);
                busThread.Start();
            }

            foreach (var thread in busThreads)
            {
                thread.Join();
            }

            passengerGenerator.Join();
        }
    }

}
