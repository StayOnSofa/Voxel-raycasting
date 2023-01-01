#version 440 core

#define VOXEL_DISTANCE 384
#define RENDER_DISNTACE 256

const float threshold = 0.5;
const float pi = 3.14;
const float fov = 1.9;

out vec4 FragColor;
in vec4 outCol;

uniform float s_time;
uniform vec3 s_resolution;
uniform vec3 o_camera;
uniform vec3 s_view;

uniform sampler2D uTexture0;
uniform isampler3D uTexture1;
uniform isampler2D uTexture2;

uniform ivec3 glowBlock;

float SqF(float x) { return x * x; }
float SqV(vec2 x) { return dot(x, x); }

float SqiF(float x) { return 1.0 - SqF(1.0 - x); }
float SqiV(vec2 x) { return 1.0 - SqV(1.0 - x); }

vec3 sun_dir;

struct TraceResult
{
    int voxelId;
    vec3 vp;
    vec3 p;
    vec3 n;
    float r;
    bool hit;
    vec3 color;
    vec2 uv;
};

vec3 background(vec3 d)
{
    const float sun_intensity = 1.0;
    vec3 sun = (pow(max(0.0, dot(d, sun_dir)), 48.0) + pow(max(0.0, dot(d, sun_dir)), 4.0) * 0.25) * sun_intensity * vec3(1.0, 0.85, 0.5);
    vec3 sky = mix(vec3(0.6, 0.65, 0.8), vec3(0.15, 0.25, 0.65), d.y) * 1.15;
    return sun + sky;
}

int voxel(vec3 vp)
{
    ivec3 voxel = ivec3(vp);
    return texelFetch(uTexture1, voxel, 0).r;
}

bool voxelB(vec3 vp)
{
    ivec3 voxel = ivec3(vp);
    return texelFetch(uTexture1, voxel, 0).r > 0;
}

TraceResult rayCast(vec3 rayPos, vec3 rayDir) {

    TraceResult r;
    
    vec3 mapPos = floor(rayPos);
    vec3 deltaDist = abs(vec3(length(rayDir)) / rayDir);
    vec3 rayStep = sign(rayDir);
    vec3 sideDist = (sign(rayDir) * (mapPos - rayPos) + (sign(rayDir) * 0.5) + 0.5) * deltaDist;
    vec3 mask;
    bool hit = false;

    for (int i = 0; i < VOXEL_DISTANCE; i++) {
        mask = step(sideDist.xyz, sideDist.yzx) * step(sideDist.xyz, sideDist.zxy);
        sideDist += vec3(mask) * deltaDist;
        mapPos += vec3(mask) * rayStep;

        if (mapPos.y > 256 && mapPos.y > 0.0) break;

        int v = voxel(mapPos);

        if (v > 0)
        {
            r.voxelId = v;
            r.vp = mapPos;
            
            hit = true;
            break;
        }

    }
    
    vec3 endRayPos = rayDir / dot(mask * rayDir, vec3(1)) * dot(mask * (mapPos + step(rayDir, vec3(0)) - rayPos), vec3(1)) + rayPos;
    vec2 uv;
    vec3 tangent1;
    vec3 tangent2;
    if (abs(mask.x) > 0.) {
        uv = endRayPos.yz;
        tangent1 = vec3(0,1,0);
        tangent2 = vec3(0,0,1);
    }
    else if (abs(mask.y) > 0.) {
        uv = endRayPos.xz;
        tangent1 = vec3(1,0,0);
        tangent2 = vec3(0,0,1);
    }
    else {
        uv = endRayPos.xy;
        tangent1 = vec3(1,0,0);
        tangent2 = vec3(0,1,0);
    }

    uv = fract(uv);
    
    r.n = -rayStep * mask;
    r.p = endRayPos;
    r.hit = hit;
    r.r = length(rayPos - endRayPos);;
    r.uv = uv;
    
    return r;
}


TraceResult traceVoxel(vec3 p, vec3 d, float dist)
{
    TraceResult r;
    r.hit = false;
    r.n = -d;
    r.r = dist;

    vec3 id = 1.0 / d;
    vec3 sd = sign(d);
    vec3 nd = max(-sd, 0.0);
    vec3 vp = floor(p) - nd * vec3(equal(floor(p), p));

    for (int i = 0; i < VOXEL_DISTANCE; ++i)
    {
        if (dist <= 0.0 || p.y > 256 && d.y > 0.0)
        break;

        int v = voxel(vp);
        
        if (v > 0)
        {
            r.voxelId = v;
            r.vp = vp;
            r.p = p;
            r.r = dist;
            r.hit = true;
            return r;
        }

        vec3 n = mix(floor(p + 1.0), ceil(p - 1.0), nd);
        vec3 ls = (n - p) * id;
        float l = min(min(ls.x, ls.y), ls.z);
        vec3 a = vec3(equal(vec3(l), ls));

        p = mix(p + d * l, n, a);
        vp += sd * a;
        r.n = -sd * a;
        dist -= l;
    }

    return r;
}


