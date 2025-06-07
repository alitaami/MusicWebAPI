using MediatR;
using MusicWebAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.Application.Features.Properties.Queries.Songs
{
    public class GetRecommendedSongsQuery : IRequest<List<GetSongsViewModel>>
    {
        public string UserId { get; set; }
        public int Count { get; set; }

        public GetRecommendedSongsQuery(string userId, int count)
        {
            Count = count;
            UserId = userId;
        }
    }

}
