using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridUI : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[6];
    public Sprite spriteGridBg;
    public GameObject mainCanvas;
    public Font songTi;

    private GameObject GridBg;//格子背景的父对象
    private GameObject Grid;//元素的父对象

    private float x;//生成元素RectTransform的posX
    private float y;//生成元素RectTransform的posY

    private GameData gameData;//json数据对象
    private string jsonPath;//json数据获取的地址
    private MyWindowData myWindowData;//配置文件的内容
    private float intervalPx;//元素的间隔
    private float interval; //两个元素中心点的距离，即元素本身的Size + intervalPx
    private float gridSize;//元素的width和Height
    private float leaveSize; //屏幕左右两边共预留的像素

    private List<List<GridBean>> gridListManager = new List<List<GridBean>>();//管理所有列的List
    private List<GridBean> gridList;//一列元素的List
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

    private int dropGridCounts;//元素下移格子数
    private float dropHeight;//掉落高度
    private int moveObjectIndex;//掉落元素索引
    private float gameBgWith;//背景图宽度
    private float gameBgHeight;//背景图高度

    private GameObject targetBoard;//目标板对象
    private GameObject targetCount;//目标类型数量
    private int deleteCounts;//已消除的数量
    private GameObject great;//完成目标后显示的对象
    private GameObject targetGrid;//目标类型

    // Use this for initialization
    void Start()
    {
        //初始化游戏场景
        initGameBg();

        //初始化元素
        initUI();

        //初始化关卡完成界面
        initFinishPlayLevel();
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
        {
            great.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2, gameBgHeight / 2, 0.0f);
        }
        else
        {
            great.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gameBgHeight / 2, 0.0f);
        }
        great.SetActive(false);
    }

    private void initGameBg()
    {
        //[1]创建UI对象
        GameObject gameBackground = new GameObject();
        GameObject branch = new GameObject();
        targetBoard = new GameObject();

        //[1]给UI对象命名
        gameBackground.name = "gameBackground";
        branch.name = "branch";
        targetBoard.name = "targetBoard";

        //[2]添加对象组件
        gameBackground.AddComponent<Image>();
        branch.AddComponent<Image>();
        targetBoard.AddComponent<Image>();

        //[3]设置对象父对象
        gameBackground.GetComponent<RectTransform>().SetParent(mainCanvas.transform);
        branch.GetComponent<RectTransform>().SetParent(gameBackground.transform);
        targetBoard.GetComponent<RectTransform>().SetParent(gameBackground.transform);

        //[4]加载UI所需Sprite
        Sprite sprite_gameBackground = new Sprite();
        Sprite sprite_branch = new Sprite();
        Sprite sprite_targetBoard = new Sprite();

        sprite_gameBackground = Resources.Load("game_background", sprite_gameBackground.GetType()) as Sprite;
        sprite_branch = Resources.Load("branch", sprite_branch.GetType()) as Sprite;
        sprite_targetBoard = Resources.Load("target_board", sprite_targetBoard.GetType()) as Sprite;
  
        gameBackground.GetComponent<Image>().sprite = sprite_gameBackground;
        branch.GetComponent<Image>().sprite = sprite_branch;
        targetBoard.GetComponent<Image>().sprite = sprite_targetBoard;

        //[5]设置对象position和大小
        Debug.Log("Screen.width:" + Screen.width);
        Debug.Log("Screen.height:" + Screen.height);
        //gameBackground.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        //gameBackground.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        gameBackground.GetComponent<Image>().SetNativeSize();
        gameBgWith = gameBackground.GetComponent<RectTransform>().rect.width;
        gameBgHeight = gameBackground.GetComponent<RectTransform>().rect.height;
        Debug.Log("gameBgWith:" + gameBgWith);
        Debug.Log("gameBgHeight:" + gameBgHeight);
        //如果屏幕高宽比例大于背景高宽比，则使用屏幕宽度作为背景宽度，则反之
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
        {
            gameBgWith = Screen.width;
            gameBgHeight = Screen.width * gameBgHeight / gameBgWith;
            gameBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith, gameBgHeight);
            gameBackground.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gameBgHeight / 2, 0.0f);
        }
        else
        {
            gameBgWith = Screen.height / (gameBgHeight / gameBgWith);
            gameBgHeight = Screen.height;
            gameBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith, gameBgHeight);
            gameBackground.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, Screen.height / 2, 0.0f);
        }
        Debug.Log("gameBackground.position:" + gameBackground.GetComponent<RectTransform>().position);
        
        branch.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.75f, gameBgHeight * 0.2f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
        {
            branch.GetComponent<RectTransform>().position = new Vector3(gameBgWith * 0.75f / 2, gameBgHeight - gameBgHeight * 0.20f / 2, 0.0f);
        }
        else
        {
            branch.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 - gameBgWith * (0.5f - 0.75f / 2), gameBgHeight - gameBgHeight * 0.20f / 2, 0.0f);
        }
       
        targetBoard.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.15f, gameBgHeight * 0.1f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
        {
            targetBoard.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2 , gameBgHeight - gameBgHeight * 0.1f / 2, 0.0f);
        }
        else
        {
            targetBoard.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 , gameBgHeight - gameBgHeight * 0.1f / 2, 0.0f);
        }
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

        //[1]读取配置，设置关卡目标类型和数量
        //jsonPath = Application.dataPath;
        //myWindowData = JsonUtil.getMyWindowDataFromJson(jsonPath, "myWindowData.json");
        myWindowData = new MyWindowData();
        myWindowData.playLevel = 1;
        myWindowData.targetType = UnityEngine.Random.Range(0, 6);
        myWindowData.targetCount = UnityEngine.Random.Range(15, 30);
        targetGrid = Instantiate(Resources.Load("prefabs/grid"), targetBoard.transform) as GameObject;
        targetGrid.name = "targetGrid";
        Destroy(targetGrid.GetComponent<SpriteRenderer>());
        targetGrid.AddComponent<Image>();
        targetGrid.GetComponent<Image>().sprite = sprites[myWindowData.targetType];
        targetGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.1f * 0.7f, gameBgWith * 0.1f * 0.7f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
        {
            targetGrid.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2 - gameBgWith * 0.1f * 0.7f * 1 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgHeight * 0.1f * 2/3, 0.0f);
        }
        else
        {
            targetGrid.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 - gameBgWith * 0.1f * 0.7f * 1 / 3, gameBgHeight - gameBgHeight * 0.1f + gameBgWith * 0.1f * 2 / 3, 0.0f);
        }

        targetCount = new GameObject();
        targetCount.name = "targetCount";
        targetCount.AddComponent<Text>();
        targetCount.GetComponent<Text>().text = "x" + myWindowData.targetCount;
        targetCount.GetComponent<Text>().fontSize = (int)(gameBgWith * 0.1f * 0.7f / 2);
        targetCount.GetComponent<Text>().fontStyle = FontStyle.Bold;
        targetCount.GetComponent<Text>().color = Color.yellow;
        targetCount.GetComponent<Text>().font = songTi;
        targetCount.transform.SetParent(targetBoard.transform);
        targetCount.GetComponent<RectTransform>().sizeDelta = new Vector2(gameBgWith * 0.1f * 0.7f, gameBgWith * 0.1f * 0.7f);
        if (Screen.height / Screen.width >= gameBgHeight / gameBgWith)
        {
            targetCount.GetComponent<RectTransform>().position = new Vector3(gameBgWith / 2 + gameBgWith * 0.1f * 0.7f * 1 * 2 / 3, gameBgHeight - gameBgHeight * 0.1f * 2/3, 0.0f);
        }
        else
        {
            targetCount.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2 + gameBgWith * 0.1f * 0.7f * 1 * 2 / 3, gameBgHeight - gameBgHeight * 0.1f * 2 / 3 , 0.0f);
        }

        //[2]设置格子大小 
        gameData = new GameData();
        gameData.horizontal = 9;
        gameData.vertical = 9;
        leaveSize = 0;
        Debug.Log("leaveSize:"+ leaveSize);
        intervalPx = 1.0f;
        if (Screen.height >= Screen.width)
        {
            if (gameData.vertical > 9)
            {
                gridSize = (Screen.width - leaveSize - (gameData.vertical - 1) * intervalPx) / gameData.vertical;
            }
            else
            {
                gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;
            }
        }
        else
        {
            if (gameData.horizontal > 9)
            {
                gridSize = (gameBgWith - leaveSize - (gameData.horizontal - 1) * intervalPx) / gameData.horizontal;
            }
            else
            {
                gridSize = (gameBgWith - leaveSize - (9 - 1) * intervalPx) / 9;
            }
        }

        interval = gridSize + intervalPx;

        //[3]动态绘制元素
        //[3.1]设置每一列元素的初始位置 x 
        if (Screen.height >= Screen.width)
        {
            x = leaveSize / 2 + gridSize / 2;
        }
        else
        {
            x = Screen.width / 2 - gameBgWith / 2 + leaveSize / 2 + gridSize / 2;
        }

        for (int vertical = 0; vertical < gameData.vertical; vertical++, x = x + interval)
        {
            //[3.2]设置第一行元素的初始位置 y
            y = Screen.height * 0.75f - gridSize / 2 + interval;
            gridList = new List<GridBean>();
            for (int horizontal = 0; horizontal < gameData.horizontal + 1; horizontal++, y = y - interval)
            {
                //[3.3]生成元素背景
                if (horizontal != 0)
                {
                    GameObject gridbase = Instantiate(Resources.Load("prefabs/gridbase"), GridBg.transform) as GameObject;
                    Destroy(gridbase.GetComponent<SpriteRenderer>());
                    gridbase.name = "gridbase" + vertical.ToString() + (horizontal-1).ToString();
                    gridbase.AddComponent<Image>();
                    gridbase.GetComponent<Image>().sprite = spriteGridBg;
                    gridbase.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                    gridbase.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                }

                //[3.4]生成元素
                GameObject grid = Instantiate(Resources.Load("prefabs/grid"), Grid.transform) as GameObject;
                Destroy(grid.GetComponent<SpriteRenderer>());
                grid.AddComponent<Image>();
                GridBean gridBean = new GridBean();
                gridBean.spritesIndex = UnityEngine.Random.Range(0, 6);
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
                    gridDropList.Add(gridBean);
                }

                //[3.6]储存格式显示信息
                if (horizontal != 0)
                {
                    gridBean.listHorizontal = horizontal - 1;
                    gridBean.listVertical = vertical;
                    grid.name = "grid" + gridBean.listVertical.ToString() + gridBean.listHorizontal.ToString();
                    gridList.Add(gridBean);
                }
            }

            gridListManager.Add(gridList);
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
            {
                x = leaveSize / 2;
            }
            else
            {
                x = Screen.width / 2 - gameBgWith / 2 + leaveSize / 2;
            }
            intervalPx = 1.0f;

            //[3]鼠标点中格子区域才会响应，记录初次点中的元素信息
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                startHorizontal = (int)((y - Input.mousePosition.y) / (gridSize + intervalPx));
                startVertical = (int)((Input.mousePosition.x - x) / (gridSize + intervalPx));
                Debug.Log("startVertical:" + startVertical);
                Debug.Log("startHorizontal:" + startHorizontal);
                startPointX = gridListManager[startVertical][startHorizontal].gridObject.GetComponent<RectTransform>().position.x;
                startPointY = gridListManager[startVertical][startHorizontal].gridObject.GetComponent<RectTransform>().position.y;
                startPoint = new Vector3(startPointX, startPointY, 0.0f);
                lineConnectGridList.Add(gridListManager[startVertical][startHorizontal]);
            }
        }

        if (Input.GetMouseButton(0))
        {
            //[1]鼠标点中格子区域才会响应，记录划动经过的元素信息
            if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * gameData.vertical + intervalPx * (gameData.vertical - 1)) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((gameData.horizontal * gridSize + intervalPx * (gameData.horizontal - 1)))))
            {
                nextHorizontal = (int)((y - Input.mousePosition.y) / (gridSize + intervalPx));
                nextVertical = (int)((Input.mousePosition.x - x) / (gridSize + intervalPx));

                //[2]判断鼠标划动经过的对象是否已在数组中，如果已经存在，则不响应
                if (!lineConnectGridList.Contains(gridListManager[nextVertical][nextHorizontal]))
                {
                    if ((nextHorizontal != startHorizontal || nextVertical != startVertical) && gridListManager[nextVertical][nextHorizontal].spritesIndex == gridListManager[startVertical][startHorizontal].spritesIndex && System.Math.Abs(nextHorizontal - startHorizontal) <= 1 && System.Math.Abs(nextVertical - startVertical) <= 1)
                    {
                        nextPointX = gridListManager[nextVertical][nextHorizontal].gridObject.GetComponent<RectTransform>().position.x;
                        nextPointY = gridListManager[nextVertical][nextHorizontal].gridObject.GetComponent<RectTransform>().position.y;
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
                        lineConnectGridList.Add(gridListManager[nextVertical][nextHorizontal]);
                        lineObjList.Add(line);
                        drawCounts++;
                        startHorizontal = nextHorizontal;
                        startVertical = nextVertical;
                        startPoint = nextPoint;
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
                        foreach (GridBean grid in gridListManager[currenttVertical])
                        {
                            if (grid.listHorizontal == gridBean.listHorizontal)
                            {
                                //[2]计算是否消除了目标类型
                                if(grid.spritesIndex == myWindowData.targetType)
                                {
                                    deleteCounts++;
                                }
                                gridListManager[gridBean.listVertical].Remove(grid);
                                break;
                            }
                        }
                        //[3]消除元素
                        Destroy(gridBean.gridObject);
                    }
                    gridDrop();

                    //[4]刷新目标数量
                    if (deleteCounts >= myWindowData.targetCount)
                    {
                        //[4.1]完成任务
                        great.SetActive(true);
                        myWindowData.targetCount = UnityEngine.Random.Range(15, 30);
                        targetCount.GetComponent<Text>().text = myWindowData.targetCount.ToString();
                        deleteCounts = 0;
                        myWindowData.targetType = UnityEngine.Random.Range(0, 6);
                        targetGrid.GetComponent<Image>().sprite = sprites[myWindowData.targetType];
                    }
                    else
                    {
                        //[4.2]仍未完成目标
                        targetCount.GetComponent<Text>().text = "x" + (myWindowData.targetCount- deleteCounts);
                    }
                }
            }

            //[5]移除lineConnectGridList的内容
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
    }

    //格子补充函数，实现掉落效果
    private void gridDrop()
    {
        //遍历所有列，若数量不等于9，则相应列进行掉落
        for (int i = 0; i < gameData.vertical; i++)
        {
            if (gridListManager[i].Count < 9)
            {
                //遍历数组，从后往前检测是否需要移动补充空位 , 检测位置
                for (int x = gridListManager[i].Count - 1, checkIndex = gameData.horizontal - 1; x >= 0; x--, checkIndex--)
                {
                    //判断剩余List的最后一个对象是否处于最大的位置
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
                        //修改GridBean下移后的listHorizontal信息
                        gridListManager[i][x].listHorizontal += dropGridCounts;
                        //修改GridBean下以后的GameObject的位置信息
                        gridListManager[i][x].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropHeight, 0.0f);
                    }

                    //补充元素
                    if (x == 0)
                    {
                        for (int y = checkIndex; y > 0; y--)
                        {
                            //获取下落位置的元素信息，以作为补充使用
                            Vector3 newDropPosition = gridDropList[i].gridObject.GetComponent<RectTransform>().position;
                            int newDropHorizontal = gridDropList[i].listHorizontal;

                            //下移的距离
                            dropHeight = y * interval;

                            //将掉落元素下移一位，修改相关信息
                            gridDropList[i].gridObject.GetComponent<RectTransform>().position += new Vector3(0.0f, -dropHeight, 0.0f);
                            gridDropList[i].listHorizontal += (y - 1);
                            gridDropList[i].gridObject.name = "grid" + i.ToString() + gridDropList[i].listHorizontal.ToString();
                            gridDropList[i].gridObject.SetActive(true);

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
                            gridDropList.Insert(i, gridBean);
                        }
                    }
                }
            }
        }
    }
}
