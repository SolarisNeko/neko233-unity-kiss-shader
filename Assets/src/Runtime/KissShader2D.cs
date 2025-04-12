using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
public class KissShader2D : MonoBehaviour
{
    // shader 参数
    private static readonly int ShaderProp_Progress01 = Shader.PropertyToID("_Progress01");

    // 特效的 material
    [SerializeField]
    public Material shaderMaterial;

    // 原始的 material
    private Material _originMaterial;

    // 目标图形组件
    private Graphic _targetGraphic;

    // 特效持续时间
    private float _effectDuration;

    // 特效计时器
    private float _effectTimer;

    // 特效是否循环播放
    private bool _isLooping;

    // 是否正在播放特效
    private bool _isPlaying;

    // 完成回调
    private Action _callbackForEnd;

    public float EffectDuration => _effectDuration;

    // 是否循环播放
    public bool IsLoopPlayEffect { get; private set; }

    // 当组件被添加时初始化
    private void Awake()
    {
        // 获取图形组件
        _targetGraphic = GetComponent<Graphic>();

        // 保存原始材质引用
        if (_targetGraphic != null)
        {
            _originMaterial = _targetGraphic.material;
        }
    }

    // 在销毁时恢复原始材质
    private void OnDestroy()
    {
        RestoreOriginalMaterial();
    }

    // 播放一次性特效
    public void PlayEffectOnce(
        float durationSecond,
        Action callbackForEnd = null
    )
    {
        // 如果材质无效则退出
        if (shaderMaterial == null || _targetGraphic == null)
        {
            return;
        }

        _callbackForEnd = callbackForEnd;
        // 保存原始材质（如果尚未保存）
        if (_originMaterial == null)
        {
            _originMaterial = _targetGraphic.material;
        }

        IsLoopPlayEffect = false;
        // 停止任何正在播放的特效
        StopEffect();

        // 设置特效材质（使用实例化以避免修改原始资源）
        shaderMaterial = new Material(shaderMaterial);
        _targetGraphic.material = shaderMaterial;

        // 设置特效参数
        _effectDuration = durationSecond;
        _effectTimer = 0f;
        _isLooping = false;
        _isPlaying = true;
    }

    // 循环播放特效
    public void PlayEffectLoop(
        float durationSecond
    )
    {
        // 如果材质无效则退出
        if (shaderMaterial == null || _targetGraphic == null)
        {
            return;
        }

        IsLoopPlayEffect = true;
        _callbackForEnd = null;
        // 保存原始材质（如果尚未保存）
        if (_originMaterial == null)
        {
            _originMaterial = _targetGraphic.material;
        }

        // 停止任何正在播放的特效
        StopEffect();

        // 设置特效材质（使用实例化以避免修改原始资源）
        this.shaderMaterial = new Material(shaderMaterial);
        _targetGraphic.material = this.shaderMaterial;

        // 设置特效参数
        _effectDuration = durationSecond;
        _effectTimer = 0f;
        _isLooping = true;
        _isPlaying = true;
    }

    // 停止当前特效播放
    public void StopEffect()
    {
        // 重置状态
        _isPlaying = false;

        // 恢复原始材质
        RestoreOriginalMaterial();

        // 完成
        _callbackForEnd?.Invoke();
        _callbackForEnd = null;
    }

    // 恢复原始材质
    private void RestoreOriginalMaterial()
    {
        // 如果目标图形组件存在且原始材质有效，则恢复原始材质
        if (_targetGraphic != null && _originMaterial != null)
        {
            _targetGraphic.material = _originMaterial;
        }
    }

    // 每帧更新特效状态
    private void Update()
    {
        // 如果没有在播放特效则退出
        if (!_isPlaying || shaderMaterial == null)
            return;

        // 更新计时器
        _effectTimer += Time.deltaTime;

        // 设置归一化进度
        shaderMaterial.SetFloat(ShaderProp_Progress01, Mathf.Clamp01(_effectTimer / _effectDuration));

        // 检查特效是否完成
        if (_effectTimer >= _effectDuration)
        {
            if (_isLooping)
            {
                // 循环播放则重置计时器
                _effectTimer = 0f;
            }
            else
            {
                // 一次性播放则停止特效
                StopEffect();
            }
        }
    }

    // 为编辑器模式添加OnDisable处理
    private void OnDisable()
    {
        // 确保组件禁用时恢复原始材质
        if (_isPlaying)
        {
            StopEffect();
        }
    }

    // 暂停特效播放
    public void PauseEffect()
    {
        _isPlaying = false;
    }

    // 恢复特效播放
    public void ResumeEffect()
    {
        if (shaderMaterial != null)
        {
            _isPlaying = true;
        }
    }

    // 设置特效材质属性（浮点值）
    public void SetShaderFloat(string propertyName, float value)
    {
        if (shaderMaterial != null && _isPlaying)
        {
            shaderMaterial.SetFloat(propertyName, value);
        }
    }

    // 设置特效材质属性（颜色值）
    public void SetShaderColor(string propertyName, Color value)
    {
        if (shaderMaterial != null && _isPlaying)
        {
            shaderMaterial.SetColor(propertyName, value);
        }
    }

    // 设置特效材质属性（纹理）
    public void SetShaderTexture(string propertyName, Texture value)
    {
        if (shaderMaterial != null && _isPlaying)
        {
            shaderMaterial.SetTexture(propertyName, value);
        }
    }
}