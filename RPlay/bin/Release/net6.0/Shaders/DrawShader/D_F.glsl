#version 330 core
#include "hg_sdf.glsl" 

uniform vec3 s_resolution;

out vec4 FragColor;
in vec4 outCol;

uniform vec3 s_mouse;
uniform sampler2D uTexture0;

vec3 GetColorByPixel(vec2 pixel)
{
    if (pixel.x < 0 || pixel.x > s_resolution.x) return vec3(0,0,0);
    if (pixel.y < 0 || pixel.y > s_resolution.y) return vec3(0,0,0);
    
    vec2 uv = vec2(pixel / s_resolution.xy);
    return texture(uTexture0, uv).rgb;
}

void main()
{
    vec2 myPixel = gl_FragCoord.xy;
    vec3 myPixelColor = GetColorByPixel(myPixel);
    
    if (s_mouse.z > 0)
    {
        float x = s_mouse.x;
        float y = s_resolution.y - s_mouse.y;

        if (distance(vec2(x,y), gl_FragCoord.xy) < 10)
        myPixelColor.rgb = vec3(1,0,0);
    }
    
    vec3 topPixel = GetColorByPixel(myPixel + vec2(0,1));
    vec3 downPixel = GetColorByPixel(myPixel + vec2(0,-1));
    

    if (myPixelColor.r < 0.5)
    {
        if (topPixel.r > 0.5)
        {
            myPixelColor = vec3(1,0,0);
        }
    }
    
    FragColor = vec4(myPixelColor, 1.0f);
}