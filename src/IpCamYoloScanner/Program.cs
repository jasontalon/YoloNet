using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Yolo;
namespace YoloConsole
{
    class Program
    {
        private static Logger _logger { get { return LogManager.GetCurrentClassLogger(); } }

        static (string camUrl, string[] classes, string model, int durationInSeconds) LoadVariables(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();

            var s = args.Select(p => (arg: p.Split('=')[0].TrimStart('-'), val: p.Split('=')[1]));
            var cam = s.FirstOrDefault(p => p.arg.Equals("cam", StringComparison.InvariantCultureIgnoreCase)).val ?? "porch1";
            var classes = s.FirstOrDefault(p => p.arg.Equals("classes", StringComparison.InvariantCultureIgnoreCase)).val?.Split(',');
            var model = s.FirstOrDefault(p => p.arg.Equals("model", StringComparison.InvariantCultureIgnoreCase)).val ?? "yolov3";

            int.TryParse(s.FirstOrDefault(p => p.arg.Equals("duration", StringComparison.InvariantCultureIgnoreCase)).val ?? "60", out int durationInSeconds);

            var camUrl = string.Format(config["cam_url"], cam);

            return (camUrl, classes, model, durationInSeconds);
        }

        static void Main(string[] args)
        {
            try
            {
                var variables = LoadVariables(args);

                ReadCam(variables.model, variables.camUrl, variables.classes, variables.durationInSeconds);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
            }
            finally
            {
                Console.WriteLine("Done");
                _logger.Info("Done");

                LogManager.Shutdown();
                Console.ReadLine();
            }
        }

        static YoloWrapper InitWrapper(string model)
        {
            var dir = Directory.GetCurrentDirectory();

            return new YoloWrapper(Path.Combine(dir, $"{model}.cfg"), Path.Combine(dir, $"{model}.weights"), Path.Combine(dir, $"{model}.names"));
        }

        static async void ReadCam(string model, string camUrl, string[] classes, int scanDuration = 60)
        {
            var yolo = InitWrapper(model);

            using (WebClient client = new WebClient())
            {
                var dtUntil = DateTime.Now.AddSeconds(scanDuration);

                int count = 0;
                while (dtUntil >= DateTime.Now)
                {
                    var data = client.DownloadData(camUrl);

                    var results = await Task.Run(() => yolo.Detect(data, classes));

                    if (!results?.Any() ?? false) continue;
                    Console.WriteLine($"{count} {DateTime.Now}");
                    results.ForEach((pred) => Console.WriteLine($"{pred.Name} {pred.Probability} X={pred.X} Y={pred.Y} Width={pred.Width} Height={pred.Height}"));

                    Console.WriteLine("-----------------");
                    count++;
                }
            }
        }
    }
}
