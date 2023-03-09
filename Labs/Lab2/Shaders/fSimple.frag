#version 330

//uniform keyword signifies that this variable is linked from the application, and is not
//a per fragment value
in vec4 oColour;

//The output is simply set to the uniform colour
out vec4 FragColour;

void main()
{
	FragColour = oColour;
}