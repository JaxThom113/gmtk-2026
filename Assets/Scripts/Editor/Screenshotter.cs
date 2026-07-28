using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.IO;
using UnityEngine.Rendering.Universal;

/*

In order to use this script, you must have a Main Camera and a UI Camera
- add UI camera to the stack of the Main Camera
- click on the Main Camera > Tools > Screenshotter

*/

public class ScreenshotExporter
{
    private const int width = 1920; 
    private const int height = 1080; 

    [MenuItem("Tools/Screenshotter/Export Screenshot")]
    static void Export()
    {
        Camera sceneCam = Selection.activeGameObject?.GetComponent<Camera>();
        if (sceneCam == null)
        {
            Debug.LogError("Select your Base (scene) camera first.");
            return;
        }

        Camera uiCam = GameObject.Find("UI Camera")?.GetComponent<Camera>();
        if (uiCam == null)
        {
            Debug.LogError("Could not find UI camera.");
            return;
        }

        /*
            Render Scene
            (HDR so bloom/emissive isn't clipped)
        */

        bool wasHDR = sceneCam.allowHDR;
        sceneCam.allowHDR = true;

        RenderTexture sceneRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
        sceneRT.Create();
        sceneCam.targetTexture = sceneRT;
        sceneCam.Render();

        Texture2D sceneTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture.active = sceneRT;
        sceneTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        sceneTex.Apply();
        sceneCam.targetTexture = null;
        sceneCam.allowHDR = wasHDR;

        /*
            Render UI
            (forced to Base + transparent)
        */

        var uiCamData = uiCam.GetUniversalAdditionalCameraData();
        var originalRenderType = uiCamData.renderType;
        var originalBgColor = uiCam.backgroundColor;
        var originalClearFlags = uiCam.clearFlags;

        uiCamData.renderType = CameraRenderType.Base;
        uiCam.clearFlags = CameraClearFlags.SolidColor;
        uiCam.backgroundColor = new Color(0, 0, 0, 0);

        RenderTexture uiRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        uiRT.Create();
        uiCam.targetTexture = uiRT;
        uiCam.Render();

        Texture2D uiTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture.active = uiRT;
        uiTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        uiTex.Apply();
        uiCam.targetTexture = null;

        uiCamData.renderType = originalRenderType;
        uiCam.clearFlags = originalClearFlags;
        uiCam.backgroundColor = originalBgColor;
        RenderTexture.active = null;

        /*
            Composite UI Over Scene 
            (straight alpha blend)
        */

        Color[] scenePixels = sceneTex.GetPixels();
        Color[] uiPixels = uiTex.GetPixels();
        Color[] outPixels = new Color[scenePixels.Length];

        for (int i = 0; i < outPixels.Length; i++)
        {
            Color s = scenePixels[i].gamma;
            Color u = uiPixels[i];
            
            outPixels[i] = u + s * (1f - u.a);
            outPixels[i].a = 1f;
        }

        Texture2D finalTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        finalTex.SetPixels(outPixels);
        finalTex.Apply();

        /*
            Save & Cleanup
        */

        byte[] bytes = finalTex.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel("Save Screenshot", "", "Screenshot.png", "png");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log("Saved to " + path);
        }

        sceneRT.Release();
        uiRT.Release();
        Object.DestroyImmediate(sceneRT);
        Object.DestroyImmediate(uiRT);
        Object.DestroyImmediate(sceneTex);
        Object.DestroyImmediate(uiTex);
        Object.DestroyImmediate(finalTex);
    }

    [MenuItem("Tools/Screenshotter/Export Screenshot No BG")]
    static void ExportNoBG()
    {
        Camera sceneCam = Selection.activeGameObject?.GetComponent<Camera>();
        if (sceneCam == null)
        {
            Debug.LogError("Select your Base (scene) camera first.");
            return;
        }

        Camera uiCam = GameObject.Find("UI Camera")?.GetComponent<Camera>();
        if (uiCam == null)
        {
            Debug.LogError("Could not find UI camera.");
            return;
        }

        /*
            Render Scene
        */

        CameraClearFlags originalClearFlags = sceneCam.clearFlags;
        Color originalBgColor = sceneCam.backgroundColor;
        bool originalHDR = sceneCam.allowHDR;

        var sceneCamData = sceneCam.GetUniversalAdditionalCameraData();
        bool originalPost = sceneCamData.renderPostProcessing;

        sceneCam.clearFlags = CameraClearFlags.SolidColor;
        sceneCam.backgroundColor = new Color(0, 0, 0, 0);
        sceneCam.allowHDR = true;
        sceneCamData.renderPostProcessing = false;

        RenderTexture sceneRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
        sceneRT.Create();
        sceneCam.targetTexture = sceneRT;
        sceneCam.Render();

        Texture2D sceneTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture.active = sceneRT;
        sceneTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        sceneTex.Apply();

        sceneCam.targetTexture = null;
        sceneCam.clearFlags = originalClearFlags;
        sceneCam.backgroundColor = originalBgColor;
        sceneCam.allowHDR = originalHDR;
        sceneCamData.renderPostProcessing = originalPost;

        /*
            Render UI
        */

        var uiCamData = uiCam.GetUniversalAdditionalCameraData();

        var originalRenderType = uiCamData.renderType;
        var originalUIClear = uiCam.clearFlags;
        var originalUIBg = uiCam.backgroundColor;

        uiCamData.renderType = CameraRenderType.Base;
        uiCam.clearFlags = CameraClearFlags.SolidColor;
        uiCam.backgroundColor = new Color(0, 0, 0, 0);

        RenderTexture uiRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        uiRT.Create();
        uiCam.targetTexture = uiRT;
        uiCam.Render();

        Texture2D uiTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture.active = uiRT;
        uiTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        uiTex.Apply();

        uiCam.targetTexture = null;
        uiCamData.renderType = originalRenderType;
        uiCam.clearFlags = originalUIClear;
        uiCam.backgroundColor = originalUIBg;
        
        RenderTexture.active = null;

        /*
            Composite
        */

        Color[] scenePixels = sceneTex.GetPixels();
        Color[] uiPixels = uiTex.GetPixels();
        Color[] outPixels = new Color[scenePixels.Length];

        for (int i = 0; i < outPixels.Length; i++)
        {
            Color s = scenePixels[i].gamma;
            Color u = uiPixels[i];

            Color result = u + s * (1f - u.a);
            result.a = Mathf.Max(s.a, u.a);
            outPixels[i] = result;
        }

        Texture2D finalTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        finalTex.SetPixels(outPixels);
        finalTex.Apply();

        /*
            Save & Cleanup
        */
        byte[] bytes = finalTex.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel("Save Screenshot", "", "Screenshot_No_BG.png", "png");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log("Saved to " + path);
        }

        sceneRT.Release();
        uiRT.Release();
        Object.DestroyImmediate(sceneRT);
        Object.DestroyImmediate(uiRT);
        Object.DestroyImmediate(sceneTex);
        Object.DestroyImmediate(uiTex);
        Object.DestroyImmediate(finalTex);
    }
}