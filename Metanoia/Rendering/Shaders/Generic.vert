#version 330

in vec3 pos;
in vec3 nrm;
in vec2 uv0;
in vec4 clr;
in vec4 bone;
in vec4 weight;

uniform mat4 mvp;
uniform int selectedBone;

out vec3 FragPos;
out vec3 N;
out vec2 UV0;
out vec4 Color;
out float BoneWeight;

void main()
{
	N = nrm;//normalize((inverse(transpose(mvp)) * vec4(nrm, 1)).xyz);
	UV0 = uv0;
	FragPos = (mvp * vec4(pos, 1)).xyz;
	Color = clr;
	
	if(bone.x == selectedBone)
		BoneWeight = weight.x;
	if(bone.y == selectedBone)
		BoneWeight = weight.y;
	if(bone.z == selectedBone)
		BoneWeight = weight.z;
	if(bone.w == selectedBone)
		BoneWeight = weight.w;
	//if(weight.x == 0 && weight.y == 0 && weight.z == 0 && weight.w == 0)
	//	BoneWeight = 1;

	gl_Position = mvp * vec4(pos, 1);
}