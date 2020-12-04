float4x4 WorldViewProjectionMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float Brightness : TEXCOORD0; // not really texture coordinates
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float Brightness : TEXCOORD0; // not really texture coordinates
};

struct PixelShaderOutput
{
    float4 Color0 : COLOR0;
    float4 Color1 : COLOR1;
    float4 Color2 : COLOR2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjectionMatrix);
    output.Brightness = input.Brightness;

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;

    output.Color0 = float4(input.Brightness, 0.0, 0.0, 1.0);
    output.Color1 = float4(0.0, input.Brightness, 0.0, 1.0);
    output.Color2 = float4(0.0, 0.0, input.Brightness, 1.0);

    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}