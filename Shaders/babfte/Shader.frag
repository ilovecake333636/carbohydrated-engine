#version 460 core
out vec4 FragColor;
in vec2 texCoord;
in vec4 color;

uniform sampler2D texture0;

void main()
{
    //FragColor = texture(texture0, texCoord);
    FragColor = color;
}