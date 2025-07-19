// File: FOVMaterialFactory.cs
using UnityEngine;
using UnityEngine.Rendering;

public static class FOVMaterialFactory
{
    private static Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
    private static Shader standard = Shader.Find("Standard");

    public static Material CreateTransparent()
    {
        Material mat;
        if (urpLit != null)
        {
            mat = new Material(urpLit);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        }
        else
        {
            mat = new Material(standard);
            mat.SetFloat("_Mode", 3f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
        }
        mat.renderQueue = (int)RenderQueue.Transparent;
        return mat;
    }
}
