using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class GridConn : MonoBehaviour
{
    private float x, y;//格子左边界x，格子上边界y
    private float drawWidth;//线段宽度
    private float drawHeight;//线段高度
    private Vector3 startPoint;//线段初始点
    private Vector3 nextPoint;//线段结束点
    private Vector3 drawPoint;//线段对象的position
    private int startHorizontal;//线段开始位置所处于的对象行
    private int startVertical;//线段开始位置所处于的对象列
    private int nextHorizontal;//线段结束位置所处于的对象行
    private int nextVertical;//线段结束位置所处于的对象行
    private int nextListIndex;
    private int startListIndex;
    private float lineRotationZ;//线段对象旋转角度
    private int drawCounts = 0;//线段数
    private static int deleteCounts = 0;

    private bool isRemovePoison;
    private bool isDestroyGrid;
    private List<GameObject> lineObjList = new List<GameObject>();//线段对象管理List
    private List<GridBean> lineConnectGridList = new List<GridBean>();//线段连接的方块管理List
    private int mIceLevel;
    private static int beanPodIndexMax;
    private static int beanPodIndexMin;
    private static int beanPodIndex;

    private List<string> moveList = new List<string>();

    //初始化GridDrop所需用到的变量
    private static GameObject mGreat;
    private static GameObject mStepCounts;
    private static GameObject mTargetCount;
    private static EditorData mEditorData;
    private static GameData mGameData;
    private static List<List<GridBean>> mGridListManager;
    private static List<GridBean> mGridOfBeanPodList;
    private static List<GridBean> mFrostingList;
    private static List<GridBean> mPoisonList;
    private static GameObject mGrid;
    private static List<Sprite> mAllSprites;
    private static List<Sprite> mSpecialSprites;
    private static float mGridSize;
    private static float mIntervalPx;
    private static List<List<GridBaseBean>> mGridBaseListManager;
    private static List<IceBean> mIceDataList;
    private static List<TimboBean> mTimboDataList;
    private static int mCreateBeanPodStep;
    private static int mIsCreateBeanPod;
    private static GameObject mGameBackground;

    /// <summary>
    /// 初始化GridUI中的对象和变量
    /// </summary>
    public static void initGridUIAttribute()
    {
        mEditorData = GridUIAttributeManager.getInstance().editorData;
        mGameData = GridUIAttributeManager.getInstance().gameData;
        mGridListManager = GridUIAttributeManager.getInstance().gridListManager;
        mGrid = GridUIAttributeManager.getInstance().Gird;
        mGameBackground = GridUIAttributeManager.getInstance().GameBackground;
        mAllSprites = GridUIAttributeManager.getInstance().allSprites;
        mGridSize = GridUIAttributeManager.getInstance().gridSize;
        mIntervalPx = GridUIAttributeManager.getInstance().intervalPx;
        mGridBaseListManager = GridUIAttributeManager.getInstance().gridBaseListManager;
        mGridOfBeanPodList = GridUIAttributeManager.getInstance().gridOfBeanPodList;
        mTimboDataList = GridUIAttributeManager.getInstance().timboDataList;
        mIceDataList = GridUIAttributeManager.getInstance().iceDataList;
        mFrostingList = GridUIAttributeManager.getInstance().frostingList;
        mPoisonList = GridUIAttributeManager.getInstance().poisonList;
        mCreateBeanPodStep = GridUIAttributeManager.getInstance().createBeanPodStep;
        mIsCreateBeanPod = GridUIAttributeManager.getInstance().isCreateBeanPod;
        mGreat = GridUIAttributeManager.getInstance().great;
        mStepCounts = GridUIAttributeManager.getInstance().stepCounts;
        mTargetCount = GridUIAttributeManager.getInstance().targetCount;
        mSpecialSprites = GridUIAttributeManager.getInstance().specialSprites;
        deleteCounts = GridUIAttributeManager.getInstance().deleteCounts;
    }

    // Update is called once per frame
    void Update()
    {
        //获取第一个点中的方块信息
        if (Input.GetMouseButtonDown(0))
            getStartGrid();

        //判断移动后的下一个方块是否与第一个方块类型相同
        if (Input.GetMouseButton(0))
            getNextGridAndDrawLine();

        //松手后判断方块连接数是否可消除，并进行冰块、雪块等判断消除和掉落信息记录
        if (Input.GetMouseButtonUp(0))
            updateUIAndGirdMsg();

        //执行方块掉落
        GridDrop.gridDrop();
    }

    private void getStartGrid()
    {
        //[1]隐藏目标完成后的对象
        mGreat.SetActive(false);

        float gameBgWith = mGameBackground.GetComponent<RectTransform>().rect.width;
        float leaveSize = GridUIAttributeManager.getInstance().leaveSize;
        //[2]计算格子所在边界，x为左边界，y为上边界
        y = Screen.height * 0.75f;
        if (Screen.height >= Screen.width)
            x = leaveSize / 2;
        else
            x = Screen.width / 2 - gameBgWith / 2 + leaveSize / 2;
        float intervalPx = 1.0f;

        //[3]鼠标点中格子区域才会响应，记录初次点中的方块信息
        if (Input.mousePosition.x > x && Input.mousePosition.x < (x + mGridSize * mGameData.vertical + intervalPx * (mGameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((mGameData.horizontal * mGridSize + intervalPx * (mGameData.horizontal - 1)))))
        {
            startVertical = (int)((Input.mousePosition.x - x) / (mGridSize + intervalPx));
            startHorizontal = startListIndex = (int)((y - Input.mousePosition.y) / (mGridSize + intervalPx));
            int startSpriteIndex = mGridBaseListManager[startVertical][startHorizontal].spriteIndex;
            if (mGridBaseListManager[startVertical][startHorizontal].isHasGrid && startSpriteIndex != -1 && !(startSpriteIndex >= 15 && startSpriteIndex <= 20))
            {
                //判断startVertical该列上没有方块
                for (int h = startHorizontal - 1; h >= 0; h--)
                {
                    if (!mGridBaseListManager[startVertical][h].isHasGrid)
                        startListIndex--;
                }

                if (mGridListManager[startVertical][startListIndex].spriteIndex != -1)
                {
                    startPoint = mGridListManager[startVertical][startListIndex].gridObject.GetComponent<RectTransform>().position;
                    lineConnectGridList.Add(mGridListManager[startVertical][startListIndex]);
                }
            }
        }
    }

    private void getNextGridAndDrawLine()
    {
        //[1]鼠标点中格子区域才会响应，记录划动经过的方块信息
        if (Input.mousePosition.x > x && Input.mousePosition.x < (x + mGridSize * mGameData.vertical + mIntervalPx * (mGameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((mGameData.horizontal * mGridSize + mIntervalPx * (mGameData.horizontal - 1)))))
        {
            nextVertical = (int)((Input.mousePosition.x - x) / (mGridSize + mIntervalPx));
            nextHorizontal = nextListIndex = (int)((y - Input.mousePosition.y) / (mGridSize + mIntervalPx));
            int nextSpriteIndex = mGridBaseListManager[nextVertical][nextHorizontal].spriteIndex;
            if (mGridBaseListManager[nextVertical][nextHorizontal].isHasGrid && nextSpriteIndex != -1 && !(nextSpriteIndex >= 15 && nextSpriteIndex <= 20))
            {

                //判断nextVertical该列上没有方块
                for (int h = nextHorizontal - 1; h >= 0; h--)
                {
                    if (!mGridBaseListManager[nextVertical][h].isHasGrid)
                        nextListIndex--;
                }

                //[2]判断鼠标划动经过的对象是否已在数组中，如果已经存在，则不响应
                if (!lineConnectGridList.Contains(mGridListManager[nextVertical][nextListIndex]))
                {
                    if ((nextHorizontal != startHorizontal || nextVertical != startVertical) && mGridListManager[nextVertical][nextListIndex].spriteIndex == mGridListManager[startVertical][startListIndex].spriteIndex && System.Math.Abs(nextHorizontal - startHorizontal) <= 1 && System.Math.Abs(nextVertical - startVertical) <= 1)
                    {
                        if (mGridListManager[nextVertical][nextListIndex].spriteIndex != -1)
                        {
                            nextPoint = mGridListManager[nextVertical][nextListIndex].gridObject.GetComponent<RectTransform>().position;

                            //[3]绘制线段
                            GameObject line = new GameObject();
                            line.AddComponent<Image>();
                            line.GetComponent<Image>().color = new Color(1.0f, 0, 0, 0.5f);

                            //[3.1]计算旋转角度
                            //左上
                            if (nextHorizontal < startHorizontal && nextVertical < startVertical)
                            {
                                lineRotationZ = 225.0f;
                                drawPoint = startPoint + new Vector3(-mGridSize / 2, mGridSize / 2, 0.0f);
                            }
                            //正上方
                            if (nextHorizontal < startHorizontal && nextVertical == startVertical)
                            {
                                lineRotationZ = 180.0f;
                                drawPoint = startPoint + new Vector3(0.0f, mGridSize / 2, 0.0f);
                            }
                            //右上
                            if (nextHorizontal < startHorizontal && nextVertical > startVertical)
                            {
                                lineRotationZ = 135.0f;
                                drawPoint = startPoint + new Vector3(mGridSize / 2, mGridSize / 2, 0.0f);
                            }
                            //左边
                            if (nextHorizontal == startHorizontal && nextVertical < startVertical)
                            {
                                lineRotationZ = 270.0f;
                                drawPoint = startPoint + new Vector3(-mGridSize / 2, 0.0f, 0.0f);
                            }
                            //右边
                            if (nextHorizontal == startHorizontal && nextVertical > startVertical)
                            {
                                lineRotationZ = 90.0f;
                                drawPoint = startPoint + new Vector3(mGridSize / 2, 0.0f, 0.0f);
                            }
                            //左下
                            if (nextHorizontal > startHorizontal && nextVertical < startVertical)
                            {
                                lineRotationZ = 315.0f;
                                drawPoint = startPoint + new Vector3(-mGridSize / 2, -mGridSize / 2, 0.0f);
                            }
                            //正下方
                            if (nextHorizontal > startHorizontal && nextVertical == startVertical)
                            {
                                lineRotationZ = 360.0f;
                                drawPoint = startPoint + new Vector3(0.0f, -mGridSize / 2, 0.0f);
                            }
                            //右下
                            if (nextHorizontal > startHorizontal && nextVertical > startVertical)
                            {
                                lineRotationZ = 45.0f;
                                drawPoint = startPoint + new Vector3(mGridSize / 2, -mGridSize / 2, 0.0f);
                            }

                            //[3.2]计算线段宽度，为方块的1/8
                            drawWidth = mGridSize / 8.0f;
                            //[3.3]计算线段长度，为两个中心的距离
                            drawHeight = (startPoint - nextPoint).magnitude;
                            //[3.4]设置线段宽度
                            line.GetComponent<RectTransform>().sizeDelta = new Vector2(drawWidth, drawHeight);
                            //[3.5]设置线段开始位置
                            line.GetComponent<RectTransform>().position = drawPoint;
                            line.transform.SetParent(mGrid.transform);
                            //[3.6]设置线段的旋转角度
                            line.GetComponent<RectTransform>().Rotate(new Vector3(0.0f, 0.0f, lineRotationZ));
                            //[3.7]线段连接对象和线段数组添加方块，将划动经过的点作为初始点
                            lineConnectGridList.Add(mGridListManager[nextVertical][nextListIndex]);
                            lineObjList.Add(line);
                            drawCounts++;

                            startHorizontal = nextHorizontal;
                            startVertical = nextVertical;
                            startListIndex = nextListIndex;
                            startPoint = nextPoint;
                        }
                    }
                }
            }
        }
    }

    private void updateUIAndGirdMsg()
    {
        //[1]在格子区域抬起鼠标，则默认用户希望消除方块，否则则撤销消除
        if (Input.mousePosition.x > x && Input.mousePosition.x < (x + mGridSize * mGameData.vertical + mIntervalPx * (mGameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((mGameData.horizontal * mGridSize + mIntervalPx * (mGameData.horizontal - 1)))))
        {
            if (drawCounts >= 2)
            {
                isRemovePoison = false;

                //优先判断是否有特效方块
                foreach (GridBean gridBean in lineConnectGridList)
                {
                    
                    if (gridBean.specialTpye >= 0)
                    {
                        //TODO 实现特效方块消除效果

                    }
                }

                //判断是否需要消除lineConnectGridList剩余的普通方块
                foreach (GridBean gridBean in lineConnectGridList)
                {
                    int v = gridBean.vertical;
                    int h = gridBean.horizontal;

                    isDestroyGrid = false;
                    for (int removeIndex = 0; removeIndex < mGridListManager[v].Count; removeIndex++)
                    {
                        if (mGridListManager[v][removeIndex].horizontal == h)
                        {
                            //[2]计算是否消除了目标类型
                            if (gridBean.spriteIndex == mEditorData.targetType)
                                deleteCounts++;

                            //[3]判断方块上方是否有树藤
                            if (mGridBaseListManager[v][h].spriteIndex == 21)
                            {
                                foreach (TimboBean timboBean in mTimboDataList)
                                {
                                    if (timboBean.timboVertical == v && timboBean.timboHorizontal == h)
                                    {
                                        Destroy(timboBean.timbo);
                                        mTimboDataList.Remove(timboBean);
                                        mGridBaseListManager[v][h].spriteIndex = 0;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                mGridBaseListManager[v][h].isHasGrid = false;
                                mGridListManager[v].RemoveAt(removeIndex);
                                isDestroyGrid = true;
                            }
                            break;
                        }
                    }

                    //[3]判断方块底部是否有冰块
                    if (mIceDataList != null)
                    {
                        foreach (IceBean iceBean in mIceDataList)
                        {
                            if (iceBean.iceVertical == v && iceBean.iceHorizontal == h)
                            {
                                mIceLevel = iceBean.iceLevel;
                                if (mIceLevel > 0)
                                {
                                    mIceLevel--;
                                    if (mIceLevel > 0)
                                    {
                                        iceBean.ice.GetComponent<Image>().sprite = mAllSprites[mIceLevel + 9];
                                        iceBean.iceLevel = mIceLevel;
                                    }
                                    else
                                    {
                                        if (mEditorData.targetType == 11)
                                        {
                                            deleteCounts++;
                                        }
                                        Destroy(iceBean.ice);
                                        mIceDataList.Remove(iceBean);
                                    }
                                }
                                break;
                            }
                        }
                    }

                    //[4]判断消除附近是否有雪块
                    if (mFrostingList != null)
                        checkFrosting(gridBean);

                    //[5]判断消除附近是否有毒液
                    if (mPoisonList != null)
                        checkPoison(gridBean);

                    //[6]消除方块
                    if (isDestroyGrid)
                        Destroy(gridBean.gridObject);
                }

                //[7]还原雪块消除信息
                if (mFrostingList != null)
                {
                    for (int i = 0; i < mFrostingList.Count; i++)
                    {
                        mFrostingList[i].isFrostingRemove = false;
                    }
                }

                //[8]判断是否生成特效方块
                if (lineConnectGridList.Count >= 10)
                {
                    //产生魔力鸟
                    createSpecialGrid(0);
                }
                else
                {
                    if (lineConnectGridList.Count >= 6)
                    {
                        int startV = lineConnectGridList[0].vertical;
                        int startH = lineConnectGridList[0].horizontal;
                        //判断都在同一列或者同一行，产生横竖直线特效方块
                        bool isTheSameVertical = true;
                        foreach (GridBean g in lineConnectGridList)
                        {
                            if (g.vertical != startV)
                            {
                                isTheSameVertical = false;
                                break;
                            }
                        }

                        if (isTheSameVertical)
                        {
                            //产生竖线特效
                            createSpecialGrid(1);
                        }
                        else
                        {
                            bool isTheSameHorizontal = true;
                            foreach (GridBean g in lineConnectGridList)
                            {
                                if (g.horizontal != startH)
                                {
                                    isTheSameHorizontal = false;
                                    break;
                                }
                            }
                            if (isTheSameHorizontal)
                            {
                                //产生横线特效
                                createSpecialGrid(2);
                            }
                            else
                            {
                                //若不为上述两种情况，则产生爆炸方块
                                createSpecialGrid(3);
                            }
                        }
                    }
                }

                //[9]检查是否需要生成毒液
                checkIsCreatePoison();

                //[10]记录方块掉落信息
                GridDrop.recordGridDropMsg();

                //[11]刷新步数信息
                mEditorData.stepCounts--;
                if (mEditorData.targetType == 13)
                    mIsCreateBeanPod--;
                if (mEditorData.stepCounts > 0)
                {
                    mStepCounts.GetComponent<Text>().text = mEditorData.stepCounts.ToString();
                    if (mEditorData.targetType == 13 && mIsCreateBeanPod == 0)
                        createBeanPod();
                }
                else
                {
                    //[11.1]已没有步数，提示失败
                }

                //[12]刷新目标数量
                updateTargetCounts(deleteCounts);
            }
        }

        //[13]移除线段对象和线段数量
        if (lineObjList.Count > 0)
        {
            foreach (GameObject obj in lineObjList)
            {
                Destroy(obj);
            }
            lineObjList.Clear();
        }
        drawCounts = 0;

        //[14]移除lineConnectGridList的内容
        lineConnectGridList.Clear();
    }

    //生成特效方块
    private void createSpecialGrid(int specialTpye)
    {
        //获取连线第一个方块的位置
        int v = lineConnectGridList[0].vertical;
        int h = lineConnectGridList[0].horizontal;

        GameObject grid = Instantiate(Resources.Load("prefabs/grid"), mGrid.transform) as GameObject;
        grid.GetComponent<RectTransform>().position = mGridBaseListManager[v][h].gridBase.GetComponent<RectTransform>().position;
        grid.GetComponent<RectTransform>().sizeDelta = new Vector2(mGridSize, mGridSize);
        Destroy(grid.GetComponent<SpriteRenderer>());
        grid.AddComponent<Image>();

        GridBean gridBean = new GridBean();
        gridBean.gridObject = grid;
        gridBean.moveHorizontal = 9;
        gridBean.vertical = v;
        gridBean.horizontal = h;
        gridBean.isTop = false;

        gridBean.spriteIndex = lineConnectGridList[0].spriteIndex;
        gridBean.specialTpye = specialTpye;
        //根据specialTpye特效类型判断生成哪种特效
        switch (specialTpye)
        {
            case 0://魔力鸟
                gridBean.specialIndex = 0;
                break;
            case 1://竖线
            case 2://横线
                gridBean.specialIndex = lineConnectGridList[0].spriteIndex -1;
                break;
            case 3://爆炸
                gridBean.specialIndex = lineConnectGridList[0].spriteIndex - 1 + 6;
                break;
        }
        grid.GetComponent<Image>().sprite = mSpecialSprites[gridBean.specialIndex];
        grid.name = "grid" + v.ToString() + h.ToString();
        mGridBaseListManager[v][h].spriteIndex = gridBean.spriteIndex;
        mGridBaseListManager[v][h].isHasGrid = true;

        //获取当前位置所在列的索引，进行添加
        int index = 0;
        foreach(GridBean g in mGridListManager[v])
        {
            if (h > g.horizontal)
                index++;
        }
        mGridListManager[v].Insert(index, gridBean);
    }

    //生成一个金豆荚
    public static void createBeanPod()
    {
        beanPodIndexMin = 0;
        beanPodIndexMax = 0;
        for (int v = 0; v < 9; v++)
        {
            if (mGridListManager[v][0].dropHeight > 0)
            {
                if (beanPodIndexMin == 0)
                    beanPodIndexMin = v;
                if (beanPodIndexMax == 0)
                    beanPodIndexMax = v;
                if (v > beanPodIndexMax)
                    beanPodIndexMax = v;
            }
        }
        beanPodIndex = UnityEngine.Random.Range(beanPodIndexMin, beanPodIndexMax + 1);
        mGridListManager[beanPodIndex][0].spriteIndex = 13;
        mGridListManager[beanPodIndex][0].gridObject.GetComponent<Image>().sprite = mAllSprites[13];
        mGridOfBeanPodList.Add(mGridListManager[beanPodIndex][0]);
        mIsCreateBeanPod = mCreateBeanPodStep;
    }

    //更新通过目标剩余数量
    public static void updateTargetCounts(int deleteCounts)
    {
        if (deleteCounts >= mEditorData.targetCounts)
        {
            //[6.1]完成任务
            mGreat.SetActive(true);
            mEditorData.targetCounts = UnityEngine.Random.Range(15, 30);
            mTargetCount.GetComponent<Text>().text = mEditorData.targetCounts.ToString();
            deleteCounts = 0;
            mEditorData.stepCounts = 20;
            mStepCounts.GetComponent<Text>().text = mEditorData.stepCounts.ToString();
        }
        else
        {
            //[6.2]仍未完成目标
            mTargetCount.GetComponent<Text>().text = "x" + (mEditorData.targetCounts - deleteCounts);
        }
    }

    //检查消除方块的上下左右是否有雪块
    private void checkFrosting(GridBean gridBean)
    {
        for (int i = 0; i < mFrostingList.Count; i++)
        {
            if ((System.Math.Abs(mFrostingList[i].vertical - gridBean.vertical) <= 1 && mFrostingList[i].horizontal - gridBean.horizontal == 0) || (mFrostingList[i].vertical - gridBean.vertical == 0 && System.Math.Abs(mFrostingList[i].horizontal - gridBean.horizontal) <= 1))
            {
                if (!mFrostingList[i].isFrostingRemove)
                {
                    mFrostingList[i].spriteIndex--;
                    if (mFrostingList[i].spriteIndex >= 15)
                    {
                        mFrostingList[i].gridObject.GetComponent<Image>().sprite = mAllSprites[mFrostingList[i].spriteIndex];
                    }
                    else
                    {
                        Destroy(mFrostingList[i].gridObject);
                        deleteCounts++;
                        mGridBaseListManager[mFrostingList[i].vertical][mFrostingList[i].horizontal].isHasGrid = false;
                    }
                    mGridBaseListManager[mFrostingList[i].vertical][mFrostingList[i].horizontal].spriteIndex = mFrostingList[i].spriteIndex;
                    mFrostingList[i].isFrostingRemove = true;
                }
            }
        }

        //更新雪块List
        for (int v = 0; v < 9; v++)
        {
            for (int h = 0; h < 9; h++)
            {
                if (mGridBaseListManager[v][h].spriteIndex == 14)
                {
                    foreach (GridBean grid in mFrostingList)
                    {
                        if (grid.vertical == v && grid.horizontal == h)
                        {
                            mFrostingList.Remove(grid);
                            mGridListManager[v].Remove(grid);
                            break;
                        }
                    }
                }
            }
        }
    }

    //检查消除方块的上下左右是否有毒液
    private void checkPoison(GridBean gridBean)
    {
        int checkCounts = mPoisonList.Count;
        for (int x = 0; x < checkCounts; x++)
        {
            for (int i = 0, v, h; i < mPoisonList.Count; i++)
            {
                v = mPoisonList[i].vertical;
                h = mPoisonList[i].horizontal;
                if ((System.Math.Abs(v - gridBean.vertical) <= 1 && h - gridBean.horizontal == 0) || (v - gridBean.vertical == 0 && System.Math.Abs(h - gridBean.horizontal) <= 1))
                {
                    mGridListManager[v].Remove(mPoisonList[i]);
                    Destroy(mPoisonList[i].gridObject);
                    mGridBaseListManager[v][h].gridBean = null;
                    mGridBaseListManager[v][h].isHasGrid = false;
                    mGridBaseListManager[v][h].spriteIndex = 0;//随机赋值，只要不是挖空，雪块、毒液的资源索引值即可
                    isRemovePoison = true;
                    mPoisonList.RemoveAt(i);
                    deleteCounts++;
                    break;
                }
            }
        }
    }

    //检查是否需要生产毒液
    private void checkIsCreatePoison()
    {
        int checkCounts = mPoisonList.Count;
        List<GridBean> newPoison = new List<GridBean>();
        //若一次消除中，没有任何毒液消除，则所有毒液随机向可移动的方向蔓延一个
        if (!isRemovePoison)
        {
            int topGridSpriteIndex;
            int buttomGridSpriteIndex;
            int leftGridSpriteIndex;
            int rightGridSpriteIndex;
            int topGridIndex;
            int buttomGridIndex;
            int leftGridIndex;
            int rightGridIndex;
            int randomIndex;
            for (int i = 0, v, h; i < mPoisonList.Count; i++)
            {
                v = mPoisonList[i].vertical;
                h = mPoisonList[i].horizontal;
                if (h > 0)
                {
                    topGridSpriteIndex = mGridBaseListManager[v][h - 1].spriteIndex;
                    if (mGridBaseListManager[v][h - 1].isHasGrid && topGridSpriteIndex != -1 && !(topGridSpriteIndex >= 15 && topGridSpriteIndex <= 20))
                        moveList.Add("Top");
                }
                if (h < 8)
                {
                    buttomGridSpriteIndex = mGridBaseListManager[v][h + 1].spriteIndex;
                    if (mGridBaseListManager[v][h + 1].isHasGrid && buttomGridSpriteIndex != -1 && !(buttomGridSpriteIndex >= 15 && buttomGridSpriteIndex <= 20))
                        moveList.Add("Buttom");
                }
                if (v > 0)
                {
                    leftGridSpriteIndex = mGridBaseListManager[v - 1][h].spriteIndex;
                    if (mGridBaseListManager[v - 1][h].isHasGrid && leftGridSpriteIndex != -1 && !(leftGridSpriteIndex >= 15 && leftGridSpriteIndex <= 20))
                        moveList.Add("Left");
                }
                if (v < 8)
                {
                    rightGridSpriteIndex = mGridBaseListManager[v + 1][h].spriteIndex;
                    if (mGridBaseListManager[v + 1][h].isHasGrid && rightGridSpriteIndex != -1 && !(rightGridSpriteIndex >= 15 && rightGridSpriteIndex <= 20))
                        moveList.Add("Right");
                }

                if (moveList.Count > 0)
                {
                    randomIndex = UnityEngine.Random.Range(0, moveList.Count);
                    switch (moveList[randomIndex])
                    {
                        case "Top":
                            topGridIndex = mGridListManager[v].IndexOf(mPoisonList[i]) - 1;
                            mGridListManager[v][topGridIndex].spriteIndex = 20;
                            mGridListManager[v][topGridIndex].gridObject.GetComponent<Image>().sprite = mAllSprites[20];
                            mGridBaseListManager[v][h - 1].spriteIndex = 20;
                            newPoison.Add(mGridListManager[v][topGridIndex]);
                            break;
                        case "Buttom":
                            buttomGridIndex = mGridListManager[v].IndexOf(mPoisonList[i]) + 1;
                            mGridListManager[v][buttomGridIndex].spriteIndex = 20;
                            mGridListManager[v][buttomGridIndex].gridObject.GetComponent<Image>().sprite = mAllSprites[20];
                            mGridBaseListManager[v][h + 1].spriteIndex = 20;
                            newPoison.Add(mGridListManager[v][buttomGridIndex]);
                            break;
                        case "Left":
                            leftGridIndex = mGridListManager[v - 1].IndexOf(mGridBaseListManager[v - 1][h].gridBean);
                            mGridListManager[v - 1][leftGridIndex].spriteIndex = 20;
                            mGridListManager[v - 1][leftGridIndex].gridObject.GetComponent<Image>().sprite = mAllSprites[20];
                            mGridBaseListManager[v - 1][h].spriteIndex = 20;
                            newPoison.Add(mGridListManager[v - 1][leftGridIndex]);
                            break;
                        case "Right":
                            rightGridIndex = mGridListManager[v + 1].IndexOf(mGridBaseListManager[v + 1][h].gridBean);
                            mGridListManager[v + 1][rightGridIndex].spriteIndex = 20;
                            mGridListManager[v + 1][rightGridIndex].gridObject.GetComponent<Image>().sprite = mAllSprites[20];
                            mGridBaseListManager[v + 1][h].spriteIndex = 20;
                            newPoison.Add(mGridListManager[v + 1][rightGridIndex]);
                            break;
                    }
                    moveList.Clear();
                }
            }

            //将生成的毒液加入到List中
            foreach (GridBean gridBean in newPoison)
            {
                mPoisonList.Add(gridBean);
            }
            newPoison.Clear();
        }
    }
}