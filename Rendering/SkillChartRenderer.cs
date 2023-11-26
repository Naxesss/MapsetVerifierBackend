using MapsetParser.objects;
using MapsetParser.starrating.skills;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using MapsetVerifierBackend.Rendering.Objects;
using TagLib.Id3v2;

namespace MapsetVerifierBackend.Rendering
{
    public class SkillChartRenderer : ChartRenderer
    {
        private const int MS_PER_PEAK = 400;

        public static new string Render(BeatmapSet beatmapSet)
        {
            Dictionary<Skill, LineChart> skillCharts = GetSkillCharts(beatmapSet);
            LineChart srChart = GetStarRatingChart(beatmapSet);

            if (srChart.Data.Count == 0)
                return "";

            return RenderContainer("Difficulty",
                Div("skill-charts",
                    Render(srChart),
                    string.Concat(skillCharts.Select(pair =>
                        Render(pair.Value)
                    ))
                )
            );
        }

        /// <summary> Used to keep track of the amount of times a specific difficulty is drawn,
        /// such that the color of all series in a chart are unique. </summary>
        private static Dictionary<LineChart, List<Beatmap>> mapsUsedInChart = new Dictionary<LineChart, List<Beatmap>>();

        private static LineChart GetStarRatingChart(BeatmapSet beatmapSet)
        {
            LineChart srChart = new LineChart(
                title:  "Star Rating",
                xLabel: "Time (Seconds)",
                yLabel: ""
            );

            foreach (Beatmap beatmap in beatmapSet.beatmaps)
            {
                if (beatmap.difficultyAttributes == null)
                    continue;

                srChart.Data.Add(GetStarRatingSeries(beatmap, srChart));
                if (!mapsUsedInChart.ContainsKey(srChart))
                    mapsUsedInChart[srChart] = new List<Beatmap>() { beatmap };
                else
                    mapsUsedInChart[srChart].Add(beatmap);
            }

            return srChart;
        }

        private static Dictionary<Skill, LineChart> GetSkillCharts(BeatmapSet beatmapSet)
        {
            Dictionary<Skill, LineChart> skillCharts = new Dictionary<Skill, LineChart>();

            foreach (Beatmap beatmap in beatmapSet.beatmaps)
            {
                if (beatmap.difficultyAttributes == null)
                    continue;

                foreach (Skill skill in beatmap.difficultyAttributes.Skills)
                {
                    if (!(skill is StrainSkill strainSkill))
                        continue;
                    
                    if (!skillCharts.ContainsKey(skill))
                    {
                        skillCharts[skill] = new LineChart(
                            title:  $"{skill}",
                            xLabel: "Time (Seconds)",
                            yLabel: ""
                        );
                    }

                    var skillSeries = GetSkillSeries(beatmap, strainSkill, skillCharts[skill]);
                    skillCharts[skill].Data.Add(skillSeries);
                    if (!mapsUsedInChart.ContainsKey(skillCharts[skill]))
                        mapsUsedInChart[skillCharts[skill]] = new List<Beatmap>() { beatmap };
                    else
                        mapsUsedInChart[skillCharts[skill]].Add(beatmap);
                }
            }

            return skillCharts;
        }

        private static Series GetSkillSeries(Beatmap beatmap, StrainSkill strainSkill, LineChart chart) =>
            GetPeakSeries(beatmap, strainSkill.GetCurrentStrainPeaks().ToList(), peak => (float)peak, chart);

        private static Series GetStarRatingSeries(Beatmap beatmap, LineChart chart)
        {
            if (beatmap.difficultyAttributes == null)
                throw new ArgumentException($"Cannot get star rating series of {beatmap}, as `difficultyAttributes` is null.");

            Dictionary<int, List<float>> accumulatedPeaks = new Dictionary<int, List<float>>();
            foreach (Skill skill in beatmap.difficultyAttributes.Skills)
            {
                if (!(skill is StrainSkill strainSkill))
                    continue;

                List<double> strainPeaks = strainSkill.GetCurrentStrainPeaks().ToList();
                
                for (int index = 0; index < strainPeaks.Count; ++index)
                {
                    if (accumulatedPeaks.ContainsKey(index))
                        accumulatedPeaks[index].Add((float)strainPeaks[index]);
                    else
                        accumulatedPeaks[index] = new List<float>() { (float)strainPeaks[index] };
                }
            }

            return GetPeakSeries(
                beatmap: beatmap,
                data:    accumulatedPeaks,
                Value:   getSkillValueToStarRatingFunc(beatmap.generalSettings.mode),
                chart:   chart
            );
        }

