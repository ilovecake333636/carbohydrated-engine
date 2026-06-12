#version 460 core
out vec4 FragColor;
in vec2 texCoord;
in vec4 color;

uniform sampler2D texture0;

void main() {
    vec4 txClr = texture(texture0, texCoord);
    float a = txClr.a-txClr.a*color.a+color.a;
    FragColor = vec4((txClr.a*(txClr.rgb-color.rgb*color.a)+color.rgb*color.a)/a, a);
}