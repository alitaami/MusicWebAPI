using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.DTOs
{
    public class RecommendationDTO
    {
        public class RecommendationModelData
        {
            public Dictionary<string, uint> UserIndexMap { get; set; }
            public Dictionary<string, uint> SongIndexMap { get; set; }
            public Dictionary<string, uint> GenreIndexMap { get; set; }
            public Dictionary<uint, string> ReverseSongIndexMap { get; set; }
        }

        public class MappedPlaylistSong
        {
            [KeyType(count: 1000)]
            public uint UserIndex { get; set; }

            [KeyType(count: 10000)]
            public uint SongIndex { get; set; }

            [KeyType(count: 100)]
            public uint GenreIndex { get; set; }

            public bool Label { get; set; }
        }

        public class SongPrediction
        {
            public float Score { get; set; }
        }
    }
}
