using UnityEditor;
using UnityEngine;

namespace KissShader
{
    [CustomEditor(typeof(KissShader2D))]
    public class Editor_KissShader2D : Editor
    {
        private float _time = 0;
        private bool isPreviewingEffect = false;


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制默认检查器
            DrawDefaultInspector();

            // 获取目标组件
            KissShader2D targetScript = (KissShader2D)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("预览控制", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            // 添加预览效果按钮
            GUI.enabled = targetScript.shaderMaterial != null;
            if (GUILayout.Button("预览一次性效果"))
            {
                targetScript.PlayEffectOnce(3);
            }

            if (GUILayout.Button("预览循环效果"))
            {
                targetScript.PlayEffectLoop(3);
            }

            GUI.enabled = true;

            // 添加停止预览按钮
            GUI.enabled = isPreviewingEffect;
            if (GUILayout.Button("停止预览"))
            {
                targetScript.StopEffect();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 显示预览状态
            if (isPreviewingEffect)
            {
                EditorGUI.ProgressBar(
                    EditorGUILayout.GetControlRect(false, 20),
                    _time / targetScript.EffectDuration,
                    $"预览进度 {(_time / targetScript.EffectDuration * 100):F0}%"
                );

                EditorGUILayout.LabelField($"预览类型: {(targetScript.IsLoopPlayEffect ? "循环" : "一次性")}");
                EditorGUILayout.LabelField($"剩余时间: {Mathf.Max(0, targetScript.EffectDuration - _time):F1}秒");
            }

            serializedObject.ApplyModifiedProperties();
        }


        // 获取预览标题
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent(isPreviewingEffect ? "特效预览" : "原始外观");
        }
    }
}