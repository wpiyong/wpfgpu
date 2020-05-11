#version 330 core
// abf.psh
// adaptive bilateral filter based on "Adaptive Bilateral Filter for Sharpness Enhancement and Noise Removal"
// based on 7X7 kernel

uniform sampler2D   texture0; //sampler texture0: register( s0 );
uniform sampler2D   texture1; //sampler texture1: register( s1 );
uniform vec4        SrcSampleSteps; //float4 SrcSampleSteps : register( c8 );
uniform vec4        DstSampleSteps; //float4 DstSampleSteps : register( c9 );

uniform vec4 bilateralParams; //: register (c23);
uniform vec4 contrast; //: register (c24);
uniform vec4 channelSelect; //:register(c26);

varying vec2 v_texCoord;
out vec4 FragColor;

void main( )
{
	vec4 c;
	vec4 cn;
    int m;
    int n;
    vec3 sum = vec3(0.0,0.0, 0.0);
    vec3 norm = vec3(0.0,0.0, 0.0);
    float ws = 0.0;
    vec3 wr = vec3(0.0,0.0, 0.0);
    float sigmas = bilateralParams.x;
    vec3 sigmar = vec3(0.0,0.0, 0.0);
    vec3 temp;

	float xStep = SrcSampleSteps.z;
	float yStep = SrcSampleSteps.w;

    vec3 theta = vec3(0.0,0.0, 0.0);
    float offset = 0.60055065;
    vec3 log = texture2D(texture1, v_texCoord).xyz*2.0*offset - offset;
    
    // adjust the theta
    theta = bilateralParams.z*log;
    theta = clamp(theta, -0.15, 0.15);
    
    // adjust the sigmar
    if(abs(log.x)>30.0/255.0)
    {
		sigmar.x = 5.0/255.0;
    }
    if(abs(log.y)>30.0/255.0)
    {
		sigmar.y = 5.0/255.0;
    }
    if(abs(log.z)>30.0/255.0)
    {
		sigmar.z = 5.0/255.0;
    }
    if(abs(log.x)>18.0/255.0&&abs(log.x)<=30.0/255.0)
    {
		sigmar.x = -sign(log.x)*log.x + 35.0/255.0;
    }
    if(abs(log.y)>18.0/255.0&&abs(log.y)<=30.0/255.0)
    {
		sigmar.y = -sign(log.y)*log.y + 35.0/255.0;
    }
    if(abs(log.z)>18.0/255.0&&abs(log.z)<=30.0/255.0)
    {
		sigmar.z = -sign(log.z)*log.z + 35.0/255.0;
    }
    if(abs(log.x)>=0.0&&abs(log.x)<=18.0/255.0)
    {
		sigmar.x = sign(log.x)*5.0/9.0*log.x + 7.0/255.0;
    }
    if(abs(log.y)>=0.0&&abs(log.y)<=18.0/255.0)
    {
		sigmar.y = sign(log.y)*5.0/9.0*log.y + 7.0/255.0;
    }
    if(abs(log.z)>=0.0&&abs(log.z)<=18.0/255.0)
    {
		sigmar.z = sign(log.z)*5.0/9.0*log.z + 7.0/255.0;
    }
    if(bilateralParams.w>0.5)
    {
		float tempSigmar = bilateralParams.y/255.0;
		sigmar = vec3(tempSigmar, tempSigmar, tempSigmar);
	}
    c = texture2D(texture0, v_texCoord);
    for(m=-3; m<4; m++)
    {
		for(n=-3; n<4; n++)
		{
			cn = texture2D(texture0, v_texCoord + vec2(m*xStep, n*yStep));
			ws = exp(-0.5*(m*m + n*n)/(sigmas*sigmas));
			wr = exp(-0.5*(c.rgb-cn.rgb-theta)*(c.rgb-cn.rgb-theta)/(sigmar*sigmar));
			temp = ws*wr;
			sum = sum + temp*cn.rgb;
			norm = norm + temp;
		}
    }
    vec4 abf;
    abf = vec4(sum / norm, 1.0)-vec4(theta*contrast.xxx, 0.0);
    abf = clamp(abf, 0.0, 1.0);
	FragColor = abf*channelSelect+c*(vec4(1,1,1,1)-channelSelect);
}
