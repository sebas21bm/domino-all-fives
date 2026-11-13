using System;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class Tile
    {
        public const int MaximumPips = 6;
        public int TileId { get; private set; }
        public int FirstValue { get; private set; }
        public int SecondValue { get; private set; }
        public SpecialPower SpecialPower { get; private set; }

        public bool IsDouble
        {
            get
            {
                return FirstValue == SecondValue;
            }
        }

        public int TotalPips
        {
            get
            {
                return FirstValue + SecondValue;
            }
        }

        public Tile(int tileId, int firstValue, int secondValue, SpecialPower specialPower)
        {
            if (tileId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tileId));
            }

            if (firstValue < 0 || firstValue > MaximumPips)
            {
                throw new ArgumentOutOfRangeException(nameof(firstValue));
            }

            if (secondValue < 0 || secondValue > MaximumPips)
            {
                throw new ArgumentOutOfRangeException(nameof(secondValue));
            }

            if (!Enum.IsDefined(typeof(SpecialPower), specialPower))
            {
                throw new ArgumentOutOfRangeException(nameof(specialPower));
            }

            TileId = tileId;
            FirstValue = firstValue;
            SecondValue = secondValue;
            SpecialPower = specialPower;
        }

        public bool Matches(int value)
        {
            return FirstValue == value || SecondValue == value;
        }

        public int GetOtherValue(int value)
        {
            if (!Matches(value))
            {
                throw new MatchRuleException(MatchIssue.InvalidPlacement);
            }

            return FirstValue == value ? SecondValue : FirstValue;
        }
    }
}
