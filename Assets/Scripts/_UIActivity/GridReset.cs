using UnityEngine;
using System.Collections.Generic;
using System;

public class GridReset : MonoBehaviour
{
    private static int theSameTypeCounts;
    private static bool isConn;
    private static int currentSpriteIndex;

    private static List<LeaveGridMsg> oneLineMsg;
    private static List<string> usableList = new List<string>();
    private static int targetCounts;
    private static List<List<LeaveGridMsg>> resourcesListManager = new List<List<LeaveGridMsg>>();

    //初始化GridReset所需用到的变量
    private static List<List<GridBean>> mGridListManager;
    private static List<List<GridBaseBean>> mGridBaseListManager;
    private static GameObject mGrid;

    /// <summary>
    /// 初始化GridUI中的对象和变量
    /// </summary>
    public static void initGridUIAttribute()
    {
        mGridListManager = GridUIAttributeManager.getInstance().gridListManager;
        mGridBaseListManager = GridUIAttributeManager.getInstance().gridBaseListManager;
        mGrid = GridUIAttributeManager.getInstance().Gird;
    }

    //判断是否为僵局并处理
    public static void checkGameisConn()
    {
        isConn = false; //若当前方块可以连接，则修改为true
        List<LeaveGridMsg> leaveGridMsgList = new List<LeaveGridMsg>();

        //(0)判断是否为僵局
        for (int v = 0; v < 9; v++)
        {
            for (int h = 0; h < 9; h++)
            {
                if (mGridBaseListManager[v][h].isHasGrid && mGridBaseListManager[v][h].spriteIndex != -1 && !(mGridBaseListManager[v][h].spriteIndex >= 15 && mGridBaseListManager[v][h].spriteIndex <= 21))
                {
                    if (!isConn)
                    {
                        //记录方块
                        LeaveGridMsg leaveGridMsg = new LeaveGridMsg();
                        leaveGridMsg.vertical = v;
                        leaveGridMsg.horizontal = h;
                        leaveGridMsg.spriteIndex = mGridBaseListManager[v][h].spriteIndex;
                        leaveGridMsgList.Add(leaveGridMsg);

                        //判断当前坐标的格子与周围是否可3连
                        theSameTypeCounts = 1;
                        currentSpriteIndex = mGridBaseListManager[v][h].spriteIndex;
                        mGridBaseListManager[v][h].isCheck = true;
                        checkTypeAroundTheGrid(v, h);

                        for (int i = 0; i < 9; i++)
                        {
                            for (int x = 0; x < 9; x++)
                            {
                                if (mGridBaseListManager[i][x].isCheck)
                                    mGridBaseListManager[i][x].isCheck = false;
                            }
                        }
                    }
                    else
                    {
                        leaveGridMsgList.Clear();
                    }
                }
            }

            if (isConn)
                break;
        }

        if (!isConn)
        {
            // 2:"monkey", 3:"panda",4: "chicken", 5:"penguin", 6:"pig",7: "rabbit"
            int[] resourceCounts = new int[] { 0, 0, 0, 0, 0, 0 };
            List<LeaveGridMsg> monkeyList = new List<LeaveGridMsg>();
            List<LeaveGridMsg> pandaList = new List<LeaveGridMsg>();
            List<LeaveGridMsg> chickenList = new List<LeaveGridMsg>();
            List<LeaveGridMsg> penguinList = new List<LeaveGridMsg>();
            List<LeaveGridMsg> pigList = new List<LeaveGridMsg>();
            List<LeaveGridMsg> rabbitList = new List<LeaveGridMsg>();

            resourcesListManager.Add(monkeyList);
            resourcesListManager.Add(pandaList);
            resourcesListManager.Add(chickenList);
            resourcesListManager.Add(penguinList);
            resourcesListManager.Add(pigList);
            resourcesListManager.Add(rabbitList);

            //(1)梳理当前剩余方块各类型的数量，比如：猴子：15，熊猫：8，以此类推；
            foreach (LeaveGridMsg leaveGridMsg in leaveGridMsgList)
            {
                switch (leaveGridMsg.spriteIndex)
                {
                    case 2://monkey
                        resourceCounts[0]++;
                        monkeyList.Add(leaveGridMsg);
                        break;
                    case 3://panda
                        resourceCounts[1]++;
                        pandaList.Add(leaveGridMsg);
                        break;
                    case 4://chicken
                        resourceCounts[2]++;
                        chickenList.Add(leaveGridMsg);
                        break;
                    case 5://penguin
                        resourceCounts[3]++;
                        penguinList.Add(leaveGridMsg);
                        break;
                    case 6://pig
                        resourceCounts[4]++;
                        pigList.Add(leaveGridMsg);
                        break;
                    case 7://rabbit
                        resourceCounts[5]++;
                        rabbitList.Add(leaveGridMsg);
                        break;
                }
            }

            //(2)对于数量大于3的类型，随机抽取一种，作为固定整合的类型，整合后的长度由（3，5）随机产生；
            List<int> resID = new List<int>();
            for (int i = 0; i < resourceCounts.Length; i++)
            {
                if (resourceCounts[i] >= 3)
                    resID.Add(i);
            }
            int targetResource = UnityEngine.Random.Range(0, resID.Count);
            if (resourceCounts[resID[targetResource]] >= 5)
                targetCounts = UnityEngine.Random.Range(3, 5);
            resID.Clear();

            //(3)获取随机坐标作为开始，并检测该坐标上下左右斜角是否有方块存在，若有方块，随机记录一个位置，以此类推，获取(2)整合长度的位置坐标List；
            setOneLine(leaveGridMsgList);

            //(4)将(2)固定整合类型的方块，与(3)的List位置上的方块互换，以确保至少有一条可3消的连线；
            LeaveGridMsg randomTargetType;
            int exchangeCounts = oneLineMsg.Count;
            while (exchangeCounts > 0)
            {
                foreach (LeaveGridMsg oneLineGridMsg in oneLineMsg)
                {
                    //随机一个同类型格子的方块
                    randomTargetType = resourcesListManager[targetResource][UnityEngine.Random.Range(0, resourcesListManager[targetResource].Count)];
                    //互换
                    exchangeGrid(oneLineGridMsg, randomTargetType);
                    //移除LeaveGridList中的位置
                    oneLineMsg.Remove(oneLineGridMsg);
                    resourcesListManager[targetResource].Remove(randomTargetType);
                    leaveGridMsgList.Remove(randomTargetType);
                    foreach (LeaveGridMsg leaveGridMsg in leaveGridMsgList)
                    {
                        if (oneLineGridMsg.vertical == leaveGridMsg.vertical && oneLineGridMsg.horizontal == leaveGridMsg.horizontal)
                        {
                            leaveGridMsgList.Remove(leaveGridMsg);
                            break;
                        }
                    }
                    break;
                }
                exchangeCounts--;
            }

            //(5)剩余方块List，随机抽取两个位置进行互换，并修改相关信息，互换后，List移除方块；
            LeaveGridMsg A, B;
            exchangeCounts = leaveGridMsgList.Count / 2;
            while (exchangeCounts > 0)
            {
                A = leaveGridMsgList[UnityEngine.Random.Range(0, leaveGridMsgList.Count)];
                leaveGridMsgList.Remove(A);
                B = leaveGridMsgList[UnityEngine.Random.Range(0, leaveGridMsgList.Count)];
                leaveGridMsgList.Remove(B);
                exchangeGrid(A, B);
                exchangeCounts--;
            }

            //(6)循环操作(6)的行为，直至剩余方块List.Count<2，无法互换的元素原地不动即可；
            leaveGridMsgList.Clear();
            foreach (List<LeaveGridMsg> list in resourcesListManager)
            {
                list.Clear();
            }
            resourcesListManager.Clear();
        }
    }

