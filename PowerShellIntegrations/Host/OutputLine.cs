﻿using System;
using System.Drawing;
using System.Text;
using System.Web;

namespace Cognifide.PowerShell.PowerShellIntegrations.Host
{
    public class OutputLine
    {
        public const string FormatResponseText = "text";
        public const string FormatResponseHtml = "html";
        public const string FormatResponseJsterm = "jsterm";

        public OutputLine(OutputLineType outputLineType, string value, ConsoleColor foregroundColor,
                          ConsoleColor backgroundColor, bool terminated)
        {
            LineType = outputLineType;
            Text = value;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Terminated = terminated;
        }

        public OutputLineType LineType { get; internal set; }
        public string Text { get; internal set; }
        public ConsoleColor ForegroundColor { get; internal set; }
        public ConsoleColor BackgroundColor { get; internal set; }
        public bool Terminated { get; internal set; }

        public void GetHtmlLine(StringBuilder output)
        {
            var outString = Terminated ? Text.TrimEnd() : Text;

            output.AppendFormat(
                Terminated
                    ? "<span style='background-color:{0};color:{1};'>{2}</span>\r\n"
                    : "<span style='background-color:{0};color:{1};'>{2}</span>",
                ProcessHtmlColor(BackgroundColor),
                ProcessHtmlColor(ForegroundColor),
                HttpUtility.HtmlEncode(outString));
        }

        public string ToHtmlString()
        {
            var outString = Terminated ? Text.TrimEnd() : Text;
            return String.Format(
                Terminated
                    ? "<span style='background-color:{0};color:{1};'>{2}</span>\r\n"
                    : "<span style='background-color:{0};color:{1};'>{2}</span>",
                ProcessHtmlColor(BackgroundColor),
                ProcessHtmlColor(ForegroundColor),
                HttpUtility.HtmlEncode(outString));
        }

        public static string ProcessHtmlColor(ConsoleColor color)
        {
            switch (color)
            {
                case (ConsoleColor.DarkBlue):
                    return "#012456";
                case (ConsoleColor.Green):
                    return "Lime";
                default:
                    return color.ToString();
            }
        }

        public static Color ProcessTerminalColor(ConsoleColor color)
        {
            switch (color)
            {
                case (ConsoleColor.DarkBlue):
                    return Color.FromArgb(1, 0x24, 0x56);
                case (ConsoleColor.Green):
                    return Color.LimeGreen;
                default:
                    return Color.FromName(color.ToString());
            }            
        }

        public void GetTerminalLine(StringBuilder output)
            //, ConsoleColor HostBackgroundColor, ConsoleColor HostForegroundColor)
        {
            var outString = Terminated ? Text.TrimEnd() : Text;
            Color htmlBackgroundColor = ProcessTerminalColor(BackgroundColor);
            Color htmlForegroundColor = ProcessTerminalColor(ForegroundColor);
            output.AppendFormat(
                Terminated
                    ? "[[;#{0}{1}{2};#{3}{4}{5}]{6}]\r\n"
                    : "[[;#{0}{1}{2};#{3}{4}{5}]{6}]",
                htmlForegroundColor.R.ToString("X2"),
                htmlForegroundColor.G.ToString("X2"),
                htmlForegroundColor.B.ToString("X2"),
                htmlBackgroundColor.R.ToString("X2"),
                htmlBackgroundColor.G.ToString("X2"),
                htmlBackgroundColor.B.ToString("X2"),
                HttpUtility.HtmlEncode(outString).Replace("[", "%((%").Replace("]", "%))%"));
        }

        public void GetPlainTextLine(StringBuilder output)
        {
            output.Append(Text);
            if (Terminated)
            {
                output.Append("\n");
            }
        }

        public void GetLine(StringBuilder temp, string stringFormat)
        {
            switch (stringFormat)
            {
                case (FormatResponseHtml):
                    GetHtmlLine(temp);
                    break;
                case (FormatResponseJsterm):
                    GetTerminalLine(temp);
                    break;
                //case (FormatResponseText):
                default:
                    GetPlainTextLine(temp);
                    break;
            }
        }
    }
}