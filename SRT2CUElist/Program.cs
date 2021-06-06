using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;

namespace SRT2CUElist
{
    class Program
    {
        static void PrintError(string txt)
        {
            Console.WriteLine(txt);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintError("Please provide an SRT file(s) to convert.");
                return;
            }
            ParceManyFiles(args);
        }

        static void ParceManyFiles(string[] args)
        {
            for (int idx = 0; idx < args.Length; idx++)
            {
                string file_path = args[idx];

                if (Path.GetExtension(file_path).ToLower() != ".srt")
                {
                    PrintError($"{file_path} is not an SRT file.");
                    return;
                }

                Console.WriteLine($"Processing file: {file_path}");
                string text = File.ReadAllText(file_path);
                string[] lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split(new char[] { '\n' }, StringSplitOptions.None);

                if (int.TryParse(lines[0], out int rnd_i))
                {
                    if (rnd_i != 1)
                    {
                        PrintError("Provided invalid SRT file.");
                        return;
                    }
                }
                else
                {
                    PrintError("Provided invalid SRT file.");
                    return;
                }

                ConvertSRT2CSV(ref lines, Path.ChangeExtension(file_path, ".csv"));
            }
        }

        static void ConvertSRT2CSV(ref string[] lines, string name)
        {
            int line_index = 0;
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                if (line_index >= lines.Length - 1)
                {
                    File.WriteAllText(name, sb.ToString());
                    return;
                }
                else
                {
                    var block = ReadBlock(ref lines, ref line_index);
                    sb.Append($"\"{block.StartTime.ToString(@"hh\:mm\:ss\:ff")}\",");
                    sb.Append($"\"{block.EndTime.ToString(@"hh\:mm\:ss\:ff")}\",");
                    sb.Append($"\"-\",");
                    sb.Append($"\"{block.Text.Replace("\n", "<br>")}\"");
                    sb.Append("\n");
                }
            }
        }

        class SRTBlock
        {
            public int Index;
            public string Text;
            public TimeSpan StartTime;
            public TimeSpan EndTime;
        }

        static SRTBlock ReadBlock(ref string[] lines, ref int next_idx)
        {
            TimeSpan start, end;
            int.TryParse(lines[next_idx++], out int idx);

            if (lines[next_idx].Contains(" --> "))
            {
                string[] timestamps = lines[next_idx++].Split(new string[] { " --> " }, StringSplitOptions.RemoveEmptyEntries);
                start = TimeSpan.ParseExact(timestamps[0], @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
                end = TimeSpan.ParseExact(timestamps[1], @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
            }
            else
            {
                PrintError("Error while parsing SRT");
                return new SRTBlock();
            }

            var sb = new StringBuilder();
            var str = lines[next_idx++];

            while (str != "")
            {
                sb.Append(str.Replace("<b>", "").Replace("</b>", ""));
                str = lines[next_idx++];
            }

            while (str == "")
            {
                if (next_idx >= lines.Length - 1)
                {
                    break;
                }

                if (lines[next_idx + 1] == "")
                {
                    str = lines[next_idx++];
                }
                else
                {
                    break;
                }
            }

            return new SRTBlock() { Index = idx, StartTime = start, EndTime = end, Text = sb.ToString() };
        }
    }
}
