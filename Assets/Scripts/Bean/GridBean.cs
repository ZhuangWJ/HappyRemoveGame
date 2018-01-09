using UnityEngine;
using UnityEngine.UI;

public class GridBean
{
    /// <summary>
    /// 格子方块的对象
    /// </summary>
    public GameObject gridObject;

    /// <summary>
    /// Sprite的资源数组索引
    /// </summary>
    public int spriteIndex;

    /// <summary>
    /// 方块所在的行
    /// </summary>
    public int listHorizontal;

    /// <summary>
    /// 方块所在的列
    /// </summary>
    public int listVertical;

    /// <summary>
    /// 需要掉落的距离
    /// </summary>
    public float dropHeight;

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
    /// 从其他列补充的掉落次数
    /// </summary>
    public int dropHeightFromOtherCounts;

    /// <summary>
    /// 是否进入传送门入口
    /// </summary>
    public bool isDropInDoor;

    /// <summary>
    /// 是否从传送门出口出来
    /// </summary>
    public bool isDropOutDoor;

    /// <summary>
    /// 掉落一格的距离
    /// </summary>
    public float hasDropHeight;

    /// <summary>
    /// 是否需要被清楚
    /// </summary>
    public bool isDestroy;

    /// <summary>
    /// 是否消除雪块，一次连线消除，只消除一次雪块
    /// </summary>
    public bool isFrostingRemove = false;

    /// <summary>
    /// GridBean的复制方法
    /// </summary>
    /// <param name="copyGridBean"></param>
    /// <param name="Grid"></param>
    /// <returns></returns>
    public static GridBean mClone(GridBean copyGridBean , GameObject Grid)
    {
        GridBean gridBean = new GridBean();
        GameObject gridObject = new GameObject();
        gridObject.AddComponent<RectTransform>();
        gridObject.GetComponent<RectTransform>().position = copyGridBean.gridObject.GetComponent<RectTransform>().position;
        gridObject.GetComponent<RectTransform>().sizeDelta = copyGridBean.gridObject.GetComponent<RectTransform>().sizeDelta;
        gridObject.AddComponent<Image>();
        gridObject.GetComponent<Image>().sprite = copyGridBean.gridObject.GetComponent<Image>().sprite;
        gridObject.transform.SetParent(Grid.transform);

        gridBean.gridObject = gridObject;
        gridBean.listHorizontal = copyGridBean.listHorizontal;
        gridBean.listVertical = copyGridBean.listVertical;
        gridBean.spriteIndex = copyGridBean.spriteIndex;

        gridBean.dropCounts = copyGridBean.dropCounts;
        gridBean.dropHeight = copyGridBean.dropHeight;

        gridBean.dropHeightFromOtherCounts = copyGridBean.dropHeightFromOtherCounts;
        gridBean.hasDropHeight = copyGridBean.hasDropHeight;

        gridBean.moveDirection = copyGridBean.moveDirection;
        gridBean.hasMoveWidth = copyGridBean.hasMoveWidth;
        gridBean.moveCounts = copyGridBean.moveCounts;

        gridBean.isTop = copyGridBean.isTop;
        gridBean.isDropInDoor = copyGridBean.isDropInDoor;
        gridBean.isDropOutDoor = copyGridBean.isDropOutDoor;
        gridBean.isDestroy = copyGridBean.isDestroy;
        gridBean.isFrostingRemove = copyGridBean.isFrostingRemove;

        return gridBean;
    }
}