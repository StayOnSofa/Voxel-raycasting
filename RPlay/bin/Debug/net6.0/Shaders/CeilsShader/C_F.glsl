#version 330 core

uniform vec3 s_resolution;

out ivec4 FragColor;
in vec4 outCol;

const int maxPower = 32;
const int placedLight = maxPower + 1;
const int palacedBlock = placedLight + 1;

uniform isampler2D uTexture0;

ivec2 voxToTexCoord(ivec3 p) {

    int x = (p.y) % 16;
    int z = (p.y) / 16;

    int px = x * 512;
    int pz = z * 512;

    return ivec2(p.x + px, p.z + pz);
}

ivec3 texCoordToVox(ivec2 texCoord) {
    int x = texCoord.x / 512;
    int z = texCoord.y / 512;
    int y = (z * 16) + x;

    return ivec3(texCoord.x - (x * 512), y, texCoord.y - (z * 512));
}

int GetVoxelID(ivec2 voxel)
{
    return texelFetch(uTexture0, voxel, 0).r;
}

int GetVoxelPower(int oPower, ivec3 voxel)
{
    if (voxel.x >= 0 && voxel.x < 512) {
        if (voxel.z >= 0 && voxel.z < 512) {
            
            int pixelId = texelFetch(uTexture0, voxToTexCoord(voxel), 0).r;

            if (pixelId == placedLight)
                pixelId = maxPower;

            if (pixelId == palacedBlock)
                pixelId = 0;

            return pixelId;
        }
    }
    
    return 0;
}

void main()
{
    ivec2 pixel = ivec2(gl_FragCoord.xy);
    
    ivec3 voxel = texCoordToVox(pixel);
    int pixelID = GetVoxelID(pixel);

    if (voxel.x < 512 && voxel.y < 256 && voxel.z < 512)
    {
        if (pixelID <= maxPower) {
            int lightPower = pixelID-1;
            
            int powerLeft = GetVoxelPower(lightPower, voxel + ivec3(1, 0, 0));
            int powerRight = GetVoxelPower(lightPower, voxel + ivec3(-1, 0, 0));

            int powerUp = GetVoxelPower(lightPower, voxel + ivec3(0, 1, 0));
            int powerDown = GetVoxelPower(lightPower, voxel + ivec3(0, -1, 0));
            
            int powerForward = GetVoxelPower(lightPower, voxel + ivec3(0, 0, 1));
            int powerDownward = GetVoxelPower(lightPower, voxel + ivec3(0, 0, -1));
            
            lightPower = max(lightPower, powerLeft);
            lightPower = max(lightPower, powerRight);
            lightPower = max(lightPower, powerForward);
            lightPower = max(lightPower, powerDownward);
            lightPower = max(lightPower, powerUp);
            lightPower = max(lightPower, powerDown);
            
            pixelID = lightPower - 1;
        }
    }

    FragColor = ivec4(ivec3(pixelID, 0, 0), 1);
}