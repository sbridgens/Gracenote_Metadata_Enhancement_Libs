﻿using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using System;
using System.Collections.Generic;

namespace SchTech.Api.Manager.GracenoteOnApi.Concrete.EqualityComparers
{
    public class CrewComparer : IEqualityComparer<GnApiProgramsSchema.crewTypeMember>
    {
        public bool Equals(GnApiProgramsSchema.crewTypeMember episodeMovieMember,
            GnApiProgramsSchema.crewTypeMember seriesSeasonMember)
        {
            return episodeMovieMember != null & episodeMovieMember?.name.first == seriesSeasonMember?.name.first &
                   episodeMovieMember?.name.last == seriesSeasonMember?.name.last;
        }

        public int GetHashCode(GnApiProgramsSchema.crewTypeMember member)
        {
            return Convert.ToInt32(member.personId);
        }
    }
}