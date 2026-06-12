#version 460 core
in vec2 TexCoords;
in vec4 c;
out vec4 color;

uniform sampler2D text;

void main() {
    color = vec4(c.rgb, c.a*texture(text, TexCoords).r);
}