#version 460 core
layout (location = 0) in vec3 start;
layout (location = 1) in vec3 end;

const float[6] v0 = float[](0,1,1,0,0,1); // start/end ig
const float[6] v1 = float[](0.2,0.2,-0.2,-0.2,0.2,-0.2); // up/down ig
uniform mat4 view;
out float t;

void main() {
    float val = v0[gl_VertexID];
    gl_Position = vec4((end*val)+(start*(1.0-val))+vec3(0,v1[gl_VertexID],0),1.0)*view;
    t = val*2;
}