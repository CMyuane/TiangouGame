using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    /// <summary>
    /// 从指定的文件路径读取文本文件内容，并返回每一行的列表。
    /// </summary>
    /// <param name="filePath">文件路径，相对于 FilePaths.root 的路径。</param>
    /// <param name="includeBlankLines">是否包含空行，默认为 true。</param>
    /// <returns>包含文件内容的字符串列表。</returns>
    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        // 如果路径不是以 '/' 开头，则将其视为相对路径并拼接根路径
        if (!filePath.StartsWith('/'))
            filePath = FilePaths.root + filePath;

        List<string> lines = new List<string>();
        try
        {
            // 使用 StreamReader 逐行读取文件内容
            using(StreamReader sr = new StreamReader(filePath))
            {
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    // 根据 includeBlankLines 参数决定是否添加空行
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
        }
        catch(FileNotFoundException ex)
        {
            // 如果文件未找到，记录错误日志
            Debug.LogError($"File not found:'{ex.FileName}'");
        }

        return lines;
    }

    /// <summary>
    /// 从 Unity 的 Resources 文件夹加载指定路径的 TextAsset，并返回每一行的列表。
    /// </summary>
    /// <param name="filePath">Resources 文件夹中的资源路径（不包含扩展名）。</param>
    /// <param name="includeBlankLines">是否包含空行，默认为 true。</param>
    /// <returns>包含资源内容的字符串列表，如果资源未找到则返回 null。</returns>
    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true)
    {
        // 从 Resources 文件夹加载 TextAsset
        TextAsset asset = Resources.Load<TextAsset>(filePath);
        if(asset == null)
        {
            // 如果资源未找到，记录错误日志
            Debug.LogError($"Asset not Found:'{filePath}'");
            return null;
        }
        // 调用重载方法处理 TextAsset 内容
        return ReadTextAsset(asset,includeBlankLines);
    }
    /// <summary>
    /// 从给定的 TextAsset 对象中读取内容，并返回每一行的列表。
    /// </summary>
    /// <param name="asset">TextAsset 对象。</param>
    /// <param name="includeBlankLines">是否包含空行，默认为 true。</param>
    /// <returns>包含资源内容的字符串列表。</returns>
    public static List<string> ReadTextAsset(TextAsset asset, bool includeBlankLines = true)
    {
        List<string> lines = new List<string>();
        // 使用 StringReader 逐行读取 TextAsset 的文本内容
        using(StringReader sr =new StringReader(asset.text))
        {
            while(sr.Peek() > -1)// Peek() 返回下一个字符的 Unicode 编码，-1 表示结束
            {
                string line = sr.ReadLine();
                // 根据 includeBlankLines 参数决定是否添加空行
                if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }
        }

        return lines;
    }
}
