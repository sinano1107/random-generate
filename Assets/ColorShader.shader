Shader "Unlit/ColorShader"
{
    SubShader
    {
        Cull Off // 両面とも描画する

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 frag (v2f_img i, fixed facing : VFACE) : SV_Target
            {
                return (facing > 0 ) ? fixed4(1, 1, 0, 1) : fixed4(0, 1, 1, 1);
            }

            ENDCG
        }
    }
}