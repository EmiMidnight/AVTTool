using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVTTool
{
    internal class ImageProcessing
    {
        public static void ExportTexture(Program.AvatarEntry entry, bool background, byte[] rgbaBytes, int width, int height, int offsetLeft, int offsetTop)
        {
            using (Image<Rgba32> baseImage = new(256, 256))
            {
                string imageName = $"{entry.id}";
                if (entry.categoryId == 7)
                {
                    if (background)
                    {
                        imageName += "_bg";
                    }
                    else
                    {
                        imageName += "_fg";
                    }
                }
                var image = Image.LoadPixelData<Abgr32>(rgbaBytes, width, height);
                baseImage.Mutate(o => o
                .DrawImage(image, new Point(offsetLeft, offsetTop), 1f)
            );
                baseImage.SaveAsPng($"{Program.fileName}/{imageName}.png");
            }
        }
    }
}
