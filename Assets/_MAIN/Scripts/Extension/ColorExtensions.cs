using UnityEngine;

public static class ColorExtensions
{   
    public static Color SetAlpha(this Color original, float alpha)
    {
        return new Color(original.r, original.g, original.b, alpha);
    }
    public static Color GetColorFromName(this Color original, string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green;
            case "blue":
                return Color.blue;
            case "yellow":
                return Color.yellow;
            case "white":
                return Color.white;
            case "black":
                return Color.black;
            case "gray":
                return Color.gray;
            case "cyan":
                return Color.cyan;
            case "magenta":
                return Color.magenta;
            case "orange":
                return new Color(1f, 0.5f, 0f); //橙色不是预定义的颜色，所以自定义橙色
            default:
                Debug.LogWarning("不支持的颜色名称: " + colorName);
                return Color.clear;
        }
    }



}
