using System;
using UnityEngine;

namespace KissShader
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class KissShader3D : MonoBehaviour
    {
        // 自定义特效材质
        [SerializeField]
        public Material shaderMaterial;

        // 特效持续时间
        [SerializeField]
        public float effectDuration = 1.0f;

        [SerializeField]
        public string _shaderPropertyName = "_Progress01";


        // 目标渲染器
        private MeshRenderer _renderer;

        // 原始材质
        private Material[] _originalMaterials;

        // 特效实例材质
        private Material[] _effectMaterials;

        // 特效计时器
        private float _effectTimer;

        // 是否循环播放
        private bool _isLooping;

        // 是否正在播放
        private bool _isPlaying;

        // 完成回调
        private Action _onEffectComplete;

        // 初始化
        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            StoreOriginalMaterials();
        }

        // 存储原始材质
        private void StoreOriginalMaterials()
        {
            if (_renderer != null)
            {
                _originalMaterials = _renderer.sharedMaterials;
            }
        }

        // 停止特效并恢复原始材质
        private void OnDisable()
        {
            if (_isPlaying)
            {
                StopEffect();
            }
        }

        // 在销毁时清理
        private void OnDestroy()
        {
            RestoreOriginalMaterials();
            CleanupEffectMaterials();
        }


        public void PlayEffectOnce(
            float duration,
            string shaderPropertyName = "_OnProgress01",
            Action onComplete = null)
        {
            this.PlayEffectOnce(shaderMaterial, duration, shaderPropertyName, onComplete);
        }

        // 播放一次性特效
        public void PlayEffectOnce(
            Material effectMaterial,
            float duration,
            string shaderPropertyName = "_OnProgress01",
            Action onComplete = null)
        {
            if (effectMaterial == null || _renderer == null)
            {
                Debug.LogError("Cannot play effect: Material or Renderer is null");
                return;
            }

            Debug.Log("Play effect: " + effectMaterial.name);
            
            // 保存回调
            if (shaderPropertyName != null)
            {
                _shaderPropertyName = shaderPropertyName;
            }

            _onEffectComplete = onComplete;

            // 确保有原始材质引用
            if (_originalMaterials == null || _originalMaterials.Length == 0)
            {
                StoreOriginalMaterials();
            }

            // 停止当前特效
            StopEffect();

            // 准备特效
            shaderMaterial = effectMaterial;
            effectDuration = duration;
            _isLooping = false;

            // 创建特效材质实例
            CreateEffectMaterialInstances();

            // 应用特效材质
            ApplyEffectMaterials();

            // 开始播放
            _effectTimer = 0f;
            _isPlaying = true;
        }

        public void PlayEffectLoop(float duration)
        {
            this.PlayEffectLoop(shaderMaterial, duration);
        }

        // 循环播放特效
        public void PlayEffectLoop(Material effectMaterial, float duration)
        {
            if (effectMaterial == null || _renderer == null)
            {
                Debug.LogWarning("Cannot play loop effect: Material or Renderer is null");
                return;
            }

            // 清除回调
            _onEffectComplete = null;

            // 确保有原始材质引用
            if (_originalMaterials == null || _originalMaterials.Length == 0)
            {
                StoreOriginalMaterials();
            }

            // 停止当前特效
            StopEffect();

            // 准备特效
            shaderMaterial = effectMaterial;
            effectDuration = duration;
            _isLooping = true;

            // 创建特效材质实例
            CreateEffectMaterialInstances();

            // 应用特效材质
            ApplyEffectMaterials();

            // 开始播放
            _effectTimer = 0f;
            _isPlaying = true;
        }


        // 停止特效
        public void StopEffect()
        {
            // 重置状态
            _isPlaying = false;

            // 恢复原始材质
            RestoreOriginalMaterials();

            // 清理特效材质
            CleanupEffectMaterials();

            // 调用完成回调
            if (!_isLooping && _onEffectComplete != null)
            {
                var callback = _onEffectComplete;
                _onEffectComplete = null;
                callback.Invoke();
            }
        }

        // 创建特效材质实例
        private void CreateEffectMaterialInstances()
        {
            if (_renderer == null || shaderMaterial == null)
                return;

            int materialCount = _originalMaterials.Length;
            _effectMaterials = new Material[materialCount];

            // for (int i = 0; i < materialCount; i++)
            // {
            //     _effectMaterials[i] = new Material(shaderMaterial);
            //
            //     // 复制主纹理（如果有）
            //     if (_originalMaterials[i].HasProperty("_MainTex") && _effectMaterials[i].HasProperty("_MainTex"))
            //     {
            //         _effectMaterials[i].SetTexture("_MainTex", _originalMaterials[i].GetTexture("_MainTex"));
            //     }
            // }
        }

        // 应用特效材质到渲染器
        private void ApplyEffectMaterials()
        {
            if (_renderer == null || _effectMaterials == null)
                return;

            _renderer.materials = _effectMaterials;
        }

        // 恢复原始材质
        private void RestoreOriginalMaterials()
        {
            if (_renderer != null && _originalMaterials != null && _originalMaterials.Length > 0)
            {
                _renderer.materials = _originalMaterials;
            }
        }

        // 清理特效材质实例
        private void CleanupEffectMaterials()
        {
            if (_effectMaterials != null)
            {
                for (int i = 0; i < _effectMaterials.Length; i++)
                {
                    if (_effectMaterials[i] != null)
                    {
                        if (Application.isPlaying)
                            Destroy(_effectMaterials[i]);
                        else
                            DestroyImmediate(_effectMaterials[i]);
                    }
                }

                _effectMaterials = null;
            }
        }

        // 更新特效
        private void Update()
        {
            if (!_isPlaying || _effectMaterials == null)
                return;

            // 没有持续时间
            if (effectDuration <= 0)
            {
                StopEffect();
                return;
            }

            // 更新计时器
            _effectTimer += Time.deltaTime;

            // 更新特效参数
            float progress = Mathf.Clamp01(_effectTimer / effectDuration);
            UpdateShaderParameters(progress);

            // 检查特效是否完成
            if (_effectTimer >= effectDuration)
            {
                if (_isLooping)
                {
                    // 循环播放，重置计时器
                    _effectTimer = 0f;
                }
                else
                {
                    // 一次性特效，停止播放
                    StopEffect();
                }
            }
        }

        // 更新Shader参数
        private void UpdateShaderParameters(float progress)
        {
            if (_effectMaterials == null)
                return;

            foreach (var material in _effectMaterials)
            {
                if (material != null)
                {
                    // 更新进度参数（如果shader中定义了该参数）
                    if (material.HasProperty(this._shaderPropertyName))
                    {
                        material.SetFloat(this._shaderPropertyName, progress);
                    }
                }
            }
        }

        // 暂停特效
        public void PauseEffect()
        {
            _isPlaying = false;
        }

        // 恢复特效
        public void ResumeEffect()
        {
            if (_effectMaterials != null)
            {
                _isPlaying = true;
            }
        }

        // 设置浮点参数
        public void SetShaderFloat(string propertyName, float value)
        {
            if (_effectMaterials == null)
                return;

            foreach (var material in _effectMaterials)
            {
                if (material != null && material.HasProperty(propertyName))
                {
                    material.SetFloat(propertyName, value);
                }
            }
        }

        // 设置颜色参数
        public void SetShaderColor(string propertyName, Color value)
        {
            if (_effectMaterials == null)
                return;

            foreach (var material in _effectMaterials)
            {
                if (material != null && material.HasProperty(propertyName))
                {
                    material.SetColor(propertyName, value);
                }
            }
        }

        // 设置纹理参数
        public void SetShaderTexture(string propertyName, Texture value)
        {
            if (_effectMaterials == null)
                return;

            foreach (var material in _effectMaterials)
            {
                if (material != null && material.HasProperty(propertyName))
                {
                    material.SetTexture(propertyName, value);
                }
            }
        }

        // 设置向量参数
        public void SetShaderVector(string propertyName, Vector4 value)
        {
            if (_effectMaterials == null)
                return;

            foreach (var material in _effectMaterials)
            {
                if (material != null && material.HasProperty(propertyName))
                {
                    material.SetVector(propertyName, value);
                }
            }
        }

        // 获取当前播放状态
        public bool IsPlaying => _isPlaying;

        // 获取当前循环状态
        public bool IsLooping => _isLooping;

        // 获取当前进度
        public float Progress => _isPlaying ? Mathf.Clamp01(_effectTimer / effectDuration) : 0f;

        // 获取剩余时间
        public float RemainingTime => _isPlaying ? Mathf.Max(0, effectDuration - _effectTimer) : 0f;
    }
}