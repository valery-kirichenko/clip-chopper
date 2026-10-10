using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Unosquare.FFME;

namespace ClipChopper
{
    internal static class KeyframeProber
    {
        public static async Task<TimeSpan> FindClosestKeyframeTimeAsync(string filePath,
            TimeSpan time, IProgress<int> progress)
        {
            var ffprobePath = Path.Combine(Library.FFmpegDirectory, "ffprobe.exe");
            var keyframe = TimeSpan.Zero;

            string args = $"-threads {Environment.ProcessorCount} -select_streams v -skip_frame nokey " +
                $"-show_frames -print_format csv " +
                $"-show_entries frame=key_frame,pkt_pts_time \"{filePath}\"";

            var startInfo = new ProcessStartInfo()
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = ffprobePath,
                Arguments = args
            };

            using (var probe = Process.Start(startInfo))
            {
                Debug.Assert(probe != null, nameof(probe) + " != null");
                probe.OutputDataReceived += (s, e) =>
                {
                    if (e.Data is null) return;

                    var data = e.Data.Split(',');
                    var splittedTime = data[2].Split('.');
                    
                    // TODO: move this logic to new function.
                    TimeSpan frame = TimeSpan.FromSeconds(
                        int.Parse(splittedTime[0])) + TimeSpan.ParseExact(splittedTime[1],
                        "ffffff", System.Globalization.CultureInfo.InvariantCulture
                    );

                    if (data[1] == "1")
                    {
                        if (frame > time)
                        {
                            probe.Kill();
                        }
                        else
                        {
                            keyframe = frame;
                            progress.Report((int)(frame.TotalMilliseconds / time.TotalMilliseconds * 100));
                        }
                    }
                };
                probe.BeginOutputReadLine();
                await probe.WaitForExitAsync();
            }

            return keyframe;
        }
    }
}
