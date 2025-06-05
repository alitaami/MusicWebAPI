using MusicWebAPI.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services.Base
{
    public interface IServiceManager
    {
        IUserService User { get; }
        ISongService Home { get; }
        IRecommendationService Recommendation { get; }
    }
}
