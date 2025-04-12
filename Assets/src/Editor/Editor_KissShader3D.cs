using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace KissShader
{
    [CustomEditor(typeof(KissShader3D))]
    public class Editor_KissShader3D : Editor
    {
        private KissShader3D shader3D;
        private float previewProgress = 0f;
        private bool isPreviewActive = false;
        private float previewStartTime;
        private bool loopPreview = false;
        private float previewDuration = 1f;
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> previewMaterials = new Dictionary<Renderer, Material[]>();
        private bool materialsModified = false;

        private void OnEnable()
        {
            shader3D = (KissShader3D)target;
            EditorApplication.update += OnEditorUpdate;
            Undo.undoRedoPerformed += OnUndoRedo;
            
            // 确保序列化对象可以被编辑
            serializedObject.Update();
            
            // 在启用编辑器时存储原始材质
            StoreOriginalMaterials();
        }

        private void OnDisable()
        {
            // 如果有修改过材质，恢复原始状态
            if (materialsModified)
            {
                RestoreOriginalMaterials();
                CleanupPreviewMaterials();
            }
            
            EditorApplication.update -= OnEditorUpdate;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }
        
        private void OnUndoRedo()
        {
            // 当撤销/重做操作执行时，确保材质状态正确
            if (materialsModified)
            {
                UpdateShaderProgress(shader3D, previewProgress);
            }
        }

        private void StoreOriginalMaterials()
        {
            originalMaterials.Clear();
            
            // 如果目标为空，直接返回
            if (shader3D == null) return;
            
            // 获取当前对象上的渲染器
            Renderer renderer = shader3D.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 创建材质数组的深拷贝
                Material[] sharedMats = renderer.sharedMaterials;
                Material[] originalMats = new Material[sharedMats.Length];
                
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    originalMats[i] = sharedMats[i];
                }
                
                originalMaterials[renderer] = originalMats;
            }
        }

        private void OnEditorUpdate()
        {
            if (!isPreviewActive || shader3D == null) return;
            
            float elapsed = Time.realtimeSinceStartup - previewStartTime;
            float normalizedTime = Mathf.Clamp01(elapsed / previewDuration);
            
            // 更新预览进度
            if (normalizedTime >= 1f)
            {
                if (loopPreview)
                {
                    // 循环播放时重置时间
                    previewStartTime = Time.realtimeSinceStartup;
                    normalizedTime = 0f;
                }
                else
                {
                    // 单次播放完成后停止
                    isPreviewActive = false;
                }
            }
            
            // 更新进度值
            previewProgress = normalizedTime;
            UpdateShaderProgress(shader3D, previewProgress);
            
            // 请求重绘Inspector界面
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            // 更新序列化对象
            serializedObject.Update();
            
            // 绘制默认Inspector
            DrawDefaultInspector();
            
            // 如果目标为空，直接返回
            if (shader3D == null) return;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Shader预览控制", EditorStyles.boldLabel);
            
            // 绘制水平分割线
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // 预览持续时间调整
            EditorGUI.BeginChangeCheck();
            previewDuration = EditorGUILayout.FloatField("预览持续时间(秒)", previewDuration);
            previewDuration = Mathf.Max(0.1f, previewDuration); // 确保持续时间为正值
            
            // 手动预览进度控制滑块
            EditorGUI.BeginChangeCheck();
            previewProgress = EditorGUILayout.Slider("预览进度", previewProgress, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                // 如果手动调整了滑块，停止自动预览
                isPreviewActive = false;
                
                // 确保材质已创建
                if (!materialsModified)
                {
                    CreatePreviewMaterials();
                }
                
                UpdateShaderProgress(shader3D, previewProgress);
            }
            
            EditorGUILayout.Space(5);
            
            // 预览控制按钮行
            EditorGUILayout.BeginHorizontal();
            
            // 预览一次按钮
            GUI.enabled = !isPreviewActive || loopPreview;
            if (GUILayout.Button("预览一次", GUILayout.Height(30)))
            {
                StartPreview(false);
            }
            
            // 循环预览按钮
            GUI.enabled = !isPreviewActive || !loopPreview;
            if (GUILayout.Button("循环预览", GUILayout.Height(30)))
            {
                StartPreview(true);
            }
            
            // 停止预览按钮
            GUI.enabled = isPreviewActive;
            if (GUILayout.Button("停止预览", GUILayout.Height(30)))
            {
                StopPreview();
            }
            
            // 重置按钮状态
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // 还原按钮
            GUI.enabled = materialsModified;
            if (GUILayout.Button("还原材质", GUILayout.Height(30)))
            {
                RestoreOriginalMaterials();
                CleanupPreviewMaterials();
                previewProgress = 0f;
                materialsModified = false;
            }
            GUI.enabled = true;
            
            // 应用所有修改
            serializedObject.ApplyModifiedProperties();
        }
        
        private void UpdateShaderProgress(KissShader3D script, float progress)
        {
            // 如果目标为空，直接返回
            if (script == null) return;
            
            Renderer renderer = script.GetComponent<Renderer>();
            if (renderer == null || !previewMaterials.ContainsKey(renderer)) return;
            
            Material[] materials = previewMaterials[renderer];
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat != null && mat.HasProperty("_Progress01"))
                {
                    mat.SetFloat("_Progress01", progress);
                }
            }
        }
        
        private void CreatePreviewMaterials()
        {
            // 清理任何现有的预览材质
            CleanupPreviewMaterials();
            
            // 如果目标为空，直接返回
            if (shader3D == null) return;
            
            Renderer renderer = shader3D.GetComponent<Renderer>();
            if (renderer == null) return;
            
            Material[] sharedMaterials = renderer.sharedMaterials;
            Material[] instanceMaterials = new Material[sharedMaterials.Length];
            
            // 为每个材质创建实例
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                if (sharedMaterials[i] != null)
                {
                    // 使用Material构造函数创建新实例
                    instanceMaterials[i] = new Material(sharedMaterials[i]);
                    
                    // 为了调试，可以设置实例材质的名称
                    instanceMaterials[i].name = sharedMaterials[i].name + " (Preview)";
                }
            }
            
            // 存储预览材质
            previewMaterials[renderer] = instanceMaterials;
            
            // 应用预览材质到渲染器
            Undo.RecordObject(renderer, "Apply Preview Materials");
            renderer.materials = instanceMaterials; // 使用materials而非sharedMaterials
            
            materialsModified = true;
        }
        
        private void StartPreview(bool loop)
        {
            // 创建预览材质（如果尚未创建）
            if (!materialsModified)
            {
                CreatePreviewMaterials();
            }
            
            // 设置预览参数
            previewStartTime = Time.realtimeSinceStartup;
            loopPreview = loop;
            isPreviewActive = true;
        }
        
        private void StopPreview()
        {
            isPreviewActive = false;
        }
        
        private void RestoreOriginalMaterials()
        {
            // 停止任何活动预览
            isPreviewActive = false;
            
            // 如果目标为空，直接返回
            if (shader3D == null) return;
            
            // 遍历所有存储的原始材质
            foreach (var kvp in originalMaterials)
            {
                Renderer renderer = kvp.Key;
                Material[] materials = kvp.Value;
                
                if (renderer != null)
                {
                    // 记录撤销
                    Undo.RecordObject(renderer, "Restore Original Materials");
                    
                    // 恢复原始材质
                    renderer.sharedMaterials = materials;
                }
            }
            
            materialsModified = false;
        }
        
        private void CleanupPreviewMaterials()
        {
            // 销毁所有预览材质以防止内存泄漏
            foreach (var kvp in previewMaterials)
            {
                Material[] materials = kvp.Value;
                
                if (materials != null)
                {
                    foreach (Material mat in materials)
                    {
                        if (mat != null)
                        {
                            DestroyImmediate(mat);
                        }
                    }
                }
            }
            
            previewMaterials.Clear();
        }
        
        private Texture TryGetTexture(KissShader3D script)
        {
            // 尝试从材质中获取纹理用于预览
            if (script == null) return null;
            
            if (script.shaderMaterial != null && script.shaderMaterial.mainTexture != null)
            {
                return script.shaderMaterial.mainTexture;
            }
            
            Renderer renderer = script.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
            {
                return renderer.sharedMaterial.mainTexture;
            }
            
            return null;
        }
        
        public override bool HasPreviewGUI()
        {
            return true;
        }
        
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (shader3D == null) return;
            
            Texture tex = TryGetTexture(shader3D);
            if (tex != null)
            {
                EditorGUI.DrawPreviewTexture(r, tex);
            }
            else
            {
                EditorGUI.LabelField(r, "无预览纹理可用");
            }
        }
        
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Shader预览");
        }
    }
}