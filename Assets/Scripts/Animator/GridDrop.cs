using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridDrop : MonoBehaviour
{
    //初始化GridDrop所需用到的变量
    private static GameData mGameData;
    private static List<List<GridBean>> mGridListManager;
    private static float mInterval;
    private static List<GridBean> mGridDropList;
    private static GameObject mGrid;
    private static List<Sprite> mSprites;
    private static float mGridSize;
    private static List<List<GridBaseBean>> mGridBaseListManager;
    private static List<DoorBean> mDoorDataList;

    private static float dropY;//每次掉落的高度
    private static float gridBorderY = Screen.height * 0.75f; //格子上边界
    private static int dropGridCounts;//掉落的格子数
    private static float dropHeight;//所需下落的总高度
    private static int outIndex;
    private static int minEmptyIndex;//当列中挖空格子的最低格子位置
    private static int addCounts;//所需从掉落备用行补充的元素个数
    private static bool leftHasGrid;//从左边一列上一个格子是否有元素
    private static bool rightHasGrid;//从右边一列上一个格子是否有元素
    private static int randomSide;//若左右两边上一个格子均有元素，则随机产生0，1作为50%选择的依据
    private static float moveX;//每帧横移的距离
    private static int minHorizontal;//统一掉落的最小索引
    private static int checkIndex;//执行掉落遍历的索引变量
    private static int checkCounts;//检测isHasGrid为空的次数，取决于格子数
    private static bool hasDoor;
    private static bool isDelete;
    private static float gridHideInDoor;
    private static float gridShowOutDoor;
    private static bool endRecord;

    /// <summary>
    /// 接收GridUI中的对象和变量
    /// </summary>
    public static void initGridDrop(GameData gameData, List<List<GridBean>> gridListManager, float interval, List<GridBean> gridDropList, GameObject Grid, List<Sprite> sprites, float gridSize, List<List<GridBaseBean>> gridBaseListManager, List<DoorBean> doorDataList)
    {
        mGameData = gameData;
        mGridListManager = gridListManager;
        mInterval = interval;
        mGridDropList = gridDropList;
        mGrid = Grid;
        mSprites = sprites;
        mGridSize = gridSize;
        mGridBaseListManager = gridBaseListManager;
        mDoorDataList = doorDataList;
    }

    /// <summary>
    /// 执行元素掉落动画
    /// </summary>
    public static void gridDrop()
    {
        //[0]定义每帧掉落距离和横移距离
        dropY = moveX = mInterval * 0.1f;

        //[1]遍历所有列，完成元素掉落
        for (int h = 18; h > 0; h--)
        {
            for (int i = 0; i < mGameData.vertical; i++)
            {
                if (mGridListManager[i].Count >= h)
                {
                    checkIndex = h - 1;

                    //[3]判断剩余List的最后一个对象是否处于最大的位置，若不是，则读取掉落距离并执行，每帧移动后并修改相关信息
                    if (mGridListManager[i][checkIndex].dropHeight > 0 || mGridListManager[i][checkIndex].hasMoveWidth > 0 || mGridListManager[i][checkIndex].hasDropHeight > 0)
                    {
                        //[3.1]先进行垂直掉落
                        if (mGridListManager[i][checkIndex].dropHeight > dropY)
                        {
                            //执行掉落
                            mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropY, 0.0f);
                            mGridListManager[i][checkIndex].dropHeight -= dropY;

                            //若为补充元素掉落，则掉落距离超过第一个格子一半，则开始显示
                            if (mGridListManager[i][checkIndex].isTop && mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position.y <= gridBorderY)
                            {
                                mGridListManager[i][checkIndex].gridObject.SetActive(true);
                                mGridListManager[i][checkIndex].isTop = false;
                            }

                            //若为传送门入口元素掉落，则掉落距离超过传送门下方格子，则开始隐藏
                            foreach (DoorBean doorBean in mDoorDataList)
                            {
                                if (doorBean.inVertical == i)
                                {
                                    gridHideInDoor = mGridBaseListManager[i][doorBean.inHorizontal].gridBase.GetComponent<RectTransform>().position.y;
                                    if (mGridListManager[i][checkIndex].isDropInDoor && mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position.y <= gridHideInDoor)
                                    {
                                        mGridListManager[i][checkIndex].gridObject.SetActive(false);
                                        mGridListManager[i][checkIndex].isDropInDoor = false;
                                        break;
                                    }
                                }
                            }

                            //若为传送门出口元素掉落，则掉落距离超过传送门上方格子，则开始显示
                            foreach (DoorBean doorBean in mDoorDataList)
                            {
                                if (doorBean.outVertical == i)
                                {
                                    gridShowOutDoor = (mGridBaseListManager[i][doorBean.outHorizontal].gridBase.GetComponent<RectTransform>().position + new Vector3(0.0f, mGridSize / 2, 0.0f)).y;
                                    if (mGridListManager[i][checkIndex].isDropOutDoor && mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position.y <= gridShowOutDoor)
                                    {
                                        mGridListManager[i][checkIndex].gridObject.SetActive(true);
                                        mGridListManager[i][checkIndex].isDropOutDoor = false;
                                        break;
                                    }
                                }
                            }

                            continue;
                        }
                        else
                        {
                            mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -mGridListManager[i][checkIndex].dropHeight, 0.0f);
                            mGridListManager[i][checkIndex].dropHeight = 0;
                            mGridListManager[i][checkIndex].dropCounts = 0;

                            //若为传送门入口列下移到最底部，则移除元素
                            isDelete = false;
                            foreach (DoorBean doorBean in mDoorDataList)
                            {
                                if (doorBean.inVertical == i)
                                {
                                    if (mGridListManager[doorBean.inVertical][checkIndex].isDestroy)
                                    {
                                        Destroy(mGridListManager[doorBean.inVertical][checkIndex].gridObject);
                                        mGridListManager[doorBean.inVertical].RemoveAt(checkIndex);
                                        isDelete = true;
                                        break;
                                    }
                                }
                            }
                            if (isDelete)
                                continue;
                        }

                        //[3.2]垂直掉落完后，进行横移
                        if (mGridListManager[i][checkIndex].moveCounts >= mGridListManager[i][checkIndex].dropHeightFromOtherCounts)
                        {
                            if (mGridListManager[i][checkIndex].moveCounts > 0)
                            {
                                if (mGridListManager[i][checkIndex].hasMoveWidth > moveX)
                                {
                                    //往左边移动
                                    if (mGridListManager[i][checkIndex].moveDirection == -1)
                                    {
                                        mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(-moveX, 0.0f, 0.0f);
                                    }
                                    //往右边移动
                                    if (mGridListManager[i][checkIndex].moveDirection == 1)
                                    {
                                        mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(moveX, 0.0f, 0.0f);
                                    }
                                    mGridListManager[i][checkIndex].hasMoveWidth -= moveX;

                                    continue;
                                }
                                else
                                {
                                    //往左边移动
                                    if (mGridListManager[i][checkIndex].moveDirection == -1)
                                        mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(-mGridListManager[i][checkIndex].hasMoveWidth, 0.0f, 0.0f);

                                    //往右边移动
                                    if (mGridListManager[i][checkIndex].moveDirection == 1)
                                        mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(mGridListManager[i][checkIndex].hasMoveWidth, 0.0f, 0.0f);

                                    if (mGridListManager[i][checkIndex].moveCounts > 0)
                                    {
                                        mGridListManager[i][checkIndex].moveCounts--;
                                        if (mGridListManager[i][checkIndex].moveCounts == 0)
                                            mGridListManager[i][checkIndex].hasMoveWidth = 0;
                                        else
                                            mGridListManager[i][checkIndex].hasMoveWidth = mInterval;
                                    }
                                }
                            }
                        }

                        //[3.3]最后进行从其他列补充的垂直掉落
                        if (mGridListManager[i][checkIndex].dropHeightFromOtherCounts > 0)
                        {
                            if (mGridListManager[i][checkIndex].hasDropHeight > dropY)
                            {
                                //执行掉落
                                mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropY, 0.0f);
                                mGridListManager[i][checkIndex].hasDropHeight -= dropY;
                            }
                            else
                            {
                                mGridListManager[i][checkIndex].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -mGridListManager[i][checkIndex].hasDropHeight, 0.0f);

                                if (mGridListManager[i][checkIndex].dropHeightFromOtherCounts > 0)
                                {
                                    mGridListManager[i][checkIndex].dropHeightFromOtherCounts--;
                                    if (mGridListManager[i][checkIndex].dropHeightFromOtherCounts == 0)
                                        mGridListManager[i][checkIndex].hasDropHeight = 0;
                                    else
                                        mGridListManager[i][checkIndex].hasDropHeight = mInterval;
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
    public static void recordGridDropMsg()
    {
        //[1]遍历所有列，若数量不等于9，则相应列进行掉落
        for (int i = 0; i < mGameData.vertical; i++)
        {
            if (mGridListManager[i].Count < 9)
            {
                //[2]遍历数组，从后往前检测是否需要移动补充空位 , 检测位置
                minEmptyIndex = -1;
                for (int x = mGridListManager[i].Count - 1, checkIndex = mGameData.horizontal - 1; x >= 0; x--, checkIndex--)
                {
                    //若List的x处是否为挖空，则也不需掉落
                    if (mGridBaseListManager[i][mGridListManager[i][x].listHorizontal].spriteIndex == -1)
                    {
                        if (minEmptyIndex < mGridListManager[i][x].listHorizontal)
                            minEmptyIndex = mGridListManager[i][x].listHorizontal;

                        //若中途遇到格子为空，若有传送门出口，则也可进行补充
                        endRecord = false;
                        if (!mGridBaseListManager[i][checkIndex].isHasGrid)
                        {
                            foreach (DoorBean doorBean in mDoorDataList)
                            {
                                if (doorBean.outVertical == i && doorBean.outHorizontal <= checkIndex)
                                {
                                    dropFromDoor(doorBean, checkIndex - x);
                                    endRecord = true;
                                    break;
                                }
                            }
                        }
                        if (endRecord)
                            break;

                        //若mGridListManager已检测完，则最后判断是从顶部备用行补充，还是传送门补充
                        if (x == 0)
                        {
                            //获取空格子上方所需补充的个数
                            if (minEmptyIndex > 0)
                            {
                                addCounts = minEmptyIndex;
                                for (int s = minEmptyIndex; s > 0; s--)
                                {
                                    if (mGridBaseListManager[i][s - 1].isHasGrid)
                                        addCounts--;
                                }
                            }
                            else
                            {
                                addCounts = checkIndex;
                            }

                            //检测该列上方是否有传送门出口，若有，则不从备用行补充元素
                            hasDoor = false;
                            foreach (DoorBean doorBean in mDoorDataList)
                            {
                                //需补充元素上方有传送门
                                if (doorBean.outVertical == i)
                                {
                                    dropFromDoor(doorBean, addCounts);
                                    hasDoor = true;
                                    break;
                                }
                            }
                            if (!hasDoor)
                            {
                                //若无传送门，则从默认备用行掉落补充
                                if (addCounts > 0)
                                    dropFromTopList(i, addCounts);
                            }
                        }

                        continue;
                    }

                    //[3]判断剩余List的最后一个对象是否处于最大的位置，若不是，则记录需要掉落的相关信息
                    if (mGridListManager[i][x].listHorizontal == checkIndex)
                    {
                        if (x != 0)
                            continue;
                    }
                    else
                    {
                        ////判断可否掉落
                        dropGridCounts = 0;
                        for (int y = mGridListManager[i][x].listHorizontal + 1; y <= checkIndex; y++)
                        {
                            //若下方为挖空格子，则直接不需掉落
                            if (mGridBaseListManager[i][mGridListManager[i][x].listHorizontal + 1].spriteIndex == -1)
                                break;

                            //若下方没有元素，则掉落距离+1
                            if (!mGridBaseListManager[i][y].isHasGrid)
                                dropGridCounts++;

                            //若跨了一个元素以上才碰到挖空，则直接跳出，返回dropGridCounts
                            if (y > mGridListManager[i][x].listHorizontal + 1 && mGridBaseListManager[i][y].spriteIndex == -1)
                                break;
                        }

                        if (dropGridCounts > 0)
                        {
                            //下移的距离
                            dropHeight = dropGridCounts * mInterval;

                            //记录元素需要掉落的高度
                            mGridListManager[i][x].dropHeight = dropHeight;
                            mGridListManager[i][x].dropCounts = dropGridCounts;

                            //修改GridBean下移后的listHorizontal信息
                            mGridBaseListManager[i][mGridListManager[i][x].listHorizontal].isHasGrid = false;
                            mGridListManager[i][x].listHorizontal += mGridListManager[i][x].dropCounts;
                            mGridListManager[i][x].gridObject.name = "grid" + i.ToString() + mGridListManager[i][x].listHorizontal.ToString();
                            mGridBaseListManager[i][mGridListManager[i][x].listHorizontal].isHasGrid = true;
                        }
                    }

                    //[4]记录所需补充元素掉落的相关信息，和生成补充元素
                    if (x == 0)
                    {
                        //[4.1]获取空格子上方所需补充的个数
                        if (minEmptyIndex > 0)
                        {
                            addCounts = minEmptyIndex;
                            for (int s = minEmptyIndex; s > 0; s--)
                            {
                                if (mGridBaseListManager[i][s - 1].isHasGrid)
                                    addCounts--;
                            }
                        }
                        else
                        {
                            addCounts = checkIndex;
                        }

                        //[4.2]检测该列上方是否有传送门出口，若有，则不从备用行补充元素
                        hasDoor = false;
                        foreach (DoorBean doorBean in mDoorDataList)
                        {
                            //[4.2.1]需补充元素上方有传送门
                            if (doorBean.outVertical == i && doorBean.outHorizontal <= checkIndex)
                            {
                                dropFromDoor(doorBean, addCounts);
                                hasDoor = true;
                                break;
                            }
                        }
                        if (!hasDoor)
                        {
                            //[4.2.2]若无传送门，则从默认备用行掉落补充
                            if (addCounts > 0)
                                dropFromTopList(i, addCounts);
                        }
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
                    if (mGridBaseListManager[v][h].spriteIndex != -1 && !mGridBaseListManager[v][h].isHasGrid && mGridBaseListManager[v][h - 1].spriteIndex != -1)
                        dropFromOtherVertical(v, h);
                }
            }
            checkCounts++;
        }
    }

    /// <summary>
    /// 从其他列补充挖空下方的元素，记录掉落的相关信息
    /// </summary>
    private static void dropFromOtherVertical(int v, int h)
    {
        //判断空格子左右上方是否均有元素，记录元素所需横移和掉落的信息
        rightHasGrid = false;
        leftHasGrid = false;
        switch (v)
        {
            case 0:
                if (mGridBaseListManager[v + 1][h - 1].isHasGrid)
                {
                    for (int x = v + 1; x <= 8; x++)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (mGridBaseListManager[x][i].spriteIndex == -1)
                                break;

                            if (i == 0)
                                rightHasGrid = true;
                        }
                        if (rightHasGrid)
                        {
                            dropFromRightVertical(v, h);
                            break;
                        }
                    }
                }
                break;
            case 8:
                if (mGridBaseListManager[v - 1][h - 1].isHasGrid)
                {
                    for (int x = v - 1; x >= 0; x--)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (mGridBaseListManager[x][i].spriteIndex == -1)
                                break;

                            if (x == 0 && i == 0)
                                leftHasGrid = true;
                        }
                        if (leftHasGrid)
                        {
                            dropFromLeftVertical(v, h);
                            break;
                        }
                    }
                }
                break;
            default:
                if (mGridBaseListManager[v + 1][h - 1].isHasGrid)
                {
                    for (int x = v + 1; x <= 8; x++)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (mGridBaseListManager[x][i].spriteIndex == -1)
                                break;

                            if (i == 0)
                                rightHasGrid = true;
                        }

                        if (rightHasGrid)
                            break;
                    }
                }
                if (mGridBaseListManager[v - 1][h - 1].isHasGrid)
                {
                    for (int x = v - 1; x >= 0; x--)
                    {
                        for (int i = h - 1; i >= 0; i--)
                        {
                            if (mGridBaseListManager[x][i].spriteIndex == -1)
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
                    randomSide = UnityEngine.Random.Range(0, 2);
                    switch (randomSide)
                    {
                        case 0:
                            dropFromRightVertical(v, h);
                            break;
                        case 1:
                            dropFromLeftVertical(v, h);
                            break;
                    }
                }
                else
                {
                    if (rightHasGrid)
                        dropFromRightVertical(v, h);
                    if (leftHasGrid)
                        dropFromLeftVertical(v, h);
                }
                break;
        }
    }

    /// <summary>
    /// 从左边列补充元素
    /// </summary>
    private static void dropFromLeftVertical(int v, int h)
    {
        //修改从隔壁列补充元素的掉落信息
        for (int moveIndex = mGridListManager[v - 1].Count - 1; moveIndex >= 0; moveIndex--)
        {
            if (mGridListManager[v - 1][moveIndex].listHorizontal == h - 1)
            {
                mGridListManager[v - 1][moveIndex].moveDirection = 1;
                mGridListManager[v - 1][moveIndex].moveCounts += 1;
                mGridListManager[v - 1][moveIndex].hasMoveWidth = mInterval;
                mGridListManager[v - 1][moveIndex].dropHeightFromOtherCounts += 1;
                mGridListManager[v - 1][moveIndex].hasDropHeight = mInterval;
                mGridListManager[v - 1][moveIndex].dropCounts += 1;
                mGridListManager[v - 1][moveIndex].listHorizontal += 1;
                mGridListManager[v - 1][moveIndex].listVertical = v;
                mGridListManager[v - 1][moveIndex].gridObject.name = "grid" + v.ToString() + h.ToString();

                //将补充元素添加进v列
                for (int i = mGridListManager[v].Count - 1; i >= 0; i--)
                {
                    if (h < mGridListManager[v][i].listHorizontal)
                    {
                        continue;
                    }
                    else
                    {
                        mGridListManager[v].Insert(i + 1, mGridListManager[v - 1][moveIndex]);
                        break;
                    }
                }

                //移除v-1的元素
                mGridListManager[v - 1].RemoveAt(moveIndex);
                break;
            }
        }
        mGridBaseListManager[v][h].isHasGrid = true;
        mGridBaseListManager[v - 1][h - 1].isHasGrid = false;

        //上方元素统一往下掉落一格
        dropOneHeight(v - 1, h - 1);
    }

    /// <summary>
    /// 从右边列补充元素
    /// </summary>
    private static void dropFromRightVertical(int v, int h)
    {
        //修改从隔壁列补充元素的掉落信息
        for (int moveIndex = mGridListManager[v + 1].Count - 1; moveIndex >= 0; moveIndex--)
        {
            if (mGridListManager[v + 1][moveIndex].listHorizontal == h - 1)
            {
                mGridListManager[v + 1][moveIndex].moveDirection = -1;
                mGridListManager[v + 1][moveIndex].moveCounts += 1;
                mGridListManager[v + 1][moveIndex].hasMoveWidth = mInterval;
                mGridListManager[v + 1][moveIndex].dropHeightFromOtherCounts += 1;
                mGridListManager[v + 1][moveIndex].hasDropHeight = mInterval;
                mGridListManager[v + 1][moveIndex].dropCounts += 1;
                mGridListManager[v + 1][moveIndex].listHorizontal += 1;
                mGridListManager[v + 1][moveIndex].listVertical = v;
                mGridListManager[v + 1][moveIndex].gridObject.name = "grid" + v.ToString() + h.ToString();

                //将补充元素添加进v列
                for (int i = mGridListManager[v].Count - 1; i >= 0; i--)
                {
                    if (h < mGridListManager[v][i].listHorizontal)
                    {
                        continue;
                    }
                    else
                    {
                        mGridListManager[v].Insert(i + 1, mGridListManager[v + 1][moveIndex]);
                        break;
                    }
                }

                //移除v+1的元素
                mGridListManager[v + 1].RemoveAt(moveIndex);
                break;
            }
        }
        mGridBaseListManager[v + 1][h - 1].isHasGrid = false;
        mGridBaseListManager[v][h].isHasGrid = true;

        //v+1列上方元素统一往下掉落一格
        dropOneHeight(v + 1, h - 1);
    }

    /// <summary>
    /// 上方元素统一往下掉落一个
    /// </summary>
    private static void dropOneHeight(int v, int maxHorizontal)
    {
        //获取最小掉落索引，若上方有空格，则只需移动挖空后的元素即可
        minHorizontal = 0;
        for (int h = 8; h >= 0; h--)
        {
            if (mGridBaseListManager[v][h].spriteIndex == -1)
            {
                minHorizontal = h;
                break;
            }
        }

        //下移的距离
        dropHeight = 1 * mInterval;

        //将minHorizontal和maxHorizontal之间的元素往下移动一格
        for (int h = maxHorizontal; h >= minHorizontal; h--)
        {
            //判断是否为挖空状态
            if (mGridBaseListManager[v][h].spriteIndex == -1)
                break;

            //若不为挖空状态，则元素下移一格
            for (int listIndex = mGridListManager[v].Count - 1; listIndex >= 0; listIndex--)
            {
                if (mGridListManager[v][listIndex].listHorizontal == h)
                {
                    mGridBaseListManager[v][h].isHasGrid = false;

                    //记录元素需要掉落的高度
                    mGridListManager[v][listIndex].dropHeight += dropHeight;
                    mGridListManager[v][listIndex].dropCounts += 1;

                    //修改GridBean下移后的listHorizontal信息
                    mGridListManager[v][listIndex].listHorizontal += 1;
                    mGridListManager[v][listIndex].gridObject.name = "grid" + v.ToString() + mGridListManager[v][h].listHorizontal.ToString();
                    mGridBaseListManager[v][mGridListManager[v][listIndex].listHorizontal].isHasGrid = true;
                    break;
                }
            }
        }

        //若上方所有元素均掉落，则需从备用元素中补充掉落
        if (minHorizontal == 0)
            dropFromTopList(v, 1);
    }

    /// <summary>
    /// 从备用行补充元素到List中
    /// </summary>
    private static void dropFromTopList(int v, int addCounts)
    {
        for (int a = addCounts, topMoveCounts = 0; a > 0; a--, topMoveCounts++)
        {
            Vector3 newDropPosition = mGridDropList[v].gridObject.GetComponent<RectTransform>().position;
            int newDropHorizontal = mGridDropList[v].listHorizontal;

            //下移的距离
            dropHeight = a * mInterval;

            //修改备用掉落元素的信息
            mGridDropList[v].dropHeight = dropHeight + mInterval * topMoveCounts;
            mGridDropList[v].dropCounts = a;
            mGridDropList[v].isTop = true;
            mGridDropList[v].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, +(mInterval * topMoveCounts), 0.0f);

            //管理所有列的List对象对应添加元素
            mGridListManager[v].Insert(0, mGridDropList[v]);

            //修改GridBean下移后的listHorizontal信息
            mGridListManager[v][0].listHorizontal = (a - 1);
            mGridListManager[v][0].gridObject.name = "grid" + v.ToString() + mGridListManager[v][0].listHorizontal.ToString();
            mGridBaseListManager[v][mGridListManager[v][0].listHorizontal].isHasGrid = true;

            //移除掉落备用List对应位置的元素
            mGridDropList.RemoveAt(v);

            //在下移的掉落对象位置，创建一个元素备用，添加于掉落备用List中
            GameObject grid = Instantiate(Resources.Load("prefabs/grid"), mGrid.transform) as GameObject;
            Destroy(grid.GetComponent<SpriteRenderer>());
            grid.AddComponent<Image>();
            GridBean gridBean = new GridBean();
            gridBean.spritesIndex = UnityEngine.Random.Range(0, 6);
            grid.GetComponent<Image>().sprite = mSprites[gridBean.spritesIndex];
            grid.GetComponent<RectTransform>().position = newDropPosition;
            grid.GetComponent<RectTransform>().sizeDelta = new Vector2(mGridSize, mGridSize);
            grid.name = "grid" + v.ToString() + newDropHorizontal.ToString();
            grid.SetActive(false);
            gridBean.gridObject = grid;
            gridBean.listHorizontal = newDropHorizontal;
            gridBean.listVertical = v;
            gridBean.isTop = true;
            mGridDropList.Insert(v, gridBean);
        }
    }

    /// <summary>
    /// 从传送门补充元素到List中
    /// </summary>
    private static void dropFromDoor(DoorBean doorBean, int addCounts)
    {
        int inV = doorBean.inVertical;
        int inH = doorBean.inHorizontal;
        int outV = doorBean.outVertical;
        int outH = doorBean.outHorizontal;

        int inVGridCounts = 0;
        //判断入口列还剩余多少个元素，是否足以掉落至出口列，
        for (int h = inH; h >= 0; h--)
        {
            if (mGridBaseListManager[inV][h].isHasGrid && mGridBaseListManager[inV][h].spriteIndex != -1)
                inVGridCounts++;
        }

        //若不足，则从备用列生成补充
        if (inVGridCounts < addCounts)
            dropFromTopList(inV, addCounts - inVGridCounts);

        //根据addCounts执行循环
        for (int i = 0, inIndex = inH, topMoveCounts = 0; i < addCounts; i++, inIndex--, topMoveCounts++)
        {
            //下移的距离
            dropHeight = addCounts * mInterval;

            //修改入口列的掉落信息和List处理
            mGridListManager[inV][inIndex].dropHeight = dropHeight;
            mGridListManager[inV][inIndex].dropCounts = addCounts;
            mGridBaseListManager[inV][mGridListManager[inV][inIndex].listHorizontal].isHasGrid = false;
            mGridListManager[inV][inIndex].listHorizontal += mGridListManager[inV][inIndex].dropCounts;
            mGridListManager[inV][inIndex].gridObject.name = "grid" + inV.ToString() + mGridListManager[inV][inIndex].listHorizontal.ToString();
            mGridListManager[inV][inIndex].isDropInDoor = true;
            mGridListManager[inV][inIndex].isDestroy = true;

            //复制一个对象
            GridBean gridBean = GridBean.mClone(mGridListManager[inV][inIndex], mGrid);

            //计算传送出口在GridList中的索引
            outIndex = 0;
            for (int x = 0; x < mGridListManager[outV].Count-1; x++)
            {
                if (mGridListManager[outV][x].listHorizontal < outH)
                    outIndex++;
                else
                    break;
            }

            //将该元素添加进传送出口的索引位置
            mGridListManager[outV].Insert(outIndex, gridBean);

            //修改出口列的掉落信息和List处理
            mGridListManager[outV][outIndex].listHorizontal = outH + addCounts - 1 - i;
            mGridListManager[outV][outIndex].listVertical = outV;
            mGridListManager[outV][outIndex].gridObject.GetComponent<RectTransform>().position = mGridBaseListManager[outV][outH].gridBase.GetComponent<RectTransform>().position + new Vector3(0.0f, (i + 1) * mInterval, 0.0f);
            mGridListManager[outV][outIndex].gridObject.name = "grid" + outV.ToString() + mGridListManager[outV][outIndex].listHorizontal.ToString();
            mGridBaseListManager[outV][mGridListManager[outV][outIndex].listHorizontal].isHasGrid = true;
            mGridListManager[outV][outIndex].gridObject.SetActive(false);
            mGridListManager[outV][outIndex].isDropOutDoor = true;

            //入口的列的剩余元素，需掉落addCounts个格子
            if (i + 1 == addCounts)
            {
                for (int x = inH - addCounts; x >= 0; x--)
                {
                    mGridListManager[inV][x].dropHeight += addCounts * mInterval;
                    mGridListManager[inV][x].dropCounts += addCounts;
                    mGridBaseListManager[inV][mGridListManager[inV][x].listHorizontal].isHasGrid = false;
                    mGridListManager[inV][x].listHorizontal += addCounts;
                    mGridListManager[inV][x].gridObject.name = "grid" + inV.ToString() + mGridListManager[inV][x].listHorizontal.ToString();
                    mGridBaseListManager[inV][mGridListManager[inV][x].listHorizontal].isHasGrid = true;
                }
            }
        }

        //从备用行补充传送门入口元素
        dropFromTopList(inV, addCounts);
    }
}
