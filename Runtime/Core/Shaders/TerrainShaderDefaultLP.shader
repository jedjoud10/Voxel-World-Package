// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

Shader "Custom/TerrainShaderDefaultLP" {
    Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }


            // ------------------------------------------------------------
            // Surface shader code generated out of a CGPROGRAM block:


            // ---- forward rendering base pass:
            Pass {
                Name "FORWARD"
                Tags { "LightMode" = "ForwardBase" }

        CGPROGRAM
        // compile directives
        #pragma vertex vert_surf
        #pragma fragment frag_surf
        #pragma target 3.0
        #pragma shader_feature _ _INVERSEMATRIX_EXACT
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile_fwdbase
        #include "HLSLSupport.cginc"
        #define UNITY_INSTANCED_LOD_FADE
        #define UNITY_INSTANCED_SH
        #define UNITY_INSTANCED_LIGHTMAPSTS
        #include "UnityShaderVariables.cginc"
        #include "UnityShaderUtilities.cginc"
        // -------- variant for: <when no other keywords are defined>
        #if !defined(INSTANCING_ON) && !defined(_INVERSEMATRIX_EXACT)
        // Surface shader code generated based on:
        // vertex modifier: 'vert'
        // writes to per-pixel normal: YES
        // writes to emission: no
        // writes to occlusion: no
        // needs world space reflection vector: no
        // needs world space normal vector: YES
        // needs screen space position: no
        // needs world space position: no
        // needs view direction: no
        // needs world space view direction: no
        // needs world space position for lighting: YES
        // needs world space view direction for lighting: YES
        // needs world space view direction for lightmaps: no
        // needs vertex color: YES
        // needs VFACE: no
        // passes tangent-to-world matrix to pixel shader: YES
        // reads from normal: no
        // 1 texcoords actually used
        //   float2 _MainTex
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityPBSLighting.cginc"
        #include "AutoLight.cginc"

        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

        // Original surface shader snippet:
        #line 6
        /* UNITY: Original start of shader */
                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                    //#pragma target 3.0
                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                    sampler2D _MainTex;

                    struct Input {
                        float2 uv_MainTex;
                        float3 cameraRelativeWorldPos;
                        float3 worldNormal;
                        float3 color : COLOR;
                        INTERNAL_DATA
                    };


                    // pass camera relative world position from vertex to fragment
                    void vert(inout appdata_full v, out Input o)
                    {
                        UNITY_INITIALIZE_OUTPUT(Input,o);
                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                    }

                    void surf(Input IN, inout SurfaceOutputStandard o) {



                #if !defined(UNITY_PASS_META)
                        // flat world normal from position derivatives
                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                        // construct world to tangent matrix
                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                        // inverse transform matrix
                        half3x3 w2tRotation;
                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                        half det = dot(worldT.xyz, w2tRotation[0]);

                        w2tRotation *= rcp(det);

                        // apply world to tangent transform to flat world normal
                        o.Normal = mul(w2tRotation, flatWorldNormal);
                        o.Metallic = IN.uv_MainTex.y;
                        o.Smoothness = IN.uv_MainTex.x;
                        o.Albedo = IN.color; // vertex RGB
                #endif
                    }


                    // vertex-to-fragment interpolation data
                    // no lightmaps:
                    #ifndef LIGHTMAP_ON
                    // half-precision fragment shader registers:
                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    #define FOG_COMBINED_WITH_TSPACE
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                      float4 tSpace0 : TEXCOORD1;
                      float4 tSpace1 : TEXCOORD2;
                      float4 tSpace2 : TEXCOORD3;
                      nointerpolation nointerpolation fixed4 color : COLOR0;
                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                      #if UNITY_SHOULD_SAMPLE_SH
                      half3 sh : TEXCOORD5; // SH
                      #endif
                      UNITY_LIGHTING_COORDS(6,7)
                      #if SHADER_TARGET >= 30
                      float4 lmap : TEXCOORD8;
                      #endif
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    // high-precision fragment shader registers:
                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                      float4 tSpace0 : TEXCOORD1;
                      float4 tSpace1 : TEXCOORD2;
                      float4 tSpace2 : TEXCOORD3;
                      nointerpolation fixed4 color : COLOR0;
                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                      #if UNITY_SHOULD_SAMPLE_SH
                      half3 sh : TEXCOORD5; // SH
                      #endif
                      UNITY_FOG_COORDS(6)
                      UNITY_SHADOW_COORDS(7)
                      #if SHADER_TARGET >= 30
                      float4 lmap : TEXCOORD8;
                      #endif
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    #endif
                    // with lightmaps:
                    #ifdef LIGHTMAP_ON
                    // half-precision fragment shader registers:
                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    #define FOG_COMBINED_WITH_TSPACE
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                      float4 tSpace0 : TEXCOORD1;
                      float4 tSpace1 : TEXCOORD2;
                      float4 tSpace2 : TEXCOORD3;
                      nointerpolation fixed4 color : COLOR0;
                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                      float4 lmap : TEXCOORD5;
                      UNITY_LIGHTING_COORDS(6,7)
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    // high-precision fragment shader registers:
                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                    struct v2f_surf {
                      UNITY_POSITION(pos);
                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                      float4 tSpace0 : TEXCOORD1;
                      float4 tSpace1 : TEXCOORD2;
                      float4 tSpace2 : TEXCOORD3;
                      nointerpolation fixed4 color : COLOR0;
                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                      float4 lmap : TEXCOORD5;
                      UNITY_FOG_COORDS(6)
                      UNITY_SHADOW_COORDS(7)
                      UNITY_VERTEX_INPUT_INSTANCE_ID
                      UNITY_VERTEX_OUTPUT_STEREO
                    };
                    #endif
                    #endif
                    float4 _MainTex_ST;

                    // vertex shader
                    v2f_surf vert_surf(appdata_full v) {
                      UNITY_SETUP_INSTANCE_ID(v);
                      v2f_surf o;
                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                      Input customInputData;
                      vert(v, customInputData);
                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                      o.pos = UnityObjectToClipPos(v.vertex);
                      o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                      o.color = v.color;
                      #ifdef DYNAMICLIGHTMAP_ON
                      o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                      #endif
                      #ifdef LIGHTMAP_ON
                      o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                      #endif

                      // SH/ambient and vertex lights
                      #ifndef LIGHTMAP_ON
                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                          o.sh = 0;
                          // Approximated illumination from non-important point lights
                          #ifdef VERTEXLIGHT_ON
                            o.sh += Shade4PointLights(
                              unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                              unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                              unity_4LightAtten0, worldPos, worldNormal);
                          #endif
                          o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                        #endif
                      #endif // !LIGHTMAP_ON

                      UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                      #ifdef FOG_COMBINED_WITH_TSPACE
                        UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
                      #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                        UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
                      #else
                        UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                      #endif
                      return o;
                    }

                    // fragment shader
                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                      UNITY_SETUP_INSTANCE_ID(IN);
                    // prepare and unpack data
                    Input surfIN;
                    #ifdef FOG_COMBINED_WITH_TSPACE
                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                    #else
                      UNITY_EXTRACT_FOG(IN);
                    #endif
                    #ifdef FOG_COMBINED_WITH_TSPACE
                      UNITY_RECONSTRUCT_TBN(IN);
                    #else
                      UNITY_EXTRACT_TBN(IN);
                    #endif
                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                    surfIN.uv_MainTex.x = 1.0;
                    surfIN.cameraRelativeWorldPos.x = 1.0;
                    surfIN.worldNormal.x = 1.0;
                    surfIN.color.x = 1.0;
                    surfIN.uv_MainTex = IN.pack0.xy;
                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                    #ifndef USING_DIRECTIONAL_LIGHT
                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                    #else
                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                    #endif
                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    surfIN.worldNormal = 0.0;
                    surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                    surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                    surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                    surfIN.color = IN.color;
                    #ifdef UNITY_COMPILER_HLSL
                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                    #else
                    SurfaceOutputStandard o;
                    #endif
                    o.Albedo = 0.0;
                    o.Emission = 0.0;
                    o.Alpha = 0.0;
                    o.Occlusion = 1.0;
                    fixed3 normalWorldVertex = fixed3(0,0,1);
                    o.Normal = fixed3(0,0,1);

                    // call surface function
                    surf(surfIN, o);

                    // compute lighting & shadowing factor
                    UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                    fixed4 c = 0;
                    float3 worldN;
                    worldN.x = dot(_unity_tbn_0, o.Normal);
                    worldN.y = dot(_unity_tbn_1, o.Normal);
                    worldN.z = dot(_unity_tbn_2, o.Normal);
                    worldN = normalize(worldN);
                    o.Normal = worldN;

                    // Setup lighting environment
                    UnityGI gi;
                    UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                    gi.indirect.diffuse = 0;
                    gi.indirect.specular = 0;
                    gi.light.color = _LightColor0.rgb;
                    gi.light.dir = lightDir;
                    // Call GI (lightmaps/SH/reflections) lighting function
                    UnityGIInput giInput;
                    UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                    giInput.light = gi.light;
                    giInput.worldPos = worldPos;
                    giInput.worldViewDir = worldViewDir;
                    giInput.atten = atten;
                    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                      giInput.lightmapUV = IN.lmap;
                    #else
                      giInput.lightmapUV = 0.0;
                    #endif
                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                      giInput.ambient = IN.sh;
                    #else
                      giInput.ambient.rgb = 0.0;
                    #endif
                    giInput.probeHDR[0] = unity_SpecCube0_HDR;
                    giInput.probeHDR[1] = unity_SpecCube1_HDR;
                    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                      giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                    #endif
                    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                      giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                      giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                      giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                      giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                      giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                    #endif
                    LightingStandard_GI(o, giInput, gi);

                    // realtime lighting: call lighting function
                    c += LightingStandard(o, worldViewDir, gi);
                    UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                    UNITY_OPAQUE_ALPHA(c.a);
                    return c;
                  }


                  #endif

                        // -------- variant for: INSTANCING_ON 
                        #if defined(INSTANCING_ON) && !defined(_INVERSEMATRIX_EXACT)
                        // Surface shader code generated based on:
                        // vertex modifier: 'vert'
                        // writes to per-pixel normal: YES
                        // writes to emission: no
                        // writes to occlusion: no
                        // needs world space reflection vector: no
                        // needs world space normal vector: YES
                        // needs screen space position: no
                        // needs world space position: no
                        // needs view direction: no
                        // needs world space view direction: no
                        // needs world space position for lighting: YES
                        // needs world space view direction for lighting: YES
                        // needs world space view direction for lightmaps: no
                        // needs vertex color: YES
                        // needs VFACE: no
                        // passes tangent-to-world matrix to pixel shader: YES
                        // reads from normal: no
                        // 1 texcoords actually used
                        //   float2 _MainTex
                        #include "UnityCG.cginc"
                        #include "Lighting.cginc"
                        #include "UnityPBSLighting.cginc"
                        #include "AutoLight.cginc"

                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                        // Original surface shader snippet:
                        #line 6
                        /* UNITY: Original start of shader */
                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                    //#pragma target 3.0
                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                    sampler2D _MainTex;

                                    struct Input {
                                        float2 uv_MainTex;
                                        float3 cameraRelativeWorldPos;
                                        float3 worldNormal;
                                        float3 color : COLOR;
                                        INTERNAL_DATA
                                    };


                                    // pass camera relative world position from vertex to fragment
                                    void vert(inout appdata_full v, out Input o)
                                    {
                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                    }

                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                #if !defined(UNITY_PASS_META)
                                        // flat world normal from position derivatives
                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                        // construct world to tangent matrix
                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                        // inverse transform matrix
                                        half3x3 w2tRotation;
                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                        w2tRotation *= rcp(det);

                                        // apply world to tangent transform to flat world normal
                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                        o.Metallic = IN.uv_MainTex.y;
                                        o.Smoothness = IN.uv_MainTex.x;
                                        o.Albedo = IN.color; // vertex RGB
                                #endif
                                    }


                                    // vertex-to-fragment interpolation data
                                    // no lightmaps:
                                    #ifndef LIGHTMAP_ON
                                    // half-precision fragment shader registers:
                                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                    #define FOG_COMBINED_WITH_TSPACE
                                    struct v2f_surf {
                                      UNITY_POSITION(pos);
                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                      float4 tSpace0 : TEXCOORD1;
                                      float4 tSpace1 : TEXCOORD2;
                                      float4 tSpace2 : TEXCOORD3;
                                      nointerpolation fixed4 color : COLOR0;
                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                      #if UNITY_SHOULD_SAMPLE_SH
                                      half3 sh : TEXCOORD5; // SH
                                      #endif
                                      UNITY_LIGHTING_COORDS(6,7)
                                      #if SHADER_TARGET >= 30
                                      float4 lmap : TEXCOORD8;
                                      #endif
                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                      UNITY_VERTEX_OUTPUT_STEREO
                                    };
                                    #endif
                                    // high-precision fragment shader registers:
                                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                    struct v2f_surf {
                                      UNITY_POSITION(pos);
                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                      float4 tSpace0 : TEXCOORD1;
                                      float4 tSpace1 : TEXCOORD2;
                                      float4 tSpace2 : TEXCOORD3;
                                      nointerpolation fixed4 color : COLOR0;
                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                      #if UNITY_SHOULD_SAMPLE_SH
                                      half3 sh : TEXCOORD5; // SH
                                      #endif
                                      UNITY_FOG_COORDS(6)
                                      UNITY_SHADOW_COORDS(7)
                                      #if SHADER_TARGET >= 30
                                      float4 lmap : TEXCOORD8;
                                      #endif
                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                      UNITY_VERTEX_OUTPUT_STEREO
                                    };
                                    #endif
                                    #endif
                                    // with lightmaps:
                                    #ifdef LIGHTMAP_ON
                                    // half-precision fragment shader registers:
                                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                    #define FOG_COMBINED_WITH_TSPACE
                                    struct v2f_surf {
                                      UNITY_POSITION(pos);
                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                      float4 tSpace0 : TEXCOORD1;
                                      float4 tSpace1 : TEXCOORD2;
                                      float4 tSpace2 : TEXCOORD3;
                                      nointerpolation fixed4 color : COLOR0;
                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                      float4 lmap : TEXCOORD5;
                                      UNITY_LIGHTING_COORDS(6,7)
                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                      UNITY_VERTEX_OUTPUT_STEREO
                                    };
                                    #endif
                                    // high-precision fragment shader registers:
                                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                    struct v2f_surf {
                                      UNITY_POSITION(pos);
                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                      float4 tSpace0 : TEXCOORD1;
                                      float4 tSpace1 : TEXCOORD2;
                                      float4 tSpace2 : TEXCOORD3;
                                      nointerpolation fixed4 color : COLOR0;
                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                      float4 lmap : TEXCOORD5;
                                      UNITY_FOG_COORDS(6)
                                      UNITY_SHADOW_COORDS(7)
                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                      UNITY_VERTEX_OUTPUT_STEREO
                                    };
                                    #endif
                                    #endif
                                    float4 _MainTex_ST;

                                    // vertex shader
                                    v2f_surf vert_surf(appdata_full v) {
                                      UNITY_SETUP_INSTANCE_ID(v);
                                      v2f_surf o;
                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                      Input customInputData;
                                      vert(v, customInputData);
                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                      o.pos = UnityObjectToClipPos(v.vertex);
                                      o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                      o.color = v.color;
                                      #ifdef DYNAMICLIGHTMAP_ON
                                      o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                      #endif
                                      #ifdef LIGHTMAP_ON
                                      o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                      #endif

                                      // SH/ambient and vertex lights
                                      #ifndef LIGHTMAP_ON
                                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                          o.sh = 0;
                                          // Approximated illumination from non-important point lights
                                          #ifdef VERTEXLIGHT_ON
                                            o.sh += Shade4PointLights(
                                              unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                                              unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                                              unity_4LightAtten0, worldPos, worldNormal);
                                          #endif
                                          o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                        #endif
                                      #endif // !LIGHTMAP_ON

                                      UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                                      #ifdef FOG_COMBINED_WITH_TSPACE
                                        UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
                                      #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                        UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
                                      #else
                                        UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                                      #endif
                                      return o;
                                    }

                                    // fragment shader
                                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                      UNITY_SETUP_INSTANCE_ID(IN);
                                    // prepare and unpack data
                                    Input surfIN;
                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                    #else
                                      UNITY_EXTRACT_FOG(IN);
                                    #endif
                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                      UNITY_RECONSTRUCT_TBN(IN);
                                    #else
                                      UNITY_EXTRACT_TBN(IN);
                                    #endif
                                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                    surfIN.uv_MainTex.x = 1.0;
                                    surfIN.cameraRelativeWorldPos.x = 1.0;
                                    surfIN.worldNormal.x = 1.0;
                                    surfIN.color.x = 1.0;
                                    surfIN.uv_MainTex = IN.pack0.xy;
                                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                    #ifndef USING_DIRECTIONAL_LIGHT
                                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                    #else
                                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                    #endif
                                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                    surfIN.worldNormal = 0.0;
                                    surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                    surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                    surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                    surfIN.color = IN.color;
                                    #ifdef UNITY_COMPILER_HLSL
                                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                    #else
                                    SurfaceOutputStandard o;
                                    #endif
                                    o.Albedo = 0.0;
                                    o.Emission = 0.0;
                                    o.Alpha = 0.0;
                                    o.Occlusion = 1.0;
                                    fixed3 normalWorldVertex = fixed3(0,0,1);
                                    o.Normal = fixed3(0,0,1);

                                    // call surface function
                                    surf(surfIN, o);

                                    // compute lighting & shadowing factor
                                    UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                                    fixed4 c = 0;
                                    float3 worldN;
                                    worldN.x = dot(_unity_tbn_0, o.Normal);
                                    worldN.y = dot(_unity_tbn_1, o.Normal);
                                    worldN.z = dot(_unity_tbn_2, o.Normal);
                                    worldN = normalize(worldN);
                                    o.Normal = worldN;

                                    // Setup lighting environment
                                    UnityGI gi;
                                    UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                    gi.indirect.diffuse = 0;
                                    gi.indirect.specular = 0;
                                    gi.light.color = _LightColor0.rgb;
                                    gi.light.dir = lightDir;
                                    // Call GI (lightmaps/SH/reflections) lighting function
                                    UnityGIInput giInput;
                                    UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                    giInput.light = gi.light;
                                    giInput.worldPos = worldPos;
                                    giInput.worldViewDir = worldViewDir;
                                    giInput.atten = atten;
                                    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                      giInput.lightmapUV = IN.lmap;
                                    #else
                                      giInput.lightmapUV = 0.0;
                                    #endif
                                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                      giInput.ambient = IN.sh;
                                    #else
                                      giInput.ambient.rgb = 0.0;
                                    #endif
                                    giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                    giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                      giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                    #endif
                                    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                      giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                      giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                      giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                      giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                      giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                    #endif
                                    LightingStandard_GI(o, giInput, gi);

                                    // realtime lighting: call lighting function
                                    c += LightingStandard(o, worldViewDir, gi);
                                    UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                                    UNITY_OPAQUE_ALPHA(c.a);
                                    return c;
                                  }


                                  #endif

                                        // -------- variant for: _INVERSEMATRIX_EXACT 
                                        #if defined(_INVERSEMATRIX_EXACT) && !defined(INSTANCING_ON)
                                        // Surface shader code generated based on:
                                        // vertex modifier: 'vert'
                                        // writes to per-pixel normal: YES
                                        // writes to emission: no
                                        // writes to occlusion: no
                                        // needs world space reflection vector: no
                                        // needs world space normal vector: YES
                                        // needs screen space position: no
                                        // needs world space position: no
                                        // needs view direction: no
                                        // needs world space view direction: no
                                        // needs world space position for lighting: YES
                                        // needs world space view direction for lighting: YES
                                        // needs world space view direction for lightmaps: no
                                        // needs vertex color: YES
                                        // needs VFACE: no
                                        // passes tangent-to-world matrix to pixel shader: YES
                                        // reads from normal: no
                                        // 1 texcoords actually used
                                        //   float2 _MainTex
                                        #include "UnityCG.cginc"
                                        #include "Lighting.cginc"
                                        #include "UnityPBSLighting.cginc"
                                        #include "AutoLight.cginc"

                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                        // Original surface shader snippet:
                                        #line 6
                                        /* UNITY: Original start of shader */
                                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                    //#pragma target 3.0
                                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                    sampler2D _MainTex;

                                                    struct Input {
                                                        float2 uv_MainTex;
                                                        float3 cameraRelativeWorldPos;
                                                        float3 worldNormal;
                                                        float3 color : COLOR;
                                                        INTERNAL_DATA
                                                    };


                                                    // pass camera relative world position from vertex to fragment
                                                    void vert(inout appdata_full v, out Input o)
                                                    {
                                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                    }

                                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                                #if !defined(UNITY_PASS_META)
                                                        // flat world normal from position derivatives
                                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                        // construct world to tangent matrix
                                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                        // inverse transform matrix
                                                        half3x3 w2tRotation;
                                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                                        w2tRotation *= rcp(det);

                                                        // apply world to tangent transform to flat world normal
                                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                                        o.Metallic = IN.uv_MainTex.y;
                                                        o.Smoothness = IN.uv_MainTex.x;
                                                        o.Albedo = IN.color; // vertex RGB
                                                #endif
                                                    }


                                                    // vertex-to-fragment interpolation data
                                                    // no lightmaps:
                                                    #ifndef LIGHTMAP_ON
                                                    // half-precision fragment shader registers:
                                                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                    #define FOG_COMBINED_WITH_TSPACE
                                                    struct v2f_surf {
                                                      UNITY_POSITION(pos);
                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                      float4 tSpace0 : TEXCOORD1;
                                                      float4 tSpace1 : TEXCOORD2;
                                                      float4 tSpace2 : TEXCOORD3;
                                                      nointerpolation fixed4 color : COLOR0;
                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                      #if UNITY_SHOULD_SAMPLE_SH
                                                      half3 sh : TEXCOORD5; // SH
                                                      #endif
                                                      UNITY_LIGHTING_COORDS(6,7)
                                                      #if SHADER_TARGET >= 30
                                                      float4 lmap : TEXCOORD8;
                                                      #endif
                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                    };
                                                    #endif
                                                    // high-precision fragment shader registers:
                                                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                    struct v2f_surf {
                                                      UNITY_POSITION(pos);
                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                      float4 tSpace0 : TEXCOORD1;
                                                      float4 tSpace1 : TEXCOORD2;
                                                      float4 tSpace2 : TEXCOORD3;
                                                      nointerpolation fixed4 color : COLOR0;
                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                      #if UNITY_SHOULD_SAMPLE_SH
                                                      half3 sh : TEXCOORD5; // SH
                                                      #endif
                                                      UNITY_FOG_COORDS(6)
                                                      UNITY_SHADOW_COORDS(7)
                                                      #if SHADER_TARGET >= 30
                                                      float4 lmap : TEXCOORD8;
                                                      #endif
                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                    };
                                                    #endif
                                                    #endif
                                                    // with lightmaps:
                                                    #ifdef LIGHTMAP_ON
                                                    // half-precision fragment shader registers:
                                                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                    #define FOG_COMBINED_WITH_TSPACE
                                                    struct v2f_surf {
                                                      UNITY_POSITION(pos);
                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                      float4 tSpace0 : TEXCOORD1;
                                                      float4 tSpace1 : TEXCOORD2;
                                                      float4 tSpace2 : TEXCOORD3;
                                                      nointerpolation fixed4 color : COLOR0;
                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                      float4 lmap : TEXCOORD5;
                                                      UNITY_LIGHTING_COORDS(6,7)
                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                    };
                                                    #endif
                                                    // high-precision fragment shader registers:
                                                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                    struct v2f_surf {
                                                      UNITY_POSITION(pos);
                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                      float4 tSpace0 : TEXCOORD1;
                                                      float4 tSpace1 : TEXCOORD2;
                                                      float4 tSpace2 : TEXCOORD3;
                                                      nointerpolation fixed4 color : COLOR0;
                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                      float4 lmap : TEXCOORD5;
                                                      UNITY_FOG_COORDS(6)
                                                      UNITY_SHADOW_COORDS(7)
                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                    };
                                                    #endif
                                                    #endif
                                                    float4 _MainTex_ST;

                                                    // vertex shader
                                                    v2f_surf vert_surf(appdata_full v) {
                                                      UNITY_SETUP_INSTANCE_ID(v);
                                                      v2f_surf o;
                                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                      Input customInputData;
                                                      vert(v, customInputData);
                                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                      o.pos = UnityObjectToClipPos(v.vertex);
                                                      o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                      o.color = v.color;
                                                      #ifdef DYNAMICLIGHTMAP_ON
                                                      o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                      #endif
                                                      #ifdef LIGHTMAP_ON
                                                      o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                      #endif

                                                      // SH/ambient and vertex lights
                                                      #ifndef LIGHTMAP_ON
                                                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                          o.sh = 0;
                                                          // Approximated illumination from non-important point lights
                                                          #ifdef VERTEXLIGHT_ON
                                                            o.sh += Shade4PointLights(
                                                              unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                                                              unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                                                              unity_4LightAtten0, worldPos, worldNormal);
                                                          #endif
                                                          o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                        #endif
                                                      #endif // !LIGHTMAP_ON

                                                      UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                                                      #ifdef FOG_COMBINED_WITH_TSPACE
                                                        UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
                                                      #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                        UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
                                                      #else
                                                        UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                                                      #endif
                                                      return o;
                                                    }

                                                    // fragment shader
                                                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                      UNITY_SETUP_INSTANCE_ID(IN);
                                                    // prepare and unpack data
                                                    Input surfIN;
                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                    #else
                                                      UNITY_EXTRACT_FOG(IN);
                                                    #endif
                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                      UNITY_RECONSTRUCT_TBN(IN);
                                                    #else
                                                      UNITY_EXTRACT_TBN(IN);
                                                    #endif
                                                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                    surfIN.uv_MainTex.x = 1.0;
                                                    surfIN.cameraRelativeWorldPos.x = 1.0;
                                                    surfIN.worldNormal.x = 1.0;
                                                    surfIN.color.x = 1.0;
                                                    surfIN.uv_MainTex = IN.pack0.xy;
                                                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                    #ifndef USING_DIRECTIONAL_LIGHT
                                                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                    #else
                                                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                    #endif
                                                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                    surfIN.worldNormal = 0.0;
                                                    surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                    surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                    surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                    surfIN.color = IN.color;
                                                    #ifdef UNITY_COMPILER_HLSL
                                                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                    #else
                                                    SurfaceOutputStandard o;
                                                    #endif
                                                    o.Albedo = 0.0;
                                                    o.Emission = 0.0;
                                                    o.Alpha = 0.0;
                                                    o.Occlusion = 1.0;
                                                    fixed3 normalWorldVertex = fixed3(0,0,1);
                                                    o.Normal = fixed3(0,0,1);

                                                    // call surface function
                                                    surf(surfIN, o);

                                                    // compute lighting & shadowing factor
                                                    UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                                                    fixed4 c = 0;
                                                    float3 worldN;
                                                    worldN.x = dot(_unity_tbn_0, o.Normal);
                                                    worldN.y = dot(_unity_tbn_1, o.Normal);
                                                    worldN.z = dot(_unity_tbn_2, o.Normal);
                                                    worldN = normalize(worldN);
                                                    o.Normal = worldN;

                                                    // Setup lighting environment
                                                    UnityGI gi;
                                                    UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                    gi.indirect.diffuse = 0;
                                                    gi.indirect.specular = 0;
                                                    gi.light.color = _LightColor0.rgb;
                                                    gi.light.dir = lightDir;
                                                    // Call GI (lightmaps/SH/reflections) lighting function
                                                    UnityGIInput giInput;
                                                    UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                    giInput.light = gi.light;
                                                    giInput.worldPos = worldPos;
                                                    giInput.worldViewDir = worldViewDir;
                                                    giInput.atten = atten;
                                                    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                      giInput.lightmapUV = IN.lmap;
                                                    #else
                                                      giInput.lightmapUV = 0.0;
                                                    #endif
                                                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                      giInput.ambient = IN.sh;
                                                    #else
                                                      giInput.ambient.rgb = 0.0;
                                                    #endif
                                                    giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                    giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                      giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                    #endif
                                                    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                      giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                      giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                      giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                      giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                      giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                    #endif
                                                    LightingStandard_GI(o, giInput, gi);

                                                    // realtime lighting: call lighting function
                                                    c += LightingStandard(o, worldViewDir, gi);
                                                    UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                                                    UNITY_OPAQUE_ALPHA(c.a);
                                                    return c;
                                                  }


                                                  #endif

                                                        // -------- variant for: _INVERSEMATRIX_EXACT INSTANCING_ON 
                                                        #if defined(_INVERSEMATRIX_EXACT) && defined(INSTANCING_ON)
                                                        // Surface shader code generated based on:
                                                        // vertex modifier: 'vert'
                                                        // writes to per-pixel normal: YES
                                                        // writes to emission: no
                                                        // writes to occlusion: no
                                                        // needs world space reflection vector: no
                                                        // needs world space normal vector: YES
                                                        // needs screen space position: no
                                                        // needs world space position: no
                                                        // needs view direction: no
                                                        // needs world space view direction: no
                                                        // needs world space position for lighting: YES
                                                        // needs world space view direction for lighting: YES
                                                        // needs world space view direction for lightmaps: no
                                                        // needs vertex color: YES
                                                        // needs VFACE: no
                                                        // passes tangent-to-world matrix to pixel shader: YES
                                                        // reads from normal: no
                                                        // 1 texcoords actually used
                                                        //   float2 _MainTex
                                                        #include "UnityCG.cginc"
                                                        #include "Lighting.cginc"
                                                        #include "UnityPBSLighting.cginc"
                                                        #include "AutoLight.cginc"

                                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                        // Original surface shader snippet:
                                                        #line 6
                                                        /* UNITY: Original start of shader */
                                                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                    //#pragma target 3.0
                                                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                    sampler2D _MainTex;

                                                                    struct Input {
                                                                        float2 uv_MainTex;
                                                                        float3 cameraRelativeWorldPos;
                                                                        float3 worldNormal;
                                                                        float3 color : COLOR;
                                                                        INTERNAL_DATA
                                                                    };


                                                                    // pass camera relative world position from vertex to fragment
                                                                    void vert(inout appdata_full v, out Input o)
                                                                    {
                                                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                    }

                                                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                #if !defined(UNITY_PASS_META)
                                                                        // flat world normal from position derivatives
                                                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                        // construct world to tangent matrix
                                                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                        // inverse transform matrix
                                                                        half3x3 w2tRotation;
                                                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                                                        w2tRotation *= rcp(det);

                                                                        // apply world to tangent transform to flat world normal
                                                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                        o.Metallic = IN.uv_MainTex.y;
                                                                        o.Smoothness = IN.uv_MainTex.x;
                                                                        o.Albedo = IN.color; // vertex RGB
                                                                #endif
                                                                    }


                                                                    // vertex-to-fragment interpolation data
                                                                    // no lightmaps:
                                                                    #ifndef LIGHTMAP_ON
                                                                    // half-precision fragment shader registers:
                                                                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                                    #define FOG_COMBINED_WITH_TSPACE
                                                                    struct v2f_surf {
                                                                      UNITY_POSITION(pos);
                                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                      float4 tSpace0 : TEXCOORD1;
                                                                      float4 tSpace1 : TEXCOORD2;
                                                                      float4 tSpace2 : TEXCOORD3;
                                                                      nointerpolation fixed4 color : COLOR0;
                                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                      #if UNITY_SHOULD_SAMPLE_SH
                                                                      half3 sh : TEXCOORD5; // SH
                                                                      #endif
                                                                      UNITY_LIGHTING_COORDS(6,7)
                                                                      #if SHADER_TARGET >= 30
                                                                      float4 lmap : TEXCOORD8;
                                                                      #endif
                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                    };
                                                                    #endif
                                                                    // high-precision fragment shader registers:
                                                                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                                    struct v2f_surf {
                                                                      UNITY_POSITION(pos);
                                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                      float4 tSpace0 : TEXCOORD1;
                                                                      float4 tSpace1 : TEXCOORD2;
                                                                      float4 tSpace2 : TEXCOORD3;
                                                                      nointerpolation fixed4 color : COLOR0;
                                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                      #if UNITY_SHOULD_SAMPLE_SH
                                                                      half3 sh : TEXCOORD5; // SH
                                                                      #endif
                                                                      UNITY_FOG_COORDS(6)
                                                                      UNITY_SHADOW_COORDS(7)
                                                                      #if SHADER_TARGET >= 30
                                                                      float4 lmap : TEXCOORD8;
                                                                      #endif
                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                    };
                                                                    #endif
                                                                    #endif
                                                                    // with lightmaps:
                                                                    #ifdef LIGHTMAP_ON
                                                                    // half-precision fragment shader registers:
                                                                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                                    #define FOG_COMBINED_WITH_TSPACE
                                                                    struct v2f_surf {
                                                                      UNITY_POSITION(pos);
                                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                      float4 tSpace0 : TEXCOORD1;
                                                                      float4 tSpace1 : TEXCOORD2;
                                                                      float4 tSpace2 : TEXCOORD3;
                                                                      nointerpolation fixed4 color : COLOR0;
                                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                      float4 lmap : TEXCOORD5;
                                                                      UNITY_LIGHTING_COORDS(6,7)
                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                    };
                                                                    #endif
                                                                    // high-precision fragment shader registers:
                                                                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                                                                    struct v2f_surf {
                                                                      UNITY_POSITION(pos);
                                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                      float4 tSpace0 : TEXCOORD1;
                                                                      float4 tSpace1 : TEXCOORD2;
                                                                      float4 tSpace2 : TEXCOORD3;
                                                                      nointerpolation fixed4 color : COLOR0;
                                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                      float4 lmap : TEXCOORD5;
                                                                      UNITY_FOG_COORDS(6)
                                                                      UNITY_SHADOW_COORDS(7)
                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                    };
                                                                    #endif
                                                                    #endif
                                                                    float4 _MainTex_ST;

                                                                    // vertex shader
                                                                    v2f_surf vert_surf(appdata_full v) {
                                                                      UNITY_SETUP_INSTANCE_ID(v);
                                                                      v2f_surf o;
                                                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                      Input customInputData;
                                                                      vert(v, customInputData);
                                                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                      o.pos = UnityObjectToClipPos(v.vertex);
                                                                      o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                      o.color = v.color;
                                                                      #ifdef DYNAMICLIGHTMAP_ON
                                                                      o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                                      #endif
                                                                      #ifdef LIGHTMAP_ON
                                                                      o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                      #endif

                                                                      // SH/ambient and vertex lights
                                                                      #ifndef LIGHTMAP_ON
                                                                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                          o.sh = 0;
                                                                          // Approximated illumination from non-important point lights
                                                                          #ifdef VERTEXLIGHT_ON
                                                                            o.sh += Shade4PointLights(
                                                                              unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                                                                              unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                                                                              unity_4LightAtten0, worldPos, worldNormal);
                                                                          #endif
                                                                          o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                                        #endif
                                                                      #endif // !LIGHTMAP_ON

                                                                      UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                                                                      #ifdef FOG_COMBINED_WITH_TSPACE
                                                                        UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
                                                                      #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                        UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
                                                                      #else
                                                                        UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                                                                      #endif
                                                                      return o;
                                                                    }

                                                                    // fragment shader
                                                                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                      UNITY_SETUP_INSTANCE_ID(IN);
                                                                    // prepare and unpack data
                                                                    Input surfIN;
                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                    #else
                                                                      UNITY_EXTRACT_FOG(IN);
                                                                    #endif
                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                      UNITY_RECONSTRUCT_TBN(IN);
                                                                    #else
                                                                      UNITY_EXTRACT_TBN(IN);
                                                                    #endif
                                                                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                    surfIN.uv_MainTex.x = 1.0;
                                                                    surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                    surfIN.worldNormal.x = 1.0;
                                                                    surfIN.color.x = 1.0;
                                                                    surfIN.uv_MainTex = IN.pack0.xy;
                                                                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                    #ifndef USING_DIRECTIONAL_LIGHT
                                                                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                    #else
                                                                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                    #endif
                                                                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                    surfIN.worldNormal = 0.0;
                                                                    surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                    surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                    surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                    surfIN.color = IN.color;
                                                                    #ifdef UNITY_COMPILER_HLSL
                                                                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                    #else
                                                                    SurfaceOutputStandard o;
                                                                    #endif
                                                                    o.Albedo = 0.0;
                                                                    o.Emission = 0.0;
                                                                    o.Alpha = 0.0;
                                                                    o.Occlusion = 1.0;
                                                                    fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                    o.Normal = fixed3(0,0,1);

                                                                    // call surface function
                                                                    surf(surfIN, o);

                                                                    // compute lighting & shadowing factor
                                                                    UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                                                                    fixed4 c = 0;
                                                                    float3 worldN;
                                                                    worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                    worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                    worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                    worldN = normalize(worldN);
                                                                    o.Normal = worldN;

                                                                    // Setup lighting environment
                                                                    UnityGI gi;
                                                                    UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                    gi.indirect.diffuse = 0;
                                                                    gi.indirect.specular = 0;
                                                                    gi.light.color = _LightColor0.rgb;
                                                                    gi.light.dir = lightDir;
                                                                    // Call GI (lightmaps/SH/reflections) lighting function
                                                                    UnityGIInput giInput;
                                                                    UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                                    giInput.light = gi.light;
                                                                    giInput.worldPos = worldPos;
                                                                    giInput.worldViewDir = worldViewDir;
                                                                    giInput.atten = atten;
                                                                    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                                      giInput.lightmapUV = IN.lmap;
                                                                    #else
                                                                      giInput.lightmapUV = 0.0;
                                                                    #endif
                                                                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                      giInput.ambient = IN.sh;
                                                                    #else
                                                                      giInput.ambient.rgb = 0.0;
                                                                    #endif
                                                                    giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                                    giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                                    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                                      giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                                    #endif
                                                                    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                                      giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                                      giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                                      giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                                      giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                                      giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                                    #endif
                                                                    LightingStandard_GI(o, giInput, gi);

                                                                    // realtime lighting: call lighting function
                                                                    c += LightingStandard(o, worldViewDir, gi);
                                                                    UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                                                                    UNITY_OPAQUE_ALPHA(c.a);
                                                                    return c;
                                                                  }


                                                                  #endif


                                                                  ENDCG

                                                                  }

        // ---- forward rendering additive lights pass:
        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardAdd" }
            ZWrite Off Blend One One

    CGPROGRAM
                                                                      // compile directives
                                                                      #pragma vertex vert_surf
                                                                      #pragma fragment frag_surf
                                                                      #pragma target 3.0
                                                                      #pragma shader_feature _ _INVERSEMATRIX_EXACT
                                                                      #pragma multi_compile_instancing
                                                                      #pragma multi_compile_fog
                                                                      #pragma skip_variants INSTANCING_ON
                                                                      #pragma multi_compile_fwdadd_fullshadows
                                                                      #include "HLSLSupport.cginc"
                                                                      #define UNITY_INSTANCED_LOD_FADE
                                                                      #define UNITY_INSTANCED_SH
                                                                      #define UNITY_INSTANCED_LIGHTMAPSTS
                                                                      #include "UnityShaderVariables.cginc"
                                                                      #include "UnityShaderUtilities.cginc"
                                                                      // -------- variant for: <when no other keywords are defined>
                                                                      #if !defined(INSTANCING_ON) && !defined(_INVERSEMATRIX_EXACT)
                                                                      // Surface shader code generated based on:
                                                                      // vertex modifier: 'vert'
                                                                      // writes to per-pixel normal: YES
                                                                      // writes to emission: no
                                                                      // writes to occlusion: no
                                                                      // needs world space reflection vector: no
                                                                      // needs world space normal vector: YES
                                                                      // needs screen space position: no
                                                                      // needs world space position: no
                                                                      // needs view direction: no
                                                                      // needs world space view direction: no
                                                                      // needs world space position for lighting: YES
                                                                      // needs world space view direction for lighting: YES
                                                                      // needs world space view direction for lightmaps: no
                                                                      // needs vertex color: YES
                                                                      // needs VFACE: no
                                                                      // passes tangent-to-world matrix to pixel shader: YES
                                                                      // reads from normal: no
                                                                      // 1 texcoords actually used
                                                                      //   float2 _MainTex
                                                                      #include "UnityCG.cginc"
                                                                      #include "Lighting.cginc"
                                                                      #include "UnityPBSLighting.cginc"
                                                                      #include "AutoLight.cginc"

                                                                      #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                      #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                      #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                      // Original surface shader snippet:
                                                                      #line 6
                                                                      /* UNITY: Original start of shader */
                                                                                  //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                  //#pragma target 3.0
                                                                                  //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                  sampler2D _MainTex;

                                                                                  struct Input {
                                                                                      float2 uv_MainTex;
                                                                                      float3 cameraRelativeWorldPos;
                                                                                      float3 worldNormal;
                                                                                      float3 color : COLOR;
                                                                                      INTERNAL_DATA
                                                                                  };


                                                                                  // pass camera relative world position from vertex to fragment
                                                                                  void vert(inout appdata_full v, out Input o)
                                                                                  {
                                                                                      UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                      o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                  }

                                                                                  void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                              #if !defined(UNITY_PASS_META)
                                                                                      // flat world normal from position derivatives
                                                                                      half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                      // construct world to tangent matrix
                                                                                      half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                      half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                      half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                      // inverse transform matrix
                                                                                      half3x3 w2tRotation;
                                                                                      w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                      w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                      w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                      half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                      w2tRotation *= rcp(det);

                                                                                      // apply world to tangent transform to flat world normal
                                                                                      o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                      o.Metallic = IN.uv_MainTex.y;
                                                                                      o.Smoothness = IN.uv_MainTex.x;
                                                                                      o.Albedo = IN.color; // vertex RGB
                                                                              #endif
                                                                                  }


                                                                                  // vertex-to-fragment interpolation data
                                                                                  struct v2f_surf {
                                                                                    UNITY_POSITION(pos);
                                                                                    nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                                    float3 tSpace0 : TEXCOORD1;
                                                                                    float3 tSpace1 : TEXCOORD2;
                                                                                    float3 tSpace2 : TEXCOORD3;
                                                                                    float3 worldPos : TEXCOORD4;
                                                                                    nointerpolation fixed4 color : COLOR0;
                                                                                    float3 custompack0 : TEXCOORD5; // cameraRelativeWorldPos
                                                                                    UNITY_LIGHTING_COORDS(6,7)
                                                                                    UNITY_FOG_COORDS(8)
                                                                                    UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                    UNITY_VERTEX_OUTPUT_STEREO
                                                                                  };
                                                                                  float4 _MainTex_ST;

                                                                                  // vertex shader
                                                                                  v2f_surf vert_surf(appdata_full v) {
                                                                                    UNITY_SETUP_INSTANCE_ID(v);
                                                                                    v2f_surf o;
                                                                                    UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                    UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                    Input customInputData;
                                                                                    vert(v, customInputData);
                                                                                    o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                    o.pos = UnityObjectToClipPos(v.vertex);
                                                                                    o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                                                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                    fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                    o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
                                                                                    o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
                                                                                    o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
                                                                                    o.worldPos.xyz = worldPos;
                                                                                    o.color = v.color;

                                                                                    UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                                                                                    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                                                                                    return o;
                                                                                  }

                                                                                  // fragment shader
                                                                                  fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                                    UNITY_SETUP_INSTANCE_ID(IN);
                                                                                  // prepare and unpack data
                                                                                  Input surfIN;
                                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                  #else
                                                                                    UNITY_EXTRACT_FOG(IN);
                                                                                  #endif
                                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                    UNITY_RECONSTRUCT_TBN(IN);
                                                                                  #else
                                                                                    UNITY_EXTRACT_TBN(IN);
                                                                                  #endif
                                                                                  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                  surfIN.uv_MainTex.x = 1.0;
                                                                                  surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                  surfIN.worldNormal.x = 1.0;
                                                                                  surfIN.color.x = 1.0;
                                                                                  surfIN.uv_MainTex = IN.pack0.xy;
                                                                                  surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                  float3 worldPos = IN.worldPos.xyz;
                                                                                  #ifndef USING_DIRECTIONAL_LIGHT
                                                                                    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                  #else
                                                                                    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                  #endif
                                                                                  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                                  surfIN.worldNormal = 0.0;
                                                                                  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                                  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                                  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                                  surfIN.color = IN.color;
                                                                                  #ifdef UNITY_COMPILER_HLSL
                                                                                  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                  #else
                                                                                  SurfaceOutputStandard o;
                                                                                  #endif
                                                                                  o.Albedo = 0.0;
                                                                                  o.Emission = 0.0;
                                                                                  o.Alpha = 0.0;
                                                                                  o.Occlusion = 1.0;
                                                                                  fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                                  o.Normal = fixed3(0,0,1);

                                                                                  // call surface function
                                                                                  surf(surfIN, o);
                                                                                  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                                                                                  fixed4 c = 0;
                                                                                  float3 worldN;
                                                                                  worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                                  worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                                  worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                                  worldN = normalize(worldN);
                                                                                  o.Normal = worldN;

                                                                                  // Setup lighting environment
                                                                                  UnityGI gi;
                                                                                  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                                  gi.indirect.diffuse = 0;
                                                                                  gi.indirect.specular = 0;
                                                                                  gi.light.color = _LightColor0.rgb;
                                                                                  gi.light.dir = lightDir;
                                                                                  gi.light.color *= atten;
                                                                                  c += LightingStandard(o, worldViewDir, gi);
                                                                                  c.a = 0.0;
                                                                                  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                                                                                  UNITY_OPAQUE_ALPHA(c.a);
                                                                                  return c;
                                                                                }


                                                                                #endif

                                                                                      // -------- variant for: _INVERSEMATRIX_EXACT 
                                                                                      #if defined(_INVERSEMATRIX_EXACT) && !defined(INSTANCING_ON)
                                                                                      // Surface shader code generated based on:
                                                                                      // vertex modifier: 'vert'
                                                                                      // writes to per-pixel normal: YES
                                                                                      // writes to emission: no
                                                                                      // writes to occlusion: no
                                                                                      // needs world space reflection vector: no
                                                                                      // needs world space normal vector: YES
                                                                                      // needs screen space position: no
                                                                                      // needs world space position: no
                                                                                      // needs view direction: no
                                                                                      // needs world space view direction: no
                                                                                      // needs world space position for lighting: YES
                                                                                      // needs world space view direction for lighting: YES
                                                                                      // needs world space view direction for lightmaps: no
                                                                                      // needs vertex color: YES
                                                                                      // needs VFACE: no
                                                                                      // passes tangent-to-world matrix to pixel shader: YES
                                                                                      // reads from normal: no
                                                                                      // 1 texcoords actually used
                                                                                      //   float2 _MainTex
                                                                                      #include "UnityCG.cginc"
                                                                                      #include "Lighting.cginc"
                                                                                      #include "UnityPBSLighting.cginc"
                                                                                      #include "AutoLight.cginc"

                                                                                      #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                      #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                      #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                      // Original surface shader snippet:
                                                                                      #line 6
                                                                                      /* UNITY: Original start of shader */
                                                                                                  //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                  //#pragma target 3.0
                                                                                                  //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                  sampler2D _MainTex;

                                                                                                  struct Input {
                                                                                                      float2 uv_MainTex;
                                                                                                      float3 cameraRelativeWorldPos;
                                                                                                      float3 worldNormal;
                                                                                                      float3 color : COLOR;
                                                                                                      INTERNAL_DATA
                                                                                                  };


                                                                                                  // pass camera relative world position from vertex to fragment
                                                                                                  void vert(inout appdata_full v, out Input o)
                                                                                                  {
                                                                                                      UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                      o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                  }

                                                                                                  void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                              #if !defined(UNITY_PASS_META)
                                                                                                      // flat world normal from position derivatives
                                                                                                      half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                      // construct world to tangent matrix
                                                                                                      half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                      half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                      half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                      // inverse transform matrix
                                                                                                      half3x3 w2tRotation;
                                                                                                      w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                      w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                      w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                      half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                      w2tRotation *= rcp(det);

                                                                                                      // apply world to tangent transform to flat world normal
                                                                                                      o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                      o.Metallic = IN.uv_MainTex.y;
                                                                                                      o.Smoothness = IN.uv_MainTex.x;
                                                                                                      o.Albedo = IN.color; // vertex RGB
                                                                                              #endif
                                                                                                  }


                                                                                                  // vertex-to-fragment interpolation data
                                                                                                  struct v2f_surf {
                                                                                                    UNITY_POSITION(pos);
                                                                                                    nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                                                    float3 tSpace0 : TEXCOORD1;
                                                                                                    float3 tSpace1 : TEXCOORD2;
                                                                                                    float3 tSpace2 : TEXCOORD3;
                                                                                                    float3 worldPos : TEXCOORD4;
                                                                                                    nointerpolation fixed4 color : COLOR0;
                                                                                                    float3 custompack0 : TEXCOORD5; // cameraRelativeWorldPos
                                                                                                    UNITY_LIGHTING_COORDS(6,7)
                                                                                                    UNITY_FOG_COORDS(8)
                                                                                                    UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                    UNITY_VERTEX_OUTPUT_STEREO
                                                                                                  };
                                                                                                  float4 _MainTex_ST;

                                                                                                  // vertex shader
                                                                                                  v2f_surf vert_surf(appdata_full v) {
                                                                                                    UNITY_SETUP_INSTANCE_ID(v);
                                                                                                    v2f_surf o;
                                                                                                    UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                    UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                    Input customInputData;
                                                                                                    vert(v, customInputData);
                                                                                                    o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                    o.pos = UnityObjectToClipPos(v.vertex);
                                                                                                    o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                                                                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                    fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                    o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
                                                                                                    o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
                                                                                                    o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
                                                                                                    o.worldPos.xyz = worldPos;
                                                                                                    o.color = v.color;

                                                                                                    UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
                                                                                                    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                                                                                                    return o;
                                                                                                  }

                                                                                                  // fragment shader
                                                                                                  fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                                                    UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                  // prepare and unpack data
                                                                                                  Input surfIN;
                                                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                  #else
                                                                                                    UNITY_EXTRACT_FOG(IN);
                                                                                                  #endif
                                                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                    UNITY_RECONSTRUCT_TBN(IN);
                                                                                                  #else
                                                                                                    UNITY_EXTRACT_TBN(IN);
                                                                                                  #endif
                                                                                                  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                  surfIN.uv_MainTex.x = 1.0;
                                                                                                  surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                  surfIN.worldNormal.x = 1.0;
                                                                                                  surfIN.color.x = 1.0;
                                                                                                  surfIN.uv_MainTex = IN.pack0.xy;
                                                                                                  surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                  float3 worldPos = IN.worldPos.xyz;
                                                                                                  #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                  #else
                                                                                                    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                  #endif
                                                                                                  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                                                  surfIN.worldNormal = 0.0;
                                                                                                  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                                                  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                                                  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                                                  surfIN.color = IN.color;
                                                                                                  #ifdef UNITY_COMPILER_HLSL
                                                                                                  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                  #else
                                                                                                  SurfaceOutputStandard o;
                                                                                                  #endif
                                                                                                  o.Albedo = 0.0;
                                                                                                  o.Emission = 0.0;
                                                                                                  o.Alpha = 0.0;
                                                                                                  o.Occlusion = 1.0;
                                                                                                  fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                                                  o.Normal = fixed3(0,0,1);

                                                                                                  // call surface function
                                                                                                  surf(surfIN, o);
                                                                                                  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                                                                                                  fixed4 c = 0;
                                                                                                  float3 worldN;
                                                                                                  worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                                                  worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                                                  worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                                                  worldN = normalize(worldN);
                                                                                                  o.Normal = worldN;

                                                                                                  // Setup lighting environment
                                                                                                  UnityGI gi;
                                                                                                  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                                                  gi.indirect.diffuse = 0;
                                                                                                  gi.indirect.specular = 0;
                                                                                                  gi.light.color = _LightColor0.rgb;
                                                                                                  gi.light.dir = lightDir;
                                                                                                  gi.light.color *= atten;
                                                                                                  c += LightingStandard(o, worldViewDir, gi);
                                                                                                  c.a = 0.0;
                                                                                                  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
                                                                                                  UNITY_OPAQUE_ALPHA(c.a);
                                                                                                  return c;
                                                                                                }


                                                                                                #endif


                                                                                                ENDCG

                                                                                                }

                                                                      // ---- deferred shading pass:
                                                                      Pass {
                                                                          Name "DEFERRED"
                                                                          Tags { "LightMode" = "Deferred" }

                                                                  CGPROGRAM
                                                                                                    // compile directives
                                                                                                    #pragma vertex vert_surf
                                                                                                    #pragma fragment frag_surf
                                                                                                    #pragma target 3.0
                                                                                                    #pragma shader_feature _ _INVERSEMATRIX_EXACT
                                                                                                    #pragma multi_compile_instancing
                                                                                                    #pragma exclude_renderers nomrt
                                                                                                    #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
                                                                                                    #pragma multi_compile_prepassfinal
                                                                                                    #include "HLSLSupport.cginc"
                                                                                                    #define UNITY_INSTANCED_LOD_FADE
                                                                                                    #define UNITY_INSTANCED_SH
                                                                                                    #define UNITY_INSTANCED_LIGHTMAPSTS
                                                                                                    #include "UnityShaderVariables.cginc"
                                                                                                    #include "UnityShaderUtilities.cginc"
                                                                                                    // -------- variant for: <when no other keywords are defined>
                                                                                                    #if !defined(INSTANCING_ON) && !defined(_INVERSEMATRIX_EXACT)
                                                                                                    // Surface shader code generated based on:
                                                                                                    // vertex modifier: 'vert'
                                                                                                    // writes to per-pixel normal: YES
                                                                                                    // writes to emission: no
                                                                                                    // writes to occlusion: no
                                                                                                    // needs world space reflection vector: no
                                                                                                    // needs world space normal vector: YES
                                                                                                    // needs screen space position: no
                                                                                                    // needs world space position: no
                                                                                                    // needs view direction: no
                                                                                                    // needs world space view direction: no
                                                                                                    // needs world space position for lighting: YES
                                                                                                    // needs world space view direction for lighting: YES
                                                                                                    // needs world space view direction for lightmaps: no
                                                                                                    // needs vertex color: YES
                                                                                                    // needs VFACE: no
                                                                                                    // passes tangent-to-world matrix to pixel shader: YES
                                                                                                    // reads from normal: no
                                                                                                    // 1 texcoords actually used
                                                                                                    //   float2 _MainTex
                                                                                                    #include "UnityCG.cginc"
                                                                                                    #include "Lighting.cginc"
                                                                                                    #include "UnityPBSLighting.cginc"

                                                                                                    #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                    #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                    #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                    // Original surface shader snippet:
                                                                                                    #line 6
                                                                                                    /* UNITY: Original start of shader */
                                                                                                                //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                //#pragma target 3.0
                                                                                                                //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                sampler2D _MainTex;

                                                                                                                struct Input {
                                                                                                                    float2 uv_MainTex;
                                                                                                                    float3 cameraRelativeWorldPos;
                                                                                                                    float3 worldNormal;
                                                                                                                    float3 color : COLOR;
                                                                                                                    INTERNAL_DATA
                                                                                                                };


                                                                                                                // pass camera relative world position from vertex to fragment
                                                                                                                void vert(inout appdata_full v, out Input o)
                                                                                                                {
                                                                                                                    UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                    o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                }

                                                                                                                void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                            #if !defined(UNITY_PASS_META)
                                                                                                                    // flat world normal from position derivatives
                                                                                                                    half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                    // construct world to tangent matrix
                                                                                                                    half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                    half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                    half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                    // inverse transform matrix
                                                                                                                    half3x3 w2tRotation;
                                                                                                                    w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                    w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                    w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                    half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                    w2tRotation *= rcp(det);

                                                                                                                    // apply world to tangent transform to flat world normal
                                                                                                                    o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                    o.Metallic = IN.uv_MainTex.y;
                                                                                                                    o.Smoothness = IN.uv_MainTex.x;
                                                                                                                    o.Albedo = IN.color; // vertex RGB
                                                                                                            #endif
                                                                                                                }


                                                                                                                // vertex-to-fragment interpolation data
                                                                                                                struct v2f_surf {
                                                                                                                  UNITY_POSITION(pos);
                                                                                                                  nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                                                                  float4 tSpace0 : TEXCOORD1;
                                                                                                                  float4 tSpace1 : TEXCOORD2;
                                                                                                                  float4 tSpace2 : TEXCOORD3;
                                                                                                                  nointerpolation fixed4 color : COLOR0;
                                                                                                                  float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                                                                #ifndef DIRLIGHTMAP_OFF
                                                                                                                  float3 viewDir : TEXCOORD5;
                                                                                                                #endif
                                                                                                                  float4 lmap : TEXCOORD6;
                                                                                                                #ifndef LIGHTMAP_ON
                                                                                                                  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                    half3 sh : TEXCOORD7; // SH
                                                                                                                  #endif
                                                                                                                #else
                                                                                                                  #ifdef DIRLIGHTMAP_OFF
                                                                                                                    float4 lmapFadePos : TEXCOORD7;
                                                                                                                  #endif
                                                                                                                #endif
                                                                                                                  UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                  UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                };
                                                                                                                float4 _MainTex_ST;

                                                                                                                // vertex shader
                                                                                                                v2f_surf vert_surf(appdata_full v) {
                                                                                                                  UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                  v2f_surf o;
                                                                                                                  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                  UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                  Input customInputData;
                                                                                                                  vert(v, customInputData);
                                                                                                                  o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                  o.pos = UnityObjectToClipPos(v.vertex);
                                                                                                                  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                                                                                  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
                                                                                                                  #ifndef DIRLIGHTMAP_OFF
                                                                                                                  o.viewDir.x = dot(viewDirForLight, worldTangent);
                                                                                                                  o.viewDir.y = dot(viewDirForLight, worldBinormal);
                                                                                                                  o.viewDir.z = dot(viewDirForLight, worldNormal);
                                                                                                                  #endif
                                                                                                                  o.color = v.color;
                                                                                                                #ifdef DYNAMICLIGHTMAP_ON
                                                                                                                  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                                                                                #else
                                                                                                                  o.lmap.zw = 0;
                                                                                                                #endif
                                                                                                                #ifdef LIGHTMAP_ON
                                                                                                                  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                  #ifdef DIRLIGHTMAP_OFF
                                                                                                                    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
                                                                                                                    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
                                                                                                                  #endif
                                                                                                                #else
                                                                                                                  o.lmap.xy = 0;
                                                                                                                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                      o.sh = 0;
                                                                                                                      o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                                                                                    #endif
                                                                                                                #endif
                                                                                                                  return o;
                                                                                                                }
                                                                                                                #ifdef LIGHTMAP_ON
                                                                                                                float4 unity_LightmapFade;
                                                                                                                #endif
                                                                                                                fixed4 unity_Ambient;

                                                                                                                // fragment shader
                                                                                                                void frag_surf(v2f_surf IN,
                                                                                                                    out half4 outGBuffer0 : SV_Target0,
                                                                                                                    out half4 outGBuffer1 : SV_Target1,
                                                                                                                    out half4 outGBuffer2 : SV_Target2,
                                                                                                                    out half4 outEmission : SV_Target3
                                                                                                                #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                    , out half4 outShadowMask : SV_Target4
                                                                                                                #endif
                                                                                                                ) {
                                                                                                                  UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                  // prepare and unpack data
                                                                                                                  Input surfIN;
                                                                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                  #else
                                                                                                                    UNITY_EXTRACT_FOG(IN);
                                                                                                                  #endif
                                                                                                                  #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                    UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                  #else
                                                                                                                    UNITY_EXTRACT_TBN(IN);
                                                                                                                  #endif
                                                                                                                  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                  surfIN.uv_MainTex.x = 1.0;
                                                                                                                  surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                  surfIN.worldNormal.x = 1.0;
                                                                                                                  surfIN.color.x = 1.0;
                                                                                                                  surfIN.uv_MainTex = IN.pack0.xy;
                                                                                                                  surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                  #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                  #else
                                                                                                                    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                  #endif
                                                                                                                  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                                                                  surfIN.worldNormal = 0.0;
                                                                                                                  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                                                                  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                                                                  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                                                                  surfIN.color = IN.color;
                                                                                                                  #ifdef UNITY_COMPILER_HLSL
                                                                                                                  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                  #else
                                                                                                                  SurfaceOutputStandard o;
                                                                                                                  #endif
                                                                                                                  o.Albedo = 0.0;
                                                                                                                  o.Emission = 0.0;
                                                                                                                  o.Alpha = 0.0;
                                                                                                                  o.Occlusion = 1.0;
                                                                                                                  fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                                                                  o.Normal = fixed3(0,0,1);

                                                                                                                  // call surface function
                                                                                                                  surf(surfIN, o);
                                                                                                                fixed3 originalNormal = o.Normal;
                                                                                                                  float3 worldN;
                                                                                                                  worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                                                                  worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                                                                  worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                                                                  worldN = normalize(worldN);
                                                                                                                  o.Normal = worldN;
                                                                                                                  half atten = 1;

                                                                                                                  // Setup lighting environment
                                                                                                                  UnityGI gi;
                                                                                                                  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                                                                  gi.indirect.diffuse = 0;
                                                                                                                  gi.indirect.specular = 0;
                                                                                                                  gi.light.color = 0;
                                                                                                                  gi.light.dir = half3(0,1,0);
                                                                                                                  // Call GI (lightmaps/SH/reflections) lighting function
                                                                                                                  UnityGIInput giInput;
                                                                                                                  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                                                                                  giInput.light = gi.light;
                                                                                                                  giInput.worldPos = worldPos;
                                                                                                                  giInput.worldViewDir = worldViewDir;
                                                                                                                  giInput.atten = atten;
                                                                                                                  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                                                                                    giInput.lightmapUV = IN.lmap;
                                                                                                                  #else
                                                                                                                    giInput.lightmapUV = 0.0;
                                                                                                                  #endif
                                                                                                                  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                    giInput.ambient = IN.sh;
                                                                                                                  #else
                                                                                                                    giInput.ambient.rgb = 0.0;
                                                                                                                  #endif
                                                                                                                  giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                                                                                  giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                                                                                  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                                                                                    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                                                                                  #endif
                                                                                                                  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                                                                                    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                                                                                    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                                                                                    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                                                                                    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                                                                                    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                                                                                  #endif
                                                                                                                  LightingStandard_GI(o, giInput, gi);

                                                                                                                  // call lighting function to output g-buffer
                                                                                                                  outEmission = LightingStandard_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
                                                                                                                  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                    outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
                                                                                                                  #endif
                                                                                                                  #ifndef UNITY_HDR_ON
                                                                                                                  outEmission.rgb = exp2(-outEmission.rgb);
                                                                                                                  #endif
                                                                                                                }


                                                                                                                #endif

                                                                                                                // -------- variant for: INSTANCING_ON 
                                                                                                                #if defined(INSTANCING_ON) && !defined(_INVERSEMATRIX_EXACT)
                                                                                                                // Surface shader code generated based on:
                                                                                                                // vertex modifier: 'vert'
                                                                                                                // writes to per-pixel normal: YES
                                                                                                                // writes to emission: no
                                                                                                                // writes to occlusion: no
                                                                                                                // needs world space reflection vector: no
                                                                                                                // needs world space normal vector: YES
                                                                                                                // needs screen space position: no
                                                                                                                // needs world space position: no
                                                                                                                // needs view direction: no
                                                                                                                // needs world space view direction: no
                                                                                                                // needs world space position for lighting: YES
                                                                                                                // needs world space view direction for lighting: YES
                                                                                                                // needs world space view direction for lightmaps: no
                                                                                                                // needs vertex color: YES
                                                                                                                // needs VFACE: no
                                                                                                                // passes tangent-to-world matrix to pixel shader: YES
                                                                                                                // reads from normal: no
                                                                                                                // 1 texcoords actually used
                                                                                                                //   float2 _MainTex
                                                                                                                #include "UnityCG.cginc"
                                                                                                                #include "Lighting.cginc"
                                                                                                                #include "UnityPBSLighting.cginc"

                                                                                                                #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                                #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                                #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                                // Original surface shader snippet:
                                                                                                                #line 6
                                                                                                                /* UNITY: Original start of shader */
                                                                                                                            //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                            //#pragma target 3.0
                                                                                                                            //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                            sampler2D _MainTex;

                                                                                                                            struct Input {
                                                                                                                                float2 uv_MainTex;
                                                                                                                                float3 cameraRelativeWorldPos;
                                                                                                                                float3 worldNormal;
                                                                                                                                float3 color : COLOR;
                                                                                                                                INTERNAL_DATA
                                                                                                                            };


                                                                                                                            // pass camera relative world position from vertex to fragment
                                                                                                                            void vert(inout appdata_full v, out Input o)
                                                                                                                            {
                                                                                                                                UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                                o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                            }

                                                                                                                            void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                                        #if !defined(UNITY_PASS_META)
                                                                                                                                // flat world normal from position derivatives
                                                                                                                                half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                                // construct world to tangent matrix
                                                                                                                                half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                                half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                                half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                                // inverse transform matrix
                                                                                                                                half3x3 w2tRotation;
                                                                                                                                w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                                w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                                w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                                half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                                w2tRotation *= rcp(det);

                                                                                                                                // apply world to tangent transform to flat world normal
                                                                                                                                o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                                o.Metallic = IN.uv_MainTex.y;
                                                                                                                                o.Smoothness = IN.uv_MainTex.x;
                                                                                                                                o.Albedo = IN.color; // vertex RGB
                                                                                                                        #endif
                                                                                                                            }


                                                                                                                            // vertex-to-fragment interpolation data
                                                                                                                            struct v2f_surf {
                                                                                                                              UNITY_POSITION(pos);
                                                                                                                              nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                                                                              float4 tSpace0 : TEXCOORD1;
                                                                                                                              float4 tSpace1 : TEXCOORD2;
                                                                                                                              float4 tSpace2 : TEXCOORD3;
                                                                                                                              nointerpolation fixed4 color : COLOR0;
                                                                                                                              float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                                                                            #ifndef DIRLIGHTMAP_OFF
                                                                                                                              float3 viewDir : TEXCOORD5;
                                                                                                                            #endif
                                                                                                                              float4 lmap : TEXCOORD6;
                                                                                                                            #ifndef LIGHTMAP_ON
                                                                                                                              #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                half3 sh : TEXCOORD7; // SH
                                                                                                                              #endif
                                                                                                                            #else
                                                                                                                              #ifdef DIRLIGHTMAP_OFF
                                                                                                                                float4 lmapFadePos : TEXCOORD7;
                                                                                                                              #endif
                                                                                                                            #endif
                                                                                                                              UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                              UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                            };
                                                                                                                            float4 _MainTex_ST;

                                                                                                                            // vertex shader
                                                                                                                            v2f_surf vert_surf(appdata_full v) {
                                                                                                                              UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                              v2f_surf o;
                                                                                                                              UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                              UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                              Input customInputData;
                                                                                                                              vert(v, customInputData);
                                                                                                                              o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                              o.pos = UnityObjectToClipPos(v.vertex);
                                                                                                                              o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                                                                                              float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                              float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                              fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                              fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                              fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                              o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                              o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                              o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                              float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
                                                                                                                              #ifndef DIRLIGHTMAP_OFF
                                                                                                                              o.viewDir.x = dot(viewDirForLight, worldTangent);
                                                                                                                              o.viewDir.y = dot(viewDirForLight, worldBinormal);
                                                                                                                              o.viewDir.z = dot(viewDirForLight, worldNormal);
                                                                                                                              #endif
                                                                                                                              o.color = v.color;
                                                                                                                            #ifdef DYNAMICLIGHTMAP_ON
                                                                                                                              o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                                                                                            #else
                                                                                                                              o.lmap.zw = 0;
                                                                                                                            #endif
                                                                                                                            #ifdef LIGHTMAP_ON
                                                                                                                              o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                              #ifdef DIRLIGHTMAP_OFF
                                                                                                                                o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
                                                                                                                                o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
                                                                                                                              #endif
                                                                                                                            #else
                                                                                                                              o.lmap.xy = 0;
                                                                                                                                #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                  o.sh = 0;
                                                                                                                                  o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                                                                                                #endif
                                                                                                                            #endif
                                                                                                                              return o;
                                                                                                                            }
                                                                                                                            #ifdef LIGHTMAP_ON
                                                                                                                            float4 unity_LightmapFade;
                                                                                                                            #endif
                                                                                                                            fixed4 unity_Ambient;

                                                                                                                            // fragment shader
                                                                                                                            void frag_surf(v2f_surf IN,
                                                                                                                                out half4 outGBuffer0 : SV_Target0,
                                                                                                                                out half4 outGBuffer1 : SV_Target1,
                                                                                                                                out half4 outGBuffer2 : SV_Target2,
                                                                                                                                out half4 outEmission : SV_Target3
                                                                                                                            #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                                , out half4 outShadowMask : SV_Target4
                                                                                                                            #endif
                                                                                                                            ) {
                                                                                                                              UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                              // prepare and unpack data
                                                                                                                              Input surfIN;
                                                                                                                              #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                              #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                                UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                              #else
                                                                                                                                UNITY_EXTRACT_FOG(IN);
                                                                                                                              #endif
                                                                                                                              #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                              #else
                                                                                                                                UNITY_EXTRACT_TBN(IN);
                                                                                                                              #endif
                                                                                                                              UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                              surfIN.uv_MainTex.x = 1.0;
                                                                                                                              surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                              surfIN.worldNormal.x = 1.0;
                                                                                                                              surfIN.color.x = 1.0;
                                                                                                                              surfIN.uv_MainTex = IN.pack0.xy;
                                                                                                                              surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                              float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                              #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                              #else
                                                                                                                                fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                              #endif
                                                                                                                              float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                                                                              surfIN.worldNormal = 0.0;
                                                                                                                              surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                                                                              surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                                                                              surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                                                                              surfIN.color = IN.color;
                                                                                                                              #ifdef UNITY_COMPILER_HLSL
                                                                                                                              SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                              #else
                                                                                                                              SurfaceOutputStandard o;
                                                                                                                              #endif
                                                                                                                              o.Albedo = 0.0;
                                                                                                                              o.Emission = 0.0;
                                                                                                                              o.Alpha = 0.0;
                                                                                                                              o.Occlusion = 1.0;
                                                                                                                              fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                                                                              o.Normal = fixed3(0,0,1);

                                                                                                                              // call surface function
                                                                                                                              surf(surfIN, o);
                                                                                                                            fixed3 originalNormal = o.Normal;
                                                                                                                              float3 worldN;
                                                                                                                              worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                                                                              worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                                                                              worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                                                                              worldN = normalize(worldN);
                                                                                                                              o.Normal = worldN;
                                                                                                                              half atten = 1;

                                                                                                                              // Setup lighting environment
                                                                                                                              UnityGI gi;
                                                                                                                              UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                                                                              gi.indirect.diffuse = 0;
                                                                                                                              gi.indirect.specular = 0;
                                                                                                                              gi.light.color = 0;
                                                                                                                              gi.light.dir = half3(0,1,0);
                                                                                                                              // Call GI (lightmaps/SH/reflections) lighting function
                                                                                                                              UnityGIInput giInput;
                                                                                                                              UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                                                                                              giInput.light = gi.light;
                                                                                                                              giInput.worldPos = worldPos;
                                                                                                                              giInput.worldViewDir = worldViewDir;
                                                                                                                              giInput.atten = atten;
                                                                                                                              #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                                                                                                giInput.lightmapUV = IN.lmap;
                                                                                                                              #else
                                                                                                                                giInput.lightmapUV = 0.0;
                                                                                                                              #endif
                                                                                                                              #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                giInput.ambient = IN.sh;
                                                                                                                              #else
                                                                                                                                giInput.ambient.rgb = 0.0;
                                                                                                                              #endif
                                                                                                                              giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                                                                                              giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                                                                                              #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                                                                                                giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                                                                                              #endif
                                                                                                                              #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                                                                                                giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                                                                                                giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                                                                                                giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                                                                                                giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                                                                                                giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                                                                                              #endif
                                                                                                                              LightingStandard_GI(o, giInput, gi);

                                                                                                                              // call lighting function to output g-buffer
                                                                                                                              outEmission = LightingStandard_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
                                                                                                                              #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                                outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
                                                                                                                              #endif
                                                                                                                              #ifndef UNITY_HDR_ON
                                                                                                                              outEmission.rgb = exp2(-outEmission.rgb);
                                                                                                                              #endif
                                                                                                                            }


                                                                                                                            #endif

                                                                                                                            // -------- variant for: _INVERSEMATRIX_EXACT 
                                                                                                                            #if defined(_INVERSEMATRIX_EXACT) && !defined(INSTANCING_ON)
                                                                                                                            // Surface shader code generated based on:
                                                                                                                            // vertex modifier: 'vert'
                                                                                                                            // writes to per-pixel normal: YES
                                                                                                                            // writes to emission: no
                                                                                                                            // writes to occlusion: no
                                                                                                                            // needs world space reflection vector: no
                                                                                                                            // needs world space normal vector: YES
                                                                                                                            // needs screen space position: no
                                                                                                                            // needs world space position: no
                                                                                                                            // needs view direction: no
                                                                                                                            // needs world space view direction: no
                                                                                                                            // needs world space position for lighting: YES
                                                                                                                            // needs world space view direction for lighting: YES
                                                                                                                            // needs world space view direction for lightmaps: no
                                                                                                                            // needs vertex color: YES
                                                                                                                            // needs VFACE: no
                                                                                                                            // passes tangent-to-world matrix to pixel shader: YES
                                                                                                                            // reads from normal: no
                                                                                                                            // 1 texcoords actually used
                                                                                                                            //   float2 _MainTex
                                                                                                                            #include "UnityCG.cginc"
                                                                                                                            #include "Lighting.cginc"
                                                                                                                            #include "UnityPBSLighting.cginc"

                                                                                                                            #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                                            #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                                            #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                                            // Original surface shader snippet:
                                                                                                                            #line 6
                                                                                                                            /* UNITY: Original start of shader */
                                                                                                                                        //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                                        //#pragma target 3.0
                                                                                                                                        //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                                        sampler2D _MainTex;

                                                                                                                                        struct Input {
                                                                                                                                            float2 uv_MainTex;
                                                                                                                                            float3 cameraRelativeWorldPos;
                                                                                                                                            float3 worldNormal;
                                                                                                                                            float3 color : COLOR;
                                                                                                                                            INTERNAL_DATA
                                                                                                                                        };


                                                                                                                                        // pass camera relative world position from vertex to fragment
                                                                                                                                        void vert(inout appdata_full v, out Input o)
                                                                                                                                        {
                                                                                                                                            UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                                            o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                                        }

                                                                                                                                        void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                                                    #if !defined(UNITY_PASS_META)
                                                                                                                                            // flat world normal from position derivatives
                                                                                                                                            half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                                            // construct world to tangent matrix
                                                                                                                                            half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                                            half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                                            half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                                            // inverse transform matrix
                                                                                                                                            half3x3 w2tRotation;
                                                                                                                                            w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                                            w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                                            w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                                            half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                                            w2tRotation *= rcp(det);

                                                                                                                                            // apply world to tangent transform to flat world normal
                                                                                                                                            o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                                            o.Metallic = IN.uv_MainTex.y;
                                                                                                                                            o.Smoothness = IN.uv_MainTex.x;
                                                                                                                                            o.Albedo = IN.color; // vertex RGB
                                                                                                                                    #endif
                                                                                                                                        }


                                                                                                                                        // vertex-to-fragment interpolation data
                                                                                                                                        struct v2f_surf {
                                                                                                                                          UNITY_POSITION(pos);
                                                                                                                                          nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                                                                                          float4 tSpace0 : TEXCOORD1;
                                                                                                                                          float4 tSpace1 : TEXCOORD2;
                                                                                                                                          float4 tSpace2 : TEXCOORD3;
                                                                                                                                          nointerpolation fixed4 color : COLOR0;
                                                                                                                                          float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                                                                                        #ifndef DIRLIGHTMAP_OFF
                                                                                                                                          float3 viewDir : TEXCOORD5;
                                                                                                                                        #endif
                                                                                                                                          float4 lmap : TEXCOORD6;
                                                                                                                                        #ifndef LIGHTMAP_ON
                                                                                                                                          #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                            half3 sh : TEXCOORD7; // SH
                                                                                                                                          #endif
                                                                                                                                        #else
                                                                                                                                          #ifdef DIRLIGHTMAP_OFF
                                                                                                                                            float4 lmapFadePos : TEXCOORD7;
                                                                                                                                          #endif
                                                                                                                                        #endif
                                                                                                                                          UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                                          UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                                        };
                                                                                                                                        float4 _MainTex_ST;

                                                                                                                                        // vertex shader
                                                                                                                                        v2f_surf vert_surf(appdata_full v) {
                                                                                                                                          UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                                          v2f_surf o;
                                                                                                                                          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                                          UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                                          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                                          Input customInputData;
                                                                                                                                          vert(v, customInputData);
                                                                                                                                          o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                                          o.pos = UnityObjectToClipPos(v.vertex);
                                                                                                                                          o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                                                                                                          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                                          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                                          fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                                          fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                                          fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                                          o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                                          o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                                          o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                                          float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
                                                                                                                                          #ifndef DIRLIGHTMAP_OFF
                                                                                                                                          o.viewDir.x = dot(viewDirForLight, worldTangent);
                                                                                                                                          o.viewDir.y = dot(viewDirForLight, worldBinormal);
                                                                                                                                          o.viewDir.z = dot(viewDirForLight, worldNormal);
                                                                                                                                          #endif
                                                                                                                                          o.color = v.color;
                                                                                                                                        #ifdef DYNAMICLIGHTMAP_ON
                                                                                                                                          o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                                                                                                        #else
                                                                                                                                          o.lmap.zw = 0;
                                                                                                                                        #endif
                                                                                                                                        #ifdef LIGHTMAP_ON
                                                                                                                                          o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                                          #ifdef DIRLIGHTMAP_OFF
                                                                                                                                            o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
                                                                                                                                            o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
                                                                                                                                          #endif
                                                                                                                                        #else
                                                                                                                                          o.lmap.xy = 0;
                                                                                                                                            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                              o.sh = 0;
                                                                                                                                              o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                                                                                                            #endif
                                                                                                                                        #endif
                                                                                                                                          return o;
                                                                                                                                        }
                                                                                                                                        #ifdef LIGHTMAP_ON
                                                                                                                                        float4 unity_LightmapFade;
                                                                                                                                        #endif
                                                                                                                                        fixed4 unity_Ambient;

                                                                                                                                        // fragment shader
                                                                                                                                        void frag_surf(v2f_surf IN,
                                                                                                                                            out half4 outGBuffer0 : SV_Target0,
                                                                                                                                            out half4 outGBuffer1 : SV_Target1,
                                                                                                                                            out half4 outGBuffer2 : SV_Target2,
                                                                                                                                            out half4 outEmission : SV_Target3
                                                                                                                                        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                                            , out half4 outShadowMask : SV_Target4
                                                                                                                                        #endif
                                                                                                                                        ) {
                                                                                                                                          UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                                          // prepare and unpack data
                                                                                                                                          Input surfIN;
                                                                                                                                          #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                            UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                                          #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                                            UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                                          #else
                                                                                                                                            UNITY_EXTRACT_FOG(IN);
                                                                                                                                          #endif
                                                                                                                                          #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                            UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                                          #else
                                                                                                                                            UNITY_EXTRACT_TBN(IN);
                                                                                                                                          #endif
                                                                                                                                          UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                                          surfIN.uv_MainTex.x = 1.0;
                                                                                                                                          surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                                          surfIN.worldNormal.x = 1.0;
                                                                                                                                          surfIN.color.x = 1.0;
                                                                                                                                          surfIN.uv_MainTex = IN.pack0.xy;
                                                                                                                                          surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                                          float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                                          #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                                            fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                                          #else
                                                                                                                                            fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                                          #endif
                                                                                                                                          float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                                                                                          surfIN.worldNormal = 0.0;
                                                                                                                                          surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                                                                                          surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                                                                                          surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                                                                                          surfIN.color = IN.color;
                                                                                                                                          #ifdef UNITY_COMPILER_HLSL
                                                                                                                                          SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                                          #else
                                                                                                                                          SurfaceOutputStandard o;
                                                                                                                                          #endif
                                                                                                                                          o.Albedo = 0.0;
                                                                                                                                          o.Emission = 0.0;
                                                                                                                                          o.Alpha = 0.0;
                                                                                                                                          o.Occlusion = 1.0;
                                                                                                                                          fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                                                                                          o.Normal = fixed3(0,0,1);

                                                                                                                                          // call surface function
                                                                                                                                          surf(surfIN, o);
                                                                                                                                        fixed3 originalNormal = o.Normal;
                                                                                                                                          float3 worldN;
                                                                                                                                          worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                                                                                          worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                                                                                          worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                                                                                          worldN = normalize(worldN);
                                                                                                                                          o.Normal = worldN;
                                                                                                                                          half atten = 1;

                                                                                                                                          // Setup lighting environment
                                                                                                                                          UnityGI gi;
                                                                                                                                          UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                                                                                          gi.indirect.diffuse = 0;
                                                                                                                                          gi.indirect.specular = 0;
                                                                                                                                          gi.light.color = 0;
                                                                                                                                          gi.light.dir = half3(0,1,0);
                                                                                                                                          // Call GI (lightmaps/SH/reflections) lighting function
                                                                                                                                          UnityGIInput giInput;
                                                                                                                                          UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                                                                                                          giInput.light = gi.light;
                                                                                                                                          giInput.worldPos = worldPos;
                                                                                                                                          giInput.worldViewDir = worldViewDir;
                                                                                                                                          giInput.atten = atten;
                                                                                                                                          #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                                                                                                            giInput.lightmapUV = IN.lmap;
                                                                                                                                          #else
                                                                                                                                            giInput.lightmapUV = 0.0;
                                                                                                                                          #endif
                                                                                                                                          #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                            giInput.ambient = IN.sh;
                                                                                                                                          #else
                                                                                                                                            giInput.ambient.rgb = 0.0;
                                                                                                                                          #endif
                                                                                                                                          giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                                                                                                          giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                                                                                                          #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                                                                                                            giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                                                                                                          #endif
                                                                                                                                          #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                                                                                                            giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                                                                                                            giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                                                                                                            giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                                                                                                            giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                                                                                                            giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                                                                                                          #endif
                                                                                                                                          LightingStandard_GI(o, giInput, gi);

                                                                                                                                          // call lighting function to output g-buffer
                                                                                                                                          outEmission = LightingStandard_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
                                                                                                                                          #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                                            outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
                                                                                                                                          #endif
                                                                                                                                          #ifndef UNITY_HDR_ON
                                                                                                                                          outEmission.rgb = exp2(-outEmission.rgb);
                                                                                                                                          #endif
                                                                                                                                        }


                                                                                                                                        #endif

                                                                                                                                        // -------- variant for: _INVERSEMATRIX_EXACT INSTANCING_ON 
                                                                                                                                        #if defined(_INVERSEMATRIX_EXACT) && defined(INSTANCING_ON)
                                                                                                                                        // Surface shader code generated based on:
                                                                                                                                        // vertex modifier: 'vert'
                                                                                                                                        // writes to per-pixel normal: YES
                                                                                                                                        // writes to emission: no
                                                                                                                                        // writes to occlusion: no
                                                                                                                                        // needs world space reflection vector: no
                                                                                                                                        // needs world space normal vector: YES
                                                                                                                                        // needs screen space position: no
                                                                                                                                        // needs world space position: no
                                                                                                                                        // needs view direction: no
                                                                                                                                        // needs world space view direction: no
                                                                                                                                        // needs world space position for lighting: YES
                                                                                                                                        // needs world space view direction for lighting: YES
                                                                                                                                        // needs world space view direction for lightmaps: no
                                                                                                                                        // needs vertex color: YES
                                                                                                                                        // needs VFACE: no
                                                                                                                                        // passes tangent-to-world matrix to pixel shader: YES
                                                                                                                                        // reads from normal: no
                                                                                                                                        // 1 texcoords actually used
                                                                                                                                        //   float2 _MainTex
                                                                                                                                        #include "UnityCG.cginc"
                                                                                                                                        #include "Lighting.cginc"
                                                                                                                                        #include "UnityPBSLighting.cginc"

                                                                                                                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                                                        // Original surface shader snippet:
                                                                                                                                        #line 6
                                                                                                                                        /* UNITY: Original start of shader */
                                                                                                                                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                                                    //#pragma target 3.0
                                                                                                                                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                                                    sampler2D _MainTex;

                                                                                                                                                    struct Input {
                                                                                                                                                        float2 uv_MainTex;
                                                                                                                                                        float3 cameraRelativeWorldPos;
                                                                                                                                                        float3 worldNormal;
                                                                                                                                                        float3 color : COLOR;
                                                                                                                                                        INTERNAL_DATA
                                                                                                                                                    };


                                                                                                                                                    // pass camera relative world position from vertex to fragment
                                                                                                                                                    void vert(inout appdata_full v, out Input o)
                                                                                                                                                    {
                                                                                                                                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                                                    }

                                                                                                                                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                                                                #if !defined(UNITY_PASS_META)
                                                                                                                                                        // flat world normal from position derivatives
                                                                                                                                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                                                        // construct world to tangent matrix
                                                                                                                                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                                                        // inverse transform matrix
                                                                                                                                                        half3x3 w2tRotation;
                                                                                                                                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                                                        w2tRotation *= rcp(det);

                                                                                                                                                        // apply world to tangent transform to flat world normal
                                                                                                                                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                                                        o.Metallic = IN.uv_MainTex.y;
                                                                                                                                                        o.Smoothness = IN.uv_MainTex.x;
                                                                                                                                                        o.Albedo = IN.color; // vertex RGB
                                                                                                                                                #endif
                                                                                                                                                    }


                                                                                                                                                    // vertex-to-fragment interpolation data
                                                                                                                                                    struct v2f_surf {
                                                                                                                                                      UNITY_POSITION(pos);
                                                                                                                                                      nointerpolation float2 pack0 : TEXCOORD0; // _MainTex
                                                                                                                                                      float4 tSpace0 : TEXCOORD1;
                                                                                                                                                      float4 tSpace1 : TEXCOORD2;
                                                                                                                                                      float4 tSpace2 : TEXCOORD3;
                                                                                                                                                      nointerpolation fixed4 color : COLOR0;
                                                                                                                                                      float3 custompack0 : TEXCOORD4; // cameraRelativeWorldPos
                                                                                                                                                    #ifndef DIRLIGHTMAP_OFF
                                                                                                                                                      float3 viewDir : TEXCOORD5;
                                                                                                                                                    #endif
                                                                                                                                                      float4 lmap : TEXCOORD6;
                                                                                                                                                    #ifndef LIGHTMAP_ON
                                                                                                                                                      #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                                        half3 sh : TEXCOORD7; // SH
                                                                                                                                                      #endif
                                                                                                                                                    #else
                                                                                                                                                      #ifdef DIRLIGHTMAP_OFF
                                                                                                                                                        float4 lmapFadePos : TEXCOORD7;
                                                                                                                                                      #endif
                                                                                                                                                    #endif
                                                                                                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                                                    };
                                                                                                                                                    float4 _MainTex_ST;

                                                                                                                                                    // vertex shader
                                                                                                                                                    v2f_surf vert_surf(appdata_full v) {
                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                                                      v2f_surf o;
                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                                                      Input customInputData;
                                                                                                                                                      vert(v, customInputData);
                                                                                                                                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                                                      o.pos = UnityObjectToClipPos(v.vertex);
                                                                                                                                                      o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                                                                                                                                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                                                      float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
                                                                                                                                                      #ifndef DIRLIGHTMAP_OFF
                                                                                                                                                      o.viewDir.x = dot(viewDirForLight, worldTangent);
                                                                                                                                                      o.viewDir.y = dot(viewDirForLight, worldBinormal);
                                                                                                                                                      o.viewDir.z = dot(viewDirForLight, worldNormal);
                                                                                                                                                      #endif
                                                                                                                                                      o.color = v.color;
                                                                                                                                                    #ifdef DYNAMICLIGHTMAP_ON
                                                                                                                                                      o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                                                                                                                                                    #else
                                                                                                                                                      o.lmap.zw = 0;
                                                                                                                                                    #endif
                                                                                                                                                    #ifdef LIGHTMAP_ON
                                                                                                                                                      o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                                                      #ifdef DIRLIGHTMAP_OFF
                                                                                                                                                        o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
                                                                                                                                                        o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
                                                                                                                                                      #endif
                                                                                                                                                    #else
                                                                                                                                                      o.lmap.xy = 0;
                                                                                                                                                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                                          o.sh = 0;
                                                                                                                                                          o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                                                                                                                                                        #endif
                                                                                                                                                    #endif
                                                                                                                                                      return o;
                                                                                                                                                    }
                                                                                                                                                    #ifdef LIGHTMAP_ON
                                                                                                                                                    float4 unity_LightmapFade;
                                                                                                                                                    #endif
                                                                                                                                                    fixed4 unity_Ambient;

                                                                                                                                                    // fragment shader
                                                                                                                                                    void frag_surf(v2f_surf IN,
                                                                                                                                                        out half4 outGBuffer0 : SV_Target0,
                                                                                                                                                        out half4 outGBuffer1 : SV_Target1,
                                                                                                                                                        out half4 outGBuffer2 : SV_Target2,
                                                                                                                                                        out half4 outEmission : SV_Target3
                                                                                                                                                    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                                                        , out half4 outShadowMask : SV_Target4
                                                                                                                                                    #endif
                                                                                                                                                    ) {
                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                                                      // prepare and unpack data
                                                                                                                                                      Input surfIN;
                                                                                                                                                      #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                        UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                                                      #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                                                        UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                                                      #else
                                                                                                                                                        UNITY_EXTRACT_FOG(IN);
                                                                                                                                                      #endif
                                                                                                                                                      #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                        UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                                                      #else
                                                                                                                                                        UNITY_EXTRACT_TBN(IN);
                                                                                                                                                      #endif
                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                                                      surfIN.uv_MainTex.x = 1.0;
                                                                                                                                                      surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                                                      surfIN.worldNormal.x = 1.0;
                                                                                                                                                      surfIN.color.x = 1.0;
                                                                                                                                                      surfIN.uv_MainTex = IN.pack0.xy;
                                                                                                                                                      surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                                                      float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                                                      #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                                                        fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                                                      #else
                                                                                                                                                        fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                                                      #endif
                                                                                                                                                      float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                                                                                                                                                      surfIN.worldNormal = 0.0;
                                                                                                                                                      surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
                                                                                                                                                      surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
                                                                                                                                                      surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
                                                                                                                                                      surfIN.color = IN.color;
                                                                                                                                                      #ifdef UNITY_COMPILER_HLSL
                                                                                                                                                      SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                                                      #else
                                                                                                                                                      SurfaceOutputStandard o;
                                                                                                                                                      #endif
                                                                                                                                                      o.Albedo = 0.0;
                                                                                                                                                      o.Emission = 0.0;
                                                                                                                                                      o.Alpha = 0.0;
                                                                                                                                                      o.Occlusion = 1.0;
                                                                                                                                                      fixed3 normalWorldVertex = fixed3(0,0,1);
                                                                                                                                                      o.Normal = fixed3(0,0,1);

                                                                                                                                                      // call surface function
                                                                                                                                                      surf(surfIN, o);
                                                                                                                                                    fixed3 originalNormal = o.Normal;
                                                                                                                                                      float3 worldN;
                                                                                                                                                      worldN.x = dot(_unity_tbn_0, o.Normal);
                                                                                                                                                      worldN.y = dot(_unity_tbn_1, o.Normal);
                                                                                                                                                      worldN.z = dot(_unity_tbn_2, o.Normal);
                                                                                                                                                      worldN = normalize(worldN);
                                                                                                                                                      o.Normal = worldN;
                                                                                                                                                      half atten = 1;

                                                                                                                                                      // Setup lighting environment
                                                                                                                                                      UnityGI gi;
                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                                                                                                                                                      gi.indirect.diffuse = 0;
                                                                                                                                                      gi.indirect.specular = 0;
                                                                                                                                                      gi.light.color = 0;
                                                                                                                                                      gi.light.dir = half3(0,1,0);
                                                                                                                                                      // Call GI (lightmaps/SH/reflections) lighting function
                                                                                                                                                      UnityGIInput giInput;
                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                                                                                                                                                      giInput.light = gi.light;
                                                                                                                                                      giInput.worldPos = worldPos;
                                                                                                                                                      giInput.worldViewDir = worldViewDir;
                                                                                                                                                      giInput.atten = atten;
                                                                                                                                                      #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                                                                                                                                                        giInput.lightmapUV = IN.lmap;
                                                                                                                                                      #else
                                                                                                                                                        giInput.lightmapUV = 0.0;
                                                                                                                                                      #endif
                                                                                                                                                      #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                                                                                                                        giInput.ambient = IN.sh;
                                                                                                                                                      #else
                                                                                                                                                        giInput.ambient.rgb = 0.0;
                                                                                                                                                      #endif
                                                                                                                                                      giInput.probeHDR[0] = unity_SpecCube0_HDR;
                                                                                                                                                      giInput.probeHDR[1] = unity_SpecCube1_HDR;
                                                                                                                                                      #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                                                                                                                                                        giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                                                                                                                                                      #endif
                                                                                                                                                      #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                                                                                                                                                        giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                                                                                                                                                        giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                                                                                                                                                        giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                                                                                                                                                        giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                                                                                                                                                        giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                                                                                                                                                      #endif
                                                                                                                                                      LightingStandard_GI(o, giInput, gi);

                                                                                                                                                      // call lighting function to output g-buffer
                                                                                                                                                      outEmission = LightingStandard_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
                                                                                                                                                      #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                                                                                                                                        outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
                                                                                                                                                      #endif
                                                                                                                                                      #ifndef UNITY_HDR_ON
                                                                                                                                                      outEmission.rgb = exp2(-outEmission.rgb);
                                                                                                                                                      #endif
                                                                                                                                                    }


                                                                                                                                                    #endif


                                                                                                                                                    ENDCG

                                                                                                                                                    }

                                                                                                    // ---- meta information extraction pass:
                                                                                                    Pass {
                                                                                                        Name "Meta"
                                                                                                        Tags { "LightMode" = "Meta" }
                                                                                                        Cull Off

                                                                                                CGPROGRAM
                                                                                                                                                        // compile directives
                                                                                                                                                        #pragma vertex vert_surf
                                                                                                                                                        #pragma fragment frag_surf
                                                                                                                                                        #pragma target 3.0
                                                                                                                                                        #pragma shader_feature _ _INVERSEMATRIX_EXACT
                                                                                                                                                        #pragma multi_compile_instancing
                                                                                                                                                        #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
                                                                                                                                                        #pragma shader_feature EDITOR_VISUALIZATION

                                                                                                                                                        #include "HLSLSupport.cginc"
                                                                                                                                                        #define UNITY_INSTANCED_LOD_FADE
                                                                                                                                                        #define UNITY_INSTANCED_SH
                                                                                                                                                        #define UNITY_INSTANCED_LIGHTMAPSTS
                                                                                                                                                        #include "UnityShaderVariables.cginc"
                                                                                                                                                        #include "UnityShaderUtilities.cginc"
                                                                                                                                                        // -------- variant for: <when no other keywords are defined>
                                                                                                                                                        #if !defined(INSTANCING_ON) && !defined(_INVERSEMATRIX_EXACT)
                                                                                                                                                        // Surface shader code generated based on:
                                                                                                                                                        // vertex modifier: 'vert'
                                                                                                                                                        // writes to per-pixel normal: YES
                                                                                                                                                        // writes to emission: no
                                                                                                                                                        // writes to occlusion: no
                                                                                                                                                        // needs world space reflection vector: no
                                                                                                                                                        // needs world space normal vector: no
                                                                                                                                                        // needs screen space position: no
                                                                                                                                                        // needs world space position: no
                                                                                                                                                        // needs view direction: no
                                                                                                                                                        // needs world space view direction: no
                                                                                                                                                        // needs world space position for lighting: YES
                                                                                                                                                        // needs world space view direction for lighting: YES
                                                                                                                                                        // needs world space view direction for lightmaps: no
                                                                                                                                                        // needs vertex color: YES
                                                                                                                                                        // needs VFACE: no
                                                                                                                                                        // passes tangent-to-world matrix to pixel shader: YES
                                                                                                                                                        // reads from normal: no
                                                                                                                                                        // 0 texcoords actually used
                                                                                                                                                        #include "UnityCG.cginc"
                                                                                                                                                        #include "Lighting.cginc"
                                                                                                                                                        #include "UnityPBSLighting.cginc"

                                                                                                                                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                                                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                                                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                                                                        // Original surface shader snippet:
                                                                                                                                                        #line 6
                                                                                                                                                        /* UNITY: Original start of shader */
                                                                                                                                                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                                                                    //#pragma target 3.0
                                                                                                                                                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                                                                    sampler2D _MainTex;

                                                                                                                                                                    struct Input {
                                                                                                                                                                        float2 uv_MainTex;
                                                                                                                                                                        float3 cameraRelativeWorldPos;
                                                                                                                                                                        float3 worldNormal;
                                                                                                                                                                        float3 color : COLOR;
                                                                                                                                                                        INTERNAL_DATA
                                                                                                                                                                    };


                                                                                                                                                                    // pass camera relative world position from vertex to fragment
                                                                                                                                                                    void vert(inout appdata_full v, out Input o)
                                                                                                                                                                    {
                                                                                                                                                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                                                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                                                                    }

                                                                                                                                                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                                                                                #if !defined(UNITY_PASS_META)
                                                                                                                                                                        // flat world normal from position derivatives
                                                                                                                                                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                                                                        // construct world to tangent matrix
                                                                                                                                                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                                                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                                                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                                                                        // inverse transform matrix
                                                                                                                                                                        half3x3 w2tRotation;
                                                                                                                                                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                                                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                                                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                                                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                                                                        w2tRotation *= rcp(det);

                                                                                                                                                                        // apply world to tangent transform to flat world normal
                                                                                                                                                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                                                                        o.Metallic = IN.uv_MainTex.y;
                                                                                                                                                                        o.Smoothness = IN.uv_MainTex.x;
                                                                                                                                                                        o.Albedo = IN.color; // vertex RGB
                                                                                                                                                                #endif
                                                                                                                                                                    }

                                                                                                                                                        #include "UnityMetaPass.cginc"

                                                                                                                                                                    // vertex-to-fragment interpolation data
                                                                                                                                                                    struct v2f_surf {
                                                                                                                                                                      UNITY_POSITION(pos);
                                                                                                                                                                      float4 tSpace0 : TEXCOORD0;
                                                                                                                                                                      float4 tSpace1 : TEXCOORD1;
                                                                                                                                                                      float4 tSpace2 : TEXCOORD2;
                                                                                                                                                                      nointerpolation fixed4 color : COLOR0;
                                                                                                                                                                      float3 custompack0 : TEXCOORD3; // cameraRelativeWorldPos
                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                      float2 vizUV : TEXCOORD4;
                                                                                                                                                                      float4 lightCoord : TEXCOORD5;
                                                                                                                                                                    #endif
                                                                                                                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                                                                    };

                                                                                                                                                                    // vertex shader
                                                                                                                                                                    v2f_surf vert_surf(appdata_full v) {
                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                                                                      v2f_surf o;
                                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                                                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                                                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                                                                      Input customInputData;
                                                                                                                                                                      vert(v, customInputData);
                                                                                                                                                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                                                                      o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                      o.vizUV = 0;
                                                                                                                                                                      o.lightCoord = 0;
                                                                                                                                                                      if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
                                                                                                                                                                        o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
                                                                                                                                                                      else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
                                                                                                                                                                      {
                                                                                                                                                                        o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                                                                        o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
                                                                                                                                                                      }
                                                                                                                                                                    #endif
                                                                                                                                                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                                                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                                                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                                                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                                                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                                                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                                                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                                                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                                                                      o.color = v.color;
                                                                                                                                                                      return o;
                                                                                                                                                                    }

                                                                                                                                                                    // fragment shader
                                                                                                                                                                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                                                                    // prepare and unpack data
                                                                                                                                                                    Input surfIN;
                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                                                                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                                                                    #else
                                                                                                                                                                      UNITY_EXTRACT_FOG(IN);
                                                                                                                                                                    #endif
                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                      UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                                                                    #else
                                                                                                                                                                      UNITY_EXTRACT_TBN(IN);
                                                                                                                                                                    #endif
                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                                                                    surfIN.uv_MainTex.x = 1.0;
                                                                                                                                                                    surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                                                                    surfIN.worldNormal.x = 1.0;
                                                                                                                                                                    surfIN.color.x = 1.0;
                                                                                                                                                                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                                                                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                                                                    #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                                                                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                                                                    #else
                                                                                                                                                                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                                                                    #endif
                                                                                                                                                                    surfIN.color = IN.color;
                                                                                                                                                                    #ifdef UNITY_COMPILER_HLSL
                                                                                                                                                                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                                                                    #else
                                                                                                                                                                    SurfaceOutputStandard o;
                                                                                                                                                                    #endif
                                                                                                                                                                    o.Albedo = 0.0;
                                                                                                                                                                    o.Emission = 0.0;
                                                                                                                                                                    o.Alpha = 0.0;
                                                                                                                                                                    o.Occlusion = 1.0;
                                                                                                                                                                    fixed3 normalWorldVertex = fixed3(0,0,1);

                                                                                                                                                                    // call surface function
                                                                                                                                                                    surf(surfIN, o);
                                                                                                                                                                    UnityMetaInput metaIN;
                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
                                                                                                                                                                    metaIN.Albedo = o.Albedo;
                                                                                                                                                                    metaIN.Emission = o.Emission;
                                                                                                                                                                  #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                    metaIN.VizUV = IN.vizUV;
                                                                                                                                                                    metaIN.LightCoord = IN.lightCoord;
                                                                                                                                                                  #endif
                                                                                                                                                                    return UnityMetaFragment(metaIN);
                                                                                                                                                                  }


                                                                                                                                                                  #endif

                                                                                                                                                                        // -------- variant for: INSTANCING_ON 
                                                                                                                                                                        #if defined(INSTANCING_ON) && !defined(_INVERSEMATRIX_EXACT)
                                                                                                                                                                        // Surface shader code generated based on:
                                                                                                                                                                        // vertex modifier: 'vert'
                                                                                                                                                                        // writes to per-pixel normal: YES
                                                                                                                                                                        // writes to emission: no
                                                                                                                                                                        // writes to occlusion: no
                                                                                                                                                                        // needs world space reflection vector: no
                                                                                                                                                                        // needs world space normal vector: no
                                                                                                                                                                        // needs screen space position: no
                                                                                                                                                                        // needs world space position: no
                                                                                                                                                                        // needs view direction: no
                                                                                                                                                                        // needs world space view direction: no
                                                                                                                                                                        // needs world space position for lighting: YES
                                                                                                                                                                        // needs world space view direction for lighting: YES
                                                                                                                                                                        // needs world space view direction for lightmaps: no
                                                                                                                                                                        // needs vertex color: YES
                                                                                                                                                                        // needs VFACE: no
                                                                                                                                                                        // passes tangent-to-world matrix to pixel shader: YES
                                                                                                                                                                        // reads from normal: no
                                                                                                                                                                        // 0 texcoords actually used
                                                                                                                                                                        #include "UnityCG.cginc"
                                                                                                                                                                        #include "Lighting.cginc"
                                                                                                                                                                        #include "UnityPBSLighting.cginc"

                                                                                                                                                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                                                                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                                                                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                                                                                        // Original surface shader snippet:
                                                                                                                                                                        #line 6
                                                                                                                                                                        /* UNITY: Original start of shader */
                                                                                                                                                                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                                                                                    //#pragma target 3.0
                                                                                                                                                                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                                                                                    sampler2D _MainTex;

                                                                                                                                                                                    struct Input {
                                                                                                                                                                                        float2 uv_MainTex;
                                                                                                                                                                                        float3 cameraRelativeWorldPos;
                                                                                                                                                                                        float3 worldNormal;
                                                                                                                                                                                        float3 color : COLOR;
                                                                                                                                                                                        INTERNAL_DATA
                                                                                                                                                                                    };


                                                                                                                                                                                    // pass camera relative world position from vertex to fragment
                                                                                                                                                                                    void vert(inout appdata_full v, out Input o)
                                                                                                                                                                                    {
                                                                                                                                                                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                                                                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                                                                                    }

                                                                                                                                                                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                                                                                                #if !defined(UNITY_PASS_META)
                                                                                                                                                                                        // flat world normal from position derivatives
                                                                                                                                                                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                                                                                        // construct world to tangent matrix
                                                                                                                                                                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                                                                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                                                                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                                                                                        // inverse transform matrix
                                                                                                                                                                                        half3x3 w2tRotation;
                                                                                                                                                                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                                                                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                                                                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                                                                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                                                                                        w2tRotation *= rcp(det);

                                                                                                                                                                                        // apply world to tangent transform to flat world normal
                                                                                                                                                                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                                                                                        o.Metallic = IN.uv_MainTex.y;
                                                                                                                                                                                        o.Smoothness = IN.uv_MainTex.x;
                                                                                                                                                                                        o.Albedo = IN.color; // vertex RGB
                                                                                                                                                                                #endif
                                                                                                                                                                                    }

                                                                                                                                                                        #include "UnityMetaPass.cginc"

                                                                                                                                                                                    // vertex-to-fragment interpolation data
                                                                                                                                                                                    struct v2f_surf {
                                                                                                                                                                                      UNITY_POSITION(pos);
                                                                                                                                                                                      float4 tSpace0 : TEXCOORD0;
                                                                                                                                                                                      float4 tSpace1 : TEXCOORD1;
                                                                                                                                                                                      float4 tSpace2 : TEXCOORD2;
                                                                                                                                                                                      nointerpolation fixed4 color : COLOR0;
                                                                                                                                                                                      float3 custompack0 : TEXCOORD3; // cameraRelativeWorldPos
                                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                      float2 vizUV : TEXCOORD4;
                                                                                                                                                                                      float4 lightCoord : TEXCOORD5;
                                                                                                                                                                                    #endif
                                                                                                                                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                                                                                    };

                                                                                                                                                                                    // vertex shader
                                                                                                                                                                                    v2f_surf vert_surf(appdata_full v) {
                                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                                                                                      v2f_surf o;
                                                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                                                                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                                                                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                                                                                      Input customInputData;
                                                                                                                                                                                      vert(v, customInputData);
                                                                                                                                                                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                                                                                      o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
                                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                      o.vizUV = 0;
                                                                                                                                                                                      o.lightCoord = 0;
                                                                                                                                                                                      if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
                                                                                                                                                                                        o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
                                                                                                                                                                                      else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
                                                                                                                                                                                      {
                                                                                                                                                                                        o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                                                                                        o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
                                                                                                                                                                                      }
                                                                                                                                                                                    #endif
                                                                                                                                                                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                                                                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                                                                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                                                                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                                                                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                                                                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                                                                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                                                                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                                                                                      o.color = v.color;
                                                                                                                                                                                      return o;
                                                                                                                                                                                    }

                                                                                                                                                                                    // fragment shader
                                                                                                                                                                                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                                                                                    // prepare and unpack data
                                                                                                                                                                                    Input surfIN;
                                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                                                                                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                                                                                    #else
                                                                                                                                                                                      UNITY_EXTRACT_FOG(IN);
                                                                                                                                                                                    #endif
                                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                                      UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                                                                                    #else
                                                                                                                                                                                      UNITY_EXTRACT_TBN(IN);
                                                                                                                                                                                    #endif
                                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                                                                                    surfIN.uv_MainTex.x = 1.0;
                                                                                                                                                                                    surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                                                                                    surfIN.worldNormal.x = 1.0;
                                                                                                                                                                                    surfIN.color.x = 1.0;
                                                                                                                                                                                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                                                                                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                                                                                    #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                                                                                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                                                                                    #else
                                                                                                                                                                                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                                                                                    #endif
                                                                                                                                                                                    surfIN.color = IN.color;
                                                                                                                                                                                    #ifdef UNITY_COMPILER_HLSL
                                                                                                                                                                                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                                                                                    #else
                                                                                                                                                                                    SurfaceOutputStandard o;
                                                                                                                                                                                    #endif
                                                                                                                                                                                    o.Albedo = 0.0;
                                                                                                                                                                                    o.Emission = 0.0;
                                                                                                                                                                                    o.Alpha = 0.0;
                                                                                                                                                                                    o.Occlusion = 1.0;
                                                                                                                                                                                    fixed3 normalWorldVertex = fixed3(0,0,1);

                                                                                                                                                                                    // call surface function
                                                                                                                                                                                    surf(surfIN, o);
                                                                                                                                                                                    UnityMetaInput metaIN;
                                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
                                                                                                                                                                                    metaIN.Albedo = o.Albedo;
                                                                                                                                                                                    metaIN.Emission = o.Emission;
                                                                                                                                                                                  #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                    metaIN.VizUV = IN.vizUV;
                                                                                                                                                                                    metaIN.LightCoord = IN.lightCoord;
                                                                                                                                                                                  #endif
                                                                                                                                                                                    return UnityMetaFragment(metaIN);
                                                                                                                                                                                  }


                                                                                                                                                                                  #endif

                                                                                                                                                                                        // -------- variant for: _INVERSEMATRIX_EXACT 
                                                                                                                                                                                        #if defined(_INVERSEMATRIX_EXACT) && !defined(INSTANCING_ON)
                                                                                                                                                                                        // Surface shader code generated based on:
                                                                                                                                                                                        // vertex modifier: 'vert'
                                                                                                                                                                                        // writes to per-pixel normal: YES
                                                                                                                                                                                        // writes to emission: no
                                                                                                                                                                                        // writes to occlusion: no
                                                                                                                                                                                        // needs world space reflection vector: no
                                                                                                                                                                                        // needs world space normal vector: no
                                                                                                                                                                                        // needs screen space position: no
                                                                                                                                                                                        // needs world space position: no
                                                                                                                                                                                        // needs view direction: no
                                                                                                                                                                                        // needs world space view direction: no
                                                                                                                                                                                        // needs world space position for lighting: YES
                                                                                                                                                                                        // needs world space view direction for lighting: YES
                                                                                                                                                                                        // needs world space view direction for lightmaps: no
                                                                                                                                                                                        // needs vertex color: YES
                                                                                                                                                                                        // needs VFACE: no
                                                                                                                                                                                        // passes tangent-to-world matrix to pixel shader: YES
                                                                                                                                                                                        // reads from normal: no
                                                                                                                                                                                        // 0 texcoords actually used
                                                                                                                                                                                        #include "UnityCG.cginc"
                                                                                                                                                                                        #include "Lighting.cginc"
                                                                                                                                                                                        #include "UnityPBSLighting.cginc"

                                                                                                                                                                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                                                                                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                                                                                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                                                                                                        // Original surface shader snippet:
                                                                                                                                                                                        #line 6
                                                                                                                                                                                        /* UNITY: Original start of shader */
                                                                                                                                                                                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                                                                                                    //#pragma target 3.0
                                                                                                                                                                                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                                                                                                    sampler2D _MainTex;

                                                                                                                                                                                                    struct Input {
                                                                                                                                                                                                        float2 uv_MainTex;
                                                                                                                                                                                                        float3 cameraRelativeWorldPos;
                                                                                                                                                                                                        float3 worldNormal;
                                                                                                                                                                                                        float3 color : COLOR;
                                                                                                                                                                                                        INTERNAL_DATA
                                                                                                                                                                                                    };


                                                                                                                                                                                                    // pass camera relative world position from vertex to fragment
                                                                                                                                                                                                    void vert(inout appdata_full v, out Input o)
                                                                                                                                                                                                    {
                                                                                                                                                                                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                                                                                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                                                                                                    }

                                                                                                                                                                                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                                                                                                                #if !defined(UNITY_PASS_META)
                                                                                                                                                                                                        // flat world normal from position derivatives
                                                                                                                                                                                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                                                                                                        // construct world to tangent matrix
                                                                                                                                                                                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                                                                                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                                                                                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                                                                                                        // inverse transform matrix
                                                                                                                                                                                                        half3x3 w2tRotation;
                                                                                                                                                                                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                                                                                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                                                                                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                                                                                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                                                                                                        w2tRotation *= rcp(det);

                                                                                                                                                                                                        // apply world to tangent transform to flat world normal
                                                                                                                                                                                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                                                                                                        o.Metallic = IN.uv_MainTex.y;
                                                                                                                                                                                                        o.Smoothness = IN.uv_MainTex.x;
                                                                                                                                                                                                        o.Albedo = IN.color; // vertex RGB
                                                                                                                                                                                                #endif
                                                                                                                                                                                                    }

                                                                                                                                                                                        #include "UnityMetaPass.cginc"

                                                                                                                                                                                                    // vertex-to-fragment interpolation data
                                                                                                                                                                                                    struct v2f_surf {
                                                                                                                                                                                                      UNITY_POSITION(pos);
                                                                                                                                                                                                      float4 tSpace0 : TEXCOORD0;
                                                                                                                                                                                                      float4 tSpace1 : TEXCOORD1;
                                                                                                                                                                                                      float4 tSpace2 : TEXCOORD2;
                                                                                                                                                                                                      nointerpolation fixed4 color : COLOR0;
                                                                                                                                                                                                      float3 custompack0 : TEXCOORD3; // cameraRelativeWorldPos
                                                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                                      float2 vizUV : TEXCOORD4;
                                                                                                                                                                                                      float4 lightCoord : TEXCOORD5;
                                                                                                                                                                                                    #endif
                                                                                                                                                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                                                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                                                                                                    };

                                                                                                                                                                                                    // vertex shader
                                                                                                                                                                                                    v2f_surf vert_surf(appdata_full v) {
                                                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                                                                                                      v2f_surf o;
                                                                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                                                                                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                                                                                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                                                                                                      Input customInputData;
                                                                                                                                                                                                      vert(v, customInputData);
                                                                                                                                                                                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                                                                                                      o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
                                                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                                      o.vizUV = 0;
                                                                                                                                                                                                      o.lightCoord = 0;
                                                                                                                                                                                                      if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
                                                                                                                                                                                                        o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
                                                                                                                                                                                                      else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
                                                                                                                                                                                                      {
                                                                                                                                                                                                        o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                                                                                                        o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
                                                                                                                                                                                                      }
                                                                                                                                                                                                    #endif
                                                                                                                                                                                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                                                                                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                                                                                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                                                                                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                                                                                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                                                                                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                                                                                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                                                                                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                                                                                                      o.color = v.color;
                                                                                                                                                                                                      return o;
                                                                                                                                                                                                    }

                                                                                                                                                                                                    // fragment shader
                                                                                                                                                                                                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                                                                                                    // prepare and unpack data
                                                                                                                                                                                                    Input surfIN;
                                                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                                                                                                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                                                                                                    #else
                                                                                                                                                                                                      UNITY_EXTRACT_FOG(IN);
                                                                                                                                                                                                    #endif
                                                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                                                      UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                                                                                                    #else
                                                                                                                                                                                                      UNITY_EXTRACT_TBN(IN);
                                                                                                                                                                                                    #endif
                                                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                                                                                                    surfIN.uv_MainTex.x = 1.0;
                                                                                                                                                                                                    surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                                                                                                    surfIN.worldNormal.x = 1.0;
                                                                                                                                                                                                    surfIN.color.x = 1.0;
                                                                                                                                                                                                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                                                                                                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                                                                                                    #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                                                                                                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                                                                                                    #else
                                                                                                                                                                                                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                                                                                                    #endif
                                                                                                                                                                                                    surfIN.color = IN.color;
                                                                                                                                                                                                    #ifdef UNITY_COMPILER_HLSL
                                                                                                                                                                                                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                                                                                                    #else
                                                                                                                                                                                                    SurfaceOutputStandard o;
                                                                                                                                                                                                    #endif
                                                                                                                                                                                                    o.Albedo = 0.0;
                                                                                                                                                                                                    o.Emission = 0.0;
                                                                                                                                                                                                    o.Alpha = 0.0;
                                                                                                                                                                                                    o.Occlusion = 1.0;
                                                                                                                                                                                                    fixed3 normalWorldVertex = fixed3(0,0,1);

                                                                                                                                                                                                    // call surface function
                                                                                                                                                                                                    surf(surfIN, o);
                                                                                                                                                                                                    UnityMetaInput metaIN;
                                                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
                                                                                                                                                                                                    metaIN.Albedo = o.Albedo;
                                                                                                                                                                                                    metaIN.Emission = o.Emission;
                                                                                                                                                                                                  #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                                    metaIN.VizUV = IN.vizUV;
                                                                                                                                                                                                    metaIN.LightCoord = IN.lightCoord;
                                                                                                                                                                                                  #endif
                                                                                                                                                                                                    return UnityMetaFragment(metaIN);
                                                                                                                                                                                                  }


                                                                                                                                                                                                  #endif

                                                                                                                                                                                                        // -------- variant for: _INVERSEMATRIX_EXACT INSTANCING_ON 
                                                                                                                                                                                                        #if defined(_INVERSEMATRIX_EXACT) && defined(INSTANCING_ON)
                                                                                                                                                                                                        // Surface shader code generated based on:
                                                                                                                                                                                                        // vertex modifier: 'vert'
                                                                                                                                                                                                        // writes to per-pixel normal: YES
                                                                                                                                                                                                        // writes to emission: no
                                                                                                                                                                                                        // writes to occlusion: no
                                                                                                                                                                                                        // needs world space reflection vector: no
                                                                                                                                                                                                        // needs world space normal vector: no
                                                                                                                                                                                                        // needs screen space position: no
                                                                                                                                                                                                        // needs world space position: no
                                                                                                                                                                                                        // needs view direction: no
                                                                                                                                                                                                        // needs world space view direction: no
                                                                                                                                                                                                        // needs world space position for lighting: YES
                                                                                                                                                                                                        // needs world space view direction for lighting: YES
                                                                                                                                                                                                        // needs world space view direction for lightmaps: no
                                                                                                                                                                                                        // needs vertex color: YES
                                                                                                                                                                                                        // needs VFACE: no
                                                                                                                                                                                                        // passes tangent-to-world matrix to pixel shader: YES
                                                                                                                                                                                                        // reads from normal: no
                                                                                                                                                                                                        // 0 texcoords actually used
                                                                                                                                                                                                        #include "UnityCG.cginc"
                                                                                                                                                                                                        #include "Lighting.cginc"
                                                                                                                                                                                                        #include "UnityPBSLighting.cginc"

                                                                                                                                                                                                        #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
                                                                                                                                                                                                        #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
                                                                                                                                                                                                        #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

                                                                                                                                                                                                        // Original surface shader snippet:
                                                                                                                                                                                                        #line 6
                                                                                                                                                                                                        /* UNITY: Original start of shader */
                                                                                                                                                                                                                    //#pragma surface surf Standard fullforwardshadows vertex:vert
                                                                                                                                                                                                                    //#pragma target 3.0
                                                                                                                                                                                                                    //#pragma shader_feature _ _INVERSEMATRIX_EXACT

                                                                                                                                                                                                                    sampler2D _MainTex;

                                                                                                                                                                                                                    struct Input {
                                                                                                                                                                                                                        float2 uv_MainTex;
                                                                                                                                                                                                                        float3 cameraRelativeWorldPos;
                                                                                                                                                                                                                        float3 worldNormal;
                                                                                                                                                                                                                        float3 color : COLOR;
                                                                                                                                                                                                                        INTERNAL_DATA
                                                                                                                                                                                                                    };


                                                                                                                                                                                                                    // pass camera relative world position from vertex to fragment
                                                                                                                                                                                                                    void vert(inout appdata_full v, out Input o)
                                                                                                                                                                                                                    {
                                                                                                                                                                                                                        UNITY_INITIALIZE_OUTPUT(Input,o);
                                                                                                                                                                                                                        o.cameraRelativeWorldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) - _WorldSpaceCameraPos.xyz;
                                                                                                                                                                                                                    }

                                                                                                                                                                                                                    void surf(Input IN, inout SurfaceOutputStandard o) {



                                                                                                                                                                                                                #if !defined(UNITY_PASS_META)
                                                                                                                                                                                                                        // flat world normal from position derivatives
                                                                                                                                                                                                                        half3 flatWorldNormal = normalize(cross(ddy(IN.cameraRelativeWorldPos.xyz), ddx(IN.cameraRelativeWorldPos.xyz)));

                                                                                                                                                                                                                        // construct world to tangent matrix
                                                                                                                                                                                                                        half3 worldT = WorldNormalVector(IN, half3(1,0,0));
                                                                                                                                                                                                                        half3 worldB = WorldNormalVector(IN, half3(0,1,0));
                                                                                                                                                                                                                        half3 worldN = WorldNormalVector(IN, half3(0,0,1));

                                                                                                                                                                                                                        // inverse transform matrix
                                                                                                                                                                                                                        half3x3 w2tRotation;
                                                                                                                                                                                                                        w2tRotation[0] = worldB.yzx * worldN.zxy - worldB.zxy * worldN.yzx;
                                                                                                                                                                                                                        w2tRotation[1] = worldT.zxy * worldN.yzx - worldT.yzx * worldN.zxy;
                                                                                                                                                                                                                        w2tRotation[2] = worldT.yzx * worldB.zxy - worldT.zxy * worldB.yzx;

                                                                                                                                                                                                                        half det = dot(worldT.xyz, w2tRotation[0]);

                                                                                                                                                                                                                        w2tRotation *= rcp(det);

                                                                                                                                                                                                                        // apply world to tangent transform to flat world normal
                                                                                                                                                                                                                        o.Normal = mul(w2tRotation, flatWorldNormal);
                                                                                                                                                                                                                        o.Metallic = IN.uv_MainTex.y;
                                                                                                                                                                                                                        o.Smoothness = IN.uv_MainTex.x;
                                                                                                                                                                                                                        o.Albedo = IN.color; // vertex RGB
                                                                                                                                                                                                                #endif
                                                                                                                                                                                                                    }

                                                                                                                                                                                                        #include "UnityMetaPass.cginc"

                                                                                                                                                                                                                    // vertex-to-fragment interpolation data
                                                                                                                                                                                                                    struct v2f_surf {
                                                                                                                                                                                                                      UNITY_POSITION(pos);
                                                                                                                                                                                                                      float4 tSpace0 : TEXCOORD0;
                                                                                                                                                                                                                      float4 tSpace1 : TEXCOORD1;
                                                                                                                                                                                                                      float4 tSpace2 : TEXCOORD2;
                                                                                                                                                                                                                      nointerpolation fixed4 color : COLOR0;
                                                                                                                                                                                                                      float3 custompack0 : TEXCOORD3; // cameraRelativeWorldPos
                                                                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                                                      float2 vizUV : TEXCOORD4;
                                                                                                                                                                                                                      float4 lightCoord : TEXCOORD5;
                                                                                                                                                                                                                    #endif
                                                                                                                                                                                                                      UNITY_VERTEX_INPUT_INSTANCE_ID
                                                                                                                                                                                                                      UNITY_VERTEX_OUTPUT_STEREO
                                                                                                                                                                                                                    };

                                                                                                                                                                                                                    // vertex shader
                                                                                                                                                                                                                    v2f_surf vert_surf(appdata_full v) {
                                                                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(v);
                                                                                                                                                                                                                      v2f_surf o;
                                                                                                                                                                                                                      UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                                                                                                                                                                                                                      UNITY_TRANSFER_INSTANCE_ID(v,o);
                                                                                                                                                                                                                      UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                                                                                                                                                                                                                      Input customInputData;
                                                                                                                                                                                                                      vert(v, customInputData);
                                                                                                                                                                                                                      o.custompack0.xyz = customInputData.cameraRelativeWorldPos;
                                                                                                                                                                                                                      o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
                                                                                                                                                                                                                    #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                                                      o.vizUV = 0;
                                                                                                                                                                                                                      o.lightCoord = 0;
                                                                                                                                                                                                                      if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
                                                                                                                                                                                                                        o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
                                                                                                                                                                                                                      else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
                                                                                                                                                                                                                      {
                                                                                                                                                                                                                        o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                                                                                                                                                                                                                        o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
                                                                                                                                                                                                                      }
                                                                                                                                                                                                                    #endif
                                                                                                                                                                                                                      float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                                                                                                                                                                                                                      float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                                                                                                                                                                                                                      fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                                                                                                                                                                                                                      fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                                                                                                                                                                                                      fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                                                                                                                                                                                                                      o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                                                                                                                                                                                                                      o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                                                                                                                                                                                                                      o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                                                                                                                                                                                                                      o.color = v.color;
                                                                                                                                                                                                                      return o;
                                                                                                                                                                                                                    }

                                                                                                                                                                                                                    // fragment shader
                                                                                                                                                                                                                    fixed4 frag_surf(v2f_surf IN) : SV_Target {
                                                                                                                                                                                                                      UNITY_SETUP_INSTANCE_ID(IN);
                                                                                                                                                                                                                    // prepare and unpack data
                                                                                                                                                                                                                    Input surfIN;
                                                                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                                                                                                                                                                                                                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                                                                                                                                                                                                                      UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                                                                                                                                                                                                                    #else
                                                                                                                                                                                                                      UNITY_EXTRACT_FOG(IN);
                                                                                                                                                                                                                    #endif
                                                                                                                                                                                                                    #ifdef FOG_COMBINED_WITH_TSPACE
                                                                                                                                                                                                                      UNITY_RECONSTRUCT_TBN(IN);
                                                                                                                                                                                                                    #else
                                                                                                                                                                                                                      UNITY_EXTRACT_TBN(IN);
                                                                                                                                                                                                                    #endif
                                                                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                                                                                                                                                                                                                    surfIN.uv_MainTex.x = 1.0;
                                                                                                                                                                                                                    surfIN.cameraRelativeWorldPos.x = 1.0;
                                                                                                                                                                                                                    surfIN.worldNormal.x = 1.0;
                                                                                                                                                                                                                    surfIN.color.x = 1.0;
                                                                                                                                                                                                                    surfIN.cameraRelativeWorldPos = IN.custompack0.xyz;
                                                                                                                                                                                                                    float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                                                                                                                                                                                                                    #ifndef USING_DIRECTIONAL_LIGHT
                                                                                                                                                                                                                      fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                                                                                                                                                                                                                    #else
                                                                                                                                                                                                                      fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                                                                                                                                                                                                                    #endif
                                                                                                                                                                                                                    surfIN.color = IN.color;
                                                                                                                                                                                                                    #ifdef UNITY_COMPILER_HLSL
                                                                                                                                                                                                                    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                                                                                                                                                                                                                    #else
                                                                                                                                                                                                                    SurfaceOutputStandard o;
                                                                                                                                                                                                                    #endif
                                                                                                                                                                                                                    o.Albedo = 0.0;
                                                                                                                                                                                                                    o.Emission = 0.0;
                                                                                                                                                                                                                    o.Alpha = 0.0;
                                                                                                                                                                                                                    o.Occlusion = 1.0;
                                                                                                                                                                                                                    fixed3 normalWorldVertex = fixed3(0,0,1);

                                                                                                                                                                                                                    // call surface function
                                                                                                                                                                                                                    surf(surfIN, o);
                                                                                                                                                                                                                    UnityMetaInput metaIN;
                                                                                                                                                                                                                    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
                                                                                                                                                                                                                    metaIN.Albedo = o.Albedo;
                                                                                                                                                                                                                    metaIN.Emission = o.Emission;
                                                                                                                                                                                                                  #ifdef EDITOR_VISUALIZATION
                                                                                                                                                                                                                    metaIN.VizUV = IN.vizUV;
                                                                                                                                                                                                                    metaIN.LightCoord = IN.lightCoord;
                                                                                                                                                                                                                  #endif
                                                                                                                                                                                                                    return UnityMetaFragment(metaIN);
                                                                                                                                                                                                                  }


                                                                                                                                                                                                                  #endif


                                                                                                                                                                                                                  ENDCG

                                                                                                                                                                                                                  }

                                                                                                                                                        // ---- end of surface shader generated code

                                                                                                                                                    #LINE 61

    }
        FallBack "Diffuse"
}