using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneEditor : MonoBehaviour
{
    //游戏场景中的UI
    public GameObject sceneEditor; //编辑器父对象
    public GameObject openEditorButton;//打开编辑器的按钮
    public GameObject gridTypeBackground;//格子元素选择列表父对象
    public GameObject GridContentSet;//格子内容父对象
    public GameObject Arrows;//箭头对象
    public GameObject inputPlayLevel;//关卡输入框
    public GameObject inputTargetTypeCounts;//目标类型消除数量
    public GameObject targetTypeChoose;//目标类型选择对象
    public GameObject inputStepCounts;//关卡可用步数

    //所需接收GridUI数据的变量
    private static EditorData mEditorData;
    private static List<Sprite> mSprites;//元素资源列表数组
    private static GameObject mGameBackground;
    private static GameObject mGridBg;
    private static GameObject mGrid;
    private static List<List<GridBean>> mGridListManager;//游戏场景中的格子管理者
    private static List<List<GridBaseBean>> mGridBaseListManager;//游戏场景中的格子背景管理者
    private static List<DoorBean> mDoorDataList;

    private float gridTypeBackgroundHeight;//元素背景的中心店高度
    private float gridTypeChoosePositionY;//元素选择列表各类型的positionY值

    private float leaveSize = 0.0f;//屏幕宽度留白空间
    private float intervalPx = 1.0f;//相邻格子间隙
    private float gridSize;//格子类型选择的大小
    private float interval;//相邻格子中点坐标的间隔
    private float x; //作为边界或者元素坐标的x坐标变量
    private float y;//作为边界或者元素坐标的y坐标变量
    private int currentIndex;//当前元素列表的索引
    private int startHorizontal; //当前鼠标点击格子的行数
    private int startVertical;//当前鼠标点击格子的列数

    private List<List<GridBean>> gridOfEditorManager; //编辑器格子管理者
    private List<DoorBean> doorOfEditorManager;//编辑器游戏场景元素管理者
    private List<GridBean> gridList;
    private int targetTypeIndex;
    private string gridDataToJson = null;
    private static List<GridBean> mGridDataList;
    private bool isInit = true;

    private Vector3 doorPoint;
    private bool isCreateOne;
    private string doorDataToJson;

    /// <summary>
    /// 接收GridUI的对象和数据
    /// </summary>
    public static void initSceneEditor(EditorData editorData, List<Sprite> sprites, List<List<GridBean>> gridListManager, List<List<GridBaseBean>> gridBaseListManager, GameObject gameBackground, GameObject grid, GameObject gridBg, List<GridBean> gridDataList, List<DoorBean> doorDataList)
    {
        mEditorData = editorData;
        mSprites = sprites;
        mGridListManager = gridListManager;
        mGridBaseListManager = gridBaseListManager;
        mGameBackground = gameBackground;
        mGridBg = gridBg;
        mGrid = grid;
        mGridDataList = gridDataList;
        mDoorDataList = doorDataList;
    }

    // Use this for initialization
    void Start()
    {
        //隐藏编辑器界面，显示编辑器按钮
        sceneEditor.SetActive(false);
        openEditorButton.transform.SetAsLastSibling();

        //初始化格子类型选择内容
        initGridTypeChooseContent();
    }

    //初始化格子类型选择列表
    private void initGridTypeChooseContent()
    {
        //生成可选择列表
        Sprite random = new Sprite();
        Sprite visable = new Sprite();
        Sprite indoor = new Sprite();
        Sprite outdoor = new Sprite();
        random = Resources.Load<Sprite>("random") as Sprite;
        visable = Resources.Load<Sprite>("visable") as Sprite;
        indoor = Resources.Load<Sprite>("door") as Sprite;
        outdoor = Resources.Load<Sprite>("door") as Sprite;

        mSprites.Add(visable);
        mSprites.Add(random);
        mSprites.Add(indoor);
        mSprites.Add(outdoor);

        gridSize = Screen.width / 10;
        x = Arrows.GetComponent<RectTransform>().position.x;
        y = Arrows.GetComponent<RectTransform>().position.y + 60;
        Arrows.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize * 0.7f, gridSize * 0.7f);
        for (int i = 0; i < mSprites.Count; i++)
        {
            GameObject grid = Instantiate(Resources.Load("prefabs/grid"), gridTypeBackground.transform) as GameObject;
            Destroy(grid.GetComponent<SpriteRenderer>());
            grid.AddComponent<Image>();
            grid.GetComponent<Image>().sprite = mSprites[i];
            grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
            grid.AddComponent<Button>();
            grid.GetComponent<Button>().onClick.AddListener(onGridTypeClick);
            grid.GetComponent<RectTransform>().position = new Vector3(gridSize / 2 + gridSize * i, y, 0);

            //如果是传送门出入口，则需要进行翻转
            if (i == 8 || i == 9)
                grid.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
            //传送门入口
            if (i == 8)
                grid.GetComponent<RectTransform>().position = new Vector3(gridSize / 2 + gridSize * i, y - gridSize / 2, 0);
            //传送门出口
            if (i == 9)
                grid.GetComponent<RectTransform>().position = new Vector3(gridSize / 2 + gridSize * i, y + gridSize / 2, 0);

            GridBean gridBean = new GridBean();
            gridBean.gridObject = grid;
        }
    }

    //初始化格子内容
    private void initGridContentSet()
    {
        gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;
        interval = gridSize + intervalPx;
        x = leaveSize / 2 + gridSize / 2;

        //设置编辑器格子内容
        gridOfEditorManager = new List<List<GridBean>>();
        for (int vertical = 0; vertical < 9; vertical++, x = x + interval)
        {
            y = Screen.height * 0.75f - gridSize / 2;
            gridList = new List<GridBean>();
            for (int horizontal = 0; horizontal < 9; horizontal++, y = y - interval)
            {
                GameObject grid = Instantiate(Resources.Load("prefabs/grid"), GridContentSet.transform) as GameObject;
                Destroy(grid.GetComponent<SpriteRenderer>());
                grid.AddComponent<Image>();
                GridBean gridBean = new GridBean();
                if (mGridDataList != null)
                {
                    foreach (GridBean gridData in mGridDataList)
                    {
                        if (vertical == gridData.listVertical && horizontal == gridData.listHorizontal)
                        {
                            if (gridData.spritesIndex == -1)
                                gridBean.spritesIndex = 6;
                            else
                                gridBean.spritesIndex = gridData.spritesIndex;
                            break;
                        }
                        else
                        {
                            gridBean.spritesIndex = 7;
                        }
                    }
                }
                else
                {
                    gridBean.spritesIndex = 7;
                }

                grid.GetComponent<Image>().sprite = mSprites[gridBean.spritesIndex];
                grid.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                grid.AddComponent<Button>();
                grid.GetComponent<Button>().onClick.AddListener(onGridClick);
                gridBean.gridObject = grid;
                gridList.Add(gridBean);
            }
            gridOfEditorManager.Add(gridList);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //点击元素选择类型其他区域，则隐藏选择
        if (gridTypeBackground != null)
        {
            if (Input.GetMouseButtonDown(0) && (Input.mousePosition.y > gridTypeChoosePositionY + gridTypeBackgroundHeight / 2 || Input.mousePosition.y < gridTypeChoosePositionY - gridTypeBackgroundHeight / 2))
                gridTypeBackground.SetActive(false);
        }
    }

    //打开编辑器按钮点击事件
    public void onOpenEditorClick()
    {
        mGameBackground.SetActive(false);
        mGrid.SetActive(false);
        mGridBg.SetActive(false);
        sceneEditor.SetActive(true);
        gridTypeBackground.SetActive(false);

        if (isInit)
        {
            //读取配置内容，根据当前关卡显示编辑器设置
            initData();

            //初始化格子内容
            initGridContentSet();

            isInit = false;
        }

        //初始化游戏场景元素
        initGameBg();
    }

    private void initGameBg()
    {
        //设置编辑器游戏场景(传送门、树藤等)
        if (mDoorDataList != null)
        {
            doorOfEditorManager = mDoorDataList;
            foreach (DoorBean doorBean in doorOfEditorManager)
            {
                Destroy(doorBean.indoor);
                Destroy(doorBean.outdoor);
            }
            for (int i = 0; i < doorOfEditorManager.Count; i++)
            {
                //入口
                GameObject indoor = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(indoor.GetComponent<SpriteRenderer>());
                indoor.name = "indoor" + doorOfEditorManager[i].inVertical.ToString() + doorOfEditorManager[i].inHorizontal.ToString();
                indoor.AddComponent<Image>();
                indoor.GetComponent<Image>().sprite = mSprites[8];
                doorPoint = gridOfEditorManager[doorOfEditorManager[i].inVertical][doorOfEditorManager[i].inHorizontal].gridObject.GetComponent<RectTransform>().position;
                indoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, -gridSize * 2 / 3, 0.0f);
                indoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                indoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorOfEditorManager[i].indoor = indoor;

                //出口
                GameObject outdoor = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(outdoor.GetComponent<SpriteRenderer>());
                outdoor.name = "outdoor" + doorOfEditorManager[i].outVertical.ToString() + doorOfEditorManager[i].outHorizontal.ToString();
                outdoor.AddComponent<Image>();
                outdoor.GetComponent<Image>().sprite = mSprites[9];
                doorPoint = gridOfEditorManager[doorOfEditorManager[i].outVertical][doorOfEditorManager[i].outHorizontal].gridObject.GetComponent<RectTransform>().position;
                outdoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, gridSize * 2 / 3, 0.0f);
                outdoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                outdoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorOfEditorManager[i].outdoor = outdoor;
            }
        }
    }

    private void initData()
    {
        //获取配置关卡，显示类型等信息
        inputPlayLevel.GetComponent<InputField>().text = mEditorData.playLevel.ToString();
        targetTypeChoose.GetComponent<Image>().sprite = mSprites[mEditorData.targetType];
        inputStepCounts.GetComponent<InputField>().text = mEditorData.stepCounts.ToString();
        inputTargetTypeCounts.GetComponent<InputField>().text = mEditorData.targetCounts.ToString();
        targetTypeIndex = mEditorData.targetType;
    }

    //确定按钮点击事件
    public void onCommitButtonClick()
    {
        //隐藏编辑器和显示游戏内容
        mGameBackground.SetActive(true);
        mGrid.SetActive(true);
        mGridBg.SetActive(true);
        sceneEditor.SetActive(false);

        //将编辑器数据传递给游戏场景显示
        //获取管理游戏元素的GridListManager
        for (int vertical = 0; vertical < 9; vertical++)
        {
            for (int horizontal = 0; horizontal < 9; horizontal++)
            {
                //产生随机元素
                if (gridOfEditorManager[vertical][horizontal].spritesIndex == 7)
                {
                    mGridListManager[vertical][horizontal].gridObject.SetActive(true);
                    mGridBaseListManager[vertical][horizontal].gridBase.SetActive(true);
                    mGridListManager[vertical][horizontal].spritesIndex = UnityEngine.Random.Range(0, 6);
                    mGridListManager[vertical][horizontal].gridObject.GetComponent<Image>().sprite = mSprites[mGridListManager[vertical][horizontal].spritesIndex];
                    mGridBaseListManager[vertical][horizontal].isHasGrid = true;
                    mGridBaseListManager[vertical][horizontal].spriteIndex = mGridListManager[vertical][horizontal].spritesIndex;
                }

                //不显示元素，挖空格子
                if (gridOfEditorManager[vertical][horizontal].spritesIndex == 6)
                {
                    mGridListManager[vertical][horizontal].gridObject.SetActive(false);
                    mGridBaseListManager[vertical][horizontal].gridBase.SetActive(false);
                    mGridBaseListManager[vertical][horizontal].isHasGrid = true;
                    mGridBaseListManager[vertical][horizontal].spriteIndex = -1;
                }

                //固定元素
                if (gridOfEditorManager[vertical][horizontal].spritesIndex < 6)
                {
                    mGridListManager[vertical][horizontal].gridObject.SetActive(true);
                    mGridBaseListManager[vertical][horizontal].gridBase.SetActive(true);
                    mGridListManager[vertical][horizontal].spritesIndex = gridOfEditorManager[vertical][horizontal].spritesIndex;
                    mGridListManager[vertical][horizontal].gridObject.GetComponent<Image>().sprite = mSprites[mGridListManager[vertical][horizontal].spritesIndex];
                    mGridBaseListManager[vertical][horizontal].isHasGrid = true;
                    mGridBaseListManager[vertical][horizontal].spriteIndex = mGridListManager[vertical][horizontal].spritesIndex;
                }
            }
        }

        //更新消除类型和数量
        if (inputPlayLevel.GetComponent<InputField>().text != null)
            mEditorData.playLevel = int.Parse(inputPlayLevel.GetComponent<InputField>().text);
        if (inputTargetTypeCounts.GetComponent<InputField>().text != null)
            mEditorData.targetCounts = int.Parse(inputTargetTypeCounts.GetComponent<InputField>().text);
        if (inputStepCounts.GetComponent<InputField>().text != null)
            mEditorData.stepCounts = int.Parse(inputStepCounts.GetComponent<InputField>().text);

        mEditorData.targetTypeObj.GetComponent<Image>().sprite = mSprites[targetTypeIndex];
        mEditorData.targetType = targetTypeIndex;
        mEditorData.targetCountCountObj.GetComponent<Text>().text = "x" + mEditorData.targetCounts;
        mEditorData.stepCountsObj.GetComponent<Text>().text = mEditorData.stepCounts.ToString();

        //更新游戏场景内容
        if (doorOfEditorManager != null)
        {
            mDoorDataList = doorOfEditorManager;
            foreach (DoorBean doorBean in mDoorDataList)
            {
                Destroy(doorBean.indoor);
                Destroy(doorBean.outdoor);
            }
            foreach (DoorBean doorbean in mDoorDataList)
            {
                //入口
                GameObject indoor = Instantiate(Resources.Load("prefabs/gridbase"), mGridBg.transform) as GameObject;
                Destroy(indoor.GetComponent<SpriteRenderer>());
                indoor.name = "indoor" + doorbean.inVertical.ToString() + doorbean.inHorizontal.ToString();
                indoor.AddComponent<Image>();
                indoor.GetComponent<Image>().sprite = mSprites[8];
                doorPoint = mGridListManager[doorbean.inVertical][doorbean.inHorizontal].gridObject.GetComponent<RectTransform>().position;
                indoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, -gridSize * 2 / 3, 0.0f);
                indoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                indoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorbean.indoor = indoor;

                //出口
                GameObject outdoor = Instantiate(Resources.Load("prefabs/gridbase"), mGridBg.transform) as GameObject;
                Destroy(outdoor.GetComponent<SpriteRenderer>());
                outdoor.name = "outdoor" + doorbean.outVertical.ToString() + doorbean.outHorizontal.ToString();
                outdoor.AddComponent<Image>();
                outdoor.GetComponent<Image>().sprite = mSprites[9];
                doorPoint = mGridListManager[doorbean.outVertical][doorbean.outHorizontal].gridObject.GetComponent<RectTransform>().position;
                outdoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, gridSize * 2 / 3, 0.0f);
                outdoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                outdoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorbean.outdoor = outdoor;
            }
        }
    }

    //类型选择元素点击事件
    public void onGridTypeClick()
    {
        //用于计算选择了哪个类型
        gridSize = Screen.width / 10;
        currentIndex = (int)(Input.mousePosition.x / gridSize);

        //格子内容大小，用于计算坐标偏移
        gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;

        //若选择了传送门出入口，则对应生成一个UI用于显示
        if (currentIndex == 8 && startVertical >= 0 && startHorizontal >= 0)
        {
            isCreateOne = true;
            if (doorOfEditorManager != null && doorOfEditorManager.Count > 0)
            {
                //遍历List，如果传送门入口不存在则新创建，若存在，则消除
                for (int i = 0; i < doorOfEditorManager.Count; i++)
                {
                    //删除传送门入口
                    if (doorOfEditorManager[i].inVertical == startVertical && doorOfEditorManager[i].inHorizontal == startHorizontal)
                    {
                        Destroy(doorOfEditorManager[i].indoor);
                        Destroy(doorOfEditorManager[i].outdoor);
                        doorOfEditorManager.RemoveAt(i);
                        isCreateOne = false;
                        break;
                    }
                }
            }

            //生成传送门入口
            if (isCreateOne)
            {
                GameObject indoor = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(indoor.GetComponent<SpriteRenderer>());
                indoor.name = "indoor" + startVertical.ToString() + startHorizontal.ToString();
                indoor.AddComponent<Image>();
                indoor.GetComponent<Image>().sprite = mSprites[currentIndex];
                doorPoint = mGridBaseListManager[startVertical][startHorizontal].gridBase.GetComponent<RectTransform>().position;
                indoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, -gridSize * 2 / 3, 0.0f);
                indoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                indoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));

                DoorBean doorBean = new DoorBean();
                doorBean.inVertical = startVertical;
                doorBean.inHorizontal = startHorizontal;
                doorBean.indoor = indoor;
                if (doorOfEditorManager == null)
                    doorOfEditorManager = new List<DoorBean>();
                doorOfEditorManager.Add(doorBean);
            }

        }

        //传送门出口坐标
        if (currentIndex == 9 && startVertical >= 0 && startHorizontal >= 0 && doorOfEditorManager != null)
        {
            isCreateOne = true;
            if (doorOfEditorManager.Count > 0)
            {
                //遍历List，如果传送门入口不存在则新创建，若存在，则消除
                for (int i = 0; i < doorOfEditorManager.Count; i++)
                {
                    //删除传送门入口
                    if (doorOfEditorManager[i].outVertical == startVertical && doorOfEditorManager[i].outHorizontal == startHorizontal)
                    {
                        Destroy(doorOfEditorManager[i].outdoor);
                        doorOfEditorManager[i].outVertical = -1;
                        isCreateOne = false;
                        break;
                    }
                }
            }

            //生成传送门出口
            if (isCreateOne)
            {
                GameObject outdoor = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(outdoor.GetComponent<SpriteRenderer>());
                outdoor.name = "outdoor" + startVertical.ToString() + startHorizontal.ToString();
                outdoor.AddComponent<Image>();
                outdoor.GetComponent<Image>().sprite = mSprites[currentIndex];
                doorPoint = mGridBaseListManager[startVertical][startHorizontal].gridBase.GetComponent<RectTransform>().position;
                outdoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, gridSize * 2 / 3, 0.0f);
                outdoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                outdoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));

                //遍历List，查找出口列为-1的索引，并设置
                for (int i = 0; i < doorOfEditorManager.Count; i++)
                {
                    if (doorOfEditorManager[i].outVertical == -1)
                    {
                        doorOfEditorManager[i].outVertical = startVertical;
                        doorOfEditorManager[i].outHorizontal = startHorizontal;
                        doorOfEditorManager[i].outdoor = outdoor;
                        break;
                    }
                }
            }
        }

        //设置格子内容
        if (currentIndex <= 7 && startVertical >= 0 && startHorizontal >= 0)
        {
            //计算当前鼠标点击的是哪个类型
            gridOfEditorManager[startVertical][startHorizontal].gridObject.GetComponent<Image>().sprite = mSprites[currentIndex];
            gridOfEditorManager[startVertical][startHorizontal].spritesIndex = currentIndex;
        }

        //设置消除类型
        if (currentIndex < 6 && startVertical < 0 && startHorizontal < 0)
        {
            //编辑器更新目标类型
            targetTypeChoose.GetComponent<Image>().sprite = mSprites[currentIndex];
        }

        //隐藏元素类型选择列表
        gridTypeBackground.SetActive(false);
    }

    //格子点击事件
    public void onGridClick()
    {
        //显示元素选择内容列表
        gridTypeBackground.SetActive(true);

        //设置元素选择列表位置
        gridTypeBackgroundHeight = gridTypeBackground.GetComponent<RectTransform>().rect.height;
        gridTypeChoosePositionY = Input.mousePosition.y + (gridTypeBackgroundHeight + 50) / 2;
        gridTypeBackground.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gridTypeChoosePositionY, 0.0f);

        //设置箭头位置
        Arrows.GetComponent<RectTransform>().position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + (gridTypeBackgroundHeight + 50) / 4, 0.0f);

        //[2]计算格子所在边界，x为左边界，y为上边界
        y = Screen.height * 0.75f;
        x = leaveSize / 2;
        gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;

        //[3]鼠标点中格子区域才会响应，记录初次点中的元素信息
        if (Input.mousePosition.x > x && Input.mousePosition.x < (Screen.width - x) && Input.mousePosition.y < y && Input.mousePosition.y > (y - Screen.width + x))
        {
            startHorizontal = (int)((y - Input.mousePosition.y) / (gridSize + intervalPx));
            startVertical = (int)((Input.mousePosition.x - x) / (gridSize + intervalPx));
        }

        //点击了目标类型
        if (Input.mousePosition.y > y)
        {
            startHorizontal = -1;
            startVertical = -1;
        }
    }

    //刷新按钮点击事件
    public void onReSetClick()
    {
        for (int vertical = 0; vertical < 9; vertical++)
        {
            for (int horizontal = 0; horizontal < 9; horizontal++)
            {
                //产生随机元素
                if (gridOfEditorManager[vertical][horizontal].spritesIndex != 0)
                {
                    gridOfEditorManager[vertical][horizontal].spritesIndex = 7;
                    gridOfEditorManager[vertical][horizontal].gridObject.GetComponent<Image>().sprite = mSprites[7];
                }
            }
        }
    }

    //导出配置点击事件
    public void onOutputDataClick()
    {
        //解析gridOfEditorManager数据
        for (int vertical = 0; vertical < 9; vertical++)
        {
            for (int horizontal = 0; horizontal < 9; horizontal++)
            {
                //产生随机元素
                if (gridOfEditorManager[vertical][horizontal].spritesIndex <= 6)
                    gridDataToJson = gridDataToJson + vertical + "|" + horizontal + "|" + (gridOfEditorManager[vertical][horizontal].spritesIndex) + ",";
            }
        }
        mEditorData.gridData = gridDataToJson;

        if (mDoorDataList != null)
        {
            for(int i = 0; i < mDoorDataList.Count; i++)
            {
                doorDataToJson = doorDataToJson + mDoorDataList[i].inVertical + "|" + mDoorDataList[i].inHorizontal + "|" + mDoorDataList[i].outVertical + "|" + mDoorDataList[i].outHorizontal + ",";
            }
        }
        mEditorData.doorData = doorDataToJson;

        //判断同一关卡配置是否存在

        //点击确定再保存数据
        JsonUtil.createPlayLevelJsonData(mEditorData);

        Debug.Log("outputData done!!!!!");
    }
}
