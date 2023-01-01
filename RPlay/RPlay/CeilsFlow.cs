using System.Numerics;
using RPlay.Primitives;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RPlay;

public class CeilsFlow : Flow
{
    private IKeyboard? _keyboard;
    private IMouse? _mouse;

    private FrameBuffer _frameBuffer;
    
    private ScreenShader _screenShader;
    private Shader _ceilsShader;

    private Texture _texture;

    public override void Resize(int x, int y)
    {
        _screenShader.SetVector3("s_resolution", new Vector2(x, y));
    }

    private Vector2 textureSize = new Vector2(8192, 8192);
    
    Vector2D<int> voxToTexCoord(Vector3D<int> p) {

        int x = (p.Y) % 16;
        int z = (p.Y) / 16;

        int px = x * 512;
        int pz = z * 512;

        return new Vector2D<int>(p.X + px, p.Z + pz);
    }

    private Vector3D<int> texCoordToVox(Vector2D<int> texCoord) {
        
        int x = texCoord.X / 512;
        int z = texCoord.Y / 512;
        int y = (z * 16) + x;

        return new Vector3D<int>(texCoord.X - (x * 512), y, texCoord.Y - (z * 512));
    }

    
    public override unsafe void Init()
    {
        _keyboard = Input.Keyboards.FirstOrDefault();
        _mouse = Input.Mice.FirstOrDefault();
        
        _screenShader = new ScreenShader();
        _ceilsShader = new Shader("CeilsShader/C_V.glsl", "CeilsShader/C_F.glsl");

        _frameBuffer = new FrameBuffer((int)textureSize.X, (int)textureSize.Y);
        _texture = new Texture("Noise.png", false);

        Vector3D<int> voxelCoord = new Vector3D<int>(0, 255, 0);
        
        Vector2D<int> voxelTo2D = voxToTexCoord(voxelCoord);
        Vector3D<int> voxelCoord1 = texCoordToVox(voxelTo2D);

        Console.WriteLine($"3D to 2D{voxelCoord} converted to {voxelTo2D}");
        Console.WriteLine($"2D to 3D{voxelTo2D} converted to {voxelCoord1}");
    }

    public override unsafe void Update(double dt)
    {
        var p = _mouse.Position;
        var screen = new Vector2(Window.Size.X, Window.Size.Y);
        
        var normalized = p / screen;
        normalized.Y = 1.0f - normalized.Y;
        
        var textureCoords = new Vector2(normalized.X * 512, normalized.Y * 512);

        var pixelOnTexture = voxToTexCoord(new Vector3D<int>((int)textureCoords.X, 0, (int)textureCoords.Y));
        
        
        if (MouseToggle(MouseButton.Left))
            _frameBuffer.ChangePixel(pixelOnTexture.X, pixelOnTexture.Y, placedLight);
        
        if (MouseToggle(MouseButton.Right))
            _frameBuffer.ChangePixel(pixelOnTexture.X, pixelOnTexture.Y, 0);
        
        if (_keyboard.IsKeyPressed(Key.G))
            _frameBuffer.ChangePixel(pixelOnTexture.X, pixelOnTexture.Y, palacedBlock);

    }

    private bool _keyChangeState = false;

    private bool MouseToggle(MouseButton key)
    {
        if (_mouse == null) return false;

        bool isPress = _mouse.IsButtonPressed(key);
        if (isPress != _keyChangeState)
        {
            _keyChangeState = isPress;
            return isPress;
        }

        return false;
    }
    
    const int maxPower = 32;
    const int placedLight = maxPower + 1;
    const int palacedBlock = placedLight + 1;

    private int frame = 0;
    
    public override unsafe void Draw(double dt)
    {
        frame += 1;

        if (frame % 2 == 0)
        {
            SetViewPort((int) textureSize.X, (int) textureSize.Y);
            _frameBuffer.SetupFrameBuffer();

            _ceilsShader.Use();
            _frameBuffer.Bind();

            ScreenBuffer.DrawScreenBuffer();

            _frameBuffer.EndFrameBuffer();
        }

        ViewPortToWindow();
        GL.ClearColor(0.2f,0.2f,0.2f,1f);
        GL.Clear((uint) ClearBufferMask.ColorBufferBit);

        _screenShader.Use();
        _frameBuffer.Bind();
        
        
        ScreenBuffer.DrawScreenBuffer();
    }
}