        private static Series GetPeakSeries<T>(Beatmap beatmap, IEnumerable<T> data, Func<T, float> Value, LineChart chart)
        {
            if (data == null)
                return null;
            
            Series series = new Series(
                label: beatmap.metadataSettings.version,
                color: GetGraphColor(beatmap, chart)
            );
            for (int i = 0; i < data.Count(); ++i)
            {
                float time = i * MS_PER_PEAK;
                float value = Value(data.ElementAt(i));
                series.Points.Add(new Vector2(
                    x: time / 1000f,  // Show time in seconds, rather than milliseconds.
                    y: value
                ));
            }

            return series;
        }

        private static readonly Dictionary<Beatmap.Difficulty, Color> difficultyColor = new Dictionary<Beatmap.Difficulty, Color>()
        {
            { Beatmap.Difficulty.Easy,   Color.FromArgb(125, 180,   0) },
            { Beatmap.Difficulty.Normal, Color.FromArgb( 80, 215, 255) },
            { Beatmap.Difficulty.Hard,   Color.FromArgb(255, 215,   0) },
            { Beatmap.Difficulty.Insane, Color.FromArgb(255,  80, 170) },
            { Beatmap.Difficulty.Expert, Color.FromArgb(125,  80, 255) }
        };

        private static Beatmap.Difficulty DifficultyOf(Beatmap map)
        {
            Beatmap.Difficulty diff = map.GetDifficulty(considerName: true);
            if (diff == Beatmap.Difficulty.Ultra)
                // Ultra uses a completely black color, which blends into the background too much, hence treat like Expert instead.
                return Beatmap.Difficulty.Expert;

            return diff;
        }

        private static Color GetGraphColor(Beatmap beatmap, LineChart chart)
        {
            Beatmap.Difficulty diff = DifficultyOf(beatmap);
            Color diffColor = difficultyColor[diff];
            if (!mapsUsedInChart.ContainsKey(chart))
                return diffColor;

            float red   = diffColor.R;
            float green = diffColor.G;
            float blue  = diffColor.B;

            int sameDiffsInChart =
                mapsUsedInChart
                    .GetValueOrDefault(chart)
                    .Count(map => DifficultyOf(map) == DifficultyOf(beatmap));
            for (int i = 0; i < sameDiffsInChart; ++i)
            {
                // Duplicate difficulties are not distinguishable without changing their color.
                // Idea is to change their shade, but not in a way where it can be confused with other difficulties' lines.

                float mult        = 0.7f;
                float multReverse = 1f / mult;

                red   *= mult;
                green *= mult;
                blue  *= mult;

                if (diff == Beatmap.Difficulty.Easy)   green *= multReverse;
                if (diff == Beatmap.Difficulty.Normal) blue  *= multReverse;
                if (diff == Beatmap.Difficulty.Hard)   red   *= multReverse;
                if (diff == Beatmap.Difficulty.Insane) red   *= multReverse;
                if (diff == Beatmap.Difficulty.Expert) red   *= multReverse;
            }

            return Color.FromArgb((int)red, (int)green, (int)blue);
        }

        private static Func<KeyValuePair<int, List<float>>, float> getSkillValueToStarRatingFunc(Beatmap.Mode mode)
        {
            return mode switch
            {
                Beatmap.Mode.Standard => peak => peak.Value.Sum() + Math.Abs(peak.Value[0] - peak.Value[1]) / 2,
                Beatmap.Mode.Taiko => peak => (float)(10.43 * Math.Log((peak.Value[0] * 1.4) / 8 + 1)),
                _ => peak => peak.Value.Sum()   // TODO: Implement transformation functions for Mania and Catch
            };
        }
    }
}
