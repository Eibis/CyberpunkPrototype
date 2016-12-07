Shader "Custom/Lines" 
{
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Color [_Color]
    Pass {}
}
 
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100
    Pass {
        Lighting Off
        ZWrite On
        Cull Back
        SetTexture[_] {
            constantColor [_Color]
            Combine constant
        }
    }
}

}