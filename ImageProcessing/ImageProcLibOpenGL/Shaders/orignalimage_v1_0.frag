#version 330 core

// our texture
uniform sampler2D   u_image;

uniform vec4        SrcSampleSteps;
uniform vec4        DstSampleSteps;
 
// the texCoords passed in from the vertex shader.
varying vec2 v_texCoord;
out vec4 FragColor;
void main( )
{
	vec4 c = texture2D(u_image,v_texCoord );
    FragColor = c.rgba;
}
