using Silk.NET.Windowing;
using Window = Silk.NET.Windowing.Window;

namespace RPlay
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default;
            options.Title = "RPlay";
            
            var window = Window.Create(options);
            new GameFlow().Setup(window);
            window.Run();
        }
    }
}