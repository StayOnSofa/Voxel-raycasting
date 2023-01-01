namespace RPlay;

public class ScreenShader : Shader
{
    private const string Path = "ScreenShader/";
    
    public ScreenShader() : base(Path + "S_V.glsl", Path + "S_F.glsl") { }
}