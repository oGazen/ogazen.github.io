using UnityEngine;

public static class CommonTool
{
    public static string GetBaseAssetPath()
    {
#if UNITY_EDITOR
        return Application.dataPath;
#else
        return Application.persistentDataPath;
#endif
    }
}
