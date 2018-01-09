﻿using UnityEngine;
using UnityEditor;

public class GridBaseBean
{
    /// <summary>
    /// 背景上方的方块
    /// </summary>
    public GridBean gridBean;

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

    /// <summary>
    /// 检测僵局时使用，若一次检测中被检测过，则为true
    /// </summary>
    public bool isCheck = false;
}