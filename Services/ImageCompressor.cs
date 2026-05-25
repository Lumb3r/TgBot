using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace TgBot.Services;

public class ImageCompressor
{
    /// Сжимает изображение до минимального качества JPEG.
    public MemoryStream CompressToLowQuality(Stream inputStream)
    {
        var outputStream = new MemoryStream();

        using (var image = Image.Load(inputStream))
        {
            // Устанавливаем минимальное качество JPEG для создания артефактов сжатия
            var encoder = new JpegEncoder { Quality = 7 };
            image.Save(outputStream, encoder);
        }

        outputStream.Position = 0;
        return outputStream;
    }
}