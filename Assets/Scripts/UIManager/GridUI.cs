using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridUI : MonoBehaviour
{
    private static string[] resourcesId = new string[] { "monkey", "panda", "chicken", "penguin", "pig", "rabbit" };
    private static List<Sprite> sprites = new List<Sprite>();
    public Sprite spriteGridBg;
    public Sprite spriteDoor;
    public GameObject mainCanvas;
    public Font songTi;

    private GameObject GridBg;//格子背景的父对象
    private GameObject Grid;//元素的父对象
    private GameObject GameBackground;//背景图父对象

    private float x;//生成元素RectTransform的posX
    private float y;//生成元素RectTransform的posY
    private GameObject stepCounts;
    private GameData gameData;//json数据对象
    private static EditorData editorData;//配置文件的内容
    private float intervalPx;//元素的间隔
    private float interval; //两个元素中心点的距离，即元素本身的Size + intervalPx
    private float gridSize;//元素的width和Height
    private float leaveSize; //屏幕左右两边共预留的像素

    private static List<List<GridBean>> gridListManager = new List<List<GridBean>>();//管理所有列的List
    private List<GridBean> gridList;//一列元素的List
    private static List<List<GridBaseBean>> gridBaseListManager = new List<List<GridBaseBean>>();//管理所有列背景的List
    private List<GridBaseBean> gridBaseList;//一列元素的背景List
    private List<GridBean> gridDropList = new List<GridBean>();//掉落备用元素List

    private List<GameObject> lineObjList = new List<GameObject>();//线段对象管理List
    private List<GridBean> lineConnectGridList = new List<GridBean>();//线段连接的元素管理List
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
    private float startPointX;//线段开始位置的对象的position.x
    private float startPointY;//线段开始位置的对象的position.y
    private float nextPointX;//线段结束位置的对象的position.x
    private float nextPointY;//线段结束位置的对象的position.y

    private float gameBgWith;//背景图宽度
    private float gameBgHeight;//背景图高度

    private GameObject targetBoard;//目标板对象
    private GameObject stepBoard;//步数板对象
    private GameObject targetCount;//目标类型数量
    private int deleteCounts;//已消除的数量
    private GameObject great;//完成目标后显示的对象
    private GameObject targetGrid;//目标类型

    private List<GridBean> gridDataList;
    private int nextListIndex;
    private int startListIndex;
    private List<DoorBean> doorDataList;
    private Vector3 doorPoint;

    // Use this for initialization
    void Start()
    {
        //根据配置显示游戏关卡内容
        initData();

        //初始化游戏场景
        initGameBg();

        //初始化元素
        initUI();

        //初始化关卡完成界面
        initFinishPlayLevel();

        //初始化GridDrop，传送数据
        GridDrop.initGridDrop(gameData, gridListManager, interval, gridDropList, Grid, sprites, gridSize, gridBaseListManager, doorDataList);

        //初始化SceneEditor，传送数据
        SceneEditor.initSceneEditor(editorData, sprites, gridListManager, gridBaseListManager, GameBackground, Grid, GridBg, gridDataList, doorDataList);
    }

    private void initData()
    {
        //[1]获取格子内容信息
        editorData = JsonUtil.getEditorData(1);
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
                    gridBean.spritesIndex = int.Parse(result[2]);
                    gridDataList.Add(gridBean);
                }
            }
        }

        //[2]获取场景元素信息，例如传送门，树藤，障碍物等
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
        }
    }

    private void initFinishPlayLevel()
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

    private void initGameBg()
    {
        //[0]创建UI对象
        GameBackground = new GameObject();
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

    private void initUI()
    {
        //[0]初始化父控件
        GridBg = new GameObject();
        Grid = new GameObject();
        GridBg.AddComponent<RectTransform>();
        Grid.AddComponent<RectTransform>();
        GridBg.name = "GridBg";
        Grid.name = "Grid";
        GridBg.GetComponent<RectTransform>().SetParent(mainCanvas.transform);
        Grid.GetComponent<RectTransform>().SetParent(mainCanvas.transform);

        //加载元素类型资源
        for (int i = 0; i < resourcesId.Length; i++)
        {
            Sprite sprite = new Sprite();
            sprite = Resources.Load<Sprite>(resourcesId[i]) as Sprite;
            sprites.Add(sprite);
        }

        //[1]读取配置，设置关卡目标类型和数量
        //jsonPath = Application.dataPath;
        //myWindowData = JsonUtil.getMyWindowDataFromJson(jsonPath, "myWindowData.json");
        //editorData = new EditorData();
        //editorData.playLevel = 1;
        //editorData.stepCounts = 20;
        //editorData.targetType = UnityEngine.Random.Range(0, 6);
        //editorData.targetCounts = UnityEngine.Random.Range(15, 30);

        //[1.1]设置目标类型
        targetGrid = Instantiate(Resources.Load("prefabs/grid"), targetBoard.transform) as GameObject;
        targetGrid.name = "targetGrid";
        Destroy(targetGrid.GetComponent<SpriteRenderer>());
        targetGrid.AddComponent<Image>();
        targetGrid.GetComponent<Image>().sprite = sprites[editorData.targetType];
        targetGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.1f * 0.7f, gameBgWith * 0.1f * 0.7f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
            targetGrid.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2 - gameBgWith * 0.1f * 0.7f * 1 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgHeight * 0.1f * 2 / 4, 0.0f);
        else
            targetGrid.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 - gameBgWith * 0.1f * 0.7f * 1 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgWith * 0.1f * 2 / 4, 0.0f);

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
        editorData.targetCountCountObj = targetCount;

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

        //[2]设置格子大小 
        gameData = new GameData();
        gameData.horizontal = 9;
        gameData.vertical = 9;
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

        //[3]动态创建元素
        //[3.1]设置每一列元素的初始位置 x 
        if (Screen.height >= Screen.width)
            x = leaveSize / 2 + gridSize / 2;
        else
            x = Screen.width / 2 - gameBgWith / 2 + leaveSize / 2 + gridSize / 2;

        for (int vertical = 0; vertical < gameData.vertical; vertical++, x = x + interval)
        {
            //[3.2]设置第一行元素的初始位置 y
            y = Screen.height * 0.75f - gridSize / 2 + interval;
            gridList = new List<GridBean>();
            gridBaseList = new List<GridBaseBean>();
            for (int horizontal = 0; horizontal < gameData.horizontal + 1; horizontal++, y = y - interval)
            {
                //[3.3]生成元素背景
                if (horizontal != 0)
                {
                    GameObject gridbase = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
                    Destroy(gridbase.GetComponent<SpriteRenderer>());
                    gridbase.name = "gridbase" + vertical.ToString() + (horizontal - 1).ToString();
                    gridbase.AddComponent<Image>();
                    gridbase.GetComponent<Image>().sprite = spriteGridBg;
                    gridbase.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                    gridbase.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                    GridBaseBean gridBaseBean = new GridBaseBean();
                    gridBaseBean.gridBase = gridbase;
                    gridBaseList.Add(gridBaseBean);
                }

                //[3.4]生成元素
                GameObject grid = Instantiate(Resources.Load("prefabs/grid"), Grid.transform) as GameObject;
                Destroy(grid.GetComponent<SpriteRenderer>());
                grid.AddComponent<Image>();
                GridBean gridBean = new GridBean();

                //遍历配置文件，若有对应固定的位置，则根据情况显示
                if (horizontal != 0)
                {
                    //如果配置数据为空，则默认随机
                    if (gridDataList != null)
                    {
                        foreach (GridBean gridData in gridDataList)
                        {
                            if (vertical == gridData.listVertical && (horizontal - 1) == gridData.listHorizontal)
                            {
                                //元素不显示
                                if (gridData.spritesIndex == 6)
                                {
                                    //背景和元素不显示
                                    gridBaseList[horizontal - 1].gridBase.SetActive(false);
                                    gridBaseList[horizontal - 1].isHasGrid = true;
                                    gridBaseList[horizontal - 1].spriteIndex = -1;
                                    grid.SetActive(false);
                                }

                                if (gridData.spritesIndex < 6)
                                {
                                    //根据对应index显示资源
                                    gridBean.spritesIndex = gridData.spritesIndex;
                                    gridBaseList[horizontal - 1].isHasGrid = true;
                                }
                                break;
                            }
                            else
                            {
                                gridBean.spritesIndex = UnityEngine.Random.Range(0, 6);
                                gridBaseList[horizontal - 1].isHasGrid = true;
                            }
                        }
                    }
                    else
                    {
                        gridBean.spritesIndex = UnityEngine.Random.Range(0, 6);
                        gridBaseList[horizontal - 1].isHasGrid = true;
                    }
                }
                else
                {
                    //备用元素的资源
                    gridBean.spritesIndex = UnityEngine.Random.Range(0, 6);
                }

                if (gridBean.spritesIndex < 6)
                    grid.GetComponent<Image>().sprite = sprites[gridBean.spritesIndex];
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

        //[4]生成场景元素，例如传送门、树藤、障碍物等
        foreach (DoorBean doorbean in doorDataList)
        {
            //入口
            GameObject indoor = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
            Destroy(indoor.GetComponent<SpriteRenderer>());
            indoor.name = "indoor" + doorbean.inVertical.ToString() + doorbean.inHorizontal.ToString();
            indoor.AddComponent<Image>();
            indoor.GetComponent<Image>().sprite = spriteDoor;
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
            outdoor.GetComponent<Image>().sprite = spriteDoor;
            doorPoint = gridListManager[doorbean.outVertical][doorbean.outHorizontal].gridObject.GetComponent<RectTransform>().position;
            outdoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, gridSize * 2 / 3, 0.0f);
            outdoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
            outdoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
            doorbean.outdoor = outdoor;
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

            //[3]鼠标点中格子区域才会响应，记录初次点中的元素信息
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                startVertical = (int)((Input.mousePosition.x - x) / (gridSize + intervalPx));
                startHorizontal = startListIndex = (int)((y - Input.mousePosition.y) / (gridSize + intervalPx));

                if (gridBaseListManager[startVertical][startHorizontal].isHasGrid && gridBaseListManager[startVertical][startHorizontal].spriteIndex != -1)
                {
                    //判断startVertical该列上没有元素
                    for (int h = startHorizontal - 1; h >= 0; h--)
                    {
                        if (!gridBaseListManager[startVertical][h].isHasGrid)
                            startListIndex--;
                    }

                    if (gridListManager[startVertical][startListIndex].spritesIndex != -1)
                    {
                        startPointX = gridListManager[startVertical][startListIndex].gridObject.GetComponent<RectTransform>().position.x;
                        startPointY = gridListManager[startVertical][startListIndex].gridObject.GetComponent<RectTransform>().position.y;
                        startPoint = new Vector3(startPointX, startPointY, 0.0f);

                        lineConnectGridList.Add(gridListManager[startVertical][startListIndex]);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            //[1]鼠标点中格子区域才会响应，记录划动经过的元素信息
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                nextVertical = (int)((Input.mousePosition.x - x) / (gridSize + intervalPx));
                nextHorizontal = nextListIndex = (int)((y - Input.mousePosition.y) / (gridSize + intervalPx));

                if (gridBaseListManager[nextVertical][nextHorizontal].isHasGrid && gridBaseListManager[nextVertical][nextHorizontal].spriteIndex != -1)
                {

                    //判断nextVertical该列上没有元素
                    for (int h = nextHorizontal - 1; h >= 0; h--)
                    {
                        if (!gridBaseListManager[nextVertical][h].isHasGrid)
                            nextListIndex--;
                    }

                    //[2]判断鼠标划动经过的对象是否已在数组中，如果已经存在，则不响应
                    if (!lineConnectGridList.Contains(gridListManager[nextVertical][nextListIndex]))
                    {
                        if ((nextHorizontal != startHorizontal || nextVertical != startVertical) && gridListManager[nextVertical][nextListIndex].spritesIndex == gridListManager[startVertical][startListIndex].spritesIndex && System.Math.Abs(nextHorizontal - startHorizontal) <= 1 && System.Math.Abs(nextVertical - startVertical) <= 1)
                        {
                            if (gridListManager[nextVertical][nextListIndex].spritesIndex != -1)
                            {
                                nextPointX = gridListManager[nextVertical][nextListIndex].gridObject.GetComponent<RectTransform>().position.x;
                                nextPointY = gridListManager[nextVertical][nextListIndex].gridObject.GetComponent<RectTransform>().position.y;
                                nextPoint = new Vector3(nextPointX, nextPointY, 0.0f);

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

                                //[3.2]计算线段宽度，为元素的1/8
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
                                //[3.7]线段连接对象和线段数组添加元素，将划动经过的点作为初始点
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
            //[1]在格子区域抬起鼠标，则默认用户希望消除元素，否则则撤销消除
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                if (drawCounts >= 2)
                {
                    foreach (GridBean gridBean in lineConnectGridList)
                    {
                        int currenttVertical = gridBean.listVertical;

                        for (int removeIndex = 0; removeIndex < gridListManager[currenttVertical].Count; removeIndex++)
                        {
                            if (gridListManager[currenttVertical][removeIndex].listHorizontal == gridBean.listHorizontal)
                            {
                                //[2]计算是否消除了目标类型
                                if (gridBean.spritesIndex == editorData.targetType)
                                    deleteCounts++;
                                gridBaseListManager[currenttVertical][gridBean.listHorizontal].isHasGrid = false;
                                gridListManager[currenttVertical].RemoveAt(removeIndex);
                                break;
                            }
                        }

                        //[3]消除元素
                        Destroy(gridBean.gridObject);
                    }

                    //[4]记录元素掉落信息
                    GridDrop.recordGridDropMsg();

                    //[5]刷新步数信息
                    editorData.stepCounts--;
                    if (editorData.stepCounts > 0)
                    {
                        stepCounts.GetComponent<Text>().text = editorData.stepCounts.ToString();
                    }
                    else
                    {
                        //[5.1]已没有步数，提示失败
                    }

                    //[6]刷新目标数量
                    if (deleteCounts >= editorData.targetCounts)
                    {
                        //[6.1]完成任务
                        great.SetActive(true);
                        editorData.targetCounts = UnityEngine.Random.Range(15, 30);
                        targetCount.GetComponent<Text>().text = editorData.targetCounts.ToString();
                        deleteCounts = 0;
                        editorData.targetType = UnityEngine.Random.Range(0, 6);
                        targetGrid.GetComponent<Image>().sprite = sprites[editorData.targetType];
                        editorData.stepCounts = 20;
                        stepCounts.GetComponent<Text>().text = editorData.stepCounts.ToString();
                    }
                    else
                    {
                        //[6.2]仍未完成目标
                        targetCount.GetComponent<Text>().text = "x" + (editorData.targetCounts - deleteCounts);
                    }
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

        //[8]进行元素掉落
        if (gameData != null && gridListManager != null)
            GridDrop.gridDrop();
    }
}
