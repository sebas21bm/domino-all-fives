using System;
using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class Board
    {
        private const int ScoringMultiple = 5;
        private const int UnblockedTurn = -1;
        private readonly List<Placement> _tiles = new List<Placement>();
        private readonly List<BoardEndState> _ends = new List<BoardEndState>();

        public IReadOnlyList<Placement> Tiles
        {
            get
            {
                return _tiles.AsReadOnly();
            }
        }

        public IReadOnlyList<BoardEndState> Ends
        {
            get
            {
                return _ends.AsReadOnly();
            }
        }

        public bool CanPlay(Tile tile, BoardEndPosition end, int turnNumber)
        {
            if (!Enum.IsDefined(typeof(BoardEndPosition), end))
            {
                return false;
            }

            if (_tiles.Count == 0)
            {
                return end == BoardEndPosition.Left;
            }

            BoardEndState target = FindEnd(end);
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
            foreach (Tile tile in hand)
            {
                foreach (BoardEndPosition end in Enum.GetValues(typeof(BoardEndPosition)))
                {
                    if (CanPlay(tile, end, turnNumber))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Place(Tile tile, BoardEndPosition end, int turnNumber)
        {
            if (!CanPlay(tile, end, turnNumber))
            {
                throw new MatchRuleException(MatchIssue.InvalidPlacement);
            }

            if (_tiles.Count == 0)
            {
                OpenBoard(tile);
                BlockOpeningEndWhenRequired(tile, end, turnNumber);
                return;
            }

            BoardEndState target = FindEnd(end);
            int connectingValue = target.Value;
            target.Value = tile.GetOtherValue(connectingValue);

            if (tile.IsDouble)
            {
                target.ScoreValue = tile.TotalPips;
            }
            else
            {
                target.ScoreValue = target.Value;
            }

            target.IsOpeningEnd = false;
            target.BlockedTurn = UnblockedTurn;
            if (tile.SpecialPower == SpecialPower.Block)
            {
                target.BlockedTurn = turnNumber + 1;
            }

            Placement placement = new Placement
            {
                Tile = tile,
                End = end,
                ConnectingValue = connectingValue,
                ExposedValue = target.Value
            };
            _tiles.Add(placement);
        }

        public int CalculatePoints(int turnNumber)
        {
            int[] scoringValues = GetScoringValues(turnNumber);
            int total = 0;
            foreach (int scoringValue in scoringValues)
            {
                total += scoringValue;
            }

            if (total % ScoringMultiple == 0)
            {
                return total;
            }

            return 0;
        }

        public int[] GetScoringValues(int turnNumber)
        {
            List<int> scoringValues = new List<int>();
            bool includesOpeningEnd = false;

            foreach (BoardEndState end in _ends)
            {
                bool isBlocked = end.BlockedTurn == turnNumber;
                if (isBlocked)
                {
                    continue;
                }

                if (end.IsOpeningEnd)
                {
                    includesOpeningEnd = true;
                    continue;
                }

                scoringValues.Add(end.ScoreValue);
            }

            if (includesOpeningEnd)
            {
                Tile openingTile = _tiles[0].Tile;
                scoringValues.Add(openingTile.TotalPips);
            }

            return scoringValues.ToArray();
        }

        private void BlockOpeningEndWhenRequired(
            Tile tile,
            BoardEndPosition end,
            int turnNumber)
        {
            if (tile.SpecialPower != SpecialPower.Block)
            {
                return;
            }

            BoardEndState openingEnd = FindEnd(end);
            openingEnd.BlockedTurn = turnNumber + 1;
        }

        private BoardEndState FindEnd(BoardEndPosition end)
        {
            foreach (BoardEndState candidate in _ends)
            {
                if (candidate.End == end)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void OpenBoard(Tile tile)
        {
            Placement openingPlacement = new Placement
            {
                Tile = tile,
                End = BoardEndPosition.Left,
                ConnectingValue = tile.FirstValue,
                ExposedValue = tile.SecondValue
            };
            _tiles.Add(openingPlacement);

            AddEnd(BoardEndPosition.Left, tile.FirstValue, tile.IsDouble);
            AddEnd(BoardEndPosition.Right, tile.SecondValue, tile.IsDouble);
            if (tile.IsDouble)
            {
                AddEnd(BoardEndPosition.Top, tile.FirstValue, true);
                AddEnd(BoardEndPosition.Bottom, tile.FirstValue, true);
            }
        }

        private void AddEnd(
            BoardEndPosition end,
            int value,
            bool isOpeningEnd)
        {
            BoardEndState boardEnd = new BoardEndState
            {
                End = end,
                Value = value,
                ScoreValue = value,
                IsOpeningEnd = isOpeningEnd
            };
            _ends.Add(boardEnd);
        }
    }
}
