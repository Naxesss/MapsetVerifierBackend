﻿using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using MapsetParser.settings;
using MapsetParser.statics;
using MapsetVerifierFramework.objects.resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MapsetVerifierBackend.renderer
{
    class OverviewRenderer : BeatmapInfoRenderer
    {
        public static string Render(BeatmapSet aBeatmapSet)
        {
            return String.Concat(
                    RenderBeatmapInfo(aBeatmapSet),
                    Div("paste-separator"),
                    RenderMetadata(aBeatmapSet),
                    RenderGeneralSettings(aBeatmapSet),
                    RenderDifficultySettings(aBeatmapSet),
                    RenderStatistics(aBeatmapSet),
                    RenderResources(aBeatmapSet),
                    RenderColourSettings(aBeatmapSet),
                    Div("overview-footer")
                );
        }

        private static string RenderMetadata(BeatmapSet aBeatmapSet)
        {
            return
                RenderContainer("Metadata",
                    (aBeatmapSet.beatmaps.Any(aBeatmap =>
                        aBeatmap.metadataSettings.artist?.ToString() !=
                        aBeatmap.metadataSettings.artistUnicode?.ToString()) ?

                            // If the romanised field is always the same as the unicode field, we don't need to display them separately.
                            RenderField("Artist",
                                RenderBeatmapContent(aBeatmapSet, "Romanised", aBeatmap =>
                                    aBeatmap.metadataSettings.artist?.ToString()) +
                                RenderBeatmapContent(aBeatmapSet, "Unicode", aBeatmap =>
                                    aBeatmap.metadataSettings.artistUnicode?.ToString())
                            ) :
                            RenderBeatmapContent(aBeatmapSet, "Artist", aBeatmap =>
                                aBeatmap.metadataSettings.artist?.ToString())

                    ),
                    (aBeatmapSet.beatmaps.Any(aBeatmap =>
                        aBeatmap.metadataSettings.title?.ToString() !=
                        aBeatmap.metadataSettings.titleUnicode?.ToString()) ?

                            RenderField("Title",
                                RenderBeatmapContent(aBeatmapSet, "Romanised", aBeatmap =>
                                    aBeatmap.metadataSettings.title?.ToString()) +
                                RenderBeatmapContent(aBeatmapSet, "Unicode", aBeatmap =>
                                    aBeatmap.metadataSettings.titleUnicode?.ToString())
                            ) :
                            RenderBeatmapContent(aBeatmapSet, "Title", aBeatmap =>
                                aBeatmap.metadataSettings.title?.ToString())

                    ),
                    RenderBeatmapContent(aBeatmapSet, "Creator", aBeatmap =>
                        aBeatmap.metadataSettings.creator?.ToString()),
                    RenderBeatmapContent(aBeatmapSet, "Source", aBeatmap =>
                        aBeatmap.metadataSettings.source?.ToString()),
                    RenderBeatmapContent(aBeatmapSet, "Tags", aBeatmap =>
                        aBeatmap.metadataSettings.tags?.ToString())
                );
        }

        private static string RenderGeneralSettings(BeatmapSet aBeatmapSet)
        {
            return
                RenderContainer("General Settings",
                    RenderBeatmapContent(aBeatmapSet, "Audio Filename", aBeatmap =>
                        aBeatmap.generalSettings.audioFileName?.ToString()),
                    RenderBeatmapContent(aBeatmapSet, "Audio Lead-in", aBeatmap =>
                        aBeatmap.generalSettings.audioLeadIn.ToString()),
                    RenderBeatmapContent(aBeatmapSet, "Mode", aBeatmap =>
                        aBeatmap.generalSettings.mode.ToString()),
                    RenderBeatmapContent(aBeatmapSet, "Stack Leniency", aBeatmap =>
                    {
                        // Stack leniency only does stuff for standard.
                        if (aBeatmap.generalSettings.mode == Beatmap.Mode.Standard)
                            return aBeatmap.generalSettings.stackLeniency.ToString();
                        else
                            return "N/A";
                    }),
                    RenderField("Countdown Settings",
                        RenderBeatmapContent(aBeatmapSet, "Has Countdown", aBeatmap =>
                        {
                            return
                                (aBeatmap.GetCountdownStartBeat() >= 0 &&
                                aBeatmap.generalSettings.countdown != GeneralSettings.Countdown.None)
                                .ToString();
                        }),
                        RenderBeatmapContent(aBeatmapSet, "Countdown Speed", aBeatmap =>
                        {
                            if (aBeatmap.GetCountdownStartBeat() >= 0 && aBeatmap.generalSettings.countdown != GeneralSettings.Countdown.None)
                                return aBeatmap.generalSettings.countdown.ToString();
                            else
                                return "N/A";
                        }),
                        RenderBeatmapContent(aBeatmapSet, "Countdown Offset", aBeatmap =>
                        {
                            if (aBeatmap.GetCountdownStartBeat() >= 0 && aBeatmap.generalSettings.countdown != GeneralSettings.Countdown.None)
                                return aBeatmap.generalSettings.countdownBeatOffset.ToString();
                            else
                                return "N/A";
                        })
                    ),
                    RenderBeatmapContent(aBeatmapSet, "Epilepsy Warning", aBeatmap =>
                    {
                        if (aBeatmap.videos.Any() || aBeatmap.HasDifficultySpecificStoryboard() || (aBeatmapSet.osb?.IsUsed() ?? false))
                            return aBeatmap.generalSettings.epilepsyWarning.ToString();
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "Letterbox During Breaks", aBeatmap =>
                    {
                        if(aBeatmap.breaks.Any() || !aBeatmap.generalSettings.letterbox)
                            return aBeatmap.generalSettings.letterbox.ToString();
                        else
                            return "N/A";
                    }),
                    RenderField("Storyboard Settings",
                        RenderBeatmapContent(aBeatmapSet, "Has Storyboard", aBeatmap =>
                        {
                            return (aBeatmap.HasDifficultySpecificStoryboard() || (aBeatmapSet.osb?.IsUsed() ?? false)).ToString();
                        }),
                        RenderBeatmapContent(aBeatmapSet, "Widescreen Support", aBeatmap =>
                        {
                            if (aBeatmap.HasDifficultySpecificStoryboard() || (aBeatmapSet.osb?.IsUsed() ?? false))
                                return aBeatmap.generalSettings.widescreenSupport.ToString();
                            else
                                return "N/A";
                        }),
                        RenderBeatmapContent(aBeatmapSet, "In Front Of Combo Fire", aBeatmap =>
                        {
                            if (aBeatmap.HasDifficultySpecificStoryboard() || (aBeatmapSet.osb?.IsUsed() ?? false))
                                return aBeatmap.generalSettings.storyInFrontOfFire.ToString();
                            else
                                return "N/A";
                        }),
                        RenderBeatmapContent(aBeatmapSet, "Use Skin Sprites", aBeatmap =>
                        {
                            if (aBeatmap.HasDifficultySpecificStoryboard() || (aBeatmapSet.osb?.IsUsed() ?? false))
                                return aBeatmap.generalSettings.useSkinSprites.ToString();
                            else
                                return "N/A";
                        })
                    ),
                    RenderBeatmapContent(aBeatmapSet, "Preview Time", aBeatmap =>
                        aBeatmap.generalSettings.previewTime >= 0 ?
                            Timestamp.Get(aBeatmap.generalSettings.previewTime) :
                            ""),
                    RenderBeatmapContent(aBeatmapSet, "Skin Preference", aBeatmap =>
                        aBeatmap.generalSettings.skinPreference?.ToString())

                    // Special N+1 Style is apparently not used by any mode, was meant for mania but was later overriden by user settings.
                );
        }

        private static string RenderDifficultySettings(BeatmapSet aBeatmapSet)
        {
            return
                RenderContainer("Difficulty Settings",
                    RenderBeatmapContent(aBeatmapSet, "HP Drain", aBeatmap =>
                        aBeatmap.difficultySettings.hpDrain.ToString(CultureInfo.InvariantCulture)),
                    RenderBeatmapContent(aBeatmapSet, "Circle Size", aBeatmap =>
                    {
                        if (aBeatmap.generalSettings.mode != Beatmap.Mode.Taiko)
                            return aBeatmap.difficultySettings.circleSize.ToString(CultureInfo.InvariantCulture);
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "Overall Difficulty", aBeatmap =>
                        aBeatmap.difficultySettings.overallDifficulty.ToString(CultureInfo.InvariantCulture)),
                    RenderBeatmapContent(aBeatmapSet, "Approach Rate", aBeatmap =>
                    {
                        if (aBeatmap.generalSettings.mode != Beatmap.Mode.Mania)
                            return aBeatmap.difficultySettings.approachRate.ToString(CultureInfo.InvariantCulture);
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "Slider Tick Rate", aBeatmap =>
                    {
                        if (aBeatmap.generalSettings.mode != Beatmap.Mode.Mania)
                            return aBeatmap.difficultySettings.sliderTickRate.ToString(CultureInfo.InvariantCulture);
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "SV Multiplier", aBeatmap =>
                    {
                        if (aBeatmap.generalSettings.mode != Beatmap.Mode.Mania)
                            return aBeatmap.difficultySettings.sliderMultiplier.ToString(CultureInfo.InvariantCulture);
                        else
                            return "N/A";
                    })
                );
        }

        private static string RenderStatistics(BeatmapSet aBeatmapSet)
        {
            return
                RenderContainer("Statistics",
                    RenderBeatmapContent(aBeatmapSet, "Old Star Rating", aBeatmap =>
                    {
                        // Current star rating calc only supports standard.
                        if (aBeatmap.generalSettings.mode == Beatmap.Mode.Standard)
                            return $"{aBeatmap.starRating:0.##}";
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "Circle Count", aBeatmap =>
                        aBeatmap.hitObjects.OfType<Circle>().Count().ToString(CultureInfo.InvariantCulture)),
                    RenderBeatmapContent(aBeatmapSet, "Slider Count", aBeatmap =>
                    {
                        // Sliders don't exist in mania.
                        if (aBeatmap.generalSettings.mode != Beatmap.Mode.Mania)
                            return aBeatmap.hitObjects.OfType<Slider>().Count().ToString(CultureInfo.InvariantCulture);
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "Spinner Count", aBeatmap =>
                    {
                        // Spinners don't exist in mania.
                        if (aBeatmap.generalSettings.mode != Beatmap.Mode.Mania)
                            return aBeatmap.hitObjects.OfType<Spinner>().Count().ToString(CultureInfo.InvariantCulture);
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "Hold Note Count", aBeatmap =>
                    {
                        // Hold notes only exist in mania.
                        if (aBeatmap.generalSettings.mode == Beatmap.Mode.Mania)
                            return aBeatmap.hitObjects.OfType<HoldNote>().Count().ToString(CultureInfo.InvariantCulture);
                        else
                            return "N/A";
                    }),
                    RenderBeatmapContent(aBeatmapSet, "Break Count", aBeatmap =>
                        aBeatmap.breaks.Count().ToString(CultureInfo.InvariantCulture)),
                    RenderBeatmapContent(aBeatmapSet, "Drain Time", aBeatmap =>
                        Timestamp.Get(aBeatmap.GetDrainTime()).ToString(CultureInfo.InvariantCulture)),
                    RenderBeatmapContent(aBeatmapSet, "Play Time", aBeatmap =>
                        Timestamp.Get(aBeatmap.GetPlayTime()).ToString(CultureInfo.InvariantCulture))
                );
        }

        private static string RenderDataSize(long aSize)
        {
            if (aSize / Math.Pow(1024, 3) >= 1)
                return $"{aSize / Math.Pow(1024, 3):0.##} GB";
            else if (aSize / Math.Pow(1024, 2) >= 1 && aSize / Math.Pow(1024, 3) < 1)
                return $"{aSize / Math.Pow(1024, 2):0.##} MB";
            else if (aSize / Math.Pow(1024, 1) >= 1 && aSize / Math.Pow(1024, 2) < 1)
                return $"{aSize / Math.Pow(1024, 1):0.##} KB";
            else
                return $"{aSize / Math.Pow(1024, 0):0.##} B";
        }

        private static string RenderFileSize(string aFullPath)
        {
            if (!File.Exists(aFullPath))
                return "";

            FileInfo fileInfo = new FileInfo(aFullPath);

            return RenderDataSize(fileInfo.Length);
        }

        private static long GetDirectorySize(DirectoryInfo aDirectoryInfo)
        {
            long totalSize = 0;

            FileInfo[] fileInfos = aDirectoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
                totalSize += fileInfo.Length;

            DirectoryInfo[] directoryInfos = aDirectoryInfo.GetDirectories();
            foreach (DirectoryInfo directoryInfo in directoryInfos)
                totalSize += GetDirectorySize(directoryInfo);

            return totalSize;
        }

        private static string RenderDirectorySize(string aFullPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(aFullPath);

            return RenderDataSize(GetDirectorySize(directoryInfo));
        }

        private static string RenderResources(BeatmapSet aBeatmapSet)
        {
            string RenderFloat(List<string> aFiles, Func<string, string> aFunc)
            {
                string content =
                    String.Join("<br>",
                        aFiles.Select(aFile =>
                        {
                            string path =
                                aBeatmapSet.hitSoundFiles.FirstOrDefault(anOtherFile =>
                                    anOtherFile.StartsWith(aFile + "."));
                            if (path == null)
                                return null;

                            return aFunc(path);
                        }).Where(aValue => aValue != null)
                    );

                if (content.Length == 0)
                    return "";

                return Div("overview-float", content);
            }

            Dictionary<string, int> hsUsedCount = new Dictionary<string, int>();

            return
                RenderContainer("Resources",
                    RenderBeatmapContent(aBeatmapSet, "Used Hit Sound File(s)", aBeatmap =>
                    {
                        List<string> usedHitSoundFiles =
                            aBeatmap.hitObjects.SelectMany(anObject => anObject.GetUsedHitSoundFiles()).ToList();

                        List<string> distinctSortedFiles =
                            usedHitSoundFiles.Distinct().OrderByDescending(aFile => aFile).ToList();

                        return
                            RenderFloat(distinctSortedFiles, aPath => Encode(aPath)) +
                            RenderFloat(distinctSortedFiles, aPath =>
                            {
                                int count = usedHitSoundFiles.Where(anOtherFile => aPath.StartsWith(anOtherFile + ".")).Count();

                                // Used for total hit sound usage overview
                                if (hsUsedCount.ContainsKey(aPath))
                                    hsUsedCount[aPath] += count;
                                else
                                    hsUsedCount[aPath] = count;

                                return $"× {count}";
                            });
                    }, false),
                    RenderField("Total Used Hit Sound File(s)",
                        (hsUsedCount.Any() ?
                            Div("overview-float",
                                String.Join("<br>",
                                    hsUsedCount.Select(aPair => aPair.Key)
                                )
                            ) +
                            Div("overview-float",
                                String.Join("<br>",
                                    hsUsedCount.Select(aPair =>
                                    {
                                        string fullPath = Path.Combine(aBeatmapSet.songPath, aPair.Key);

                                        return Encode(RenderFileSize(fullPath));
                                    })
                                )
                            ) +
                            Div("overview-float",
                                String.Join("<br>",
                                    hsUsedCount.Select(aPair =>
                                    {
                                        string fullPath = Path.Combine(aBeatmapSet.songPath, aPair.Key);
                                        double duration = Audio.GetDuration(fullPath);

                                        if (duration < 0)
                                            return "0 ms";

                                        return $"{duration:0.##} ms";
                                    })
                                )
                            ) +
                            Div("overview-float",
                                String.Join("<br>",
                                    hsUsedCount.Select(aPair =>
                                    {
                                        string fullPath = Path.Combine(aBeatmapSet.songPath, aPair.Key);

                                        return Encode(Audio.EnumToString(Audio.GetFormat(fullPath)));
                                    })
                                )
                            ) +
                            Div("overview-float",
                                String.Join("<br>",
                                    hsUsedCount.Select(aPair => "× " + aPair.Value)
                                )
                            )
                        : "")
                    ),
                    RenderBeatmapContent(aBeatmapSet, "Background File(s)", aBeatmap =>
                    {
                        if (aBeatmap.backgrounds.Any())
                        {
                            string fullPath = Path.Combine(aBeatmap.songPath, aBeatmap.backgrounds.First().path);
                            if (!File.Exists(fullPath))
                                return "";

                            string error = null;
                            TagLib.File tagFile = null;
                            try
                            { tagFile = new FileAbstraction(fullPath).GetTagFile(); }
                            catch (Exception exception)
                            {
                                error = exception.Message;
                            }

                            if (error != null || tagFile == null)
                                return
                                    Div("overview-float",
                                        Encode(aBeatmap.backgrounds.First().path)
                                    ) +
                                    Div("overview-float",
                                        Encode(RenderFileSize(fullPath))
                                    ) +
                                    Div("overview-float",
                                        Encode($"(failed getting proprties; {error})")
                                    );

                            return
                                Div("overview-float",
                                    Encode(aBeatmap.backgrounds.First().path)
                                ) +
                                Div("overview-float",
                                    Encode(RenderFileSize(fullPath))
                                ) +
                                Div("overview-float",
                                    Encode(tagFile.Properties.PhotoWidth + " x " + tagFile.Properties.PhotoHeight)
                                );
                        }
                        else
                            return "";
                    }, false),
                    RenderBeatmapContent(aBeatmapSet, "Video File(s)", aBeatmap =>
                    {
                        if (aBeatmap.videos.Any() || (aBeatmapSet.osb?.videos.Any() ?? false))
                        {
                            string fullPath = Path.Combine(aBeatmap.songPath, aBeatmap.videos.First().path);
                            if (!File.Exists(fullPath))
                                return "";

                            string error = null;
                            TagLib.File tagFile = null;
                            try
                            { tagFile = new FileAbstraction(fullPath).GetTagFile(); }
                            catch (Exception exception)
                            {
                                error = exception.Message;
                            }

                            if (error != null || tagFile == null)
                                return
                                    Div("overview-float",
                                        Encode(aBeatmap.videos.First().path)
                                    ) +
                                    Div("overview-float",
                                        Encode(RenderFileSize(fullPath))
                                    ) +
                                    Div("overview-float",
                                        Encode($"(failed getting proprties; {error})")
                                    );

                            return
                                Div("overview-float",
                                    Encode(aBeatmap.videos.First().path)
                                ) +
                                Div("overview-float",
                                    Encode(RenderFileSize(fullPath))
                                ) +
                                Div("overview-float",
                                    FormatTimestamps(Encode(Timestamp.Get(tagFile.Properties.Duration.TotalMilliseconds)))
                                ) +
                                Div("overview-float",
                                    Encode(tagFile.Properties.VideoWidth + " x " + tagFile.Properties.VideoHeight)
                                );
                        }
                        else
                            return "";
                    }, false),
                    RenderBeatmapContent(aBeatmapSet, "Audio File(s)", aBeatmap =>
                    {
                        string path = aBeatmap.GetAudioFilePath();
                        if (path == null)
                            return "";

                        FileInfo fileInfo = new FileInfo(path);
                        ManagedBass.ChannelType format = Audio.GetFormat(path);
                        double duration = Audio.GetDuration(path);

                        return
                            Div("overview-float",
                                Encode(PathStatic.RelativePath(path, aBeatmap.songPath))
                            ) +
                            Div("overview-float",
                                Encode(RenderFileSize(path))
                            ) +
                            Div("overview-float",
                                FormatTimestamps(Encode(Timestamp.Get(duration)))
                            ) +
                            Div("overview-float",
                                Encode(Audio.EnumToString(format))
                            );
                    }, false),
                    RenderBeatmapContent(aBeatmapSet, "Audio Bitrate", aBeatmap =>
                    {
                        string path = aBeatmap.GetAudioFilePath();
                        if (path == null)
                            return "N/A";

                        AudioFile audioFile = new AudioFile(path);

                        return
                            Div("overview-float",
                                (audioFile.GetLowestBitrate() == audioFile.GetHighestBitrate() ?
                                    $"CBR, {audioFile.GetLowestBitrate() / 1000:0.##} kbps" :
                                    $"VBR, {audioFile.GetLowestBitrate() / 1000:0.##} kbps to {audioFile.GetHighestBitrate() / 1000:0.##} kbps, " +
                                    $"average {audioFile.GetAverageBitrate() / 1000:0.##} kbps")
                            );
                    }, false),
                    RenderField("Has .osb",
                         Encode((aBeatmapSet.osb?.IsUsed() ?? false).ToString())
                    ),
                    RenderBeatmapContent(aBeatmapSet, "Has .osu Specific Storyboard", aBeatmap =>
                        aBeatmap.HasDifficultySpecificStoryboard().ToString()),
                    RenderBeatmapContent(aBeatmapSet, "Song Folder Size", aBeatmap =>
                        RenderDirectorySize(aBeatmap.songPath))
                );
        }

        private static float GetLuminosity(Vector3 aColour)
        {
            // HSP colour model http://alienryderflex.com/hsp.html
            return
                (float)Math.Sqrt(
                    aColour.X * aColour.X * 0.299f +
                    aColour.Y * aColour.Y * 0.587f +
                    aColour.Z * aColour.Z * 0.114f);
        }

        private static string RenderColourSettings(BeatmapSet aBeatmapSet)
        {
            int maxComboAmount = aBeatmapSet.beatmaps.Max(aBeatmap => aBeatmap.colourSettings.combos.Count());

            StringBuilder content = new StringBuilder();

            // Combo Colours
            for (int i = 0; i < maxComboAmount; ++i)
            {
                content.Append(
                    RenderBeatmapContent(aBeatmapSet, $"Combo {i + 1}", aBeatmap =>
                    {
                        if (aBeatmap.generalSettings.mode == Beatmap.Mode.Taiko ||
                            aBeatmap.generalSettings.mode == Beatmap.Mode.Mania)
                            return "N/A";

                        Vector3? comboColour = null;

                        // Need to account for that the colour at index 0 is actually the last displayed index in-game.
                        if (aBeatmap.colourSettings.combos.Count() > i + 1)
                            comboColour = aBeatmap.colourSettings.combos[i + 1];
                        else if (aBeatmap.colourSettings.combos.Count() == i + 1)
                            comboColour = aBeatmap.colourSettings.combos[0];

                        if (comboColour == null)
                            return
                                DivAttr("overview-colour",
                                    DataAttr("colour", "")
                                );
                        else
                            return
                                DivAttr("overview-colour",
                                    DataAttr("colour", comboColour?.X + "," + comboColour?.Y + "," + comboColour?.Z) +
                                    Tooltip($"Luminosity {GetLuminosity(comboColour.GetValueOrDefault()):0.#}, less than 43 or greater than 250 in kiai is bad.")
                                );
                    }, false));
            }

            // Border + Track
            content.Append(
                RenderBeatmapContent(aBeatmapSet, $"Slider Border", aBeatmap =>
                {
                    if (aBeatmap.generalSettings.mode == Beatmap.Mode.Taiko ||
                        aBeatmap.generalSettings.mode == Beatmap.Mode.Mania)
                        return "N/A";

                    Vector3? comboColour = aBeatmap.colourSettings.sliderBorder;

                    if (comboColour == null)
                        return
                            DivAttr("overview-colour",
                                DataAttr("colour", "")
                            );
                    else
                        return
                            DivAttr("overview-colour",
                                DataAttr("colour", comboColour?.X + "," + comboColour?.Y + "," + comboColour?.Z) +
                                    Tooltip($"Luminosity {GetLuminosity(comboColour.GetValueOrDefault()):0.#}, less than 43 or greater than 250 in kiai is bad.")
                            );
                }, false) +
                RenderBeatmapContent(aBeatmapSet, $"Slider Track", aBeatmap =>
                {
                    if (aBeatmap.generalSettings.mode == Beatmap.Mode.Taiko ||
                        aBeatmap.generalSettings.mode == Beatmap.Mode.Mania)
                        return "N/A";

                    Vector3? comboColour = aBeatmap.colourSettings.sliderTrackOverride;

                    if (comboColour == null)
                        return
                            DivAttr("overview-colour",
                                DataAttr("colour", "")
                            );
                    else
                        return
                            DivAttr("overview-colour",
                                DataAttr("colour", comboColour?.X + "," + comboColour?.Y + "," + comboColour?.Z) +
                                    Tooltip($"Luminosity {GetLuminosity(comboColour.GetValueOrDefault()):0.#}, less than 43 or greater than 250 in kiai is bad.")
                            );
                }, false));

            return
                RenderContainer("Colour Settings",
                    content.ToString()
                );
        }

        // Returns a single field if all values are equal, otherwise multiple.
        private static string RenderBeatmapContent(BeatmapSet aBeatmapSet, string aTitle, Func<Beatmap, string> aFunc, bool aEncode = true)
        {
            string refContent = aFunc(aBeatmapSet.beatmaps.FirstOrDefault());
            if(aBeatmapSet.beatmaps.Any(aBeatmap => aFunc(aBeatmap) != refContent))
                return
                    RenderField(aTitle,
                        String.Concat(
                        aBeatmapSet.beatmaps.Select(aBeatmap =>
                        {
                            return
                                RenderField(aBeatmap.metadataSettings.version,
                                    aEncode ?
                                        FormatTimestamps(Encode(aFunc(aBeatmap))) :
                                        aFunc(aBeatmap)
                                );
                        }))
                    );
            else
                return
                    RenderField(aTitle,
                        aEncode ?
                            FormatTimestamps(Encode(refContent)) :
                            refContent
                    );
        }

        private static string RenderContainer(string aTitle, params string[] aContents)
        {
            return
                Div("overview-container",
                    Div("overview-container-title",
                        Encode(aTitle)
                    ),
                    Div("overview-fields",
                        aContents
                    )
                );
        }

        private static string RenderField(string aTitle, params string[] aContents)
        {
            return
                Div("overview-field",
                    Div("overview-field-title",
                        Encode(aTitle)
                    ),
                    Div("overview-field-content",
                        aContents
                    )
                );
        }
    }
}