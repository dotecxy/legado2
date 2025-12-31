using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Test
{
    internal class ImageTest
    {
        [Test]
        public void TestCrop()
        {
            var path = @"E:\Users\xiaozhutui\Downloads\jimeng-2025-12-29-8363-App icon, e-book reader, abstract Chines....png";
            var path2 = @"E:\Users\xiaozhutui\Downloads\jimeng-2025-12-29-8363.png";
            var image = Image<Rgba32>.Load(path);
            var size = image.Size;
            var cropRect = new Rectangle(0, (size.Height - size.Width) / 2, size.Width, size.Width);
            image.Mutate(i =>
            {


                i.Crop(cropRect);
                i.Resize(new Size(1024, 1024));
            });
            image.Save(path2);
        }
    }
}
