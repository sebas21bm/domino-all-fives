using System.Collections.Generic;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the result of a player ranking query.
    /// </summary>
    public class RankingResultDto
    {
        public List<RankingEntryDto> TopPlayers { get; set; }
            = new List<RankingEntryDto>();

        public int CurrentPlayerRank { get; set; }
    }
}