#version 330

in vec3 pos;
in vec3 nrm;
in vec2 uv0;

uniform mat4 mvp;

out vec3 FragPos;
out vec3 N;
out vec2 UV0;

void main()
{
	N = nrm;//normalize((inverse(transpose(mvp)) * vec4(nrm, 1)).xyz);
	UV0 = uv0;
	FragPos = (mvp * vec4(pos, 1)).xyz;

	gl_Position = mvp * vec4(pos, 1);
}