    //元素互换
    private static void exchangeGrid(LeaveGridMsg gridA, LeaveGridMsg gridB)
    {
        //获取互换位置的索引
        int gridAInedx = mGridListManager[gridA.vertical].IndexOf(mGridBaseListManager[gridA.vertical][gridA.horizontal].gridBean);
        int gridBInedx = mGridListManager[gridB.vertical].IndexOf(mGridBaseListManager[gridB.vertical][gridB.horizontal].gridBean);

        //复制互换的对象
        GridBean cloneGridA = GridBean.mClone(mGridBaseListManager[gridA.vertical][gridA.horizontal].gridBean, mGrid);
        GridBean cloneGridB = GridBean.mClone(mGridBaseListManager[gridB.vertical][gridB.horizontal].gridBean, mGrid);

        //移除对象
        Destroy(mGridBaseListManager[gridA.vertical][gridA.horizontal].gridBean.gridObject);
        Destroy(mGridBaseListManager[gridB.vertical][gridB.horizontal].gridBean.gridObject);

        mGridListManager[gridA.vertical].RemoveAt(gridAInedx);
        //根据索引添加复制后的对象
        if (gridAInedx < 8)
            mGridListManager[gridA.vertical].Insert(gridAInedx, cloneGridB);
        else
            mGridListManager[gridA.vertical].Add(cloneGridB);

        mGridListManager[gridB.vertical].RemoveAt(gridBInedx);
        if (gridBInedx < 8)
            mGridListManager[gridB.vertical].Insert(gridBInedx, cloneGridA);
        else
            mGridListManager[gridB.vertical].Add(cloneGridA);

        //修改互换的数据
        mGridListManager[gridA.vertical][gridAInedx].listVertical = gridA.vertical;
        mGridListManager[gridA.vertical][gridAInedx].listHorizontal = gridA.horizontal;
        mGridListManager[gridA.vertical][gridAInedx].gridObject.GetComponent<RectTransform>().position = mGridBaseListManager[gridA.vertical][gridA.horizontal].gridBase.GetComponent<RectTransform>().position;
        mGridListManager[gridA.vertical][gridAInedx].gridObject.name = "grid" + gridA.vertical.ToString() + gridA.horizontal.ToString();
        mGridBaseListManager[gridA.vertical][gridA.horizontal].gridBean = mGridListManager[gridA.vertical][gridAInedx];
        mGridBaseListManager[gridA.vertical][gridA.horizontal].spriteIndex = mGridListManager[gridA.vertical][gridAInedx].spriteIndex;

        mGridListManager[gridB.vertical][gridBInedx].listVertical = gridB.vertical;
        mGridListManager[gridB.vertical][gridBInedx].listHorizontal = gridB.horizontal;
        mGridListManager[gridB.vertical][gridBInedx].gridObject.GetComponent<RectTransform>().position = mGridBaseListManager[gridB.vertical][gridB.horizontal].gridBase.GetComponent<RectTransform>().position;
        mGridListManager[gridB.vertical][gridBInedx].gridObject.name = "grid" + gridB.vertical.ToString() + gridB.horizontal.ToString();
        mGridBaseListManager[gridB.vertical][gridB.horizontal].gridBean = mGridListManager[gridB.vertical][gridBInedx];
        mGridBaseListManager[gridB.vertical][gridB.horizontal].spriteIndex = mGridListManager[gridB.vertical][gridBInedx].spriteIndex;
    }

