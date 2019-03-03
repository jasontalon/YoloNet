using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yolo
{
    public class YoloWrapper
    {
        private readonly YoloCppWrapper YoloCppWrapper;

        private readonly List<string> Classes;

        public YoloWrapper(string configFile, string weightsFile, string namesFile, int gpu = 0)
        {
            if (!File.Exists(configFile) || !File.Exists(weightsFile) || !File.Exists(namesFile))
                throw new FileNotFoundException("some files are missing.");

            YoloCppWrapper = new YoloCppWrapper(configFile, weightsFile, gpu);

            Classes = File.ReadAllText(namesFile)?.Split(Environment.NewLine)?.ToList();
        }

        public List<Prediction> Detect(string file) => Detect(file, null, 0);
        
        public List<Prediction> Detect(string file, string[] classesToDetect) => Detect(file, classesToDetect, 0);

        public List<Prediction> Detect(string file, float threshold) => Detect(file, null, threshold);

        public List<Prediction> Detect(string file, string[] classesToDetect, float threshold)
        {
            return YoloCppWrapper.Detect(file)?.Where(p => p.frames_counter > 0 && p.h > 0 && p.w > 0).
                Select(r => new Prediction()
                {
                    Height = (int)r.h,
                    Width = (int)r.w,
                    Name = Classes[(int)r.obj_id],
                    Probability = r.prob,
                    X = (int)r.x,
                    Y = (int)r.y
                }).Where(s => (classesToDetect?.Any() ?? false) ? classesToDetect.Contains(s.Name) : true &&
                                threshold > 0 ? s.Probability >= threshold : true)?.OrderByDescending(d => d.Probability)?.ToList();
        }

        public List<Prediction> Detect(byte[] file) => Detect(file, null, 0);

        public List<Prediction> Detect(byte[] file, float threshold) => Detect(file, null, threshold);

        public List<Prediction> Detect(byte[] file, string[] classesToDetect) => Detect(file, classesToDetect, 0);

        public List<Prediction> Detect(byte[] file, string[] classesToDetect, float threshold)
        {
            return YoloCppWrapper.Detect(file)?.Where(p => p.h > 0 && p.w > 0).
                Select(r => new Prediction()
                {
                    Height = (int)r.h,
                    Width = (int)r.w,
                    Name = Classes[(int)r.obj_id],
                    Probability = r.prob,
                    X = (int)r.x,
                    Y = (int)r.y
                }).Where(s => (classesToDetect?.Any() ?? false) ? classesToDetect.Contains(s.Name) : true &&
                                threshold > 0 ? s.Probability >= threshold : true)?.OrderByDescending(d => d.Probability)?.ToList();
        }
    }
}
