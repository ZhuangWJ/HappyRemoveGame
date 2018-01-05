using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridActivity : MonoBehaviour
{
    private static string[] gridBaseTpyeId = new string[] { "gridbase" };
    private static List<Sprite> baseSprites = new List<Sprite>();
    private static string[] resourcesId = new string[] { "random", "visable", "monkey", "panda", "chicken", "penguin", "pig", "rabbit", "door", "door", "ice1", "ice2", "ice3", "beanpod", "basket", "frosting1", "frosting2", "frosting3", "frosting4", "frosting5", "poison", "lock" };
    private static List<Sprite> allSprites = new List<Sprite>();

    public GameObject mainCanvas;
    public Font songTi;

    private GameObject GridBg;//格子背景的父对象
    private GameObject Grid;//方块的父对象
    private GameObject GameBackground;//背景图父对象

    private float x;//生成方块RectTransform的posX
    private float y;//生成方块RectTransform的posY
    private static GameObject stepCounts;
    private GameData gameData;//json数据对象
    private static EditorData editorData;//配置文件的内容
    private float intervalPx;//方块的间隔
    private float interval; //两个方块中心点的距离，即方块本身的Size + intervalPx
    private float gridSize;//方块的width和Height
    private float leaveSize; //屏幕左右两边共预留的像素

    private static List<List<GridBean>> gridListManager = new List<List<GridBean>>();//管理所有列的List
    private List<GridBean> gridList;//一列方块的List
    private static List<List<GridBaseBean>> gridBaseListManager = new List<List<GridBaseBean>>();//管理所有列背景的List
    private List<GridBaseBean> gridBaseList;//一列方块的背景List
    private List<GridBean> gridDropList = new List<GridBean>();//掉落备用方块List

    private List<GameObject> lineObjList = new List<GameObject>();//线段对象管理List
    private List<GridBean> lineConnectGridList = new List<GridBean>();//线段连接的方块管理List
    private float drawWidth;//线段宽度
    private float drawHeight;//线段高度
    private Vector3 startPoint;//线段初始点
    private Vector3 nextPoint;//线段结束点
    private Vector3 drawPoint;//线段对象的position
    private int startHorizontal;//线段开始位置所处于的对象行
    private int startVertical;//线段开始位置所处于的对象列
    private int nextHorizontal;//线段结束位置所处于的对象行
    private int nextVertical;//线段结束位置所处于的对象行
    private float lineRotationZ;//线段对象旋转角度
    private int drawCounts = 0;//线段数

    private float gameBgWith;//背景图宽度
    private float gameBgHeight;//背景图高度

    private GameObject targetBoard;//目标板对象
    private GameObject stepBoard;//步数板对象
    private static GameObject targetCount;//目标类型数量
    private int deleteCounts;//已消除的数量
    private static GameObject great;//完成目标后显示的对象
    private static GameObject targetGrid;//目标类型
    private List<BeanPod> beanPodList;
    private static List<GridBean> gridOfBeanPodList = new List<GridBean>();//金豆荚List
    private static List<GridBean> frostingList = new List<GridBean>();//雪块List
    private static List<GridBean> poisonList = new List<GridBean>();//毒液List

    private List<GridBean> gridDataList;
    private int nextListIndex;
    private int startListIndex;
    private List<DoorBean> doorDataList;
    private Vector3 doorPoint;
    private List<IceBean> iceDataList;
    private int mIceLevel;
    private List<BasketBean> basketDataList;
    private static int createBeanPodStep;
    private static int isCreateBeanPod;
    private static int beanPodIndexMax;
    private static int beanPodIndexMin;
    private static int beanPodIndex;
    private bool isRemovePoison;
    private List<string> moveList = new List<string>();

    // Use this for initialization
    void Start()
    {
        //读取配置和解析数据
        initData();

        //初始化游戏背景
        initGameBg();

        //加载资源
        initResources();

        //初始化关卡数据，如通过目标，数量，步数
        initPlayLevelMsg();

        //初始化方块
        initGrid();

        //初始化关卡完成界面
        initFinishPlayLevel();

        //初始化GridDrop，传送数据
        GridDrop.initGridUIAttribute();

        GridUIAttributeManager.getInstance().deleteCounts = deleteCounts;
    }

    private void initResources()
    {
        //加载方块背景类型资源
        for (int i = 0; i < gridBaseTpyeId.Length; i++)
        {
            Sprite sprite = new Sprite();
            sprite = Resources.Load<Sprite>(gridBaseTpyeId[i]) as Sprite;
            baseSprites.Add(sprite);
        }

        //加载方块背景类型资源
        for (int i = 0; i < resourcesId.Length; i++)
        {
            Sprite sprite = new Sprite();
            sprite = Resources.Load<Sprite>(resourcesId[i]) as Sprite;
            allSprites.Add(sprite);
        }
        GridUIAttributeManager.getInstance().allSprites = allSprites;
    }

    private void initPlayLevelMsg()
    {
        //[1]设置目标类型
        targetGrid = Instantiate(Resources.Load("prefabs/grid"), targetBoard.transform) as GameObject;
        targetGrid.name = "targetGrid";
        Destroy(targetGrid.GetComponent<SpriteRenderer>());
        targetGrid.AddComponent<Image>();
        targetGrid.GetComponent<Image>().sprite = allSprites[editorData.targetType];
        targetGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.1f * 0.7f, gameBgWith * 0.1f * 0.7f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            targetGrid.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2 - gameBgWith * 0.1f * 0.7f * 1 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgHeight * 0.1f * 2 / 4, 0.0f);
        else
            targetGrid.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 - gameBgWith * 0.1f * 0.7f * 1 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgWith * 0.1f * 2 / 4, 0.0f);

        //[1.1]判断通过目标是否金豆荚
        if (editorData.targetType == 13)
        {
            beanPodList = new List<BeanPod>();
            BeanPod beanPod = new BeanPod();
            beanPod.beanPodVeritcal = UnityEngine.Random.Range(0, 9);
            beanPod.beanPodHorizontal = 0;
            beanPodList.Add(beanPod);

            //生成金豆荚的平均步数
            createBeanPodStep = editorData.stepCounts / editorData.targetCounts;
            isCreateBeanPod = createBeanPodStep;
        }

        //[1.2]设置目标数量
        targetCount = new GameObject();
        targetCount.name = "targetCount";
        targetCount.AddComponent<Text>();
        targetCount.GetComponent<Text>().text = "x" + editorData.targetCounts;
        targetCount.GetComponent<Text>().fontSize = (int)(gameBgWith * 0.1f * 0.7f / 2);
        targetCount.GetComponent<Text>().fontStyle = FontStyle.Bold;
        targetCount.GetComponent<Text>().color = Color.yellow;
        targetCount.GetComponent<Text>().font = songTi;
        targetCount.transform.SetParent(targetBoard.transform);
        targetCount.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.1f * 0.8f, gameBgWith * 0.1f * 0.8f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            targetCount.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2 + gameBgWith * 0.1f * 0.7f * 1 * 2 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgHeight * 0.1f * 2 / 4 - gameBgWith * 0.1f * 0.5f, 0.0f);
        else
            targetCount.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 + gameBgWith * 0.1f * 0.7f * 1 * 2 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgHeight * 0.1f * 2 / 4 - gameBgWith * 0.1f * 0.5f, 0.0f);
        editorData.targetTypeObj = targetGrid;
        editorData.targetCountObj = targetCount;

        //[1.3]设置可用步数
        stepCounts = new GameObject();
        stepCounts.name = "stepCounts";
        stepCounts.AddComponent<Text>();
        stepCounts.GetComponent<Text>().text = editorData.stepCounts.ToString();
        stepCounts.GetComponent<Text>().fontSize = (int)(gameBgWith * 0.1f);
        stepCounts.GetComponent<Text>().fontStyle = FontStyle.Bold;
        stepCounts.GetComponent<Text>().color = Color.red;
        stepCounts.GetComponent<Text>().font = songTi;
        stepCounts.transform.SetParent(stepBoard.transform);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            stepCounts.GetComponent<RectTransform>().position = new Vector3(gameBgWith - gameBgWith * 0.2f / 2, gameBgHeight - gameBgWith * 0.2f / 2, 0.0f);
        else
            stepCounts.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 + gameBgWith / 2 - gameBgWith * 0.2f / 2, gameBgHeight - gameBgWith * 0.2f / 2, 0.0f);
        editorData.stepCountsObj = stepCounts;
    }

    public void initData()
    {
        //[0]读取配置，获取对象
        editorData = JsonUtil.getEditorData(7);
        GridUIAttributeManager.getInstance().editorData = editorData;

        //[1]获取格子内容信息
        if (!editorData.gridData.Equals(""))
        {
            string[] gridDatas = editorData.gridData.Split(',');
            gridDataList = new List<GridBean>();
            foreach (string grid in gridDatas)
            {
                if (!grid.Equals(""))
                {
                    string[] result = grid.Split('|');
                    GridBean gridBean = new GridBean();
                    gridBean.listVertical = int.Parse(result[0]);
                    gridBean.listHorizontal = int.Parse(result[1]);
                    gridBean.spriteIndex = int.Parse(result[2]);
                    gridDataList.Add(gridBean);
                }
            }
            GridUIAttributeManager.getInstance().gridDataList = gridDataList;
        }

        //[2]获取传送门数据
        if (!editorData.doorData.Equals(""))
        {
            string[] doorDatas = editorData.doorData.Split(',');
            doorDataList = new List<DoorBean>();
            foreach (string door in doorDatas)
            {
                if (!door.Equals(""))
                {
                    string[] result = door.Split('|');
                    DoorBean doorBean = new DoorBean();
                    doorBean.inVertical = int.Parse(result[0]);
                    doorBean.inHorizontal = int.Parse(result[1]);
                    doorBean.outVertical = int.Parse(result[2]);
                    doorBean.outHorizontal = int.Parse(result[3]);
                    doorDataList.Add(doorBean);
                }
            }
            GridUIAttributeManager.getInstance().doorDataList = doorDataList;
        }

        //[3]获取冰块数据
        if (!editorData.iceData.Equals(""))
        {
            string[] iceDatas = editorData.iceData.Split(',');
            iceDataList = new List<IceBean>();
            foreach (string ice in iceDatas)
            {
                if (!ice.Equals(""))
                {
                    string[] result = ice.Split('|');
                    IceBean iceBean = new IceBean();
                    iceBean.iceVertical = int.Parse(result[0]);
                    iceBean.iceHorizontal = int.Parse(result[1]);
                    iceBean.iceLevel = int.Parse(result[2]);
                    iceDataList.Add(iceBean);
                }
            }
            GridUIAttributeManager.getInstance().iceDataList = iceDataList;
        }

        //获取金豆荚篮子数据
        if (!editorData.basketData.Equals(""))
        {
            string[] basketDatas = editorData.basketData.Split(',');
            basketDataList = new List<BasketBean>();
            foreach (string basket in basketDatas)
            {
                if (!basket.Equals(""))
                {
                    string[] result = basket.Split('|');
                    BasketBean basketBean = new BasketBean();
                    basketBean.basketVertical = int.Parse(result[0]);
                    basketBean.basketHorizontal = int.Parse(result[1]);
                    basketDataList.Add(basketBean);
                }
            }
            GridUIAttributeManager.getInstance().basketDataList = basketDataList;
        }
    }

    public void initFinishPlayLevel()
    {
        great = new GameObject();
        great.name = "win";
        great.AddComponent<Image>();
        great.GetComponent<RectTransform>().SetParent(mainCanvas.transform);
        Sprite sprite_great = new Sprite();
        sprite_great = Resources.Load("great_text", sprite_great.GetType()) as Sprite;
        great.GetComponent<Image>().sprite = sprite_great;
        great.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith / 2, gameBgWith / 2);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            great.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2, gameBgHeight / 2, 0.0f);
        else
            great.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gameBgHeight / 2, 0.0f);
        great.SetActive(false);
    }

    public void initGameBg()
    {
        //[0]创建UI对象
        GameBackground = new GameObject();
        GridUIAttributeManager.getInstance().GameBackground = GameBackground;
        GameObject branch = new GameObject();
        targetBoard = new GameObject();
        stepBoard = new GameObject();

        //[1]给UI对象命名
        GameBackground.name = "gameBackground";
        branch.name = "branch";
        targetBoard.name = "targetBoard";
        stepBoard.name = "stepBoard";

        //[2]添加对象组件
        GameBackground.AddComponent<Image>();
        branch.AddComponent<Image>();
        targetBoard.AddComponent<Image>();
        stepBoard.AddComponent<Image>();

        //[3]设置对象父对象
        GameBackground.GetComponent<RectTransform>().SetParent(mainCanvas.transform);
        branch.GetComponent<RectTransform>().SetParent(GameBackground.transform);
        targetBoard.GetComponent<RectTransform>().SetParent(GameBackground.transform);
        stepBoard.GetComponent<RectTransform>().SetParent(GameBackground.transform);

        //[4]加载UI所需Sprite
        Sprite sprite_gameBackground = new Sprite();
        Sprite sprite_branch = new Sprite();
        Sprite sprite_targetBoard = new Sprite();
        Sprite sprite_stepBoard = new Sprite();

        sprite_gameBackground = Resources.Load("background_04", sprite_gameBackground.GetType()) as Sprite;
        sprite_branch = Resources.Load("branch", sprite_branch.GetType()) as Sprite;
        sprite_targetBoard = Resources.Load("target_board", sprite_targetBoard.GetType()) as Sprite;
        sprite_stepBoard = Resources.Load("stepboard", sprite_stepBoard.GetType()) as Sprite;

        GameBackground.GetComponent<Image>().sprite = sprite_gameBackground;
        branch.GetComponent<Image>().sprite = sprite_branch;
        targetBoard.GetComponent<Image>().sprite = sprite_targetBoard;
        stepBoard.GetComponent<Image>().sprite = sprite_stepBoard;

        //[5]设置对象position和大小
        GameBackground.GetComponent<Image>().SetNativeSize();
        gameBgWith = GameBackground.GetComponent<RectTransform>().rect.width;
        gameBgHeight = GameBackground.GetComponent<RectTransform>().rect.height;
        //如果屏幕高宽比例大于背景高宽比，则使用屏幕宽度作为背景宽度，则反之
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
        {
            gameBgWith = Screen.width;
            gameBgHeight = Screen.width * gameBgHeight / gameBgWith;
            GameBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith, gameBgHeight);
            GameBackground.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gameBgHeight / 2, 0.0f);
        }
        else
        {
            gameBgWith = Screen.height / (gameBgHeight / gameBgWith);
            gameBgHeight = Screen.height;
            GameBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith, gameBgHeight);
            GameBackground.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, Screen.height / 2, 0.0f);
        }
        // Debug.Log("gameBackground.position:" + gameBackground.GetComponent<RectTransform>().position);

        branch.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.75f, gameBgHeight * 0.2f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            branch.GetComponent<RectTransform>().position = new Vector3(gameBgWith * 0.75f / 2, gameBgHeight - gameBgHeight * 0.23f / 2, 0.0f);
        else
            branch.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 - gameBgWith * (0.5f - 0.75f / 2), gameBgHeight - gameBgHeight * 0.23f / 2, 0.0f);

        targetBoard.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.18f, gameBgHeight * 0.13f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            targetBoard.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2, gameBgHeight - gameBgHeight * 0.1f / 2, 0.0f);
        else
            targetBoard.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gameBgHeight - gameBgHeight * 0.1f / 2, 0.0f);

        stepBoard.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.2f, gameBgWith * 0.2f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            stepBoard.GetComponent<RectTransform>().position = new Vector3(gameBgWith - gameBgWith * 0.2f / 2, gameBgHeight - gameBgWith * 0.2f / 2, 0.0f);
        else
            stepBoard.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 + gameBgWith / 2 - gameBgWith * 0.2f / 2, gameBgHeight - gameBgWith * 0.2f / 2, 0.0f);
    }

    public void initGrid()
    {
        //[0]初始化父控件
        GridBg = new GameObject();
        Grid = new GameObject();
        GridUIAttributeManager.getInstance().Gird = Grid;
        GridUIAttributeManager.getInstance().GridBg = GridBg;

        GridBg.AddComponent<RectTransform>();
        Grid.AddComponent<RectTransform>();
        GridBg.name = "GridBg";
        Grid.name = "Grid";
        GridBg.GetComponent<RectTransform>().SetParent(mainCanvas.transform);
        Grid.GetComponent<RectTransform>().SetParent(mainCanvas.transform);

        //[2]设置格子大小 
        gameData = new GameData();
        gameData.horizontal = 9;
        gameData.vertical = 9;
        GridUIAttributeManager.getInstance().gameData = gameData;
        leaveSize = 0;
        intervalPx = 1.0f;
        if (Screen.height >= Screen.width)
        {
            if (gameData.vertical > 9)
                gridSize = (Screen.width - leaveSize - (gameData.vertical - 1) * intervalPx) / gameData.vertical;
            else
                gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;
        }
        else
        {
            if (gameData.horizontal > 9)
                gridSize = (gameBgWith - leaveSize - (gameData.horizontal - 1) * intervalPx) / gameData.horizontal;
            else
                gridSize = (gameBgWith - leaveSize - (9 - 1) * intervalPx) / 9;
        }

        interval = gridSize + intervalPx;
        GridUIAttributeManager.getInstance().gridSize = gridSize;
        GridUIAttributeManager.getInstance().interval = interval;
        //[3]动态创建方块
        //[3.1]设置每一列方块的初始位置 x 
        if (Screen.height >= Screen.width)
            x = leaveSize / 2 + gridSize / 2;
        else
            x = Screen.width / 2 - gameBgWith / 2 + leaveSize / 2 + gridSize / 2;

        for (int vertical = 0; vertical < gameData.vertical; vertical++, x = x + interval)
        {
            //[3.2]设置第一行方块的初始位置 y
            y = Screen.height * 0.75f - gridSize / 2 + interval;
            gridList = new List<GridBean>();
            gridBaseList = new List<GridBaseBean>();
            for (int horizontal = 0; horizontal < gameData.horizontal + 1; horizontal++, y = y - interval)
            {
                //[3.3]生成场景
                if (horizontal != 0)
                {
                    GameObject gridbase = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
                    Destroy(gridbase.GetComponent<SpriteRenderer>());
                    gridbase.name = "gridbase" + vertical.ToString() + (horizontal - 1).ToString();
                    gridbase.AddComponent<Image>();
                    gridbase.GetComponent<Image>().sprite = baseSprites[0];
                    gridbase.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                    gridbase.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                    GridBaseBean gridBaseBean = new GridBaseBean();
                    gridBaseBean.gridBase = gridbase;
                    gridBaseList.Add(gridBaseBean);
                }

                //[3.31]生成冰块
                if (iceDataList != null && horizontal != 0)
                {
                    foreach (IceBean iceBean in iceDataList)
                    {
                        if (iceBean.iceVertical == vertical && iceBean.iceHorizontal == horizontal - 1)
                        {
                            GameObject ice = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
                            Destroy(ice.GetComponent<SpriteRenderer>());
                            ice.name = "ice" + iceBean.iceVertical.ToString() + iceBean.iceHorizontal.ToString();
                            ice.AddComponent<Image>();
                            ice.GetComponent<Image>().sprite = allSprites[iceBean.iceLevel + 9];
                            ice.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                            ice.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                            iceBean.ice = ice;
                            break;
                        }
                    }
                }

                //[3.32]生成金豆荚篮子
                if (basketDataList != null && horizontal != 0)
                {
                    foreach (BasketBean basketBean in basketDataList)
                    {
                        if (basketBean.basketVertical == vertical && basketBean.basketHorizontal == horizontal - 1)
                        {
                            GameObject basket = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
                            Destroy(basket.GetComponent<SpriteRenderer>());
                            basket.name = "basket" + basketBean.basketVertical.ToString() + basketBean.basketHorizontal.ToString();
                            basket.AddComponent<Image>();
                            basket.GetComponent<Image>().sprite = allSprites[14];
                            basket.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize * 0.9f, gridSize * 0.4f);
                            basket.GetComponent<RectTransform>().position = new Vector3(x, y, 0) + new Vector3(0.0f, -gridSize / 2 - gridSize * 0.4f / 2, 0.0f);
                            basketBean.basket = basket;
                            break;
                        }
                    }
                }

                //[3.4]生成方块
                GameObject grid = Instantiate(Resources.Load("prefabs/grid"), Grid.transform) as GameObject;
                Destroy(grid.GetComponent<SpriteRenderer>());
                grid.AddComponent<Image>();
                GridBean gridBean = new GridBean();

                if (horizontal != 0)
                    gridBaseList[horizontal - 1].gridBean = gridBean;

                //遍历配置文件，若有对应固定的位置，则根据情况显示
                if (horizontal != 0)
                {
                    //默认随机
                    gridBean.spriteIndex = UnityEngine.Random.Range(2, 8);
                    gridBaseList[horizontal - 1].isHasGrid = true;
                    //如果配置数据为空，则默认随机
                    if (gridDataList != null)
                    {
                        foreach (GridBean gridData in gridDataList)
                        {
                            if (vertical == gridData.listVertical && (horizontal - 1) == gridData.listHorizontal)
                            {
                                switch (gridData.spriteIndex)
                                {
                                    case 1://不显示
                                        gridBaseList[horizontal - 1].spriteIndex = -1;
                                        gridBean.spriteIndex = gridData.spriteIndex;
                                        gridBaseList[horizontal - 1].gridBase.SetActive(false);
                                        grid.SetActive(false);
                                        break;
                                    case 15://雪块
                                    case 16:
                                    case 17:
                                    case 18:
                                    case 19:
                                        gridBean.spriteIndex = gridData.spriteIndex;
                                        gridBaseList[horizontal - 1].spriteIndex = gridData.spriteIndex;
                                        frostingList.Add(gridBean);
                                        break;
                                    case 20:
                                        gridBean.spriteIndex = gridData.spriteIndex;
                                        gridBaseList[horizontal - 1].spriteIndex = gridData.spriteIndex;
                                        poisonList.Add(gridBean);
                                        break;
                                    default://默认根据ID显示对应资源
                                        gridBean.spriteIndex = gridData.spriteIndex;
                                        break;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        //如果格子内容没有数据， 则默认为空
                        gridBean.spriteIndex = UnityEngine.Random.Range(2, 8);
                        gridBaseList[horizontal - 1].isHasGrid = true;
                    }
                }
                else
                {
                    //备用方块的资源
                    gridBean.spriteIndex = UnityEngine.Random.Range(2, 8);
                }

                //设置金豆荚位置
                if (beanPodList != null && horizontal != 0)
                {
                    foreach (BeanPod beanPod in beanPodList)
                    {
                        if (beanPod.beanPodVeritcal == vertical && beanPod.beanPodHorizontal == horizontal - 1)
                        {
                            gridBean.spriteIndex = 13;
                            gridOfBeanPodList.Add(gridBean);
                            break;
                        }
                    }
                }

                if (gridBean.spriteIndex > 1)
                    grid.GetComponent<Image>().sprite = allSprites[gridBean.spriteIndex];
                grid.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                gridBean.gridObject = grid;

                //[3.5]储存格子掉落信息
                if (horizontal == 0)
                {
                    gridBean.listHorizontal = horizontal;
                    gridBean.listVertical = vertical;
                    grid.name = "grid" + gridBean.listVertical.ToString() + gridBean.listHorizontal.ToString();
                    grid.SetActive(false);
                    gridBean.isTop = true;
                    gridDropList.Add(gridBean);
                }

                //[3.6]储存格式显示信息
                if (horizontal != 0)
                {
                    gridBean.listHorizontal = horizontal - 1;
                    gridBean.listVertical = vertical;
                    grid.name = "grid" + gridBean.listVertical.ToString() + gridBean.listHorizontal.ToString();
                    gridBean.isTop = false;
                    gridList.Add(gridBean);
                }
            }
            gridBaseListManager.Add(gridBaseList);
            gridListManager.Add(gridList);
        }
        GridUIAttributeManager.getInstance().gridDropList = gridDropList;
        GridUIAttributeManager.getInstance().gridListManager = gridListManager;
        GridUIAttributeManager.getInstance().gridBaseListManager = gridBaseListManager;
        if (gridOfBeanPodList != null)
            GridUIAttributeManager.getInstance().gridOfBeanPodList = gridOfBeanPodList;

        //[4]生成传送门
        if (doorDataList != null)
        {
            foreach (DoorBean doorbean in doorDataList)
            {
                //入口
                GameObject indoor = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
                Destroy(indoor.GetComponent<SpriteRenderer>());
                indoor.name = "indoor" + doorbean.inVertical.ToString() + doorbean.inHorizontal.ToString();
                indoor.AddComponent<Image>();
                indoor.GetComponent<Image>().sprite = allSprites[8];
                doorPoint = gridListManager[doorbean.inVertical][doorbean.inHorizontal].gridObject.GetComponent<RectTransform>().position;
                indoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, -gridSize * 2 / 3, 0.0f);
                indoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                indoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorbean.indoor = indoor;

                //出口
                GameObject outdoor = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
                Destroy(outdoor.GetComponent<SpriteRenderer>());
                outdoor.name = "outdoor" + doorbean.outVertical.ToString() + doorbean.outHorizontal.ToString();
                outdoor.AddComponent<Image>();
                outdoor.GetComponent<Image>().sprite = allSprites[9];
                doorPoint = gridListManager[doorbean.outVertical][doorbean.outHorizontal].gridObject.GetComponent<RectTransform>().position;
                outdoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, gridSize * 2 / 3, 0.0f);
                outdoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                outdoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorbean.outdoor = outdoor;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //[1]隐藏目标完成后的对象
            great.SetActive(false);

            //[2]计算格子所在边界，x为左边界，y为上边界
            y = Screen.height * 0.75f;
            if (Screen.height >= Screen.width)
                x = leaveSize / 2;
            else
                x = Screen.width / 2 - gameBgWith / 2 + leaveSize / 2;
            intervalPx = 1.0f;

            //[3]鼠标点中格子区域才会响应，记录初次点中的方块信息
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                startVertical = (int)((Input.mousePosition.x - x) / (gridSize + intervalPx));
                startHorizontal = startListIndex = (int)((y - Input.mousePosition.y) / (gridSize + intervalPx));
                int startSpriteIndex = gridBaseListManager[startVertical][startHorizontal].spriteIndex;
                if (gridBaseListManager[startVertical][startHorizontal].isHasGrid && startSpriteIndex != -1 && !(startSpriteIndex >= 15 && startSpriteIndex <= 20))
                {
                    //判断startVertical该列上没有方块
                    for (int h = startHorizontal - 1; h >= 0; h--)
                    {
                        if (!gridBaseListManager[startVertical][h].isHasGrid)
                            startListIndex--;
                    }

                    if (gridListManager[startVertical][startListIndex].spriteIndex != -1)
                    {
                        startPoint = gridListManager[startVertical][startListIndex].gridObject.GetComponent<RectTransform>().position;
                        lineConnectGridList.Add(gridListManager[startVertical][startListIndex]);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            //[1]鼠标点中格子区域才会响应，记录划动经过的方块信息
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                nextVertical = (int)((Input.mousePosition.x - x) / (gridSize + intervalPx));
                nextHorizontal = nextListIndex = (int)((y - Input.mousePosition.y) / (gridSize + intervalPx));
                int nextSpriteIndex = gridBaseListManager[nextVertical][nextHorizontal].spriteIndex;
                if (gridBaseListManager[nextVertical][nextHorizontal].isHasGrid && nextSpriteIndex != -1 && !(nextSpriteIndex >= 15 && nextSpriteIndex <= 20))
                {

                    //判断nextVertical该列上没有方块
                    for (int h = nextHorizontal - 1; h >= 0; h--)
                    {
                        if (!gridBaseListManager[nextVertical][h].isHasGrid)
                            nextListIndex--;
                    }

                    //[2]判断鼠标划动经过的对象是否已在数组中，如果已经存在，则不响应
                    if (!lineConnectGridList.Contains(gridListManager[nextVertical][nextListIndex]))
                    {
                        if ((nextHorizontal != startHorizontal || nextVertical != startVertical) && gridListManager[nextVertical][nextListIndex].spriteIndex == gridListManager[startVertical][startListIndex].spriteIndex && System.Math.Abs(nextHorizontal - startHorizontal) <= 1 && System.Math.Abs(nextVertical - startVertical) <= 1)
                        {
                            if (gridListManager[nextVertical][nextListIndex].spriteIndex != -1)
                            {
                                nextPoint = gridListManager[nextVertical][nextListIndex].gridObject.GetComponent<RectTransform>().position;
                                
                                //[3]绘制线段
                                GameObject line = new GameObject();
                                line.AddComponent<Image>();
                                line.GetComponent<Image>().color = new Color(1.0f, 0, 0, 0.5f);

                                //[3.1]计算旋转角度
                                //左上
                                if (nextHorizontal < startHorizontal && nextVertical < startVertical)
                                {
                                    lineRotationZ = 225.0f;
                                    drawPoint = startPoint + new Vector3(-gridSize / 2, gridSize / 2, 0.0f);
                                }
                                //正上方
                                if (nextHorizontal < startHorizontal && nextVertical == startVertical)
                                {
                                    lineRotationZ = 180.0f;
                                    drawPoint = startPoint + new Vector3(0.0f, gridSize / 2, 0.0f);
                                }
                                //右上
                                if (nextHorizontal < startHorizontal && nextVertical > startVertical)
                                {
                                    lineRotationZ = 135.0f;
                                    drawPoint = startPoint + new Vector3(gridSize / 2, gridSize / 2, 0.0f);
                                }
                                //左边
                                if (nextHorizontal == startHorizontal && nextVertical < startVertical)
                                {
                                    lineRotationZ = 270.0f;
                                    drawPoint = startPoint + new Vector3(-gridSize / 2, 0.0f, 0.0f);
                                }
                                //右边
                                if (nextHorizontal == startHorizontal && nextVertical > startVertical)
                                {
                                    lineRotationZ = 90.0f;
                                    drawPoint = startPoint + new Vector3(gridSize / 2, 0.0f, 0.0f);
                                }
                                //左下
                                if (nextHorizontal > startHorizontal && nextVertical < startVertical)
                                {
                                    lineRotationZ = 315.0f;
                                    drawPoint = startPoint + new Vector3(-gridSize / 2, -gridSize / 2, 0.0f);
                                }
                                //正下方
                                if (nextHorizontal > startHorizontal && nextVertical == startVertical)
                                {
                                    lineRotationZ = 360.0f;
                                    drawPoint = startPoint + new Vector3(0.0f, -gridSize / 2, 0.0f);
                                }
                                //右下
                                if (nextHorizontal > startHorizontal && nextVertical > startVertical)
                                {
                                    lineRotationZ = 45.0f;
                                    drawPoint = startPoint + new Vector3(gridSize / 2, -gridSize / 2, 0.0f);
                                }

                                //[3.2]计算线段宽度，为方块的1/8
                                drawWidth = gridSize / 8.0f;
                                //[3.3]计算线段长度，为两个中心的距离
                                drawHeight = (startPoint - nextPoint).magnitude;
                                //[3.4]设置线段宽度
                                line.GetComponent<RectTransform>().sizeDelta = new Vector2(drawWidth, drawHeight);
                                //[3.5]设置线段开始位置
                                line.GetComponent<RectTransform>().position = drawPoint;
                                line.transform.SetParent(Grid.transform);
                                //[3.6]设置线段的旋转角度
                                line.GetComponent<RectTransform>().Rotate(new Vector3(0.0f, 0.0f, lineRotationZ));
                                //[3.7]线段连接对象和线段数组添加方块，将划动经过的点作为初始点
                                lineConnectGridList.Add(gridListManager[nextVertical][nextListIndex]);
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

        if (Input.GetMouseButtonUp(0))
        {
            //[1]在格子区域抬起鼠标，则默认用户希望消除方块，否则则撤销消除
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                if (drawCounts >= 2)
                {
                    isRemovePoison = false;
                    foreach (GridBean gridBean in lineConnectGridList)
                    {
                        int currenttVertical = gridBean.listVertical;

                        for (int removeIndex = 0; removeIndex < gridListManager[currenttVertical].Count; removeIndex++)
                        {
                            if (gridListManager[currenttVertical][removeIndex].listHorizontal == gridBean.listHorizontal)
                            {
                                //[2]计算是否消除了目标类型
                                if (gridBean.spriteIndex == editorData.targetType)
                                    deleteCounts++;
                                gridBaseListManager[currenttVertical][gridBean.listHorizontal].isHasGrid = false;
                                gridListManager[currenttVertical].RemoveAt(removeIndex);
                                break;
                            }
                        }

                        //判断方块底部是否有冰块
                        if (iceDataList != null)
                        {
                            foreach (IceBean iceBean in iceDataList)
                            {
                                if (iceBean.iceVertical == currenttVertical && iceBean.iceHorizontal == gridBean.listHorizontal)
                                {
                                    mIceLevel = iceBean.iceLevel;
                                    if (mIceLevel > 0)
                                    {
                                        mIceLevel--;
                                        if (mIceLevel > 0)
                                        {
                                            iceBean.ice.GetComponent<Image>().sprite = allSprites[mIceLevel + 9];
                                            iceBean.iceLevel = mIceLevel;
                                        }
                                        else
                                        {
                                            if (editorData.targetType == 11)
                                            {
                                                deleteCounts++;
                                            }
                                            Destroy(iceBean.ice);
                                            iceDataList.Remove(iceBean);
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        //判断消除附近是否有雪块
                        if (frostingList != null)
                            checkFrosting(gridBean);

                        if (poisonList != null)
                            checkPoison(gridBean);

                        //[3]消除方块
                        Destroy(gridBean.gridObject);
                    }

                    //还原雪块消除信息
                    if (frostingList != null)
                    {
                        for (int i = 0; i < frostingList.Count; i++)
                        {
                            frostingList[i].isFrostingRemove = false;
                        }
                    }

                    //检查是否需要生成毒液
                    checkIsCreatePoison();

                    //[4]记录方块掉落信息
                    GridDrop.recordGridDropMsg();

                    //[5]刷新步数信息
                    editorData.stepCounts--;
                    if (editorData.targetType == 13)
                        isCreateBeanPod--;
                    if (editorData.stepCounts > 0)
                    {
                        stepCounts.GetComponent<Text>().text = editorData.stepCounts.ToString();
                        if (editorData.targetType == 13 && isCreateBeanPod == 0)
                            createBeanPod();
                    }
                    else
                    {
                        //[5.1]已没有步数，提示失败
                    }

                    //[6]刷新目标数量
                    updateTargetCounts(deleteCounts);
                }
            }

            //[7]移除lineConnectGridList的内容
            lineConnectGridList.RemoveRange(0, lineConnectGridList.Count);

            if (lineObjList.Count > 0)
            {
                foreach (GameObject obj in lineObjList)
                {
                    Destroy(obj);
                }
                lineObjList.RemoveRange(0, lineObjList.Count);
            }

            drawCounts = 0;
        }

        //[8]进行方块掉落
        if (gameData != null && gridListManager != null)
            GridDrop.gridDrop();
    }

    //生成一个金豆荚
    public static void createBeanPod()
    {
        beanPodIndexMin = 0;
        beanPodIndexMax = 0;
        for (int v = 0; v < 9; v++)
        {
            if (gridListManager[v][0].dropHeight > 0)
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
        gridListManager[beanPodIndex][0].spriteIndex = 13;
        gridListManager[beanPodIndex][0].gridObject.GetComponent<Image>().sprite = allSprites[13];
        gridOfBeanPodList.Add(gridListManager[beanPodIndex][0]);
        isCreateBeanPod = createBeanPodStep;
    }

    //更新通过目标剩余数量
    public static void updateTargetCounts(int deleteCounts)
    {
        if (deleteCounts >= editorData.targetCounts)
        {
            //[6.1]完成任务
            great.SetActive(true);
            editorData.targetCounts = UnityEngine.Random.Range(15, 30);
            targetCount.GetComponent<Text>().text = editorData.targetCounts.ToString();
            deleteCounts = 0;
            editorData.targetType = UnityEngine.Random.Range(2, 8);
            targetGrid.GetComponent<Image>().sprite = allSprites[editorData.targetType];
            editorData.stepCounts = 20;
            stepCounts.GetComponent<Text>().text = editorData.stepCounts.ToString();
        }
        else
        {
            //[6.2]仍未完成目标
            targetCount.GetComponent<Text>().text = "x" + (editorData.targetCounts - deleteCounts);
        }
    }

    //检查消除方块的上下左右是否有雪块
    private void checkFrosting(GridBean gridBean)
    {
        for (int i = 0; i < frostingList.Count; i++)
        {
            if ((System.Math.Abs(frostingList[i].listVertical - gridBean.listVertical) <= 1 && frostingList[i].listHorizontal - gridBean.listHorizontal == 0) || (frostingList[i].listVertical - gridBean.listVertical == 0 && System.Math.Abs(frostingList[i].listHorizontal - gridBean.listHorizontal) <= 1))
            {
                if (!frostingList[i].isFrostingRemove)
                {
                    frostingList[i].spriteIndex--;
                    if (frostingList[i].spriteIndex >= 15)
                    {
                        frostingList[i].gridObject.GetComponent<Image>().sprite = allSprites[frostingList[i].spriteIndex];
                    }
                    else
                    {
                        Destroy(frostingList[i].gridObject);
                        gridBaseListManager[frostingList[i].listVertical][frostingList[i].listHorizontal].isHasGrid = false;
                    }
                    gridBaseListManager[frostingList[i].listVertical][frostingList[i].listHorizontal].spriteIndex = frostingList[i].spriteIndex;
                    frostingList[i].isFrostingRemove = true;
                }
            }
        }

        //更新雪块List
        for (int v = 0; v < 9; v++)
        {
            for (int h = 0; h < 9; h++)
            {
                if (gridBaseListManager[v][h].spriteIndex == 14)
                {
                    foreach (GridBean grid in frostingList)
                    {
                        if (grid.listVertical == v && grid.listHorizontal == h)
                        {
                            frostingList.Remove(grid);

                            if (h != 0)
                            {
                                int removeIndex = h;
                                for (int x = h - 1; x >= 0; x--)
                                {
                                    if (!gridBaseListManager[v][x].isHasGrid)
                                        removeIndex--;
                                    if (x == 0)
                                        gridListManager[v].RemoveAt(removeIndex);
                                }
                            }
                            else
                            {
                                gridListManager[v].RemoveAt(0);
                            }
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
        int checkCounts = poisonList.Count;
        for (int x = 0; x < checkCounts; x++)
        {
            for (int i = 0, v, h; i < poisonList.Count; i++)
            {
                v = poisonList[i].listVertical;
                h = poisonList[i].listHorizontal;
                if ((System.Math.Abs(v - gridBean.listVertical) <= 1 && h - gridBean.listHorizontal == 0) || (v - gridBean.listVertical == 0 && System.Math.Abs(h - gridBean.listHorizontal) <= 1))
                {
                    gridListManager[v].Remove(poisonList[i]);
                    Destroy(poisonList[i].gridObject);
                    gridBaseListManager[v][h].gridBean = null;
                    gridBaseListManager[v][h].isHasGrid = false;
                    gridBaseListManager[v][h].spriteIndex = 0;//随机赋值，只要不是挖空，雪块、毒液的资源索引值即可
                    isRemovePoison = true;
                    poisonList.RemoveAt(i);
                    deleteCounts++;
                    break;
                }
            }
        }
    }

    //检查是否需要生产毒液
    private void checkIsCreatePoison()
    {
        int checkCounts = poisonList.Count;
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
            for (int i = 0, v, h; i < poisonList.Count; i++)
            {
                v = poisonList[i].listVertical;
                h = poisonList[i].listHorizontal;
                if (h > 0)
                {
                    topGridSpriteIndex = gridBaseListManager[v][h - 1].spriteIndex;
                    if (gridBaseListManager[v][h - 1].isHasGrid && topGridSpriteIndex != -1 && !(topGridSpriteIndex >= 15 && topGridSpriteIndex <= 20))
                        moveList.Add("Top");
                }
                if (h < 8)
                {
                    buttomGridSpriteIndex = gridBaseListManager[v][h + 1].spriteIndex;
                    if (gridBaseListManager[v][h + 1].isHasGrid && buttomGridSpriteIndex != -1 && !(buttomGridSpriteIndex >= 15 && buttomGridSpriteIndex <= 20))
                        moveList.Add("Buttom");
                }
                if (v > 0)
                {
                    leftGridSpriteIndex = gridBaseListManager[v - 1][h].spriteIndex;
                    if (gridBaseListManager[v - 1][h].isHasGrid && leftGridSpriteIndex != -1 && !(leftGridSpriteIndex >= 15 && leftGridSpriteIndex <= 20))
                        moveList.Add("Left");
                }
                if (v < 8)
                {
                    rightGridSpriteIndex = gridBaseListManager[v + 1][h].spriteIndex;
                    if (gridBaseListManager[v + 1][h].isHasGrid && rightGridSpriteIndex != -1 && !(rightGridSpriteIndex >= 15 && rightGridSpriteIndex <= 20))
                        moveList.Add("Right");
                }

                if (moveList.Count > 0)
                {
                    randomIndex = UnityEngine.Random.Range(0, moveList.Count);
                    switch (moveList[randomIndex])
                    {
                        case "Top":
                            topGridIndex = gridListManager[v].IndexOf(poisonList[i]) - 1;
                            gridListManager[v][topGridIndex].spriteIndex = 20;
                            gridListManager[v][topGridIndex].gridObject.GetComponent<Image>().sprite = allSprites[20];
                            gridBaseListManager[v][h - 1].spriteIndex = 20;
                            newPoison.Add(gridListManager[v][topGridIndex]);
                            break;
                        case "Buttom":
                            buttomGridIndex = gridListManager[v].IndexOf(poisonList[i]) + 1;
                            gridListManager[v][buttomGridIndex].spriteIndex = 20;
                            gridListManager[v][buttomGridIndex].gridObject.GetComponent<Image>().sprite = allSprites[20];
                            gridBaseListManager[v][h + 1].spriteIndex = 20;
                            newPoison.Add(gridListManager[v][buttomGridIndex]);
                            break;
                        case "Left":
                            leftGridIndex = gridListManager[v - 1].IndexOf(gridBaseListManager[v - 1][h].gridBean);
                            gridListManager[v - 1][leftGridIndex].spriteIndex = 20;
                            gridListManager[v - 1][leftGridIndex].gridObject.GetComponent<Image>().sprite = allSprites[20];
                            gridBaseListManager[v - 1][h].spriteIndex = 20;
                            newPoison.Add(gridListManager[v - 1][leftGridIndex]);
                            break;
                        case "Right":
                            rightGridIndex = gridListManager[v + 1].IndexOf(gridBaseListManager[v + 1][h].gridBean);
                            gridListManager[v + 1][rightGridIndex].spriteIndex = 20;
                            gridListManager[v + 1][rightGridIndex].gridObject.GetComponent<Image>().sprite = allSprites[20];
                            gridBaseListManager[v + 1][h].spriteIndex = 20;
                            newPoison.Add(gridListManager[v + 1][rightGridIndex]);
                            break;
                    }
                    moveList.Clear();
                }
            }

            //将生成的毒液加入到List中
            foreach (GridBean gridBean in newPoison)
            {
                poisonList.Add(gridBean);
            }

            newPoison.Clear();
        }
    }
}