    //设置一条固定连线
    private static void setOneLine(List<LeaveGridMsg> leaveGridMsgList)
    {
        oneLineMsg = new List<LeaveGridMsg>();

        //随机初始点
        LeaveGridMsg startPoint = leaveGridMsgList[UnityEngine.Random.Range(0, leaveGridMsgList.Count)];
        oneLineMsg.Add(startPoint);
        targetCounts--;
        getNextPoint();
    }

    //递归获取下个点
    private static void getNextPoint()
    {
        usableList.Clear();
        if (targetCounts != 0)
        {
            int v = oneLineMsg[oneLineMsg.Count - 1].vertical;
            int h = oneLineMsg[oneLineMsg.Count - 1].horizontal;
            checkNextPointArond(v, h);
        }
    }

    //检测下个点周围的格子是否可用，并记录
    private static void checkNextPointArond(int v, int h)
    {
        //左上
        if (v > 0 && h > 0)
            checkUsable(v - 1, h - 1, "LeftTop");
        //左
        if (v > 0)
            checkUsable(v - 1, h, "Left");
        //左下
        if (v > 0 && h < 8)
            checkUsable(v - 1, h + 1, "LeftButtom");
        //正上
        if (h > 0)
            checkUsable(v, h - 1, "Top");
        //正下
        if (h < 8)
            checkUsable(v, h + 1, "Buttom");
        //右上
        if (v < 8 && h > 0)
            checkUsable(v + 1, h - 1, "RightTop");
        //右
        if (v < 8)
            checkUsable(v + 1, h, "Right");
        //右下
        if (v < 8 && h < 8)
            checkUsable(v + 1, h + 1, "RightButtom");

        int randomNext = UnityEngine.Random.Range(0, usableList.Count);
        LeaveGridMsg leaveGridMsg = new LeaveGridMsg();
        switch (usableList[randomNext])
        {
            case "LeftTop":
                leaveGridMsg.vertical = v - 1;
                leaveGridMsg.horizontal = h - 1;
                break;
            case "Left":
                leaveGridMsg.vertical = v - 1;
                leaveGridMsg.horizontal = h;
                break;
            case "LeftButtom":
                leaveGridMsg.vertical = v - 1;
                leaveGridMsg.horizontal = h + 1;
                break;
            case "Top":
                leaveGridMsg.vertical = v;
                leaveGridMsg.horizontal = h - 1;
                break;
            case "Buttom":
                leaveGridMsg.vertical = v;
                leaveGridMsg.horizontal = h + 1;
                break;
            case "RightTop":
                leaveGridMsg.vertical = v + 1;
                leaveGridMsg.horizontal = h - 1;
                break;
            case "Right":
                leaveGridMsg.vertical = v + 1;
                leaveGridMsg.horizontal = h;
                break;
            case "RightButtom":
                leaveGridMsg.vertical = v + 1;
                leaveGridMsg.horizontal = h + 1;
                break;
        }
        oneLineMsg.Add(leaveGridMsg);
        targetCounts--;
        getNextPoint();
    }

