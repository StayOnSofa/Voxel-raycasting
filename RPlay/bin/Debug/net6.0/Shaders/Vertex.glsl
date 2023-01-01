#version 440 core
layout (location = 0) in vec3 vPos;

out vec4 outCol;

void main()
{
    gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
}