using FluentValidation;
using MusicWebAPI.Application.Features.Properties.UserSongs.Commands.AddToPlaylist;

namespace MusicWebAPI.Application.Validators
{
    public class AddToPlayListCommandValidator : AbstractValidator<AddToPlaylistCommand>
    {
        public AddToPlayListCommandValidator()
        {
            RuleFor(x => x.SongId)
                .NotEmpty().WithMessage("SongId is required.");

            RuleFor(x => x)
                .Must(x => x.PlaylistId != null || !string.IsNullOrWhiteSpace(x.PlaylistName))
                .WithMessage("One of PlaylistId or PlaylistName must be provided.");
        }
    }
}
