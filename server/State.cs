﻿using MapsetParser.objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifierBackend.Server
{
    public static class State
    {
        public static BeatmapSet LoadedBeatmapSet { get; set; }
        public static string LoadedBeatmapSetPath { get; set; }
    }
}
