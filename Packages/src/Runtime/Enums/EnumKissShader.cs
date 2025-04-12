using System.ComponentModel;
using UnityEngine.Rendering;


namespace KissShader
{
    [Description("KissShader枚举")]
    public enum EnumKissShader
    {
    }

    [Description("色调滤镜")]
    public enum ToneFilter
    {
        [Description("无")]
        None = 0,

        [Description("灰度")]
        Grayscale = 1,

        [Description("复古褐色")]
        Sepia = 2,

        [Description("负片效果")]
        Negative = 3,

        [Description("复古效果")]
        Retro = 4,

        [Description("色调分离")]
        Posterize = 5
    }

    [Description("颜色滤镜")]
    public enum ColorFilter
    {
        [Description("无")]
        None = 0,

        [Description("乘法混合")]
        Multiply = 1,

        [Description("加法混合")]
        Additive = 2,

        [Description("减法混合")]
        Subtractive = 3,

        [Description("替换颜色")]
        Replace = 4,

        [Description("亮度乘法")]
        MultiplyLuminance = 5,

        [Description("乘加混合")]
        MultiplyAdditive = 6,

        [Description("HSV颜色修改")]
        HsvModifier = 7,

        [Description("对比度调整")]
        Contrast = 8
    }

    [Description("采样滤镜")]
    public enum SamplingFilter
    {
        [Description("无")]
        None = 0,

        [Description("快速模糊")]
        BlurFast = 1,

        [Description("中等模糊")]
        BlurMedium = 2,

        [Description("精细模糊")]
        BlurDetail = 3,

        [Description("像素化")]
        Pixelation = 4,

        [Description("RGB通道偏移")]
        RgbShift = 5,

        [Description("亮度边缘检测")]
        EdgeLuminance = 6,

        [Description("透明度边缘检测")]
        EdgeAlpha = 7
    }

    [Description("过渡滤镜")]
    public enum TransitionFilter
    {
        [Description("无")]
        None = 0,

        [Description("淡入淡出")]
        Fade = 1,

        [Description("剪切过渡")]
        Cutoff = 2,

        [Description("溶解效果")]
        Dissolve = 3,

        [Description("闪亮效果")]
        Shiny = 4,

        [Description("蒙版过渡")]
        Mask = 5,

        [Description("融化效果")]
        Melt = 6,

        [Description("燃烧效果")]
        Burn = 7,

        [Description("图案过渡")]
        Pattern = 8
    }

    [Description("混合类型")]
    public enum BlendType
    {
        [Description("自定义")]
        Custom,

        [Description("透明混合")]
        AlphaBlend,

        [Description("乘法混合")]
        Multiply,

        [Description("加法混合")]
        Additive,

        [Description("柔和加法")]
        SoftAdditive,

        [Description("乘加混合")]
        MultiplyAdditive
    }

    [Description("目标模式")]
    public enum TargetMode
    {
        [Description("无")]
        None = 0,

        [Description("色相")]
        Hue = 1,

        [Description("亮度")]
        Luminance = 2
    }

    [Description("阴影模式")]
    public enum ShadowMode
    {
        [Description("无")]
        None = 0,

        [Description("阴影")]
        Shadow,

        [Description("三向阴影")]
        Shadow3,

        [Description("轮廓")]
        Outline,

        [Description("八向轮廓")]
        Outline8,

        [Description("镜像")]
        Mirror
    }

    [Description("边缘模式")]
    public enum EdgeMode
    {
        [Description("无")]
        None = 0,

        [Description("普通")]
        Plain,

        [Description("闪亮")]
        Shiny
    }

    [Description("图案区域")]
    public enum PatternArea
    {
        [Description("全部")]
        All = 0,

        [Description("内部")]
        Inner = 1,

        [Description("边缘")]
        Edge = 2
    }

    [Description("渐变模式")]
    public enum GradationMode
    {
        [Description("无")]
        None = 0,

        [Description("水平")]
        Horizontal = 1,

        [Description("水平渐变")]
        HorizontalGradient = 2,

        [Description("垂直")]
        Vertical = 3,

        [Description("垂直渐变")]
        VerticalGradient = 4,

        [Description("快速径向")]
        RadialFast = 5,

        [Description("精细径向")]
        RadialDetail = 6,

        [Description("对角线")]
        Diagonal = 11,

        [Description("对角线(右下)")]
        DiagonalToRightBottom = 7,

        [Description("对角线(左下)")]
        DiagonalToLeftBottom = 8,

        [Description("角度")]
        Angle = 9,

        [Description("角度渐变")]
        AngleGradient = 10
    }

    [Description("混合类型转换器")]
    internal static class BlendTypeConverter
    {
        [Description("将混合类型转换为源和目标混合模式")]
        public static (BlendMode, BlendMode) Convert(this (BlendType type, BlendMode src, BlendMode dst) self)
        {
            return self.type switch
            {
                BlendType.AlphaBlend => (BlendMode.One, BlendMode.OneMinusSrcAlpha),
                BlendType.Multiply => (BlendMode.DstColor, BlendMode.OneMinusSrcAlpha),
                BlendType.Additive => (BlendMode.One, BlendMode.One),
                BlendType.SoftAdditive => (BlendMode.OneMinusDstColor, BlendMode.One),
                BlendType.MultiplyAdditive => (BlendMode.DstColor, BlendMode.One),
                _ => (self.src, self.dst)
            };
        }

        [Description("将源和目标混合模式转换为混合类型")]
        public static BlendType Convert(this (BlendMode src, BlendMode dst) self)
        {
            return self switch
            {
                (BlendMode.One, BlendMode.OneMinusSrcAlpha) => BlendType.AlphaBlend,
                (BlendMode.DstColor, BlendMode.OneMinusSrcAlpha) => BlendType.Multiply,
                (BlendMode.One, BlendMode.One) => BlendType.Additive,
                (BlendMode.OneMinusDstColor, BlendMode.One) => BlendType.SoftAdditive,
                (BlendMode.DstColor, BlendMode.One) => BlendType.MultiplyAdditive,
                _ => BlendType.Custom
            };
        }
    }
}