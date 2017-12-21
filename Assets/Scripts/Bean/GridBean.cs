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
    /// 下落的格子数
    /// </summary>
    public int dropCounts;
    
    /// <summary>
    /// 是否在最顶层
    /// </summary>
    public bool isTop;

    /// <summary>
    /// 元素的类型
    /// </summary>
    public Sprite sprite;

}