﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GracenoteUpdateManager
{
    public class GracenoteUpdateController
    {
        /*TODO
            1: Create db types and mappings
            2: Create logic to parse db and process data from [GN_UpdateTracking] table
            3: Call http://on-api.gracenote.com/v3/ProgramMappings?updateId=10938407543&limit=100&api_key=wgu7uhqcqyzspwxj28mxgy4b with lowest update id from point 2
            4: parse call results and grab all pidpaid items matching platform
            5: check db for pid paid values, if match update the tracker db row for update.
            6: call http://on-api.gracenote.com/v3/Programs?updateId=10938407543&limit=100&api_key=wgu7uhqcqyzspwxj28mxgy4b which returns layer1 & 2 data
            7: Parse call results and check rootids from 4 in tracking table against results
            8: Link episode with sho data for layer 2 calls
            9: any matches update the db row for update if an update has not been flagged
         */


    }
}
