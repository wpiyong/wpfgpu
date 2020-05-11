#version 330 core
// this shader doing Laplacian of Gaussian based on 7X7 kernel

uniform sampler2D   texture0; //sampler texture0: register( s0 );
uniform vec4        SrcSampleSteps; //float4 SrcSampleSteps : register( c8 );
uniform vec4        DstSampleSteps; //float4 DstSampleSteps : register( c9 );
uniform vec4		channelSelect;  //float4 channelSelect:register(c26);

varying vec2 v_texCoord;
out vec4 FragColor;

void main()
{
    vec4 c;
    vec2 xStep = vec2(SrcSampleSteps.z,0);
	vec2 yStep = vec2(0,SrcSampleSteps.w);
    float kernel0[4]={-0.21513583, -0.082116067, 0.027181845, 0.0143398680};
    float kernel1[4]={-0.082116067, -0.014237632, 0.029361689, 0.010927679};
    float kernel2[4]={0.027181845, 0.029361689, 0.018382898, 0.0044153268};
    float kernel3[4]={0.014339868, 0.010927679, 0.0044153268, 0.00082364870};
	int ii, jj;
	vec4 logImage=vec4(0.0,0.0, 0.0, 0.0);
	float offset = 0.60055065;
	vec4 cImage = texture2D(texture0, v_texCoord);
	for( ii = -3; ii < 4; ii++)
	{
		for( jj = -3; jj < 4; jj++)
		{
			if( int( abs( ii )) == 0 )
			{
				logImage+=texture2D(texture0, v_texCoord+xStep*ii+yStep*jj)*kernel0[int(abs(jj))];
			}
			else if( int( abs( ii )) == 1 )
			{
				logImage+=texture2D(texture0, v_texCoord+xStep*ii+yStep*jj)*kernel1[int(abs(jj))];
			}
			else if( int( abs( ii )) == 2 )
			{
				logImage+=texture2D(texture0, v_texCoord+xStep*ii+yStep*jj)*kernel2[int(abs(jj))];
			}
			else if( int( abs( ii )) == 3 )
			{
				logImage+=texture2D(texture0, v_texCoord+xStep*ii+yStep*jj)*kernel3[int(abs(jj))];
			}
		}
	}
	c.rgb = (logImage.rgb + offset)/(2.0*offset);
	c = vec4(c.rgb, 1.0)*channelSelect+cImage*(vec4(1.0,1.0,1.0,1.0)-channelSelect);
	FragColor = vec4(c.rgb,1.0);
}

