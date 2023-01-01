using System.Diagnostics;
using Silk.NET.OpenGL;
using VoxReader;
using VoxReader.Interfaces;
using Color = System.Drawing.Color;

namespace RPlay.Voxels;

public class VoxelMap
{
    public static string Path => Application.Path + "/Voxels/";
    
    public const int Width = 512;
    public const int Height = 256;
    public const int Depth = 512;

    public ushort[] Map { private set; get;}
    
    public int to1D( int x, int y, int z ) {
        return (z * Width * Height) + (y * Depth) + x;
    }

    private FastNoiseLite _perlin;
    private Random _random;
    private PalleteTexture _palleteTexture;
    private Texture3D _texture3D;
    private VoxelLights _lights;
    
    public VoxelMap(VoxelLights lights)
    {
        _lights = lights;
        _perlin = new FastNoiseLite();

        _perlin.SetSeed(1337);
        _perlin.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

        _random = new Random(1337);
        _palleteTexture = new PalleteTexture("Noise1.png");

        GenerateMap();
        GenerateStructures();

        _texture3D = new Texture3D(Width, Height, Depth, (ReadOnlySpan<ushort>) Map);
    }

    private void GenerateMap()
    {
        Map = new ushort[Width * Height * Depth];
        
        ushort colorGreen1 = _palleteTexture.GetIndexByRGB(Color.Chartreuse);
        ushort colorGreen2 = _palleteTexture.GetIndexByRGB(Color.GreenYellow);
        ushort colorGreen3 = _palleteTexture.GetIndexByRGB(Color.SpringGreen);
        ushort colorBrown = _palleteTexture.GetIndexByRGB(Color.SandyBrown);
        ushort colorStone = _palleteTexture.GetIndexByRGB(Color.Coral);
        
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Depth; z++)
            {
                int height = 25 + (int) (_perlin.GetNoise(x * 0.7f, z * 0.7f) * 8);

                for (int y = 1; y <= height; y++)
                {
                    ushort color = colorGreen2;

                    if (y < height - 1)
                    {
                        Map[to1D(x, y, z)] = colorBrown;
                        
                        if (y < height - 6)
                            Map[to1D(x, y, z)] = colorStone;
                        
                        _lights.SetPixel(LightState.Block, x,y,z);
                    }
                    else
                    {

                        if (x % 2 == 0 && y % 2 == 0)
                        {
                            color = colorGreen1;
                        }
                        else
                        {
                            if (z % 2 == 0)
                                color = colorGreen3;
                        }

                        Map[to1D(x, y, z)] = color;
                        _lights.SetPixel(LightState.Block, x,y,z);
                    }
                }
            }
        }
    }
    private void GenerateStructures()
    {
         IVoxFile voxFile = VoxReader.VoxReader.Read(Path + "mechSniper.vox");
            IModel[] models = voxFile.Models;
            
            foreach (var model in models)
            {
                Voxel[] voxels = model.Voxels;
                foreach (var voxel in voxels)
                {
                    var p = voxel.Position;
                    p = new Vector3(p.X, p.Y, p.Z+25);
                    
                    var color = voxel.Color;
                    
                    Map[to1D(p.X, p.Z, p.Y)] = _palleteTexture.GetIndexByRGB(color.R, color.G, color.B);
                    _lights.SetPixel(LightState.Block, p.X, p.Z, p.Y);
                }
            }
            
            voxFile = VoxReader.VoxReader.Read(Path + "monu1.vox");
            models = voxFile.Models;

            int randomX = 50;
            int randomY = 37;
            
            foreach (var model in models)
            {
                Voxel[] voxels = model.Voxels;
                foreach (var voxel in voxels)
                {
                    var p = voxel.Position;
                    p = new Vector3(p.X + randomX, p.Y + randomY, p.Z+20);
                    
                    var color = voxel.Color;
                    
                    Map[to1D(p.X, p.Z, p.Y)] = _palleteTexture.GetIndexByRGB(color.R, color.G, color.B);
                    _lights.SetPixel(LightState.Block, p.X, p.Z, p.Y);
                }
            }
            
            
            voxFile = VoxReader.VoxReader.Read(Path + "menger.vox");
            models = voxFile.Models;

             randomX = 150;
             randomY = 77;
            
            foreach (var model in models)
            {
                Voxel[] voxels = model.Voxels;
                foreach (var voxel in voxels)
                {
                    var p = voxel.Position;
                    p = new Vector3(p.X + randomX, p.Y + randomY, p.Z+20);
                    
                    var color = voxel.Color;
                    
                    Map[to1D(p.X, p.Z, p.Y)] = _palleteTexture.GetIndexByRGB(color.R, color.G, color.B);
                    _lights.SetPixel(LightState.Block, p.X, p.Z, p.Y);
                }
            }
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0) => _texture3D.Bind(textureSlot);
    
    private bool InBorder(int x, int y, int z)
    {
        if (x >= 0 && x < Width)
        {
            if (y >= 0 && y < Height)
            {
                if (z >= 0 && z < Depth)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public ushort GetBlock(int x, int y, int z)
    {
        if (InBorder(x,y,z))
            return Map[to1D(x, y, z)];

        return 0;
    }

    public void BreakBlock(int x, int y, int z)
    {
        if (InBorder(x, y, z))
        {
            Map[to1D(x, y, z)] = 0;
            _texture3D.ChangePixel(x,y,z, 0);
        }
    }

    public void PlaceBlock(ushort block, int x, int y, int z)
    {
        if (InBorder(x, y, z))
        {
            Map[to1D(x, y, z)] = block;
            _texture3D.ChangePixel(x,y,z, block);
        }
    }
}