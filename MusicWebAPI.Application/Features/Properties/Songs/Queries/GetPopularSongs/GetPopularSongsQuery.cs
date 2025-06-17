using MediatR;
using MusicWebAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.Application.Features.Properties.Songs.Queries.GetPopularSongs
{
    public class GetPopularSongsQuery : IRequest<PaginatedResult<GetSongsViewModel>>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public GetPopularSongsQuery(int pageSize, int pageNumber)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
    }
}
