float2 Offset;

Texture2D Texture0;
sampler2D TextureSampler0 = sampler_state
{
    Texture = <Texture0>;
};

Texture2D Texture1;
sampler2D TextureSampler1 = sampler_state
{
    Texture = <Texture1>;
};

Texture2D Texture2;
sampler2D TextureSampler2 = sampler_state
{
    Texture = <Texture2>;
};

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates0 : TEXCOORD0;
	float2 TextureCoordinates1 : TEXCOORD1;
	float2 TextureCoordinates2 : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    // move from texture coordinates uv in [0, 1], to clip space positions xy in [-1, 1]
    output.Position = float4(input.TextureCoordinates * 2.0 - 1.0, 0.5, 1.0);
    output.TextureCoordinates0 = input.TextureCoordinates + float2(Offset.x, 0.0);
    output.TextureCoordinates1 = input.TextureCoordinates + float2(0.0, Offset.y);
    output.TextureCoordinates2 = input.TextureCoordinates;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 color0 = tex2D(TextureSampler0, input.TextureCoordinates0).xyz;
    float3 color1 = tex2D(TextureSampler1, input.TextureCoordinates1).xyz;
    float3 color2 = tex2D(TextureSampler2, input.TextureCoordinates2).xyz;
    float4 finalColor = float4(color0 + color1 + color2, 1.0);
    
    return finalColor;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}