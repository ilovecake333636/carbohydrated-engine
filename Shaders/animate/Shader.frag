#version 460 core
out vec4 FragColor;
in vec2 texCoord;
in vec2 TC2;
in vec2 TC3;

uniform sampler2D texture0;

void main()
{
    FragColor = texture(texture0, mod(mod(texCoord,vec2(1,1))+vec2(1,1),vec2(1,1))*TC2+TC3);
}