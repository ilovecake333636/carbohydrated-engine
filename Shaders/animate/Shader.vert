#version 460 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec2 aTC2;
layout (location = 3) in vec2 aTC3;

out vec2 texCoord;
out vec2 TC2;
out vec2 TC3;

uniform mat4 model;
uniform mat4 view;

void main()
{
    gl_Position = vec4(aPosition, 1.0f) * model * view;
    texCoord = aTexCoord;
    TC2 = aTC2;
    TC3 = aTC3;
}