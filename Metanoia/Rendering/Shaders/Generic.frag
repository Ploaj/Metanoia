#version 330

in vec3 FragPos;
in vec3 N;
in vec2 UV0;
in vec4 Color;
in float BoneWeight;

uniform mat4 mvp;
uniform sampler2D dif;
uniform int hasDif;
uniform int renderMode;

out vec4 fragColor;

void main()
{
	vec2 TexCoord0 = UV0;

	vec3 diffuseColor = vec3(1, 0, 0);
	float alpha = 1;

	if(hasDif == 1){
		diffuseColor = texture2D(dif, TexCoord0, 0).xyz;
		//alpha = texture2D(dif, TexCoord0, 0).a;
	}

	vec3 lightDir = vec3(0, 0, 1);

	float l = 0.6 + abs(dot(N, lightDir)) * 0.5;

	vec3 displayNormal = vec3(0.5) + (N / 2);

	diffuseColor.xyz *= l;

	if(alpha < 0.8)
		discard;

	if(renderMode == 0)
		fragColor = vec4(diffuseColor, alpha);
	else if (renderMode == 1 && hasDif == 0)
		fragColor = vec4(1, 0, 0, 1);
	else if (renderMode == 1 && hasDif == 1)
		fragColor = texture2D(dif, TexCoord0, 0);
	else if (renderMode == 2)
		fragColor = vec4(displayNormal, 1);
	else if (renderMode == 3)
		fragColor = Color;
	else if (renderMode == 4)
		fragColor = vec4(UV0.x, 0, UV0.y, 1);
	else if (renderMode == 5)
		fragColor = vec4(BoneWeight, 0, 0, 1);
	else if (renderMode == 6)
		fragColor = vec4(1, 1, 1, 1);
}