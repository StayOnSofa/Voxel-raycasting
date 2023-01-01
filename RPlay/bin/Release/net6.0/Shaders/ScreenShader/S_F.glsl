#version 330 core
uniform vec3 s_resolution;

out vec4 FragColor;
in vec4 outCol;

uniform isampler2D uTexture0;

const int maxPower = 32;
const int placedLight = maxPower + 1;
const int palacedBlock = placedLight + 1;

ivec2 voxToTexCoord(ivec3 p) {

    int x = (p.y) % 16;
    int z = (p.y) / 16;

    int px = x * 512;
    int pz = z * 512;

    return ivec2(p.x + px, p.z + pz);
}

void main()
{
    vec2 uv = vec2(gl_FragCoord.xy / s_resolution.xy);
    ivec2 getPixel = ivec2(uv * 512);
    
    int color = texelFetch(uTexture0, voxToTexCoord(ivec3(getPixel.x, 0,getPixel.y)), 0).r;
    
    vec3 realColor = vec3(color / float(maxPower));

    if (color == placedLight)
        realColor = vec3(0.99, 0.92, 0);

    if (color == palacedBlock)
        realColor = vec3(0.5, 0, 0);
    
    FragColor = vec4(realColor, 1.);
}