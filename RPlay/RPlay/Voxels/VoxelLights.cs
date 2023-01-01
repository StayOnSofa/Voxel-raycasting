using System.Numerics;
using RPlay.Primitives;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RPlay.Voxels;

public enum LightState : ushort
{
    Block = 34,
    Lamp = 33,
    Empty = 0,
}

public class VoxelLights
{
    private const ushort MaxPower = 32;
    private const ushort PlacedLight = MaxPower + 1;
    private const ushort PalacedBlock = PlacedLight + 1;
    
    private Vector2 textureSize = new Vector2(8192, 8192);
 
    private Flow _flow;
    private Shader _ceilsShader;
    private FrameBuffer _frameBuffer;
    
    private Vector2D<int> voxToTexCoord(Vector3D<int> p) {

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
    
    public VoxelLights(Flow flow)
    {
        _flow = flow;
        
        _ceilsShader = new Shader("CeilsShader/C_V.glsl", "CeilsShader/C_F.glsl");
        _frameBuffer = new FrameBuffer((int)textureSize.X, (int)textureSize.Y);
    }

    public void Compute()
    {
        _flow.SetViewPort((int) textureSize.X, (int) textureSize.Y);
        _frameBuffer.SetupFrameBuffer();

        _ceilsShader.Use();
        _frameBuffer.Bind();

        ScreenBuffer.DrawScreenBuffer();

        _frameBuffer.EndFrameBuffer();
        _flow.ViewPortToWindow();
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        _frameBuffer.Bind(textureSlot);
    }

    public void SetPixel(LightState lightState, int x, int y, int z)
    {
        var textureCoords = voxToTexCoord(new Vector3D<int>(x,y,z));
        ushort type = (ushort)lightState;
        
        _frameBuffer.ChangePixel(textureCoords.X, textureCoords.Y, type);
    }
}