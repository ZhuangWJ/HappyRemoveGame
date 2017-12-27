using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridDrop : MonoBehaviour
{
    private static float dropY;//每次掉落的高度
    private static float y = Screen.height * 0.75f; //格子上边界
    private static int dropGridCounts;//掉落的格子数
    private static float dropHeight;//所需下落的总高度
    private static int minEmptyIndex;//当列中挖空格子的最低格子位置
    private static int addCounts;//所需从掉落备用行补充的元素个数
    private static bool leftHasGrid;
    private static bool rightHasGrid;
    private static int randomSide;
    private static float moveX;
    private static int minHorizontal;
    private static int targetIndex;
    private static int x;
    private static int checkCounts;

    /// <summary>
    /// 执行元素掉落动画
    /// </summary>
    /// <param name="gameData">格子行列管理对象,用于获取行列数据</param>
    /// <param name="gridListManager">管理每列格子的List</param>
    /// <param name="gridSize">格子大小</param>
    public static void gridDrop(GameData gameData, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager)
    {
        //[0]定义每帧掉落距离和横移距离
        dropY = moveX = interval * 0.1f;

        //[1]遍历所有列，完成元素掉落
        for (int h = 0; h < 9; h++)
        {
            for (int i = 0; i < gameData.vertical; i++)
            {
                if (gridListManager[i].Count >= h + 1)
                {
                    x = gridListManager[i].Count - 1 - h;

                    //[3]判断剩余List的最后一个对象是否处于最大的位置，若不是，则读取掉落距离并执行，每帧移动后并修改相关信息
                    if (gridListManager[i][x].dropHeight > 0 || gridListManager[i][x].hasMoveWidth > 0 || gridListManager[i][x].hasDropHeight > 0)
                    {
                        //[3.1]先进行垂直掉落
                        if (gridListManager[i][x].dropHeight > dropY)
                        {
                            //执行掉落
                            gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropY, 0.0f);
                            gridListManager[i][x].dropHeight -= dropY;
                            
                            //若为补充元素掉落，则掉落距离超过第一个格子一半，则开始显示
                            if (gridListManager[i][x].isTop && gridListManager[i][x].gridObject.GetComponent<RectTransform>().position.y <= y)
                            {
                                gridListManager[i][x].gridObject.SetActive(true);
                                gridListManager[i][x].isTop = false;
                            }

                            continue;
                        }
                        else
                        {
                            gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -gridListManager[i][x].dropHeight, 0.0f);
                            gridListManager[i][x].dropHeight = 0;
                            gridListManager[i][x].dropCounts = 0;
                        }

                        //[3.2]垂直掉落完后，进行横移
                        if(gridListManager[i][x].moveCounts >= gridListManager[i][x].dropHeightFromOtherCounts)
                        {
                            if(gridListManager[i][x].moveCounts > 0)
                            {
                                if (gridListManager[i][x].hasMoveWidth > moveX)
                                {
                                    //往左边移动
                                    if (gridListManager[i][x].moveDirection == -1)
                                    {
                                        gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(-moveX, 0.0f, 0.0f);
                                    }
                                    //往右边移动
                                    if (gridListManager[i][x].moveDirection == 1)
                                    {
                                        gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(moveX, 0.0f, 0.0f);
                                    }
                                    gridListManager[i][x].hasMoveWidth -= moveX;

                                    continue;
                                }
                                else
                                {
                                    //往左边移动
                                    if (gridListManager[i][x].moveDirection == -1)
                                    {
                                        gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(-gridListManager[i][x].hasMoveWidth, 0.0f, 0.0f);
                                    }
                                    //往右边移动
                                    if (gridListManager[i][x].moveDirection == 1)
                                    {
                                        gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(gridListManager[i][x].hasMoveWidth, 0.0f, 0.0f);
                                    }

                                    if (gridListManager[i][x].moveCounts > 0)
                                    {
                                        gridListManager[i][x].moveCounts--;
                                        if (gridListManager[i][x].moveCounts == 0)
                                        {
                                            gridListManager[i][x].hasMoveWidth = 0;
                                        }
                                        else
                                        {
                                            gridListManager[i][x].hasMoveWidth = interval;
                                        }
                                    }
                                }
                            }
                        }

                        //[3.3]最后进行从其他列补充的垂直掉落
                        if (gridListManager[i][x].dropHeightFromOtherCounts > 0 )   
                        {
                            if (gridListManager[i][x].hasDropHeight > dropY)
                            {
                                //执行掉落
                                gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropY, 0.0f);
                                gridListManager[i][x].hasDropHeight -= dropY;
                            }
                            else
                            {
                                gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -gridListManager[i][x].hasDropHeight, 0.0f);

                                if (gridListManager[i][x].dropHeightFromOtherCounts > 0)
                                {
                                    gridListManager[i][x].dropHeightFromOtherCounts--;
                                    if (gridListManager[i][x].dropHeightFromOtherCounts == 0)
                                    {
                                        gridListManager[i][x].hasDropHeight = 0;
                                    }
                                    else
                                    {
                                        gridListManager[i][x].hasDropHeight = interval;
                                    }
                                }
                            }
                        }
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
    public static void recordGridDropMsg(GameData gameData, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager)
    {
        //[1]遍历所有列，若数量不等于9，则相应列进行掉落
        for (int i = 0; i < gameData.vertical; i++)
        {
            if (gridListManager[i].Count < 9)
            {
                //[2]遍历数组，从后往前检测是否需要移动补充空位 , 检测位置
                minEmptyIndex = -1;
                for (int x = gridListManager[i].Count - 1, checkIndex = gameData.horizontal - 1; x >= 0; x--, checkIndex--)
                {
                    //若List的x处是否为挖空，则也不需掉落
                    if (gridBaseListManager[i][gridListManager[i][x].listHorizontal].spriteIndex == -1)
                    {
                        if (minEmptyIndex < gridListManager[i][x].listHorizontal)
                            minEmptyIndex = gridListManager[i][x].listHorizontal;

                        if(x == 0)
                        {
                            //获取空格子上方所需补充的个数
                            if (minEmptyIndex > 0)
                            {
                                addCounts = minEmptyIndex;
                                for (int s = minEmptyIndex; s > 0; s--)
                                {
                                    if (gridBaseListManager[i][s - 1].isHasGrid)
                                        addCounts--;
                                }
                            }
                            else
                            {
                                addCounts = checkIndex;
                            }

                            if (addCounts > 0)
                                dropFromTopList(i, addCounts, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                        }

                        continue;
                    }

                    //[3]判断剩余List的最后一个对象是否处于最大的位置，若不是，则记录需要掉落的相关信息
                    if (gridListManager[i][x].listHorizontal == checkIndex)
                    {
                        if (x != 0)
                            continue;
                    }
                    else
                    {
                        ////判断可否掉落
                        dropGridCounts = 0;
                        for (int y = gridListManager[i][x].listHorizontal + 1; y <= checkIndex; y++)
                        {
                            //若下方为挖空格子，则直接不需掉落
                            if (gridBaseListManager[i][gridListManager[i][x].listHorizontal + 1].spriteIndex == -1)
                                break;

                            //若下方没有元素，则掉落距离+1
                            if (!gridBaseListManager[i][y].isHasGrid)
                                dropGridCounts++;

                            //若跨了一个元素以上才碰到挖空，则直接跳出，返回dropGridCounts
                            if (y > gridListManager[i][x].listHorizontal + 1 && gridBaseListManager[i][y].spriteIndex == -1)
                                break;
                        }

                        if (dropGridCounts > 0)
                        {
                            //下移的距离
                            dropHeight = dropGridCounts * interval;

                            //记录元素需要掉落的高度
                            gridListManager[i][x].dropHeight = dropHeight;
                            gridListManager[i][x].dropCounts = dropGridCounts;

                            //修改GridBean下移后的listHorizontal信息
                            gridBaseListManager[i][gridListManager[i][x].listHorizontal].isHasGrid = false;
                            gridListManager[i][x].listHorizontal += gridListManager[i][x].dropCounts;
                            gridListManager[i][x].gridObject.name = "grid" + i.ToString() + gridListManager[i][x].listHorizontal.ToString();
                            gridBaseListManager[i][gridListManager[i][x].listHorizontal].isHasGrid = true;
                        }
                    }

                    //[4]记录所需补充元素掉落的相关信息，和生成补充元素
                    if (x == 0)
                    {
                        //获取空格子上方所需补充的个数
                        if (minEmptyIndex > 0)
                        {
                            addCounts = minEmptyIndex;
                            for (int s = minEmptyIndex; s > 0; s--)
                            {
                                if (gridBaseListManager[i][s - 1].isHasGrid)
                                    addCounts--;
                            }
                        }
                        else
                        {
                            addCounts = checkIndex;
                        }

                        if (addCounts > 0)
                            dropFromTopList(i, addCounts, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                    }
                }
            }
        }

        //[5]若当列有挖空阻挡掉落，则从两方进行掉落补充，若隔壁两列上方没有元素，则继续查找再左两列or右两列的元素
        checkCounts = 0;
        while (checkCounts < 8)
        {
            for (int h = 8; h >= 1; h--)
            {
                for (int v = 0; v < 9; v++)
                {
                    if (gridBaseListManager[v][h].spriteIndex != -1 && !gridBaseListManager[v][h].isHasGrid && gridBaseListManager[v][h - 1].spriteIndex != -1)
                        dropFromOtherVertical(v, h, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                }
            }
            checkCounts++;
        }

    }

    /// <summary>
    /// 从其他列补充挖空下方的元素，记录掉落的相关信息
    /// </summary>
    private static void dropFromOtherVertical(int v, int h, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager)
    {
        //判断空格子左右上方是否均有元素，记录元素所需横移和掉落的信息
        rightHasGrid = false;
        leftHasGrid = false;
        switch (v)
        {
            case 0:
                if (gridBaseListManager[v + 1][h - 1].isHasGrid)
                {
                    for (int x = v + 1; x <= 8; x++)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (gridBaseListManager[x][i].spriteIndex == -1)
                                break;

                            if (i == 0)
                                rightHasGrid = true;
                        }

                        if (rightHasGrid)
                        {
                            dropFromRightVertical(v, h, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                            break;
                        }
                    }
                }
                break;
            case 8:
                if (gridBaseListManager[v - 1][h - 1].isHasGrid)
                {
                    for (int x = v - 1; x >= 0; x--)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (gridBaseListManager[x][i].spriteIndex == -1)
                                break;

                            if (x == 0 && i == 0)
                                leftHasGrid = true;
                        }

                        if (leftHasGrid)
                        {
                            dropFromLeftVertical(v, h, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                            break;
                        }
                    }
                }
                break;
            default:
                if (gridBaseListManager[v + 1][h - 1].isHasGrid)
                {
                    for (int x = v + 1; x <= 8; x++)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (gridBaseListManager[x][i].spriteIndex == -1)
                                break;

                            if (i == 0)
                                rightHasGrid = true;
                        }

                        if (rightHasGrid)
                            break;
                    }
                }

                if (gridBaseListManager[v - 1][h - 1].isHasGrid)
                {
                    for (int x = v - 1; x >= 0; x--)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (gridBaseListManager[x][i].spriteIndex == -1)
                                break;

                            if (i == 0)
                                leftHasGrid = true;
                        }

                        if (leftHasGrid)
                            break;
                    }
                }

                //左右两边均有元素，则从左右两列随机一列掉落
                if (rightHasGrid && leftHasGrid)
                {
                    randomSide = Random.Range(0, 2);
                    switch (randomSide)
                    {
                        case 0:
                            dropFromRightVertical(v, h, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                            break;
                        case 1:
                            dropFromLeftVertical(v, h, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                            break;
                    }
                }
                else
                {
                    if (rightHasGrid)
                        dropFromRightVertical(v, h, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);

                    if (leftHasGrid)
                        dropFromLeftVertical(v, h, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
                }
                break;
        }
    }

    /// <summary>
    /// 从左边列补充元素
    /// </summary>
    private static void dropFromLeftVertical(int v, int h, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager)
    {
        //修改从隔壁列补充元素的掉落信息
        for (int moveIndex = gridListManager[v - 1].Count - 1; moveIndex >= 0; moveIndex--)
        {
            if (gridListManager[v - 1][moveIndex].listHorizontal == h - 1)
            {
                gridListManager[v - 1][moveIndex].moveDirection = 1;
                gridListManager[v - 1][moveIndex].moveCounts += 1;
                gridListManager[v - 1][moveIndex].hasMoveWidth = interval;
                gridListManager[v - 1][moveIndex].dropHeightFromOtherCounts += 1;
                gridListManager[v - 1][moveIndex].hasDropHeight = interval;
                gridListManager[v - 1][moveIndex].dropCounts += 1;
                gridListManager[v - 1][moveIndex].listHorizontal += 1;
                gridListManager[v - 1][moveIndex].listVertical = v;
                gridListManager[v - 1][moveIndex].gridObject.name = "grid" + v.ToString() + h.ToString();

                //将补充元素添加进v列
                for (int i = gridListManager[v].Count - 1; i >= 0; i--)
                {
                    if (h < gridListManager[v][i].listHorizontal)
                    {
                        continue;
                    }
                    else
                    {
                        gridListManager[v].Insert(i + 1, gridListManager[v - 1][moveIndex]);
                        break;
                    }
                }

                //移除v-1的元素
                gridListManager[v - 1].RemoveAt(moveIndex);

                break;
            }
        }

        gridBaseListManager[v][h].isHasGrid = true;
        gridBaseListManager[v - 1][h - 1].isHasGrid = false;

        //上方元素统一往下掉落一格
        dropOneHeight(v - 1, h - 1, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
    }

    /// <summary>
    /// 从右边列补充元素
    /// </summary>
    private static void dropFromRightVertical(int v, int h, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager)
    {
        //修改从隔壁列补充元素的掉落信息
        for (int moveIndex = gridListManager[v + 1].Count - 1; moveIndex >= 0; moveIndex--)
        {
            if (gridListManager[v + 1][moveIndex].listHorizontal == h - 1)
            {
                gridListManager[v + 1][moveIndex].moveDirection = -1;
                gridListManager[v + 1][moveIndex].moveCounts += 1;
                gridListManager[v + 1][moveIndex].hasMoveWidth = interval;
                gridListManager[v + 1][moveIndex].dropHeightFromOtherCounts += 1;
                gridListManager[v + 1][moveIndex].hasDropHeight = interval;
                gridListManager[v + 1][moveIndex].dropCounts += 1;
                gridListManager[v + 1][moveIndex].listHorizontal += 1;
                gridListManager[v + 1][moveIndex].listVertical = v;
                gridListManager[v + 1][moveIndex].gridObject.name = "grid" + v.ToString() + h.ToString();

                //将补充元素添加进v列
                for (int i = gridListManager[v].Count - 1; i >= 0; i--)
                {
                    if (h < gridListManager[v][i].listHorizontal)
                    {
                        continue;
                    }
                    else
                    {
                        gridListManager[v].Insert(i + 1, gridListManager[v + 1][moveIndex]);
                        break;
                    }
                }

                //移除v+1的元素
                gridListManager[v + 1].RemoveAt(moveIndex);

                break;
            }
        }

        gridBaseListManager[v + 1][h - 1].isHasGrid = false;
        gridBaseListManager[v][h].isHasGrid = true;

        //v+1列上方元素统一往下掉落一格
        dropOneHeight(v + 1, h - 1, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
    }

    /// <summary>
    /// 上方元素统一往下掉落一个
    /// </summary>
    private static void dropOneHeight(int v, int maxHorizontal, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager)
    {
        //获取最小掉落索引，若上方有空格，则只需移动挖空后的元素即可
        minHorizontal = 0;
        for (int h = 8; h >= 0; h--)
        {
            if (gridBaseListManager[v][h].spriteIndex == -1)
            {
                minHorizontal = h;
                break;
            }
        }

        //下移的距离
        dropHeight = 1 * interval;

        //将minHorizontal和maxHorizontal之间的元素往下移动一格
        for (int h = maxHorizontal; h >= minHorizontal; h--)
        {
            //判断是否为挖空状态
            if (gridBaseListManager[v][h].spriteIndex == -1)
                break;

            //若不为挖空状态，则元素下移一格
            for (int listIndex = gridListManager[v].Count - 1; listIndex >= 0; listIndex--)
            {
                if (gridListManager[v][listIndex].listHorizontal == h)
                {
                    gridBaseListManager[v][h].isHasGrid = false;

                    //记录元素需要掉落的高度
                    gridListManager[v][listIndex].dropHeight += dropHeight;
                    gridListManager[v][listIndex].dropCounts += 1;

                    //修改GridBean下移后的listHorizontal信息
                    gridListManager[v][listIndex].listHorizontal += 1;
                    gridListManager[v][listIndex].gridObject.name = "grid" + v.ToString() + gridListManager[v][h].listHorizontal.ToString();
                    gridBaseListManager[v][gridListManager[v][listIndex].listHorizontal].isHasGrid = true;
                    break;
                }
            }
        }

        //若上方所有元素均掉落，则需从备用元素中补充掉落
        if (minHorizontal == 0)
            dropFromTopList(v, 1, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager);
    }

    /// <summary>
    /// 从备用行补充元素到List中
    /// </summary>
    private static void dropFromTopList(int v, int addCounts, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager)
    {
        for (int a = addCounts, topMoveCounts = 0; a > 0; a--, topMoveCounts++)
        {
            Vector3 newDropPosition = gridDropList[v].gridObject.GetComponent<RectTransform>().position;
            int newDropHorizontal = gridDropList[v].listHorizontal;

            //下移的距离
            dropHeight = a * interval;

            //修改备用掉落元素的信息
            gridDropList[v].dropHeight = dropHeight + interval * topMoveCounts;
            gridDropList[v].dropCounts = a - 1;
            gridDropList[v].isTop = true;
            gridDropList[v].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, +(interval * topMoveCounts), 0.0f);

            //管理所有列的List对象对应添加元素
            gridListManager[v].Insert(0, gridDropList[v]);

            //修改GridBean下移后的listHorizontal信息
            gridListManager[v][0].listHorizontal += gridListManager[v][0].dropCounts;
            gridListManager[v][0].gridObject.name = "grid" + v.ToString() + gridListManager[v][0].listHorizontal.ToString();
            gridBaseListManager[v][gridListManager[v][0].listHorizontal].isHasGrid = true;

            //移除掉落备用List对应位置的元素
            gridDropList.RemoveAt(v);

            //在下移的掉落对象位置，创建一个元素备用，添加于掉落备用List中
            GameObject grid = Instantiate(Resources.Load("prefabs/grid"), Grid.transform) as GameObject;
            Destroy(grid.GetComponent<SpriteRenderer>());
            grid.AddComponent<Image>();
            GridBean gridBean = new GridBean();
            gridBean.spritesIndex = Random.Range(0, 6);
            grid.GetComponent<Image>().sprite = sprites[gridBean.spritesIndex];
            grid.GetComponent<RectTransform>().position = newDropPosition;
            grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
            grid.name = "grid" + v.ToString() + newDropHorizontal.ToString();
            grid.SetActive(false);
            gridBean.gridObject = grid;
            gridBean.listHorizontal = newDropHorizontal;
            gridBean.listVertical = v;
            gridBean.isTop = true;
            gridDropList.Insert(v, gridBean);
        }
    }
}
