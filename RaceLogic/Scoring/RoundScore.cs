using System;
using RaceLogic.RoundTiming;

namespace RaceLogic.Scoring
{
    public class RoundScore<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        private readonly TRiderId riderId;
        public TRiderId RiderId => PositionDetails != null ? PositionDetails.RiderId : riderId;
        public int Points { get; }
        public int Position { get; }
        public RoundPosition<TRiderId> PositionDetails { get; }

        public RoundScore(RoundPosition<TRiderId> positionDetails, int position, int points)
        {
            PositionDetails = positionDetails;
            Position = position;
            Points = points;
        }
        
        public RoundScore(TRiderId riderId, int position, int points)
        {
            this.riderId = riderId;
            Position = position;
            Points = points;
        }
    }
}