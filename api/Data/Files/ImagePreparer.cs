namespace Planera.Data.Files;

public class ImagePreparer
{
    public static byte[] Resize(byte[] bytes, int width, int height)
    {
        using var image = Image.Load(bytes);
        var rectangle = new Rectangle(
            new(0, 0),
            new(width, height)
        );
        var resizeOptions = new ResizeOptions
        {
            Size = new Size(width, height), TargetRectangle = rectangle,
        };

        image.Mutate(x => x.Resize(resizeOptions));

        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);

        return memoryStream.ToArray();
    }
}