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
    public class GetSongsQuery : IRequest<PaginatedResult<GetSongsViewModel>>
    {
        public string Term { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public GetSongsQuery(string term, int pageSize, int pageNumber)
        {
            Term = term;
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
    }

}
