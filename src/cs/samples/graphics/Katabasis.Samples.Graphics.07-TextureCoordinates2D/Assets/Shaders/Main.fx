float2 Scale;
float2 Offset;

Texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
};

struct VertexShaderInput
{
    float2 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4((input.Position.xy * Scale.xy) + Offset.xy, 0.5, 1.0);
    
    // move from model space positions xy in [-1, 1] to texture coordinates uv in [0, 1]
    //                  ( x, y) -> (u,v)
    // top-left:        (-1,+1) -> (0,0)
    // top-right:       (+1,+1) -> (1,0)
    // bottom-right:    (+1,-1) -> (1,1)
    // bottom-left:     (-1,-1) -> (0,1)
    // u = (x+1)/+2
    // v = (y-1)/-2
    // but here we want to demonstrate what happens when you go out the normalized range: [0,1] -> [-0.5,1.5]
    output.TextureCoordinates = (input.Position + 1.0) - 0.5;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 textureColor = tex2D(TextureSampler, input.TextureCoordinates); 
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