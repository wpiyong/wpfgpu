#version 330 core

attribute vec2 a_position;
attribute vec2 a_texCoord;

uniform float u_yflip;

varying vec2 v_texCoord;

void main() 
{
    gl_Position = vec4(a_position * vec2(1, u_yflip) , 0, 1);
    v_texCoord = a_texCoord;
}