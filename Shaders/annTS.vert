#version 460 core
layout (location = 0) in vec4 vertex;
layout (location = 1) in vec4 color;
out vec2 TexCoords;
out vec4 c;

uniform vec4 tc;

void main() {
    bool IsBG = vertex.w<0;
    gl_Position = vec4(vertex.xy, IsBG?-.99999988:-.99999996, 1.0);
    TexCoords = vertex.zw*vec2(1,IsBG?-1:1);
    c = (IsBG?vec4(1.0/255.0,1.0/255.0,1.0/255.0,1.0/255.0):tc)*color;
}