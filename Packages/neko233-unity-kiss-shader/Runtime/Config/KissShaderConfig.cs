using UnityEngine;

namespace KissShader
{
    [CreateAssetMenu(fileName = "KissShaderConfig", menuName = "KissShader/Config", order = 1000)]
    public class KissShaderConfig : ScriptableObject
    {
        // Shader 材质配置
        public Material material;
        
        // 基础效果配置
        [Range(0, 1)]
        public float intensity = 1f;
        
        // 颜色配置
        public Color color = Color.white;
        [Range(0, 1)]
        public float colorIntensity = 1f;
        
        // 采样配置
        [Range(0, 1)]
        public float samplingIntensity = 0.5f;
        [Range(0.5f, 10f)]
        public float samplingWidth = 1f;
        
        // 过渡效果配置
        [Range(0, 1)]
        public float transitionRate = 0.5f;
        public bool transitionReverse = false;
        public Texture transitionTexture;
        public Vector2 transitionTextureScale = Vector2.one;
        public Vector2 transitionTextureOffset = Vector2.zero;
        
        // 边缘效果配置
        [Range(0, 1)]
        public float edgeWidth = 0.5f;
        public Color edgeColor = Color.white;
        [Range(0, 1)]
        public float edgeShinyRate = 0.5f;
        
        // 渐变效果配置
        public Color gradationColor1 = Color.white;
        public Color gradationColor2 = Color.white;
        [Range(-1, 1)]
        public float gradationOffset = 0f;
        [Range(0.01f, 10f)]
        public float gradationScale = 1f;
        [Range(0, 360)]
        public float gradationRotation = 0f;
    }
}