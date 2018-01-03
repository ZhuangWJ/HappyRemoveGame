using System.Collections.Generic;
using UnityEngine;

public class EditorData 
{
    /// <summary>
    /// 第几关
    /// </summary>
    public int playLevel;

    /// <summary>
    /// 关卡通关消除类型
    /// </summary>
    public int targetType;

    /// <summary>
    /// 关卡通关目标数
    /// </summary>
    public int targetCounts;
    
    /// <summary>
    /// 关卡可用步数
    /// </summary>
    public int stepCounts;

    /// <summary>
    /// 关卡初次加载时固定的格子内容集合
    /// </summary>
    public List<List<GridBean>> gridContentList;

    /// <summary>
    /// 是否生成配置文件
    /// </summary>
    public bool isCreateJson;

    /// <summary>
    /// 目标类型的对象
    /// </summary>
    public GameObject targetTypeObj;

    /// <summary>
    /// 目标消除数量的对象
    /// </summary>
    public GameObject targetCountCountObj;

    /// <summary>
    /// 可用步数对象
    /// </summary>
    public GameObject stepCountsObj;

    /// <summary>
    /// 是否刷新了消除目标
    /// </summary>
    public bool isUpdataTarget;

    /// <summary>
    /// 元素格子数据
    /// </summary>
    public string gridData;

    /// <summary>
    /// 传送门数据
    /// </summary>
    public string doorData;

    /// <summary>
    /// 冰块数据
    /// </summary>
    public string iceData;

    /// <summary>
    /// 金豆荚数据
    /// </summary>
    public string beanData;

    /// <summary>
    /// 金豆荚篮子数据
    /// </summary>
    public string basketData;
}