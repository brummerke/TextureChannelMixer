#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

//credits
//ui and logic  http://twitter.com/brumCGI
//Get/SetChannelValue https://twitter.com/keepeetron/

public class TextureChannelCombiner : EditorWindow
{
    [MenuItem("Window/Generation/TextureChannelCombiner")]
    public static void ShowWindow()
    {
        GetWindow<TextureChannelCombiner>().ShowUtility();
    }

    private Texture2D debugR;
    private Texture2D debugG;
    private Texture2D debugB;
    private Texture2D debugA;

    private bool isAdditive = false;

    private void DebugTexture(Texture2D debugTarget)
    {
        if (debugTarget == null) return;

        var tpath = AssetDatabase.GetAssetPath(debugTarget);
        var t = new Texture2D(1, 1);
        t.LoadImage(System.IO.File.ReadAllBytes(tpath));

        var w = debugTarget.width;
        var h = debugTarget.height;

        debugR = new Texture2D(w, h);
        debugG = new Texture2D(w, h);
        debugB = new Texture2D(w, h);
        debugA = new Texture2D(w, h);

        ClearTarget(debugR, true);
        ClearTarget(debugG, true);
        ClearTarget(debugB, true);
        ClearTarget(debugA, true);

        debugR.SetChannelValues(0, debugTarget.GetChannelValues(0), false);
        debugG.SetChannelValues(1, debugTarget.GetChannelValues(1), false);
        debugB.SetChannelValues(2, debugTarget.GetChannelValues(2), false);
        debugA.SetChannelValues(0, debugTarget.GetChannelValues(3), false);
        debugA.SetChannelValues(1, debugTarget.GetChannelValues(3), false);
        debugA.SetChannelValues(2, debugTarget.GetChannelValues(3), false);

        debugR.Apply();
        debugG.Apply();
        debugB.Apply();
        debugA.Apply();
    }

    private Texture2D source, target;
    public int sourceChannel, targetChannel;

    public void CopyToChannel()
    {
        target.SetChannelValues(targetChannel, source.GetChannelValues(sourceChannel), isAdditive);
        File.WriteAllBytes(AssetDatabase.GetAssetPath(target), target.EncodeToPNG());
    }

    public void ClearTarget(Texture2D clearTarget, bool skipDisk)
    {
        var c = clearTarget.GetPixels();
        for (int i = 0; i < c.Length; i++) c[i] = Color.black;
        clearTarget.SetPixels(c);
        clearTarget.Apply();
        if (!skipDisk)
            File.WriteAllBytes(AssetDatabase.GetAssetPath(clearTarget), clearTarget.EncodeToPNG());
    }

    private void ClearChannel()
    {
        var c = target.GetPixels();
        for (int i = 0; i < c.Length; i++)
            c[i][targetChannel] = 0f;
        target.SetPixels(c);
        target.Apply();

        File.WriteAllBytes(AssetDatabase.GetAssetPath(target), target.EncodeToPNG());
    }

    public void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Source Texture");
        GUILayout.Label("Target Texture");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        source = (Texture2D)EditorGUILayout.ObjectField(source, typeof(Texture2D), false);
        target = (Texture2D)EditorGUILayout.ObjectField(target, typeof(Texture2D), false);
        GUILayout.EndHorizontal();

        sourceChannel = EditorGUILayout.IntSlider("source channel:" + IntToChannel(sourceChannel), sourceChannel, 0, 3);
        GUILayout.BeginHorizontal();
        targetChannel = EditorGUILayout.IntSlider("target channel:" + IntToChannel(targetChannel), targetChannel, 0, 3);
        GUILayout.Label("Additive", GUILayout.Width(50f));
        isAdditive = EditorGUILayout.Toggle(isAdditive, GUILayout.Width(15f));

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Copy to existing texture"))
        {
            CopyToChannel();
            DebugTexture(target);
        }
        if (GUILayout.Button("Clear target texture's target channel"))
        {
            ClearChannel();
            DebugTexture(target);
        }
        if (GUILayout.Button("Clear target texture"))
        {
            ClearTarget(target, false);
            DebugTexture(target);
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Debug Source"))
        {
            DebugTexture(source);
        }
        if (GUILayout.Button("Debug Target"))
        {
            DebugTexture(target);
        }
        GUILayout.EndHorizontal();

        if (debugR != null)
        {
            Rect r = EditorGUILayout.GetControlRect(false, 256);
            var down = Vector2.up * r.height / 2;
            var right = Vector2.right * r.width / 2;
            down.y++;
            right.x++;
            var s = r.size;
            s.x /= 2;
            s.y /= 2;
            s.x--;
            s.y--;

            Rect rr = new Rect(r.position, s);
            Rect rg = new Rect(r.position + right, s);
            Rect rb = new Rect(r.position + down, s);
            Rect ra = new Rect(r.position + down + right, s);

            EditorGUI.DrawPreviewTexture(rr, debugR, null, ScaleMode.ScaleToFit);
            EditorGUI.DrawPreviewTexture(rg, debugG, null, ScaleMode.ScaleToFit);
            EditorGUI.DrawPreviewTexture(rb, debugB, null, ScaleMode.ScaleToFit);
            EditorGUI.DrawPreviewTexture(ra, debugA, null, ScaleMode.ScaleToFit);
        }
    }

    private string IntToChannel(int c)
    {
        switch (c)
        {
            case 0: return "(R)";
            case 1: return "(G)";
            case 2: return "(B)";
            case 3: return "(A)";
        }
        return "ERR";
    }
}

public static class Texture2dExtension
{
    public static float[] GetChannelValues(this Texture2D tex, int channel)
    {
        var colors = tex.GetPixels();
        int len = colors.Length;
        float[] values = new float[len];
        switch (channel)
        {
            case 0: for (int i = 0; i < len; i++) values[i] = colors[i].r; break;
            case 1: for (int i = 0; i < len; i++) values[i] = colors[i].g; break;
            case 2: for (int i = 0; i < len; i++) values[i] = colors[i].b; break;
            case 3: for (int i = 0; i < len; i++) values[i] = colors[i].a; break;
        }
        return values;
    }

    public static void SetChannelValues(this Texture2D tex, int channel, float[] values, bool add)
    {
        var colors = tex.GetPixels();
        int len = colors.Length;
        if (add)
        {
            switch (channel)
            {
                case 0: for (int i = 0; i < len; i++) colors[i].r += values[i]; break;
                case 1: for (int i = 0; i < len; i++) colors[i].g += values[i]; break;
                case 2: for (int i = 0; i < len; i++) colors[i].b += values[i]; break;
                case 3: for (int i = 0; i < len; i++) colors[i].a += values[i]; break;
            }
        }
        else
        {
            switch (channel)
            {
                case 0: for (int i = 0; i < len; i++) colors[i].r = values[i]; break;
                case 1: for (int i = 0; i < len; i++) colors[i].g = values[i]; break;
                case 2: for (int i = 0; i < len; i++) colors[i].b = values[i]; break;
                case 3: for (int i = 0; i < len; i++) colors[i].a = values[i]; break;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
    }
}
#endif
