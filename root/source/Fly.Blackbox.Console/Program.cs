using Fclp;
using Fly.Blackbox.Console.ExtractCoordinates;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Fly.Blackbox.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string input = "";
                string extractCoordinates = null;
                string mapTimePosition = null;
                string fixFile = null;

                var p = new FluentCommandLineParser();
                p.Setup<string>('i', "input").Callback(i => input = i).Required();
                p.Setup<string>('c', "extractCoordinates").Callback(c => extractCoordinates = c);
                p.Setup<string>('m', "mapTimePosition").Callback(m => mapTimePosition = m);
                p.Setup<string>('f', "fixFile").Callback(f => fixFile = f);
                
                var result = p.Parse(args);

                if (result.HasErrors == false)
                {
                    //IntPtr pFormatContext;
                    //FFmpeg.AVFormatContext formatContext;
                    //
                    //FFmpeg.av_register_all();
                    //FFmpeg.avcodec_register_all();
                    //
                    //var next = FFmpeg.EnumerateFormats().ToList();
                    //var aviFormat = FFmpeg.av_find_input_format("avi");
                    //
                    //var r = FFmpeg.avformat_open_input(out pFormatContext, input, aviFormat, IntPtr.Zero);
                    //r = FFmpeg.avformat_find_stream_info(pFormatContext, IntPtr.Zero);
                    //formatContext = (FFmpeg.AVFormatContext)Marshal.PtrToStructure(pFormatContext, typeof(FFmpeg.AVFormatContext));
                    //
                    //var videoDecoder = FFmpeg.avcodec_find_decoder(FFmpeg.CodecID.CODEC_ID_H264);
                    //
                    //var codecParameters = formatContext.Streams.ToList()[0].codecpar;
                    //
                    //var dic = Marshal.AllocHGlobal(24000);
                    //Marshal.WriteInt64(dic, 0);
                    ////FFmpeg.av_dict_set(dic, "_", "s", 0);
                    //
                    //r = FFmpeg.avcodec_open2(codecParameters, videoDecoder, dic);

                    var extractor = new CoordinatesExtractor();
                    IEnumerable<CameraSensors> sensors = null;

                    sensors = ExtractCoordinates(input, extractCoordinates, extractor, sensors);
                    sensors = ExtractMapTimePosition(input, mapTimePosition, extractor, sensors);

                    if(string.IsNullOrEmpty(fixFile) == false)
                    {
                        var interfile = fixFile + ".bin";

                        var fileFixer = new FileFixer.FileFixer();
                        fileFixer.Fix(input, interfile);

                        ConvertVideo(interfile, fixFile);

                        if (File.Exists(interfile))
                        {
                            File.Delete(interfile);
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            catch
            {

            }       
        }

        private static void ConvertVideo(string input, string output)
        {
            if (File.Exists(output))
            {
                File.Delete(output);
                Thread.Sleep(1000);
            }
        
            var startInfo = new ProcessStartInfo()
            {
                FileName = @"C:\Users\xunil\Desktop\ffmpeg-20160522-git-566be4f-win64-shared\bin\ffmpeg.exe",
                Arguments = $"-codec:v:0 h264 -i \"{input}\" -map 0:0 -map 0:1 -c:v:0 h264 -pix_fmt yuv420p -profile:v baseline -movflags +faststart -c:a:1 copy {output}",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();
            var result = process.StandardOutput.ReadToEnd();            
        }

        private static IEnumerable<CameraSensors> ExtractMapTimePosition(string input, string mapTimePosition, CoordinatesExtractor extractor, IEnumerable<CameraSensors> sensors)
        {
            if (string.IsNullOrEmpty(mapTimePosition) == false)
            {
                sensors = extractor.Extract(input);

                var points = new TimePoints();

                foreach (var item in sensors)
                {
                    points.Points.Add(new TimePoint()
                    {
                        Time = item.Time,
                        Longitude = item.Longitude,
                        Latitude = item.Latitude
                    });
                }

                using (var s = new StreamWriter(File.OpenWrite(mapTimePosition)))
                {
                    var serializer = new NetTopologySuite.IO.GeoJsonSerializer();
                    serializer.Serialize(s, points);
                }
            }

            return sensors;
        }

        private static IEnumerable<CameraSensors> ExtractCoordinates(string input, string extractCoordinates, CoordinatesExtractor extractor, IEnumerable<CameraSensors> sensors)
        {
            if (string.IsNullOrEmpty(extractCoordinates) == false)
            {
                sensors = extractor.Extract(input);

                var c = sensors.Select(x => new GeoAPI.Geometries.Coordinate(x.Longitude, x.Latitude)).ToArray();
                var fc = new LineString(c);

                using (var s = new StreamWriter(File.OpenWrite(extractCoordinates)))
                {
                    var serializer = new NetTopologySuite.IO.GeoJsonSerializer();
                    serializer.Serialize(s, fc);
                }
            }

            return sensors;
        }
    }

    public class TimePoints
    {
        public List<TimePoint> Points { get; set; }

        public TimePoints()
        {
            Points = new List<TimePoint>();
        }
    }

    public class TimePoint
    {
        public float Time { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
    }
}