    private static void checkUsable(int v, int h, string direction)
    {
        bool endCheck = false;
        //检测是否已在LeaveGridList中
        foreach (LeaveGridMsg leaveGridMsg in oneLineMsg)
        {
            if (leaveGridMsg.vertical == v && leaveGridMsg.horizontal == h)
            {
                endCheck = true;
                break;
            }
        }
        if (!endCheck)
        {
            //判断是否为可用方块
            int nextPointSpriteIndex = mGridBaseListManager[v][h].spriteIndex;
            if (mGridBaseListManager[v][h].isHasGrid && nextPointSpriteIndex != -1 && !(nextPointSpriteIndex >= 15 && nextPointSpriteIndex <= 21))
                usableList.Add(direction);
        }
    }

    //检测格子周围的类型
    private static void checkTypeAroundTheGrid(int v, int h)
    {
        //左上
        if (!isConn && v > 0 && h > 0)
            checkGridTpye(v - 1, h - 1);
        //左
        if (!isConn && v > 0)
            checkGridTpye(v - 1, h);
        //左下
        if (!isConn && v > 0 && h < 8)
            checkGridTpye(v - 1, h + 1);
        //正上
        if (!isConn && h > 0)
            checkGridTpye(v, h - 1);
        //正下
        if (!isConn && h < 8)
            checkGridTpye(v, h + 1);
        //右上
        if (!isConn && v < 8 && h > 0)
            checkGridTpye(v + 1, h - 1);
        //右
        if (!isConn && v < 8)
            checkGridTpye(v + 1, h);
        //右下
        if (!isConn && v < 8 && h < 8)
            checkGridTpye(v + 1, h + 1);
    }

    //检查格子类型
    private static void checkGridTpye(int v, int h)
    {
        if (!mGridBaseListManager[v][h].isCheck && mGridBaseListManager[v][h].isHasGrid && mGridBaseListManager[v][h].spriteIndex != -1 && !(mGridBaseListManager[v][h].spriteIndex >= 15 && mGridBaseListManager[v][h].spriteIndex <= 21))
        {
            mGridBaseListManager[v][h].isCheck = true;
            if (mGridBaseListManager[v][h].spriteIndex == currentSpriteIndex)
            {
                theSameTypeCounts++;
                if (theSameTypeCounts < 3)
                    checkTypeAroundTheGrid(v, h);
                else
                    isConn = true;
            }
        }
        else
        {
            mGridBaseListManager[v][h].isCheck = true;
        }
    }

}