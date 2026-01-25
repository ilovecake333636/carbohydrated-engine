#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in ivec3 aOffset;

out vec2 texCoord;

uniform mat4 view;

void main()
{
	gl_Position = vec4((aPos + vec3(aOffset.x,-aOffset.y,aOffset.z)) * 4, 1.0) * view;
	texCoord = vec2(aTexCoord.x*.015625,aTexCoord.y*.0625+.9375);
}
