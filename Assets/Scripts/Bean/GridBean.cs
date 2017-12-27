using UnityEngine;

public class GridBean
{
    /// <summary>
    /// 格子元素的对象
    /// </summary>
    public GameObject gridObject;

    /// <summary>
    /// Sprite的资源数组索引
    /// </summary>
    public int spritesIndex;

    /// <summary>
    /// 元素所在的行
    /// </summary>
    public int listHorizontal;

    /// <summary>
    /// 元素所在的列
    /// </summary>
    public int listVertical;

    /// <summary>
    /// 需要掉落的距离
    /// </summary>
    public float dropHeight;

    /// <summary>
    /// 横移的距离
    /// </summary>
    public float moveWidth;

    /// <summary>
    /// 横移的方向 -1为左边，1为右边
    /// </summary>
    public int moveDirection;

    /// <summary>
    /// 横移的次数
    /// </summary>
    public int moveCounts;

    /// <summary>
    /// 横移一格的距离
    /// </summary>
    public float hasMoveWidth;

    /// <summary>
    /// 下落的格子数
    /// </summary>
    public int dropCounts;
    
    /// <summary>
    /// 是否在最顶层
    /// </summary>
    public bool isTop;

    /// <summary>
    /// 从其他列补充的掉落距离
    /// </summary>
    public float dropHeightFromOther;

    /// <summary>
    /// 从其他列补充的掉落次数
    /// </summary>
    public int dropHeightFromOtherCounts;

    /// <summary>
    /// 掉落一格的距离
    /// </summary>
    public float hasDropHeight;

}