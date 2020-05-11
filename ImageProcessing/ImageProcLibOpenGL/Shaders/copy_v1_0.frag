#version 330 core

// our texture
uniform sampler2D   u_image;
// the texCoords passed in from the vertex shader.
varying vec2 v_texCoord;

void main( )
{
	mediump vec4 c = texture2D( u_image , v_texCoord);

	gl_FragColor = c;
}