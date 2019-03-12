using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.CalculationModel;
using RaceLogic.Interfaces;
using RaceLogic.ReferenceModel;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests
{
    public class PositionAggregatorTests
    {
        //TODO: implement cases:
        // 1. Two racers, two rounds. One racer in one round has zero points, but it is sent as round. But should be ignored. And racer with two rounds should win
        // 2. Check positions assigned
        [Fact]
        public void Test_no_ties()
        {
            var source = Rounds(R(1,2,3), R(3,1,2));
            var rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(3);
            rating[0].Check(1, 5, 3);
            rating[1].Check(3, 4, 2);
            rating[2].Check(2, 3, 1);
        }
        
        [Fact]
        public void Test_tie_mirror_positions()
        {
            var source = Rounds(
                R(1,2,3), 
                R(3,2,1));
            var rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(3);
            rating[0].Check(3, 4, 3);
            rating[1].Check(1, 4, 2);
            rating[2].Check(2, 4, 1);
        }
        
        [Fact]
        public void Test_tie_mirror_positions_missed_rounds()
        {
            var source = Rounds(
                R(1), 
                R(2));
            var rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(2);
            rating[0].Check(2, 1, 2);
            rating[1].Check(1, 1, 1);
            source = Rounds(
                R(1,3,4), 
                R(2,3,4));
            rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(4);
            rating[0].Check(3, 4, 4);
            rating[1].Check(2, 3, 3);
            rating[2].Check(1, 3, 2);
            rating[3].Check(4, 2, 1);
        }
        
        [Fact]
        public void Test_tie_missed_rounds_three()
        {
            var source = Rounds(
                R(1,3,4), 
                R(2,1,4),
                R(4,2,3));
            var rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(4);
            rating[0].Check(4, 5, 4);
            rating[1].Check(2, 5, 3);
            rating[2].Check(1, 5, 2);
            rating[3].Check(3, 3, 1);
        }
        
        [Fact]
        public void Test_tie_missed_rounds_three_both_in_last_round()
        {
            var source = Rounds(
                R(3,1,4), 
                R(2,3,4),
                R(1,2,3));
            var rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(4);
            rating[0].Check(3, 6, 4);
            rating[1].Check(1, 5, 3);
            rating[2].Check(2, 5, 2);
            rating[3].Check(4, 2, 1);
        }
        
        [Fact]
        public void Test_tie_diff_positions()
        {
            var source = Rounds(
                R(1,2,3,4,5), 
                R(3,5,2,1,4));
            var rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(5);
            rating[0].Check(3, 8, 5);
            rating[1].Check(1, 7, 4);
            rating[2].Check(2, 7, 3);
            rating[3].Check(5, 5, 2);
            rating[4].Check(4, 3, 1);
        }
        
        [Fact]
        public void Test_diff_positions_same_points()
        {
            var source = Rounds(
                R(1,2,3,4), 
                R(5,4,3,2,1));
            var rating = new PositionAggregator().Aggregate<int, RefPosition>(source);
            rating.Count.ShouldBe(5);
            rating[0].Check(1, 5, 5);
            rating[1].Check(4, 5, 4);
            rating[2].Check(2, 5, 3);
            rating[3].Check(3, 5, 2);
            rating[4].Check(5, 5, 1);
        }

        List<List<RefPosition>> Rounds(params List<RefPosition>[] rounds)
        {
            return rounds.ToList();
        }

        List<RefPosition> R(params int[] riders)
        {
            return riders.Select((x, i) => new RefPosition {RiderId = x, Points = riders.Length - i, Position = i + 1})
                .ToList();
        }

        RefPosition MapPosition(int riderId, int points, int position, List<RefPosition> pos)
        {
            return new RefPosition { RiderId = riderId, Points = points, Position = position };
        }
    }

    static class Ext
    {
        public static void Check(this AggPosition<int, RefPosition> position, int rider, int aggPoints, int points)
        {
            position.RiderId.ShouldBe(rider);
            position.AggPoints.ShouldBe(aggPoints, $"Rider {position.RiderId}");
            position.Points.ShouldBe(points, $"Rider {position.RiderId}");
        }
    }
}