using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;

namespace SchTech.Api.Manager.GracenoteOnApi.Concrete.EqualityComparers
{
    public class GenreComparer : IEqualityComparer<GnApiProgramsSchema.programsProgramGenre>
    {
        public bool Equals(GnApiProgramsSchema.programsProgramGenre episodeGenres,
            GnApiProgramsSchema.programsProgramGenre seriesGenres)
        {
            return episodeGenres.Value == seriesGenres.Value;
        }

        public int GetHashCode(GnApiProgramsSchema.programsProgramGenre genres)
        {
            return Convert.ToInt32(genres.genreId);
        }
    }
}
