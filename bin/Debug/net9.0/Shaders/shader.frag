#version 330 core
out vec4 FragColor;
in vec2 texCoord;

uniform sampler2D texture0;
uniform vec4 tx0;

void main()
{
    FragColor = texture(texture0, texCoord.xy * tx0.zw + tx0.xy);
}