using UnityEngine;
using UnityEditor;

public class GridBaseBean
{
    /// <summary>
    /// 格子背景对象
    /// </summary>
    public GameObject gridBase;

    /// <summary>
    /// 背景上方是否有方块
    /// </summary>
    public bool isHasGrid;

    /// <summary>
    /// 方块的资源索引，-1即为空
    /// </summary>
    public int spriteIndex;
}