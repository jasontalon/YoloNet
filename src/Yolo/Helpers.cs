using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Yolo
{
    public static class Helpers
    {
        /// <summary>
        /// Crops the image according to the bounding box coordinates and saves to an image file
        /// </summary>
        /// <param name="prediction"></param>
        /// <param name="image"></param>
        /// <returns>Exported image file path</returns>
        public static string ExportToImage(this Prediction prediction, byte[] image) => ExportToImage(prediction, image);

        /// <summary>
        /// Crops the image according to the bounding box coordinates and saves to an image file
        /// </summary>
        /// <param name="prediction"></param>
        /// <param name="image"></param>
        /// <param name="group"></param>
        /// <returns>Exported image file path</returns>
        public static string ExportToImage(this Prediction prediction, byte[] image, string group = "")
        {
            var rect = new Rectangle(prediction.X, prediction.Y, prediction.Width, prediction.Height);

            using (var src = Image.FromStream(new MemoryStream(image)) as Bitmap)
            using (var resizedBitmap = new Bitmap(rect.Width, rect.Height))
            using (var graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.DrawImage(src, -rect.X, -rect.Y);

                var newFileName = $"{prediction.Name}__[{prediction.Probability.ToString()}]__{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}.jpg";

                var newPath = Path.Combine(Path.GetTempPath(), group);

                if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);

                newFileName = Path.Combine(newPath, newFileName);

                resizedBitmap.Save(newFileName);

                return newFileName;
            }
        }

        /// <summary>
        /// Crops the bounding boxes and exports to a bitmap object
        /// </summary>
        /// <param name="prediction"></param>
        /// <param name="image"></param>
        /// <returns>Exported bitmap object</returns>
        public static Bitmap ExportToBitmap(this Prediction prediction, byte[] image)
        {
            var rect = new Rectangle(prediction.X, prediction.Y, prediction.Width, prediction.Height);

            using (var src = Image.FromStream(new MemoryStream(image)) as Bitmap)
            using (var resizedBitmap = new Bitmap(rect.Width, rect.Height))
            using (var graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.DrawImage(src, -rect.X, -rect.Y);

                return resizedBitmap;
            }
        }

        public static string GetRandomString(this string str, int length = 5)
        {
            string base64Guid = new Regex("[^a-zA-Z0-9 -]").
                Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                string.Empty);

            return base64Guid.Substring(0, 5);
        }
    }
}