float sample_ao(vec3 vp, vec3 p, vec3 n)
{
    const float s = 0.6;
    const float i = 1.0 - s;
    vec3 b = vp + n;
    vec3 e0 = n.zxy;
    vec3 e1 = n.yzx;
    float a = 1.0;
    if (voxelB(b + e0))
    a *= i + s * SqiF(fract(dot(-e0, p)));
    if (voxelB(b - e0))
    a *= i + s * SqiF(fract(dot(e0, p)));
    if (voxelB(b + e1))
    a *= i + s * SqiF(fract(dot(-e1, p)));
    if (voxelB(b - e1))
    a *= i + s * SqiF(fract(dot(e1, p)));
    if (voxelB(b + e0 + e1))
    a = min(a, i + s * SqiF(min(1.0, length(fract((-e0 - e1) * p)))));
    if (voxelB(b + e0 - e1))
    a = min(a, i + s * SqiF(min(1.0, length(fract((-e0 + e1) * p)))));
    if (voxelB(b - e0 + e1))
    a = min(a, i + s * SqiF(min(1.0, length(fract((e0 - e1) * p)))));
    if (voxelB(b - e0 - e1))
    a = min(a, i + s * SqiF(min(1.0, length(fract((e0 + e1) * p)))));
    return a;
}

ivec2 voxToTexCoord(ivec3 p) {

    int x = (p.y) % 16;
    int z = (p.y) / 16;

    int px = x * 512;
    int pz = z * 512;

    return ivec2(p.x + px, p.z + pz);
}

int GetLightPowerNormalize(ivec3 vp)
{
    int light = texelFetch(uTexture2, voxToTexCoord(vp), 0).r;
    
    if (light == 34)
        light = 0;
    
    return light;
}

vec3 GetLightPower(ivec3 vp)
{
    int top = GetLightPowerNormalize(vp + ivec3(0,1,0));
    int down = GetLightPowerNormalize(vp + ivec3(0,-1,0));

    int left = GetLightPowerNormalize(vp + ivec3(-1,0,0));
    int right = GetLightPowerNormalize(vp + ivec3(1,0,0));

    int forward = GetLightPowerNormalize(vp + ivec3(0,0,1));
    int backward = GetLightPowerNormalize(vp + ivec3(0,0,-1));

    int lightPower = 0;
    
    lightPower = max(lightPower, top);
    lightPower = max(lightPower, down);
    lightPower = max(lightPower, left);
    lightPower = max(lightPower, right);
    lightPower = max(lightPower, forward);
    lightPower = max(lightPower, backward);

    return vec3(lightPower / float(32));
}

vec3 ray(vec3 p, vec3 d)
{
    const vec3 grass_color = vec3(0.63, 1.0, 0.31);
    const vec3 dirt_color = vec3(0.78, 0.56, 0.4);
    const vec3 ambient_color = vec3(0.5, 0.5, 0.5);
    const vec3 sun_color = vec3(0.5, 0.5, 0.5);

    TraceResult r = rayCast(p, d);
    
    if (r.hit)
    {
        float sun_factor = max(0.0, dot(r.n, sun_dir));

        float fog_factor = min(1.0, SqF(length(r.p - p) / RENDER_DISNTACE));
        vec3 fog_color = background(d);
        float ambient_factor = sample_ao(r.vp, r.p, r.n);
        
        int block = r.voxelId;

        int x = (block-1) % 16;
        int y = (block-1) / 16;
        
        vec3 light = GetLightPower(ivec3(r.vp));
        vec3 diffuse = texelFetch(uTexture0, ivec2(x,y), 0).rgb;
        
        float dayLerp = (cos(s_time*0.1) + 1)/2;
        vec3 sumLights = mix(light, vec3(1,1,1), dayLerp);
        
        if (glowBlock == r.vp)
            diffuse = mix(vec3(0.15, 0.76, 1), diffuse, 0.5);
        
        vec3 c = diffuse * sumLights * (ambient_factor * ambient_color + sun_factor * sun_color);
        return mix(c, fog_color, fog_factor);
    }
    
    return background(d);
}

void main()
{
    vec2 sxz = cos(vec2(0.0, -pi * 0.5) - s_time * 0.1);
    sun_dir = normalize(vec3(sxz.x, 1.1, sxz.y));
    
    vec2 res = vec2(s_resolution);
    
    vec2 uv = fov * (gl_FragCoord.xy - res.xy * 0.5) / res.y;

    vec3 forward = s_view;
    vec3 right = normalize(cross(vec3(0,1,0), forward));
    vec3 up = cross(forward, right);

    vec3 rayPos = o_camera;
    vec3 rayDir = normalize(vec3(forward + uv.y * up + uv.x * right));

    vec3 color = ray(rayPos, rayDir);
    FragColor = vec4(color * gl_FragCoord.z, 1.0);
}