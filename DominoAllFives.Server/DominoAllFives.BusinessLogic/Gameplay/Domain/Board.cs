using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class Board
    {
        private const int ScoringMultiple = 5;
        private const int UnblockedTurn = -1;
        private readonly List<Placement> _tiles = new List<Placement>();
        private readonly List<BoardEnd> _ends = new List<BoardEnd>();
        public IReadOnlyList<Placement> Tiles
        {
            get
            {
                return _tiles.AsReadOnly();
            }
        }

        public IReadOnlyList<BoardEnd> Ends
        {
            get
            {
                return _ends.AsReadOnly();
            }
        }

        public bool CanPlay(Tile tile, End end, int turnNumber)
        {
            if (!Enum.IsDefined(typeof(End), end))
            {
                return false;
            }

            if (_tiles.Count == 0)
            {
                return end == End.Left;
            }

            BoardEnd target = null;
            foreach (BoardEnd candidate in _ends)
            {
                if (candidate.End == end)
                {
                    target = candidate;
                    break;
                }
            }

            if (target == null)
            {
                return false;
            }

            if (target.BlockedTurn == turnNumber)
            {
                return false;
            }

            return tile.Matches(target.Value);

        }

        public bool HasMove(IEnumerable<Tile> hand, int turnNumber)
        {
            return hand.Any(tile => Enum.GetValues(typeof(End)).Cast<End>()
                .Any(end => CanPlay(tile, end, turnNumber)));
        }

        public void Place(Tile tile, End end, int turnNumber)
        {
            if (!CanPlay(tile, end, turnNumber))
            {
                throw new MatchRuleException(MatchIssue.InvalidPlacement);
            }

            if (_tiles.Count == 0)
            {
                OpenBoard(tile);
                if (tile.SpecialPower == SpecialPower.Block)
                {
                    _ends.Single(openingEnd => openingEnd.End == end).BlockedTurn = turnNumber + 1;
                }

                return;
            }

            BoardEnd target = _ends.Single(candidate => candidate.End == end);
            int connectingValue = target.Value;
            target.Value = tile.GetOtherValue(connectingValue);
            target.ScoreValue = tile.IsDouble ? tile.TotalPips : target.Value;
            target.IsOpeningEnd = false;
            target.BlockedTurn = tile.SpecialPower == SpecialPower.Block ? turnNumber
                + 1 : UnblockedTurn;
            _tiles.Add(new Placement { Tile = tile, End = end, ConnectingValue =
                connectingValue, ExposedValue = target.Value });
        }

        public int CalculatePoints(int turnNumber)
        {
            int sum = GetScoringValues(turnNumber)
                .Sum();
            return sum % ScoringMultiple == 0 ? sum : 0;
        }

        public int[] GetScoringValues(int turnNumber)
        {
            List<BoardEnd> available = _ends.Where(end => end.BlockedTurn != turnNumber)
                .ToList();
            List<int> values = available.Where(end => !end.IsOpeningEnd)
                .Select(end => end.ScoreValue)
                .ToList();
            if (available.Any(end => end.IsOpeningEnd))
            {
                values.Add(_tiles[0].Tile.TotalPips);
            }

            return values.ToArray();
        }

        private void OpenBoard(Tile tile)
        {
            _tiles.Add(new Placement { Tile = tile, End = End.Left, ConnectingValue =
                tile.FirstValue, ExposedValue = tile.SecondValue });
            AddEnd(End.Left, tile.FirstValue, tile.IsDouble);
            AddEnd(End.Right, tile.SecondValue, tile.IsDouble);
            if (tile.IsDouble)
            {
                AddEnd(End.Top, tile.FirstValue, true);
                AddEnd(End.Bottom, tile.FirstValue, true);
            }
        }

        private void AddEnd(End end, int value, bool isOpeningEnd)
        {
            _ends.Add(new BoardEnd { End = end, Value = value, ScoreValue = value,
                IsOpeningEnd = isOpeningEnd });
        }
    }
}
