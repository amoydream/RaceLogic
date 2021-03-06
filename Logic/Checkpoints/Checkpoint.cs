using System;
using System.Collections.Generic;
using System.Threading;
using maxbl4.Infrastructure.Extensions.DateTimeExt;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.Checkpoints
{
    public class Checkpoint : ICheckpoint, IHasId<Checkpoint>
    {
        public Id<Checkpoint> Id { get; set; } = Id<Checkpoint>.NewId();
        public DateTime Timestamp { get; set; } = Constants.DefaultUtcDate;
        public string RiderId { get; set; }
        public DateTime LastSeen { get; set; } = Constants.DefaultUtcDate;
        public int Count { get; set; } = 1;
        public bool Aggregated { get; set; }
        public bool IsManual { get; set; }
        public int Rps { get; set; }

        public Checkpoint()
        {
        }

        public Checkpoint(string riderId, int count = 1) : this(riderId, Constants.DefaultUtcDate, count) { }
        
        public Checkpoint(string riderId, DateTime timestamp, int count = 1)
        {
            RiderId = riderId;
            LastSeen = Timestamp = timestamp;
            Count = count;
            UpdateRps();
        }

        public override string ToString()
        {
            return $"{RiderId} Ts:{Timestamp:t}";
        }

        public Checkpoint ToAggregated()
        {
            return new Checkpoint
            {
                RiderId = RiderId,
                Aggregated = true,
                Count = Count,
                Id = Id,
                Rps = Rps,
                Timestamp = Timestamp,
                IsManual = IsManual,
                LastSeen = LastSeen
            };
        }
        
        public Checkpoint WithRiderId(string riderId)
        {
            return new Checkpoint
            {
                RiderId = riderId,
                Aggregated = Aggregated,
                Count = Count,
                Id = Id,
                Rps = Rps,
                Timestamp = Timestamp,
                IsManual = IsManual,
                LastSeen = LastSeen
            };
        }
        
        public Checkpoint AddToAggregated(Checkpoint cp)
        {
            if (!Aggregated)
                throw new InvalidOperationException("This checkpoint is not aggregated");
            if (RiderId != cp.RiderId)
                throw new ArgumentException($"Found checkpoints with different RiderIds {RiderId} {cp.RiderId}", nameof(cp));
            Timestamp = Timestamp.TakeSmaller(cp.Timestamp);
            LastSeen = LastSeen.TakeLarger(cp.Timestamp);
            Count += cp.Count;
            IsManual |= cp.IsManual;
            UpdateRps();
            return this;
        }

        public int UpdateRps()
        {
            var interval = (LastSeen - Timestamp).TotalMilliseconds;
            if (interval < 1)
                Rps = Count;
            else
            {
                Rps = (int)Math.Ceiling(Count * 1000 / interval);
            }

            return Rps;
        }
        
        private sealed class TimestampRelationalComparer : IComparer<Checkpoint>
        {
            public int Compare(Checkpoint x, Checkpoint y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }
        
        public static IComparer<Checkpoint> TimestampComparer { get; } = new TimestampRelationalComparer();
    }
}