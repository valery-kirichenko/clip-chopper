using System;
using System.Diagnostics;

namespace ClipChopper
{
    static class KeyframeProber
    {
        public static TimeSpan FindClosestKeyframeTime(string filePath, TimeSpan time)
        {
            var ffprobePath = Unosquare.FFME.Library.FFmpegDirectory + @"\ffprobe.exe";
            var keyframe = TimeSpan.Zero;
            
            var startInfo = new ProcessStartInfo()
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = ffprobePath,
                Arguments = String.Format("-threads {0} -select_streams v -show_frames -print_format csv -show_entries frame=key_frame,pkt_dts_time \"{1}\"", Environment.ProcessorCount, filePath)
            };

            using (var probe = Process.Start(startInfo))
            {
                probe.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    if (e.Data == null) return;
                    var data = e.Data.Split(',');
                    var splitted_time = data[2].Split('.');
                    TimeSpan frame = TimeSpan.FromSeconds(Int32.Parse(splitted_time[0])) + TimeSpan.ParseExact(splitted_time[1], "ffffff", System.Globalization.CultureInfo.InvariantCulture);

                    if (data[1] == "1")
                    {
                        if (frame > time)
                        {
                            probe.Kill();
                        }
                        else
                        {
                            keyframe = frame;
                        }
                    }
                });
                probe.BeginOutputReadLine();
                probe.WaitForExit();
            }

            return keyframe;
        }
    }
}
