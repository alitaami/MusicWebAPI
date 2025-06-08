using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services
{
    public interface IRecommendationService
    {
        Task<List<object>> RecommendSongs(string userId, int count, CancellationToken cancellationToken);
        Task Train(CancellationToken cancellationToken);
    }
}
