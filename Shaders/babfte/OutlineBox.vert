#version 460 core
layout (location = 0) in vec3 OLBPS; // OutLine Box Position Scale
layout (location = 1) in vec3 OLBPO; // OutLine Box Position Offset
layout (location = 2) in float OLBA; // OutLine Box Alpha
layout (location = 3) in vec4 OLBC; // Outline Box Color
layout (location = 4) in vec3 OLBS; // OutLine Box Size
// layout (location = 5) in vec4 C0; // Matrix, col 0
// layout (location = 6) in vec4 C1;
// layout (location = 7) in vec4 C2;
layout (location = 5) in vec3 OLBR; // Outline Box Rotation
layout (location = 6) in vec3 OLBP; // Outline Box Position
out vec4 color;

uniform mat4 v;

void main() {
	// (float rx, float ry, float rz) = block.Rot;
	// (float tx, float ty, float tz) = block.Pos;
	// float num = MathF.Cos(rx), num2 = MathF.Sin(rx),
	// num3 = MathF.Cos(ry), num4 = MathF.Sin(ry),
	// num5 = MathF.Cos(rz), num6 = MathF.Sin(rz);
	// float _x2 = num2 * num4, _x3 = num * num4;

	// gl_Position = vec4(OLBPS*OLBS+OLBPO, 1) * mat4(C0,C1,C2,vec4(0,0,0,1)) * v;

	float num = cos(OLBR.x), num2 = sin(OLBR.x),
	num3 = cos(OLBR.y), num4 = sin(OLBR.y),
	num5 = cos(OLBR.z), num6 = sin(OLBR.z);
	gl_Position = vec4(OLBPS*OLBS+OLBPO, 1) * mat4(vec4(num3*num5,num2*num4*num5-num*num6,num*num4*num5+num2*num6,OLBP.x),
	vec4(num3*num6,num2*num4*num6+num*num5,num*num4*num6-num2*num5,OLBP.y),
	vec4(-num4,num2*num3,num*num3,OLBP.z),
	vec4(0,0,0,1)) * v;

	color = vec4(OLBC.r/255,OLBC.g/255,OLBC.b/255,OLBC.a*OLBA/65025);
}