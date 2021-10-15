using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using System.Linq;

namespace YouTubeDownloader
{
    class Options
    {
        [Option('u', "url", Required = true, HelpText = "YouTube URL")]
        public String Url { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file")]
        public String Output { get; set; }
    }
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var opts = CommandLine.Parser.Default.ParseArguments<Options>(args);

                Console.Title = "YoutubeExplode Demo";

                var youtube = new YoutubeClient();

                // Read the video ID
                var videoId = VideoId.Parse(opts.Value.Url);

                // Get available streams and choose the best muxed (audio + video) stream
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
                var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
                if (streamInfo is null)
                {
                    // Available streams vary depending on the video and
                    // it's possible there may not be any muxed streams.
                    Console.Error.WriteLine("This video has no muxed streams.");
                    return 1;
                }

                // Download the stream
                Console.Write(
                    $"Downloading stream: {streamInfo.VideoQuality.Label} / {streamInfo.Container.Name}... "
                );

                await youtube.Videos.Streams.DownloadAsync(streamInfo, opts.Value.Output);

                return 0;
            } catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
