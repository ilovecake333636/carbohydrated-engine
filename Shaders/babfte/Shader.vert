#version 460 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aCl1;
layout (location = 3) in vec4 aCl2;
layout (location = 4) in vec4 aCl3;

out vec2 texCoord;
out vec3 color;

uniform mat4 view;

void main()
{
	mat4 model = mat4(aCl1, aCl2, aCl3, vec4(0.0,0.0,0.0,1.0));
    gl_Position = vec4(aPos, 1.0) * model * view;
    texCoord = aTexCoord;
    color = vec3(0.2,1.0,0.0);
}