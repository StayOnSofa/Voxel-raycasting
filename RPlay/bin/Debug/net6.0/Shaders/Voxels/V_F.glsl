#version 440 core

out vec4 FragColor;

uniform sampler2D uTexture0;
uniform isampler3D uTexture1;

uniform float s_time;
uniform vec3 s_resolution;

void main() {
    vec2 pixel = gl_FragCoord.xy;

    float lerp = 0.1;
    int noise = texelFetch(uTexture1, ivec3(pixel.x, lerp * 128, pixel.y), 0).r;

    if (gl_FragCoord.x < 512)
    {
        FragColor = vec4(vec3(noise), 1);   
    }
    else
    {
        FragColor = texture(uTexture0, gl_FragCoord.xy / 1000);
    }
}