#version 330

struct LightProperties {
		vec4 Position;
		vec3 AmbientLight;
		vec3 DiffuseLight;
		vec3 SpecularLight;
};

uniform LightProperties[3] uLight;

struct MaterialProperties {
		vec3 AmbientReflectivity;
		vec3 DiffuseReflectivity;
		vec3 SpecularReflectivity;
		float Shininess;
};

uniform MaterialProperties uMaterial;

uniform sampler2D uTextureSampler0;
uniform sampler2D uTextureSampler1;
uniform sampler2D uTextureSampler2;
uniform sampler2D uTextureSampler3;
uniform sampler2D uTextureSampler4;
uniform sampler2D uTextureSampler5;

uniform int uTextureChoice;

uniform vec4 uEyePosition;

in vec4 oNormal;
in vec4 oSurfacePosition;
in vec2 oTexCoords;

out vec4 FragColour;

void main()
{
	vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);

	if (uTextureChoice == 0)
	{
		FragColour = texture(uTextureSampler0, oTexCoords);
	}
	else if (uTextureChoice == 1)
	{
		FragColour = texture(uTextureSampler1, oTexCoords);
	}
	else if (uTextureChoice == 2)
	{
		FragColour = texture(uTextureSampler2, oTexCoords);
	}
	else if (uTextureChoice == 3)
	{
		FragColour = texture(uTextureSampler3, oTexCoords);
	}
	else if (uTextureChoice == 4)
	{
		FragColour = texture(uTextureSampler4, oTexCoords);
	}
	else if (uTextureChoice == 5)
	{
		FragColour = texture(uTextureSampler5, oTexCoords);
	}

	for(int i = 0; i < 2; ++i)
	{
		vec4 lightDir = normalize(uLight[i].Position - oSurfacePosition);
		vec4 reflectedVector = reflect(-lightDir, oNormal);

		float diffuseFactor = max(dot(oNormal, lightDir), 0);
		float specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0), uMaterial.Shininess * 128);

		FragColour = FragColour + vec4(uLight[i].AmbientLight *
			uMaterial.AmbientReflectivity + uLight[i].DiffuseLight * uMaterial.DiffuseReflectivity *
			diffuseFactor + uLight[i].SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);
	}
}