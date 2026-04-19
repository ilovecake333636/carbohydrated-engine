#version 460 core
in float t;
out vec4 FragColor;
void main() {
    FragColor = vec4(2-t,t,0.0,1.0);
}