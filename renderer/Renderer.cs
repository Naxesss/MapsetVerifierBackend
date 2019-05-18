﻿using MapsetParser.objects;
using MapsetVerifier.objects;
using MapsetVerifier.objects.metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MapsetVerifierApp.renderer
{
    public class Renderer
    {
        /// <summary> Surrounds the content with a div tag using the given class(es), as well as any other attributes in the tag, like id or data. </summary>
        protected static string DivAttr(string aClass, string anAttr, params object[] aContents)
        {
            return String.Concat("<div", (aClass.Length > 0 ? " class=\"" + aClass + "\"" : ""), anAttr, ">", String.Join("", aContents), "</div>");
        }

        /// <summary> Surrounds the content with a div tag using the given class(es). </summary>
        protected static string Div(string aClass, params object[] aContents)
        {
            return DivAttr(aClass, "", aContents);
        }

        /// <summary> Surrounds the content with an a tag using the given reference.
        /// Does not need to target _blank, as this is done client-side. </summary>
        protected static string Link(string aRef, object aContent)
        {
            return "<a href=\"" + aRef + "\">" + aContent + "</a>";
        }

        /// <summary> Creates a link to the given username with the same username as content. </summary>
        protected static string UserLink(object aUsername)
        {
            return Link("https://osu.ppy.sh/users/" + aUsername, aUsername);
        }

        /// <summary> Creates a link to the given username with the same username as content. </summary>
        protected static string TimestampLink(object aTimestamp)
        {
            return Link("osu://edit/" + aTimestamp + "\" class=\"card-instance-timestamp", aTimestamp);
        }

        /// <summary> Surrounds the content in a data attribute of a given type. </summary>
        protected static string DataAttr(string aType, object aContent)
        {
            return " data-" + aType + "=\"" + aContent + "\"";
        }

        /// <summary> Surrounds the content in a tooltip data attribute. </summary>
        protected static string Tooltip(object aContent)
        {
            return DataAttr("tooltip", aContent);
        }

        protected static string DifficultiesDataAttr(Issue anIssue)
        {
            BeatmapCheckMetadata metadata = anIssue.CheckOrigin.GetMetadata() as BeatmapCheckMetadata;

            List<Beatmap.Difficulty> difficulties = new List<Beatmap.Difficulty>();
            foreach (Beatmap.Difficulty difficulty in Enum.GetValues(typeof(Beatmap.Difficulty)))
                if ((metadata?.Difficulties.Contains(difficulty) ?? true) && (
                        !anIssue.InterpretationPairs.Any(aPair => aPair.Key == "difficulty") ||
                        anIssue.InterpretationPairs.Any(aPair => aPair.Key == "difficulty" && aPair.Value == (int)difficulty)))
                    difficulties.Add(difficulty);

            if (difficulties.Count == Enum.GetValues(typeof(Beatmap.Difficulty)).Length)
                return "";

            return
                DataAttr("condition",
                    "difficulty=" + String.Join(",", difficulties.Select(aDifficulty => (int)aDifficulty))
                );
        }

        protected static string DifficultiesDataAttr(Beatmap.Difficulty[] aDifficulties)
        {
            if (aDifficulties.Count() == Enum.GetValues(typeof(Beatmap.Difficulty)).Length)
                return "";

            return
                DataAttr("condition",
                    "difficulty=" + String.Join(",", aDifficulties.Select(aDifficulty => (int)aDifficulty))
                );
        }

        protected static string InterpretDataAttr(List<KeyValuePair<string, int>> anInterpretPairs)
        {
            return
                String.Concat(
                anInterpretPairs.Select(aPair => aPair.Key).Distinct().Select(aKey =>
                {
                    return
                        DataAttr("condition",
                            aKey + "=" + String.Join(",",
                                anInterpretPairs.Where(aPair => aPair.Key == aKey).Select(aPair => aPair.Value))
                        );
                }));
        }

        /// <summary> Returns the same string but HTML encoded, meaning greater and less than signs no longer form actual tags. </summary>
        public static string Encode(string aString)
        {
            return WebUtility.HtmlEncode(aString);
        }

        /// <summary> Returns the icon of the greatest issue level of all issues given. </summary>
        protected static string GetIcon(IEnumerable<Issue> anIssues)
        {
            return
                anIssues.Any() ?
                    GetIcon(anIssues.Max(anIssue => anIssue.level)) :
                    GetIcon(Issue.Level.Check);
        }

        /// <summary> Returns the icon of the given issue level. </summary>
        protected static string GetIcon(Issue.Level aLevel)
        {
            return
                aLevel == Issue.Level.Unrankable ? "cross" :
                aLevel == Issue.Level.Warning    ? "exclamation" :
                aLevel == Issue.Level.Minor      ? "minor" :
                aLevel == Issue.Level.Error      ? "error" :
                aLevel == Issue.Level.Check      ? "check" :
                                                   "info";
        }

        /// <summary> Wraps all timestamps in the string into proper hyperlinks. </summary>
        protected static string FormatTimestamps(string aMessage)
        {
            string formattedMessage = aMessage;
            Regex stampRegex = new Regex("\\d\\d:\\d\\d:\\d\\d\\d( \\([\\d,]+\\))?");
            foreach (string value in stampRegex.Matches(aMessage).Cast<Match>().Select(aMatch => aMatch.Value).Distinct())
                formattedMessage = formattedMessage.Replace(value, TimestampLink(value));

            return formattedMessage;
        }

        /// <summary> Returns the given string with note or image tags replaced by actual html tags. </summary>
        protected static string ApplyMarkdown(string aValue)
        {
            Regex myRegex = new Regex(@"<image>([A-Za-z0-9\/]+\.jpg)(.*?)<\/image>");

            string myResult = aValue.Replace("<note>", "<div class=\"note\"><div class=\"note-text\">").Replace("</note>", "</div></div>");

            foreach (Match myMatch in myRegex.Matches(myResult))
            {
                string myText = myMatch.Groups[2].Value;
                myResult = myRegex.Replace(myResult, "<div class=\"image\" data-text=\"" + Encode(myText) + "\"><img src=\"$1\"></img></div>", 1);
            }

            return myResult;
        }
    }
}
