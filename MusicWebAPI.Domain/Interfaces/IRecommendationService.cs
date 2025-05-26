using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<object>> RecommendSongsAsync(string userId, int count, CancellationToken cancellationToken);
        Task TrainAsync(CancellationToken cancellationToken);
    }
}
