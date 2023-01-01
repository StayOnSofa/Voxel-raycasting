using RPlay.Primitives;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RPlay;

public abstract class Flow
{
    public GL GL { private set; get; }
    public IInputContext Input { private set; get; }
    
    public IWindow Window { private set; get; }

    public void Setup(IWindow window)
    {
        Window = window;

        window.Size = new Vector2D<int>(1280, 720);
        
        window.Load += () =>
        {
            GL = window.CreateOpenGL();
            Input = window.CreateInput();
            
            Shader.Register(GL);
            ScreenBuffer.Register(GL);
            Texture.Register(GL);
            FrameBuffer.Register(GL);
            Texture3D.Register(GL);
     
            Init();
            ViewPortToWindow();
        };
        
        window.Update += Update;
        window.Render += Draw;
        window.Resize += (value) =>
        {
            Resize(value.X, value.Y);
            
            Window.SwapBuffers();
            GL.Viewport(0, 0 , (uint)value.X, (uint)value.Y);
        };
    }

    public void ViewPortToWindow()
    {
        Resize(Window.Size.X, Window.Size.Y);
    }

    public void SetViewPort(int x, int y)
    {
        GL.Viewport(0, 0 , (uint)x, (uint)y);
    }

    public abstract void Resize(int x, int y);
    public abstract unsafe void Init();
    public abstract unsafe void Update(double dt);
    public abstract unsafe void Draw(double dt);
}