using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVTTool
{
    internal class ImageProcessing
    {
        public static void ExportTexture(int id, byte[] rgbaBytes, int width, int height)
        {
            using (var image = Image.LoadPixelData<Abgr32>(rgbaBytes, width, height))
            {
                // Work with the image
                image.SaveAsPng($"{Program.fileName}/{id}.png");
            }
        }
    }
}
