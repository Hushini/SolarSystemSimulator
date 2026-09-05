using UnityEngine;

// Budowa tła

public static class SkyboxBuilder
{
    // Wlacza skybox z tekstury 'Textures/stars' i zwraca true, jesli sie udalo.
    public static bool Apply()
    {
        Texture2D tex = Resources.Load<Texture2D>("Textures/stars");
        if (tex == null) return false;

        Material mat = new Material(Shader.Find("Skybox/Panoramic"));
        mat.SetTexture("_MainTex", tex);
        mat.SetInt("_Mapping", 1);     
        mat.SetFloat("_Exposure", 1f);
        mat.SetFloat("_Rotation", 0f);

        RenderSettings.skybox = mat;
        DynamicGI.UpdateEnvironment();

        if (Camera.main != null)
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        return true;
    }
}