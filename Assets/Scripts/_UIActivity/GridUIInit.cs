using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridUIInit : MonoBehaviour
{
    private static string[] gridBaseTypeId = new string[] { "gridbase" };
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

    private float gameBgWith;//背景图宽度
    private float gameBgHeight;//背景图高度

    private GameObject targetBoard;//目标板对象
    private GameObject stepBoard;//步数板对象
    private static GameObject targetCount;//目标类型数量
    private static GameObject great;//完成目标后显示的对象
    private static GameObject targetGrid;//目标类型
    private List<BeanPod> beanPodList;
    private static List<GridBean> gridOfBeanPodList = new List<GridBean>();//金豆荚List
    private static List<GridBean> frostingList = new List<GridBean>();//雪块List
    private static List<GridBean> poisonList = new List<GridBean>();//毒液List

    private List<GridBean> gridDataList;
    
    private List<DoorBean> doorDataList;
    private Vector3 doorPoint;
    private List<IceBean> iceDataList;
    private List<BasketBean> basketDataList;
    
    private List<TimboBean> timboDataList;
    private static int createBeanPodStep;
    private static int isCreateBeanPod;

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

        //传递数据
        GridConn.initGridUIAttribute();
        GridDrop.initGridUIAttribute();
        GridReset.initGridUIAttribute();
    }

    public void initData()
    {
        //[0]读取配置，获取对象
        editorData = JsonUtil.getEditorData(8);
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

        //获取树藤数据
        if (!editorData.timboData.Equals(""))
        {
            string[] timboDatas = editorData.timboData.Split(',');
            timboDataList = new List<TimboBean>();
            foreach (string timbo in timboDatas)
            {
                if (!timbo.Equals(""))
                {
                    string[] result = timbo.Split('|');
                    TimboBean timboBean = new TimboBean();
                    timboBean.timboVertical = int.Parse(result[0]);
                    timboBean.timboHorizontal = int.Parse(result[1]);
                    timboDataList.Add(timboBean);
                }
            }
            GridUIAttributeManager.getInstance().timboDataList = timboDataList;
        }
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

    private void initResources()
    {
        //加载方块背景类型资源
        for (int i = 0; i < gridBaseTypeId.Length; i++)
        {
            Sprite sprite = new Sprite();
            sprite = Resources.Load<Sprite>(gridBaseTypeId[i]) as Sprite;
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
            GridUIAttributeManager.getInstance().createBeanPodStep = createBeanPodStep;
            GridUIAttributeManager.getInstance().isCreateBeanPod = isCreateBeanPod;
        }

        //[1.2]设置目标数量
        targetCount = new GameObject();
        GridUIAttributeManager.getInstance().targetCount = targetCount;
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
        GridUIAttributeManager.getInstance().stepCounts = stepCounts;
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
        GridUIAttributeManager.getInstance().leaveSize = leaveSize;
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
        GridUIAttributeManager.getInstance().intervalPx = intervalPx;
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
                    gridBaseList[horizontal - 1].spriteIndex = gridBean.spriteIndex;
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
                                        gridBaseList[horizontal - 1].spriteIndex = gridBean.spriteIndex;
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
                        gridBaseList[horizontal - 1].spriteIndex = gridBean.spriteIndex;
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

                //生成树藤
                if (timboDataList != null && horizontal != 0)
                {
                    foreach (TimboBean timboBean in timboDataList)
                    {
                        if (timboBean.timboVertical == vertical && timboBean.timboHorizontal == horizontal - 1)
                        {
                            GameObject timbo = Instantiate(Resources.Load("prefabs/gridbase"), Grid.transform) as GameObject;
                            Destroy(timbo.GetComponent<SpriteRenderer>());
                            timbo.name = "timbo" + timboBean.timboVertical.ToString() + timboBean.timboHorizontal.ToString();
                            timbo.AddComponent<Image>();
                            timbo.GetComponent<Image>().sprite = allSprites[21];
                            timbo.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize * 1.1f, gridSize * 1.1f);
                            timbo.GetComponent<RectTransform>().position = new Vector3(x - gridSize * 0.05f, y, 0);
                            timboBean.timbo = timbo;
                            gridBaseList[horizontal - 1].spriteIndex = 21;
                            break;
                        }
                    }
                }

                if (gridBean.spriteIndex > 1)
                    grid.GetComponent<Image>().sprite = allSprites[gridBean.spriteIndex];
                grid.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                gridBean.gridObject = grid;
                gridBean.moveHorizontal = 9;

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
        if (frostingList != null)
            GridUIAttributeManager.getInstance().frostingList = frostingList;
        if(poisonList!=null)
            GridUIAttributeManager.getInstance().poisonList = poisonList;

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

    public void initFinishPlayLevel()
    {
        great = new GameObject();
        GridUIAttributeManager.getInstance().great = great;
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
}
