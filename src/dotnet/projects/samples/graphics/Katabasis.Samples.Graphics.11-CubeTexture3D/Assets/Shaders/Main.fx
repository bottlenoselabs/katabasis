float4x4 WorldViewProjectionMatrix;
float Scale;

Texture3D Texture;
sampler3D TextureSampler = sampler_state
{
    Texture = <Texture>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjectionMatrix);
    // move from model space positions xyz in [-1, 1] to texture coordinates uvw in [0, 1], then scale
    output.TextureCoordinates = (((input.Position.xyz + 1.0) * 0.5) * Scale;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 textureColor = tex3D(TextureSampler, input.TextureCoordinates); 
    return textureColor;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}