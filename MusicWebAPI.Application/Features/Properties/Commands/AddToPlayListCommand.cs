using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Features.Properties.Commands
{
    public class AddToPlaylistCommand : IRequest
    {
        public Guid SongId { get; set; }
        public Guid UserId { get; set; }
        public Guid? PlaylistId { get; set; } = null;
        public string PlaylistName { get; set; }
    }
}
