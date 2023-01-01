using System.Numerics;
using RPlay.Primitives;
using RPlay.Toggle;
using RPlay.Voxels;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RPlay;

public class GameFlow : Flow
{
    private Shader _shader;

    private double _sceneTime;
    private IKeyboard? _keyboard;
    private IMouse? _mouse;
    
    private Camera _camera;
    private Texture _texture;

    private VoxelMap _voxelMap;
    private VoxelLights _voxelLights;
    
    private CameraVoxelSelector _selector;

    private ToggleKey _toggleECS;
    private ToggleKey _toggleG;

    private ToggleMouse _toggleMouse;
    
    public override void Resize(int x, int y)
    {
       _shader.SetVector3("s_resolution", new Vector2(x, y));
    }

    public override unsafe void Init()
    {
        _shader = new Shader("Vertex.glsl", "Fragment.glsl");
    
        _texture = new Texture("Noise1.png", false);

        _keyboard = Input.Keyboards.FirstOrDefault();
        
        _toggleECS = new ToggleKey(Key.Escape, _keyboard);
        _toggleG = new ToggleKey(Key.G, _keyboard);
        
        _mouse = Input.Mice.FirstOrDefault();
        _toggleMouse = new ToggleMouse(_mouse);
        
        _camera = new Camera(this);
        _camera.Position = new Vector3(0, 0, -64);

        _voxelLights = new VoxelLights(this);
        _voxelMap = new VoxelMap(_voxelLights);

        _selector = new CameraVoxelSelector(_camera, _voxelMap, _voxelLights);

        _shader.Use();
        
        _shader.SetUniform("uTexture0", 0);
        _shader.SetUniform("uTexture1", 1);
        _shader.SetUniform("uTexture2", 2);
        
        _texture.Bind(TextureUnit.Texture0);
        _voxelMap.Bind(TextureUnit.Texture1);
        _voxelLights.Bind(TextureUnit.Texture3);
    }
    

    public override unsafe void Update(double dt)
    {
        _sceneTime += dt;
        _shader.SetFloat("s_time", _sceneTime);

        if (_keyboard == null) return;

        Vector3 velocity = Vector3.Zero;
        var cameraForward = _camera.Forward;

        if (_toggleECS.IsToggle())
            _camera.MouseGrab = !_camera.MouseGrab;

        if (_keyboard.IsKeyPressed(Key.W))
            velocity += cameraForward;
        if (_keyboard.IsKeyPressed(Key.S))
            velocity -= cameraForward;
        
        var cameraRight = Vector3.Normalize(Vector3.Cross(new Vector3(0,1,0), cameraForward));
        
        if (_keyboard.IsKeyPressed(Key.D))
            velocity += cameraRight;
        if (_keyboard.IsKeyPressed(Key.A))
            velocity -= cameraRight;
        
        if (_keyboard.IsKeyPressed(Key.Space))
            velocity.Y += 1;
        if (_keyboard.IsKeyPressed(Key.ShiftLeft))
            velocity.Y -= 1;


        float speed = 10;

        if (_keyboard.IsKeyPressed(Key.ControlLeft))
            speed *= 4;
        
        _camera.Position += velocity * speed * (float)dt;
        
        _shader.SetVector3("o_camera", _camera.Position);
        _shader.SetVector3("s_view", cameraForward);

        if (_selector.RayCast(out Vector3D<int> place))
        {
            _shader.SetVector3i("glowBlock", place);
            
            if (_toggleMouse.IsLeft())
                _selector.BreakRayCast();

            if (_toggleMouse.IsRight())
                _selector.PlaceRayCast(16);

            if (_toggleG.IsToggle())
                _selector.LampRayCast();
        }
    }

    private Random _random = new Random(1337);
    
    public override unsafe void Draw(double dt)
    {
        _voxelLights.Compute();
        
        GL.Clear((uint) ClearBufferMask.ColorBufferBit);
        GL.ClearColor(0.2f,0.2f,0.2f,1f);
        
        _shader.Use();
        
        _texture.Bind(TextureUnit.Texture0);
        _voxelMap.Bind(TextureUnit.Texture1);
        _voxelLights.Bind(TextureUnit.Texture2);
        

        ScreenBuffer.DrawScreenBuffer();
    }
}