using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

namespace RPlay;

public class PalleteTexture
{
    private static string Path => Application.Path + "Textures/";

    public static Color GetClosestColor(Color color, Color[] palette)
    {
        double minDistance = double.MaxValue;
        Color closestColor = Color.Black;
        
        foreach (Color paletteColor in palette)
        {
            double distance = GetColorDistance(color, paletteColor);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = paletteColor;
            }
        }
        
        return closestColor;
    }
    private static double GetColorDistance(Color color1, Color color2)
    {
        int r = color1.R - color2.R;
        int g = color1.G - color2.G;
        int b = color1.B - color2.B;
        return Math.Sqrt(r * r + g * g + b * b);
    }
    
    private Dictionary<ushort, Color> _idToColor;
    private Dictionary<Color, ushort> _colorToId;

    private Color[] _pallete;

    public PalleteTexture(string path)
    {
        _idToColor = new();
        _colorToId = new();

        ushort index = 1;
        
        using (var image = Image.Load<Rgba32>(Path + path))
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixelColor = image[x, y];

                    Color color = Color.FromArgb(1, pixelColor.R, pixelColor.G, pixelColor.B);

                    _idToColor.Add(index, color);
                    _colorToId.Add(color, index);

                    index += 1;
                }
            }
        }

        _pallete = _idToColor.Values.ToArray();
    }

    public ushort GetIndexByRGB(Color color)
    {
        var outputColor = GetClosestColor(color, _pallete);
        return _colorToId[outputColor];
    }
    public ushort GetIndexByRGB(int r, int g, int b)
    {
        Color color = Color.FromArgb(1, r, g, b);
        var outputColor = GetClosestColor(color, _pallete);

        return _colorToId[outputColor];
    }
}