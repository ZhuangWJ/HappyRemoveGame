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
    private EditorData mEditorData;
    private List<Sprite> mSprites;//元素资源列表数组
    private GameObject mGameBackground;
    private GameObject mGridBg;
    private GameObject mGrid;
    private List<List<GridBean>> mGridListManager;//游戏场景中的格子管理者
    private List<List<GridBaseBean>> mGridBaseListManager;//游戏场景中的格子背景管理者
    private List<DoorBean> mDoorDataList;
    private List<IceBean> mIceDataList;
    private List<GridBean> mGridDataList;
    private List<BasketBean> mBasketDataList;

    private float gridTypeBackgroundHeight;//元素背景的中心店高度
    private float gridTypeChoosePositionY;//元素选择列表各类型的positionY值

    private float leaveSize = 0.0f;//屏幕宽度留白空间
    private float intervalPx = 1.0f;//相邻格子间隙
    private float gridSize;//格子类型选择的大小
    private float originOfgridTypeBg;
    private float gridTpyeBgHeight;
    private float interval;//相邻格子中点坐标的间隔
    private float x; //作为边界或者元素坐标的x坐标变量
    private float y;//作为边界或者元素坐标的y坐标变量
    private int currentIndex;//当前元素列表的索引
    private int startHorizontal; //当前鼠标点击格子的行数
    private int startVertical;//当前鼠标点击格子的列数
    private int targetTypeIndex;
    private bool isInit = true;
    private int lines;

    private string gridDataToJson;
    private List<List<GridBean>> gridListOfEditor; //编辑器格子管理者
    private List<GridBean> gridList;

    private Vector3 doorPoint;
    private string doorDataToJson;
    private bool isCreateOne;
    private List<DoorBean> doorListOfEditor;//编辑器传送门管理者

    private Vector3 icePoint;
    private string iceDataToJson;
    private List<IceBean> iceListOfEditor;//编辑器冰块管理者

    private Vector3 basketPoint;
    private string basketDataToJson;
    private List<BasketBean> basketListOfEditor;//编辑器金豆荚篮子管理者

    /// <summary>
    /// 接收GridUI的对象和数据
    /// </summary>
    private void initGridUIAttribute()
    {
        mEditorData = GridUIAttributeManager.getInstance().editorData;
        mSprites = GridUIAttributeManager.getInstance().allSprites;
        mGridListManager = GridUIAttributeManager.getInstance().gridListManager;
        mGridBaseListManager = GridUIAttributeManager.getInstance().gridBaseListManager;
        mGameBackground = GridUIAttributeManager.getInstance().GameBackground;
        mGridBg = GridUIAttributeManager.getInstance().GridBg;
        mGrid = GridUIAttributeManager.getInstance().Gird;
        mGridDataList = GridUIAttributeManager.getInstance().gridDataList;
        mDoorDataList = GridUIAttributeManager.getInstance().doorDataList;
        mIceDataList = GridUIAttributeManager.getInstance().iceDataList;
        mBasketDataList = GridUIAttributeManager.getInstance().basketDataList;
    }

    // Use this for initialization
    void Start()
    {
        //隐藏编辑器界面，显示编辑器按钮
        sceneEditor.SetActive(false);
        openEditorButton.transform.SetAsLastSibling();

        //初始化Grid界面的相关属性、对象和List
        initGridUIAttribute();

        //初始化格子类型选择内容
        initGridTypeChooseContent();
    }

    //初始化格子类型选择列表
    private void initGridTypeChooseContent()
    {
        if (mSprites != null)
        {
            gridSize = Screen.width / 10;
            lines = (mSprites.Count - 1) / 10;

            //根据选择类型个数，设置背景高度
            gridTypeBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, gridSize * (lines + 2));
            Arrows.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
            Arrows.GetComponent<RectTransform>().position = gridTypeBackground.GetComponent<RectTransform>().position + new Vector3(0.0f, -((gridSize * (lines + 2)) / 2 - gridSize / 2), 0.0f);

            x = gridSize / 2;
            y = Arrows.GetComponent<RectTransform>().position.y + gridSize * (lines + 1);
            for (int i = 0, reSetX = 0, whichLine = 0; i < mSprites.Count; i++, reSetX++)
            {
                GameObject grid = Instantiate(Resources.Load("prefabs/grid"), gridTypeBackground.transform) as GameObject;
                Destroy(grid.GetComponent<SpriteRenderer>());
                grid.AddComponent<Image>();
                grid.GetComponent<Image>().sprite = mSprites[i];
                grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                grid.AddComponent<Button>();
                grid.GetComponent<Button>().onClick.AddListener(onGridTypeClick);
                if (reSetX == 10)
                {
                    reSetX = 0;
                    whichLine++;
                }
                grid.GetComponent<RectTransform>().position = new Vector3(x + gridSize * reSetX, y - whichLine * gridSize, 0);

                //如果是传送门出入口，则需要进行翻转
                if (i == 8 || i == 9)
                    grid.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                //传送门入口
                if (i == 8)
                    grid.GetComponent<RectTransform>().position = new Vector3(x + gridSize * reSetX, y - whichLine * gridSize - gridSize / 3, 0);
                //传送门出口
                if (i == 9)
                    grid.GetComponent<RectTransform>().position = new Vector3(x + gridSize * reSetX, y - whichLine * gridSize + gridSize / 3, 0);

                GridBean gridBean = new GridBean();
                gridBean.gridObject = grid;
            }
        }
    }

    //初始化格子内容
    private void initGridContentSet()
    {
        gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;
        interval = gridSize + intervalPx;
        x = leaveSize / 2 + gridSize / 2;

        //设置编辑器格子内容
        gridListOfEditor = new List<List<GridBean>>();
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
                                gridBean.spritesIndex = 1;
                            else
                                gridBean.spritesIndex = gridData.spritesIndex;
                            break;
                        }
                        else
                        {
                            gridBean.spritesIndex = 0;
                        }
                    }
                }
                else
                {
                    gridBean.spritesIndex = 0;
                }

                grid.GetComponent<Image>().sprite = mSprites[gridBean.spritesIndex];
                grid.GetComponent<RectTransform>().position = new Vector3(x, y, 0);
                grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                grid.AddComponent<Button>();
                grid.GetComponent<Button>().onClick.AddListener(onGridClick);
                gridBean.gridObject = grid;
                gridList.Add(gridBean);
            }
            gridListOfEditor.Add(gridList);
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

        //初始化游戏场景元素
        initGameBg();

        if (isInit)
        {
            //读取配置内容，根据当前关卡显示编辑器设置
            initData();

            //初始化格子内容
            initGridContentSet();

            isInit = false;
        }
    }

    private void initGameBg()
    {
        //设置编辑器传送门
        if (mDoorDataList != null)
        {
            doorListOfEditor = mDoorDataList;
            foreach (DoorBean doorBean in doorListOfEditor)
            {
                Destroy(doorBean.indoor);
                Destroy(doorBean.outdoor);
            }
            for (int i = 0; i < doorListOfEditor.Count; i++)
            {
                //入口
                GameObject indoor = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(indoor.GetComponent<SpriteRenderer>());
                indoor.name = "indoor" + doorListOfEditor[i].inVertical.ToString() + doorListOfEditor[i].inHorizontal.ToString();
                indoor.AddComponent<Image>();
                indoor.GetComponent<Image>().sprite = mSprites[8];
                doorPoint = gridListOfEditor[doorListOfEditor[i].inVertical][doorListOfEditor[i].inHorizontal].gridObject.GetComponent<RectTransform>().position;
                indoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, -gridSize * 2 / 3, 0.0f);
                indoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                indoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorListOfEditor[i].indoor = indoor;

                //出口
                GameObject outdoor = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(outdoor.GetComponent<SpriteRenderer>());
                outdoor.name = "outdoor" + doorListOfEditor[i].outVertical.ToString() + doorListOfEditor[i].outHorizontal.ToString();
                outdoor.AddComponent<Image>();
                outdoor.GetComponent<Image>().sprite = mSprites[9];
                doorPoint = gridListOfEditor[doorListOfEditor[i].outVertical][doorListOfEditor[i].outHorizontal].gridObject.GetComponent<RectTransform>().position;
                outdoor.GetComponent<RectTransform>().position = doorPoint + new Vector3(0.0f, gridSize * 2 / 3, 0.0f);
                outdoor.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                outdoor.GetComponent<RectTransform>().Rotate(new Vector3(75, 0.0f, 0.0f));
                doorListOfEditor[i].outdoor = outdoor;
            }
        }

        //设置编辑器冰块
        if (mIceDataList != null)
        {
            iceListOfEditor = mIceDataList;
            foreach (IceBean iceBean in iceListOfEditor)
            {
                Destroy(iceBean.ice);
            }
            for (int i = 0; i < iceListOfEditor.Count; i++)
            {
                GameObject ice = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(ice.GetComponent<SpriteRenderer>());
                ice.name = "ice" + iceListOfEditor[i].iceVertical.ToString() + iceListOfEditor[i].iceHorizontal.ToString();
                ice.AddComponent<Image>();
                ice.GetComponent<Image>().sprite = mSprites[iceListOfEditor[i].iceLevel + 9];
                icePoint = mGridBaseListManager[iceListOfEditor[i].iceVertical][iceListOfEditor[i].iceHorizontal].gridBase.GetComponent<RectTransform>().position;
                ice.GetComponent<RectTransform>().position = icePoint;
                ice.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                ice.AddComponent<Button>();
                ice.GetComponent<Button>().onClick.AddListener(onGridClick);
                iceListOfEditor[i].ice = ice;
            }
        }

        //设置金豆荚篮子
        if (mBasketDataList != null)
        {
            basketListOfEditor = mBasketDataList;
            foreach (BasketBean basketBean in basketListOfEditor)
            {
                Destroy(basketBean.basket);
            }
            for (int i = 0; i < basketListOfEditor.Count; i++)
            {
                GameObject basket = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
                Destroy(basket.GetComponent<SpriteRenderer>());
                basket.name = "basket" + basketListOfEditor[i].basketVertical.ToString() + basketListOfEditor[i].basketHorizontal.ToString();
                basket.AddComponent<Image>();
                basket.GetComponent<Image>().sprite = mSprites[14];
                basket.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize * 0.9f, gridSize*0.4f);
                basketPoint = mGridBaseListManager[basketListOfEditor[i].basketVertical][basketListOfEditor[i].basketHorizontal].gridBase.GetComponent<RectTransform>().position + new Vector3(0.0f, -gridSize / 2 - gridSize * 0.4f / 2, 0.0f);
                basket.GetComponent<RectTransform>().position = basketPoint;
                basket.AddComponent<Button>();
                basket.GetComponent<Button>().onClick.AddListener(onGridClick);
                basketListOfEditor[i].basket = basket;
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
                if (gridListOfEditor[vertical][horizontal].spritesIndex == 0)
                {
                    mGridListManager[vertical][horizontal].gridObject.SetActive(true);
                    mGridBaseListManager[vertical][horizontal].gridBase.SetActive(true);
                    mGridListManager[vertical][horizontal].spritesIndex = UnityEngine.Random.Range(2, 8);
                    mGridListManager[vertical][horizontal].gridObject.GetComponent<Image>().sprite = mSprites[mGridListManager[vertical][horizontal].spritesIndex];
                    mGridBaseListManager[vertical][horizontal].isHasGrid = true;
                    mGridBaseListManager[vertical][horizontal].spriteIndex = mGridListManager[vertical][horizontal].spritesIndex;
                }

                //不显示元素，挖空格子
                if (gridListOfEditor[vertical][horizontal].spritesIndex == 1)
                {
                    mGridListManager[vertical][horizontal].gridObject.SetActive(false);
                    mGridBaseListManager[vertical][horizontal].gridBase.SetActive(false);
                    mGridBaseListManager[vertical][horizontal].isHasGrid = true;
                    mGridBaseListManager[vertical][horizontal].spriteIndex = -1;
                }

                //固定元素
                if (gridListOfEditor[vertical][horizontal].spritesIndex > 1)
                {
                    mGridListManager[vertical][horizontal].gridObject.SetActive(true);
                    mGridBaseListManager[vertical][horizontal].gridBase.SetActive(true);
                    mGridListManager[vertical][horizontal].spritesIndex = gridListOfEditor[vertical][horizontal].spritesIndex;
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

        //更新传送门位置
        if (doorListOfEditor != null)
        {
            mDoorDataList = doorListOfEditor;
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

        //更新冰块位置
        if (iceListOfEditor != null)
        {
            mIceDataList = iceListOfEditor;
            foreach (IceBean iceBean in mIceDataList)
            {
                Destroy(iceBean.ice);
            }
            foreach (IceBean iceBean in mIceDataList)
            {
                GameObject ice = Instantiate(Resources.Load("prefabs/gridbase"), mGridBg.transform) as GameObject;
                Destroy(ice.GetComponent<SpriteRenderer>());
                ice.name = "ice" + iceBean.iceVertical.ToString() + iceBean.iceHorizontal.ToString();
                ice.AddComponent<Image>();
                ice.GetComponent<Image>().sprite = mSprites[iceBean.iceLevel + 9];
                ice.GetComponent<RectTransform>().position = mGridBaseListManager[iceBean.iceVertical][iceBean.iceHorizontal].gridBase.GetComponent<RectTransform>().position;
                ice.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
                iceBean.ice = ice;
            }
        }

        //更新金豆荚篮子位置
        if(basketListOfEditor != null)
        {
            mBasketDataList = basketListOfEditor;
            foreach (BasketBean basketBean in mBasketDataList)
            {
                Destroy(basketBean.basket);
            }
            foreach (BasketBean basketBean in mBasketDataList)
            {
                GameObject basket = Instantiate(Resources.Load("prefabs/gridbase"), mGridBg.transform) as GameObject;
                Destroy(basket.GetComponent<SpriteRenderer>());
                basket.name = "basket" + basketBean.basketVertical.ToString() + basketBean.basketHorizontal.ToString();
                basket.AddComponent<Image>();
                basket.GetComponent<Image>().sprite = mSprites[14];
                basket.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize * 0.9f, gridSize * 0.4f);
                basket.GetComponent<RectTransform>().position = mGridBaseListManager[basketBean.basketVertical][basketBean.basketHorizontal].gridBase.GetComponent<RectTransform>().position + new Vector3(0.0f, -gridSize / 2 - gridSize * 0.4f / 2, 0.0f);
                basketBean.basket = basket;
            }
        }
    }

    //类型选择元素点击事件
    public void onGridTypeClick()
    {
        //用于计算选择了哪个类型
        gridSize = Screen.width / 10;

        originOfgridTypeBg = gridTypeBackground.GetComponent<RectTransform>().position.y;
        gridTpyeBgHeight = gridTypeBackground.GetComponent<RectTransform>().sizeDelta.y;
        lines = (int)(Math.Abs(Input.mousePosition.y - (originOfgridTypeBg + gridTpyeBgHeight / 2)) / gridSize);
        currentIndex = lines * 10 + (int)(Input.mousePosition.x / gridSize);

        //格子内容大小，用于计算坐标偏移
        gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;

        //设置格子内容
        if (startVertical >= 0 && startHorizontal >= 0)
        {

            switch (currentIndex)
            {
                case 8://若选择了传送门出入口，则对应生成一个UI用于显示
                    isCreateOne = true;
                    if (doorListOfEditor != null && doorListOfEditor.Count > 0)
                    {
                        //遍历List，如果传送门入口不存在则新创建，若存在，则消除
                        for (int i = 0; i < doorListOfEditor.Count; i++)
                        {
                            //删除传送门入口
                            if (doorListOfEditor[i].inVertical == startVertical && doorListOfEditor[i].inHorizontal == startHorizontal)
                            {
                                Destroy(doorListOfEditor[i].indoor);
                                Destroy(doorListOfEditor[i].outdoor);
                                doorListOfEditor.RemoveAt(i);
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
                        if (doorListOfEditor == null)
                            doorListOfEditor = new List<DoorBean>();
                        doorListOfEditor.Add(doorBean);
                    }
                    break;
                case 9://传送门出口坐标
                    isCreateOne = true;
                    if (doorListOfEditor != null && doorListOfEditor.Count > 0)
                    {
                        //遍历List，如果传送门入口不存在则新创建，若存在，则消除
                        for (int i = 0; i < doorListOfEditor.Count; i++)
                        {
                            //删除传送门入口
                            if (doorListOfEditor[i].outVertical == startVertical && doorListOfEditor[i].outHorizontal == startHorizontal)
                            {
                                Destroy(doorListOfEditor[i].outdoor);
                                doorListOfEditor[i].outVertical = -1;
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
                        for (int i = 0; i < doorListOfEditor.Count; i++)
                        {
                            if (doorListOfEditor[i].outVertical == -1)
                            {
                                doorListOfEditor[i].outVertical = startVertical;
                                doorListOfEditor[i].outHorizontal = startHorizontal;
                                doorListOfEditor[i].outdoor = outdoor;
                                break;
                            }
                        }
                    }
                    break;
                case 10://设置冰块
                case 11:
                case 12:
                    isCreateOne = true;
                    if (iceListOfEditor.Count > 0)
                    {
                        for (int i = 0; i < iceListOfEditor.Count; i++)
                        {
                            //若已存在冰块，直接修改或者删除冰块，同一个格子，选择相同类型即去掉冰块
                            if (iceListOfEditor[i].iceVertical == startVertical && iceListOfEditor[i].iceHorizontal == startHorizontal)
                            {
                                if (iceListOfEditor[i].iceLevel != currentIndex - 9)
                                {
                                    iceListOfEditor[i].iceLevel = currentIndex - 9;
                                    iceListOfEditor[i].ice.GetComponent<Image>().sprite = mSprites[currentIndex];
                                }
                                else
                                {
                                    Destroy(iceListOfEditor[i].ice);
                                    iceListOfEditor.Remove(iceListOfEditor[i]);
                                }
                                isCreateOne = false;
                                break;
                            }
                        }
                    }
                    if (isCreateOne)
                        createIce(startVertical, startHorizontal, currentIndex);
                    break;
                case 14://设置金豆荚篮子
                    isCreateOne = true;
                    if (basketListOfEditor != null && basketListOfEditor.Count > 0)
                    {
                        for (int i = 0; i < basketListOfEditor.Count; i++)
                        {
                            if (basketListOfEditor[i].basketVertical == startVertical && basketListOfEditor[i].basketHorizontal == startHorizontal)
                            {
                                Destroy(basketListOfEditor[i].basket);
                                basketListOfEditor.Remove(basketListOfEditor[i]);
                                isCreateOne = false;
                                break;
                            }
                        }
                    }
                    if (isCreateOne)
                        createBasket(startVertical, startHorizontal);
                    break;
                default://设置格子内容
                    gridListOfEditor[startVertical][startHorizontal].gridObject.GetComponent<Image>().sprite = mSprites[currentIndex];
                    gridListOfEditor[startVertical][startHorizontal].spritesIndex = currentIndex;
                    break;
            }
        }

        //设置消除类型
        if (startVertical < 0 && startHorizontal < 0)
        {
            //编辑器更新目标类型
            targetTypeChoose.GetComponent<Image>().sprite = mSprites[currentIndex];
            targetTypeIndex = currentIndex;
        }

        //隐藏元素类型选择列表
        gridTypeBackground.SetActive(false);
    }

    /// <summary>
    /// 生成金豆荚篮子
    /// </summary>
    private void createBasket(int startVertical, int startHorizontal)
    {
        GameObject basket = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
        Destroy(basket.GetComponent<SpriteRenderer>());
        basket.name = "basket" + startVertical.ToString() + startHorizontal.ToString();
        basket.AddComponent<Image>();
        basket.GetComponent<Image>().sprite = mSprites[currentIndex];
        basketPoint = mGridBaseListManager[startVertical][startHorizontal].gridBase.GetComponent<RectTransform>().position;
        basket.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize * 0.9f, gridSize * 0.4f);
        basket.GetComponent<RectTransform>().position = basketPoint + new Vector3(0.0f, -gridSize / 2 - gridSize * 0.4f / 2, 0.0f);

        BasketBean basketBean = new BasketBean();
        basketBean.basketVertical = startVertical;
        basketBean.basketHorizontal = startHorizontal;
        basketBean.basket = basket;
        if (basketListOfEditor == null)
            basketListOfEditor = new List<BasketBean>();
        basketListOfEditor.Add(basketBean);
    }

    /// <summary>
    /// 生成冰块
    /// </summary>
    private void createIce(int startVertical, int startHorizontal, int currentIndex)
    {
        GameObject ice = Instantiate(Resources.Load("prefabs/gridbase"), GridContentSet.transform) as GameObject;
        Destroy(ice.GetComponent<SpriteRenderer>());
        ice.name = "ice" + startVertical.ToString() + startHorizontal.ToString();
        ice.AddComponent<Image>();
        ice.GetComponent<Image>().sprite = mSprites[currentIndex];
        icePoint = mGridBaseListManager[startVertical][startHorizontal].gridBase.GetComponent<RectTransform>().position;
        ice.GetComponent<RectTransform>().position = icePoint;
        ice.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
        ice.AddComponent<Button>();
        ice.GetComponent<Button>().onClick.AddListener(onGridClick);
        IceBean iceBean = new IceBean();
        iceBean.iceVertical = startVertical;
        iceBean.iceHorizontal = startHorizontal;
        iceBean.iceLevel = currentIndex - 9;
        iceBean.ice = ice;
        iceListOfEditor.Add(iceBean);
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
        Arrows.GetComponent<RectTransform>().position = new Vector3(Input.mousePosition.x, gridTypeBackground.GetComponent<RectTransform>().position.y - ((gridSize * (lines + 2)) / 2 - gridSize / 2), 0.0f);

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

            //设置选择类型的位置
            gridTypeChoosePositionY = Input.mousePosition.y - (gridTypeBackgroundHeight + 50) / 2;
            gridTypeBackground.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gridTypeChoosePositionY, 0.0f);
        }
    }

    //刷新按钮点击事件
    public void onReSetClick()
    {
        //让所有格子变成随机
        for (int vertical = 0; vertical < 9; vertical++)
        {
            for (int horizontal = 0; horizontal < 9; horizontal++)
            {
                //产生随机元素
                gridListOfEditor[vertical][horizontal].spritesIndex = 0;
                gridListOfEditor[vertical][horizontal].gridObject.GetComponent<Image>().sprite = mSprites[0];
            }
        }

        //将游戏场景的元素清楚、传送门、冰块等
        if (doorListOfEditor != null)
        {
            for (int i = 0; i < doorListOfEditor.Count; i++)
            {
                Destroy(doorListOfEditor[i].indoor);
                Destroy(doorListOfEditor[i].outdoor);
            }
            doorListOfEditor.RemoveRange(0, doorListOfEditor.Count);
        }

        if (iceListOfEditor != null)
        {
            for (int i = 0; i < iceListOfEditor.Count; i++)
            {
                Destroy(iceListOfEditor[i].ice);
            }
            iceListOfEditor.RemoveRange(0, iceListOfEditor.Count);
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
                if (gridListOfEditor[vertical][horizontal].spritesIndex != 0)
                    gridDataToJson = gridDataToJson + vertical + "|" + horizontal + "|" + (gridListOfEditor[vertical][horizontal].spritesIndex) + ",";
            }
        }
        mEditorData.gridData = gridDataToJson;

        if (mDoorDataList != null)
        {
            for (int i = 0; i < mDoorDataList.Count; i++)
            {
                doorDataToJson = doorDataToJson + mDoorDataList[i].inVertical + "|" + mDoorDataList[i].inHorizontal + "|" + mDoorDataList[i].outVertical + "|" + mDoorDataList[i].outHorizontal + ",";
            }
            mEditorData.doorData = doorDataToJson;
        }

        if (mIceDataList != null)
        {
            for (int i = 0; i < mIceDataList.Count; i++)
            {
                iceDataToJson = iceDataToJson + mIceDataList[i].iceVertical + "|" + mIceDataList[i].iceHorizontal + "|" + mIceDataList[i].iceLevel + ",";
            }
            mEditorData.iceData = iceDataToJson;
        }

        if (mBasketDataList != null)
        {
            for (int i = 0; i < mBasketDataList.Count; i++)
            {
                basketDataToJson = basketDataToJson + mBasketDataList[i].basketVertical + "|" + mBasketDataList[i].basketHorizontal + ",";
            }
            mEditorData.basketData = basketDataToJson;
        }

        //判断同一关卡配置是否存在

        //点击确定再保存数据
        JsonUtil.createPlayLevelJsonData(mEditorData);

        Debug.Log("outputData done!!!!!");
    }
}
