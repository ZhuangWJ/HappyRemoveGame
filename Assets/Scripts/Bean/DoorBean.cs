using UnityEngine;

public class DoorBean
{
    /// <summary>
    /// 传送门入口的列
    /// </summary>
    public int inVertical;

    /// <summary>
    /// 传送门入口的行
    /// </summary>
    public int inHorizontal;

    /// <summary>
    /// 传送门入口对象
    /// </summary>
    public GameObject indoor;

    /// <summary>
    /// 传送门出口的列
    /// </summary>
    public int outVertical = -1;

    /// <summary>
    /// 传送门出口的行
    /// </summary>
    public int outHorizontal;

    /// <summary>
    /// 传送门出口对象
    /// </summary>
    public GameObject outdoor;
}