using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridDrop :MonoBehaviour
{
    private static float dropY;
    private static float y;
    private static int dropGridCounts;
    private static float dropHeight;

    /// <summary>
    /// 执行元素掉落动画
    /// </summary>
    /// <param name="gameData">格子行列管理对象,用于获取行列数据</param>
    /// <param name="gridListManager">管理每列格子的List</param>
    /// <param name="gridSize">格子大小</param>
    public static void gridDrop(GameData gameData, List<List<GridBean>> gridListManager,float gridSize)
    {
        //[1]遍历所有列
        for (int i = 0; i < gameData.vertical; i++)
        {
            //[2]遍历数组，从后往前检测是否需要移动补充空位 , 检测位置
            for (int x = gameData.horizontal - 1; x >= 0; x--)
            {
                //[3]判断剩余List的最后一个对象是否处于最大的位置，若不是，则读取掉落距离并执行，每帧移动后并修改相关信息
                if (gridListManager[i][x].listHorizontal != x || (x == 0 && gridListManager[i][x].dropHeight > 0))
                {
                    //定义每帧掉落距离
                    dropY = gridSize * 0.1f;

                    //格子上边界
                    y = Screen.height * 0.75f; 

                    //元素掉落
                    if (gridListManager[i][x].dropHeight >= dropY)
                    {
                        gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropY, 0.0f);
                        gridListManager[i][x].dropHeight = gridListManager[i][x].dropHeight - dropY;
                    }
                    else
                    {
                        gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -gridListManager[i][x].dropHeight, 0.0f);
                        gridListManager[i][x].dropHeight = 0;
                        //修改GridBean下移后的listHorizontal信息
                        gridListManager[i][x].listHorizontal += gridListManager[i][x].dropCounts;
                        gridListManager[i][x].gridObject.name = "grid" + i.ToString() + gridListManager[i][x].listHorizontal.ToString();
                    }

                    //若为补充元素掉落，则掉落距离超过第一个格子一半，则开始显示
                    if (gridListManager[i][x].isTop && gridListManager[i][x].gridObject.GetComponent<RectTransform>().position.y <= y)
                    {
                        gridListManager[i][x].gridObject.SetActive(true);
                        gridListManager[i][x].isTop = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 记录元素掉落的信息
    /// </summary>
    /// <param name="gameData">格子行列管理对象,用于获取行列数据</param>
    /// <param name="gridListManager">管理每列格子的List</param>
    /// <param name="interval">格子之间的间隔</param>
    /// <param name="gridDropList">管理掉落备用行元素的List</param>
    /// <param name="Grid">格子的父对象</param>
    /// <param name="sprites">元素资源数组</param>
    /// <param name="gridSize">格子大小</param>
    public static void recordGridDropMsg(GameData gameData, List<List<GridBean>> gridListManager,float interval, List<GridBean> gridDropList,GameObject Grid,List<Sprite> sprites,float gridSize)
    {
        //[1]遍历所有列，若数量不等于9，则相应列进行掉落
        for (int i = 0; i < gameData.vertical; i++)
        {
            if (gridListManager[i].Count < 9)
            {
                //[2]遍历数组，从后往前检测是否需要移动补充空位 , 检测位置
                for (int x = gridListManager[i].Count - 1, checkIndex = gameData.horizontal - 1; x >= 0; x--, checkIndex--)
                {
                    //[3]判断剩余List的最后一个对象是否处于最大的位置，若不是，则记录需要掉落的相关信息
                    if (gridListManager[i][x].listHorizontal == checkIndex)
                    {
                        if (x != 0)
                            continue;
                    }
                    else
                    {
                        //下移格子数
                        dropGridCounts = checkIndex - gridListManager[i][x].listHorizontal;
                        //下移的距离
                        dropHeight = dropGridCounts * interval;
                        //记录元素需要掉落的高度
                        gridListManager[i][x].dropHeight = dropHeight;
                        gridListManager[i][x].dropCounts = dropGridCounts;
                    }

                    //[4]记录所需补充元素掉落的相关信息，和生成补充元素
                    if (x == 0)
                    {
                        for (int y = checkIndex, topMoveCounts = 0; y > 0; y--, topMoveCounts++)
                        {
                            //获取下落位置的元素信息，以作为补充使用
                            Vector3 newDropPosition = gridDropList[i].gridObject.GetComponent<RectTransform>().position;
                            int newDropHorizontal = gridDropList[i].listHorizontal;

                            //下移的距离
                            dropHeight = y * interval;

                            //修改备用掉落元素的信息
                            gridDropList[i].dropHeight = dropHeight + interval * topMoveCounts;
                            gridDropList[i].dropCounts = y - 1;
                            gridDropList[i].isTop = true;
                            gridDropList[i].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, +(interval * topMoveCounts), 0.0f);

                            //管理所有列的List对象对应添加元素
                            gridListManager[i].Insert(0, gridDropList[i]);

                            //移除掉落备用List对应位置的元素
                            gridDropList.RemoveAt(i);

                            //在下移的掉落对象位置，创建一个元素备用，添加于掉落备用List中
                            GameObject grid = Instantiate(Resources.Load("prefabs/grid"), Grid.transform) as GameObject;
                            Destroy(grid.GetComponent<SpriteRenderer>());
                            grid.AddComponent<Image>();
                            GridBean gridBean = new GridBean();
                            gridBean.spritesIndex = UnityEngine.Random.Range(0, 6);
                            grid.GetComponent<Image>().sprite = sprites[gridBean.spritesIndex];
                            grid.GetComponent<RectTransform>().position = newDropPosition;
                            grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                            grid.name = "grid" + i.ToString() + newDropHorizontal.ToString();
                            grid.SetActive(false);
                            gridBean.gridObject = grid;
                            gridBean.listHorizontal = newDropHorizontal;
                            gridBean.listVertical = i;
                            gridBean.isTop = true;
                            gridDropList.Insert(i, gridBean);
                        }
                    }
                }
            }
        }
    }
}
