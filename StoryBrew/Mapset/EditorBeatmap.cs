﻿using OpenTK.Mathematics;
using StoryBrew.Common.Mapset;
using StoryBrew.Util;
using StoryBrew.Storyboarding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace StoryBrew.Mapset
{
    public class EditorBeatmap : Beatmap
    {
        public readonly string Path;

        public override string AudioFilename => audioFilename;
        private string audioFilename = "audio.mp3";

        private string name = string.Empty;
        public override string Name => name;

        private long id;
        public override long Id => id;

        private double stackLeniency = .7;
        public override double StackLeniency => stackLeniency;

        private readonly List<int> bookmarks = new List<int>();
        public override IEnumerable<int> Bookmarks => bookmarks;

        private double hpDrainRate = 5;
        public override double HpDrainRate => hpDrainRate;

        private double circleSize = 5;
        public override double CircleSize => circleSize;

        private double overallDifficulty = 5;
        public override double OverallDifficulty => overallDifficulty;

        private double approachRate = 5;
        public override double ApproachRate => approachRate;

        private double sliderMultiplier = 1.4;
        public override double SliderMultiplier => sliderMultiplier;

        private double sliderTickRate = 1;
        public override double SliderTickRate => sliderTickRate;

        private bool hitObjectsPostProcessed;
        private readonly List<OsuHitObject> hitObjects = new List<OsuHitObject>();
        public override IEnumerable<OsuHitObject> HitObjects
        {
            get
            {
                if (!hitObjectsPostProcessed)
                    postProcessHitObjects();

                return hitObjects;
            }
        }

        private static readonly List<Color4> defaultComboColors = new List<Color4>()
        {
            new Color4(255, 192, 0, 255),
            new Color4(0, 202, 0, 255),
            new Color4(18, 124, 255, 255),
            new Color4(242, 24, 57, 255),
        };

        private readonly List<Color4> comboColors = new List<Color4>(defaultComboColors);
        public override IEnumerable<Color4> ComboColors => comboColors;

        public string backgroundPath = string.Empty;
        public override string BackgroundPath => backgroundPath;

        private readonly List<OsuBreak> breaks = new List<OsuBreak>();
        public override IEnumerable<OsuBreak> Breaks => breaks;

        public EditorBeatmap(string path)
        {
            Path = path;
        }

        public override string ToString() => Name;

        #region Timing

        private readonly List<ControlPoint> controlPoints = new List<ControlPoint>();

        public override IEnumerable<ControlPoint> ControlPoints => controlPoints;
        public override IEnumerable<ControlPoint> TimingPoints
        {
            get
            {
                var timingPoints = new List<ControlPoint>();
                foreach (var controlPoint in controlPoints)
                    if (!controlPoint.IsInherited)
                        timingPoints.Add(controlPoint);
                return timingPoints;
            }
        }

        public ControlPoint? GetControlPointAt(int time, Predicate<ControlPoint>? predicate)
        {
            if (controlPoints == null) return null;
            ControlPoint? closestTimingPoint = null;
            foreach (var controlPoint in controlPoints)
            {
                if (predicate != null && !predicate(controlPoint)) continue;
                if (closestTimingPoint == null || controlPoint.Offset - time <= ControlPointLeniency)
                    closestTimingPoint = controlPoint;
                else break;
            }
            return closestTimingPoint ?? ControlPoint.Default;
        }

        public override ControlPoint? GetControlPointAt(int time)
            => GetControlPointAt(time, null);

        public override ControlPoint? GetTimingPointAt(int time)
            => GetControlPointAt(time, cp => !cp.IsInherited);

        #endregion

        #region .osu parsing

        public static EditorBeatmap Load(string path)
        {
            Trace.WriteLine($"Loading beatmap {path}");
            try
            {
                var beatmap = new EditorBeatmap(path);
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, Project.Encoding))
                    reader.ParseSections(sectionName =>
                    {
                        switch (sectionName)
                        {
                            case "General": parseGeneralSection(beatmap, reader); break;
                            case "Editor": parseEditorSection(beatmap, reader); break;
                            case "Metadata": parseMetadataSection(beatmap, reader); break;
                            case "Difficulty": parseDifficultySection(beatmap, reader); break;
                            case "Events": parseEventsSection(beatmap, reader); break;
                            case "TimingPoints": parseTimingPointsSection(beatmap, reader); break;
                            case "Colours": parseColoursSection(beatmap, reader); break;
                            case "HitObjects": parseHitObjectsSection(beatmap, reader); break;
                        }
                    });
                return beatmap;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load beatmap \"{System.IO.Path.GetFileNameWithoutExtension(path)}\".", e);
            }
        }

        private static void parseGeneralSection(EditorBeatmap beatmap, StreamReader reader)
        {
            reader.ParseKeyValueSection((key, value) =>
            {
                switch (key)
                {
                    case "AudioFilename": beatmap.audioFilename = value; break;
                    case "StackLeniency": beatmap.stackLeniency = double.Parse(value, CultureInfo.InvariantCulture); break;
                }
            });
        }
        private static void parseEditorSection(EditorBeatmap beatmap, StreamReader reader)
        {
            reader.ParseKeyValueSection((key, value) =>
            {
                switch (key)
                {
                    case "Bookmarks":
                        foreach (var bookmark in value.Split(','))
                            if (value.Length > 0)
                                beatmap.bookmarks.Add(int.Parse(bookmark));
                        break;
                }
            });
        }
        private static void parseMetadataSection(EditorBeatmap beatmap, StreamReader reader)
        {
            reader.ParseKeyValueSection((key, value) =>
            {
                switch (key)
                {
                    case "Version": beatmap.name = value; break;
                    case "BeatmapID": beatmap.id = long.Parse(value); break;
                }
            });
        }
        private static void parseDifficultySection(EditorBeatmap beatmap, StreamReader reader)
        {
            reader.ParseKeyValueSection((key, value) =>
            {
                switch (key)
                {
                    case "HPDrainRate": beatmap.hpDrainRate = double.Parse(value, CultureInfo.InvariantCulture); break;
                    case "CircleSize": beatmap.circleSize = double.Parse(value, CultureInfo.InvariantCulture); break;
                    case "OverallDifficulty": beatmap.overallDifficulty = double.Parse(value, CultureInfo.InvariantCulture); break;
                    case "ApproachRate": beatmap.approachRate = double.Parse(value, CultureInfo.InvariantCulture); break;
                    case "SliderMultiplier": beatmap.sliderMultiplier = double.Parse(value, CultureInfo.InvariantCulture); break;
                    case "SliderTickRate": beatmap.sliderTickRate = double.Parse(value, CultureInfo.InvariantCulture); break;
                }
            });
        }
        private static void parseTimingPointsSection(EditorBeatmap beatmap, StreamReader reader)
        {
            reader.ParseSectionLines(line => beatmap.controlPoints.Add(ControlPoint.Parse(line)));
            beatmap.controlPoints.Sort();
        }
        private static void parseColoursSection(EditorBeatmap beatmap, StreamReader reader)
        {
            beatmap.comboColors.Clear();
            reader.ParseKeyValueSection((key, value) =>
            {
                if (!key.StartsWith("Combo"))
                    return;

                var rgb = value.Split(',');
                beatmap.comboColors.Add(new Color4(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]), 255));
            });

            if (beatmap.comboColors.Count == 0)
                beatmap.comboColors.AddRange(defaultComboColors);
        }
        private static void parseEventsSection(EditorBeatmap beatmap, StreamReader reader)
        {
            reader.ParseSectionLines(line =>
            {
                if (line.StartsWith("//")) return;
                if (line.StartsWith(" ")) return;

                var values = line.Split(',');
                switch (values[0])
                {
                    case "0":
                        beatmap.backgroundPath = removePathQuotes(values[2]);
                        break;
                    case "2":
                        beatmap.breaks.Add(OsuBreak.Parse(beatmap, line));
                        break;
                }
            }, false);
        }
        private static void parseHitObjectsSection(EditorBeatmap beatmap, StreamReader reader)
        {
            OsuHitObject? previousHitObject = null;
            var colorIndex = 0;
            var comboIndex = 0;

            reader.ParseSectionLines(line =>
            {
                var hitobject = OsuHitObject.Parse(beatmap, line) ?? throw new Exception();

                if (hitobject.NewCombo || previousHitObject == null || (previousHitObject.Flags & HitObjectFlag.Spinner) > 0)
                {
                    hitobject.Flags |= HitObjectFlag.NewCombo;

                    var colorIncrement = hitobject.ComboOffset;
                    if ((hitobject.Flags & HitObjectFlag.Spinner) == 0)
                        colorIncrement++;
                    colorIndex = (colorIndex + colorIncrement) % beatmap.comboColors.Count;
                    comboIndex = 1;
                }
                else comboIndex++;

                hitobject.ComboIndex = comboIndex;
                hitobject.ColorIndex = colorIndex;
                hitobject.Color = beatmap.comboColors[colorIndex];

                beatmap.hitObjects.Add(hitobject);
                previousHitObject = hitobject;
            });
        }

        private void postProcessHitObjects()
        {
            hitObjectsPostProcessed = true;

            var stackLenienceSquared = 3 * 3; // Distance in osuPixels
            var preemtTime = GetDifficultyRange(ApproachRate, 1800, 1200, 450);

            for (var i = hitObjects.Count - 1; i > 0; i--)
            {
                var objectI = hitObjects[i];

                if (objectI.StackIndex != 0 || objectI is OsuSpinner)
                    continue;

                var n = i;
                if (objectI is OsuCircle)
                {
                    while (--n >= 0)
                    {
                        var objectN = hitObjects[n];
                        if (objectN is OsuSpinner)
                            continue;

                        if (objectI.StartTime - (preemtTime * StackLeniency) > objectN.EndTime)
                            break;

                        var spanN = objectN as OsuSlider;
                        if (spanN != null && (spanN.PlayfieldEndPosition - objectI.PlayfieldPosition).LengthSquared < stackLenienceSquared)
                        {
                            var offset = objectI.StackIndex - objectN.StackIndex + 1;
                            for (var j = n + 1; j <= i; j++)
                            {
                                if ((spanN.PlayfieldEndPosition - hitObjects[j].PlayfieldPosition).LengthSquared < stackLenienceSquared)
                                    hitObjects[j].StackIndex -= offset;
                            }
                            break;
                        }

                        if ((objectN.PlayfieldPosition - objectI.PlayfieldPosition).LengthSquared < stackLenienceSquared)
                        {
                            objectN.StackIndex = objectI.StackIndex + 1;
                            objectI = objectN;
                        }
                    }
                }
                else if (objectI is OsuSlider)
                {
                    while (--n >= 0)
                    {
                        var objectN = hitObjects[n];
                        if (objectN is OsuSpinner)
                            continue;

                        if (objectI.StartTime - (preemtTime * StackLeniency) > objectN.StartTime)
                            break;

                        var spanN = objectN as OsuSlider;
                        if (((spanN?.PlayfieldEndPosition ?? objectN.PlayfieldPosition) - objectI.PlayfieldPosition).LengthSquared < stackLenienceSquared)
                        {
                            objectN.StackIndex = objectI.StackIndex + 1;
                            objectI = objectN;
                        }
                    }
                }
            }

            var hitobjectScale = (1.0f - 0.7f * (CircleSize - 5) / 5) / 2;
            var hitObjectRadius = 64 * hitobjectScale;
            var stackOffset = (float)hitObjectRadius / 10;

            hitObjects.ForEach(h => h.StackOffset = new Vector2(-stackOffset, -stackOffset) * h.StackIndex);
        }

        private static string removePathQuotes(string path)
            => path.StartsWith('\"') && path.EndsWith('\"') ? path[1..^1] : path;

        #endregion
    }
}
