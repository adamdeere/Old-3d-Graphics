#version 330

//The in keyword signifies that this variable will be provided as a per vertex attribute,
//and should come from the previous stage in the pipeline, in our case it's a vec3 
//type (3D floating point vector).
in vec3 vPosition;

in vec3 vColour;
out vec4 oColour;

void main()
{
	//gl_Position is a built in output variable of type vec4 (4d floating point vector)
	//x and y values come from vPosition, z is set to 0 and w is set to 1 (w comes from the
	//fact we are working in a homogenous 4 dimensional space) - No idea
	gl_Position = vec4(vPosition, 1);
	//This gets passed in to the fragment shader (fSimple.frag)

	oColour = vec4(vColour, 1);
}
