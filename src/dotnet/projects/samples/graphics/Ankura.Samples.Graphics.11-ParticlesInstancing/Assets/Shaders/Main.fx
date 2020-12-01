float4x4 WorldViewProjectionMatrix;

struct VertexShaderInputVertex
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderInputInstance
{
    float4 PositionOffset : POSITION1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInputVertex inputVertex, VertexShaderInputInstance inputInstance)
{
    VertexShaderOutput output;

    output.Position = mul(inputVertex.Position + inputInstance.PositionOffset, WorldViewProjectionMatrix);
    output.Color = inputVertex.Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.Color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}