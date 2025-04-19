using UnityEngine;

/// <summary>
/// 文件路径管理类，用于存储和管理游戏数据的根路径。
/// </summary>
public class FilePaths
{
    // 游戏数据的根路径，基于 Unity 的 Application.dataPath
    public static readonly string root = $"{Application.dataPath}/gameData/";
}