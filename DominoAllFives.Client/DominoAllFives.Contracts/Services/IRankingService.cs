using DominoAllFives.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Represents the service responsible for handling ranking-related operations.
    /// </summary>
    public interface IRankingService
    {
        RankingResultDto GetRanking(int playerId);
    }
}
