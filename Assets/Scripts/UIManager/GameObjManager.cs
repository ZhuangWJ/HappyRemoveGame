using System.Collections.Generic;
using UnityEngine;

public class GameObjManager
{
    /// <summary>
    /// 游戏背景图父对象
    /// </summary>
    public GameObject gameBackground;

    /// <summary>
    /// 格子背景父对象
    /// </summary>
    public GameObject gridBg;

    /// <summary>
    /// 格子父对象
    /// </summary>
    public GameObject grid;

    /// <summary>
    /// GridUI的资源列表数组
    /// </summary>
    public List<Sprite> sprites;

    /// <summary>
    ///  游戏内容格子的管理者
    /// </summary>
    public List<List<GridBean>> gridListManager;

    /// <summary>
    /// 游戏格子背景管理者
    /// </summary>
    public List<List<GridBaseBean>> gridBaseListManager;

    /// <summary>
    /// 是否需要更新游戏内容格子的管理者
    /// </summary>
    public bool isUpdateGridListManager = false;

}