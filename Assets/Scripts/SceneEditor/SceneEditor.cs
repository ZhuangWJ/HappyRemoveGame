using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneEditor : MonoBehaviour
{
    public GameObject sceneEditor; //编辑器父对象
    public GameObject openEditorButton;//打开编辑器的按钮
    public GameObject gridTypeBackground;//格子元素选择列表父对象
    public GameObject GridContentSet;//格子内容父对象
    public GameObject Arrows;//箭头对象
    public GameObject inputPlayLevel;//关卡输入框
    public GameObject inputTargetTypeCounts;//目标类型消除数量
    public GameObject targetTypeChoose;

    private float gridTypeBackgroundHeight;//元素背景的中心店高度
    private float gridTypeChoosePositionY;//元素选择列表各类型的positionY值
    private List<Sprite> sprites;//元素资源列表数组
    private GameObjManager gameObjeManager; //游戏对象管理者
    private static EditorObjManager editorObjManager; //编辑器对象管理者
    private float leaveSize;//屏幕宽度留白空间
    private float intervalPx;//相邻格子间隙
    private float gridSize;//格子大小
    private float interval;//相邻格子中点坐标的间隔
    private float x; //作为边界或者元素坐标的x坐标变量
    private float y;//作为边界或者元素坐标的y坐标变量
    private int currentIndex;//当前元素列表的索引
    private int startHorizontal; //当前鼠标点击格子的行数
    private int startVertical;//当前鼠标点击格子的列数
    private MyWindowData myWindowData;

    private List<List<GridBean>> gridOfEditorManager;
    private List<GridBean> gridList;

    private List<List<GridBean>> gridListManager;
    private int targetTypeIndex;

    internal static EditorObjManager getGridTypeChoose()
    {
        if (editorObjManager != null)
            return editorObjManager;
        return null;
    }

    // Use this for initialization
    void Start()
    {
        //隐藏编辑器界面，显示编辑器按钮
        sceneEditor.SetActive(false);
        openEditorButton.transform.SetAsLastSibling();

        //获取格子内容选择对象
        editorObjManager = new EditorObjManager();
        editorObjManager.Arrows = Arrows;

        //初始化格子类型选择内容
        initGridTypeChooseContent();

        //初始化格子布局
        initGridContentSet();
    }

    //初始化格子类型选择列表
    private void initGridTypeChooseContent()
    {
        //生成可选择列表
        Sprite random = new Sprite();
        Sprite visable = new Sprite();
        random = Resources.Load<Sprite>("random") as Sprite;
        visable = Resources.Load<Sprite>("visable") as Sprite;

        sprites = new List<Sprite>();
        sprites.Add(random);
        sprites.Add(visable);
        for (int i = 0; i < GridUI.getSprites().Count; i++)
        {
            sprites.Add(GridUI.getSprites()[i]);
        }

        x = editorObjManager.Arrows.GetComponent<RectTransform>().position.x;
        y = editorObjManager.Arrows.GetComponent<RectTransform>().position.y + 50;
        gridSize = Screen.width * (1 - 0.1f) / 10;
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject grid = Instantiate(Resources.Load("prefabs/grid"), gridTypeBackground.transform) as GameObject;
            Destroy(grid.GetComponent<SpriteRenderer>());
            grid.AddComponent<Image>();
            grid.GetComponent<Image>().sprite = sprites[i];
            grid.GetComponent<RectTransform>().position = new Vector3(x + gridSize * (i + 1), y, 0);
            grid.GetComponent<RectTransform>().sizeDelta = new Vector2(gridSize, gridSize);
            grid.AddComponent<Button>();
            grid.GetComponent<Button>().onClick.AddListener(onGridTypeClick);

            GridBean gridBean = new GridBean();
            gridBean.gridObject = grid;
        }
    }

    //初始化格子内容
    private void initGridContentSet()
    {
        leaveSize = 0.0f;
        intervalPx = 0.1f;
        gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;
        interval = gridSize + intervalPx;
        x = leaveSize / 2 + gridSize / 2;

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
                gridBean.sprite = sprites[0];
                gridBean.spritesIndex = 0;
                grid.GetComponent<Image>().sprite = gridBean.sprite;
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
        gameObjeManager = GridUI.getMainCanvasManager();
        if (gameObjeManager != null)
        {
            gameObjeManager.gameBackground.SetActive(false);
            gameObjeManager.grid.SetActive(false);
            gameObjeManager.gridBg.SetActive(false);
        }
        sceneEditor.SetActive(true);
        gridTypeBackground.SetActive(false);

        //获得目标类型和数量的管理对象
        myWindowData = GridUI.getMyWindowDataObj();
    }

    //确定按钮点击事件
    public void onCommitButtonClick()
    {
        //隐藏编辑器和显示游戏内容
        gameObjeManager = GridUI.getMainCanvasManager();
        if (gameObjeManager != null)
        {
            gameObjeManager.gameBackground.SetActive(true);
            gameObjeManager.grid.SetActive(true);
            gameObjeManager.gridBg.SetActive(true);
        }
        sceneEditor.SetActive(false);

        //将编辑器数据传递给游戏场景显示
        //获取管理游戏元素的GridListManager
        gridListManager = gameObjeManager.gridListManager;

        for (int vertical = 0; vertical < 9; vertical++)
        {
            for (int horizontal = 0; horizontal < 9; horizontal++)
            {
                //产生随机元素
                if (gridOfEditorManager[vertical][horizontal].spritesIndex == 0)
                {
                    gridListManager[vertical][horizontal].spritesIndex = UnityEngine.Random.Range(0, 6);
                    gridListManager[vertical][horizontal].gridObject.GetComponent<Image>().sprite = gameObjeManager.sprites[gridListManager[vertical][horizontal].spritesIndex];
                }

                //不显示元素，挖空格子
                if (gridOfEditorManager[vertical][horizontal].spritesIndex == 1)
                {
                    gridListManager[vertical][horizontal].gridObject.SetActive(false);
                }

                //固定元素
                if (gridOfEditorManager[vertical][horizontal].spritesIndex > 1)
                {
                    gridListManager[vertical][horizontal].spritesIndex = gridOfEditorManager[vertical][horizontal].spritesIndex - 2;
                    gridListManager[vertical][horizontal].gridObject.GetComponent<Image>().sprite = gameObjeManager.sprites[gridListManager[vertical][horizontal].spritesIndex];
                }
            }
        }

        gameObjeManager.gridListManager = gridListManager;
        gameObjeManager.isUpdateGridListManager = true;

        //更新消除类型和数量
        if (inputPlayLevel.GetComponent<InputField>().text != null)
        {
            myWindowData.playLevel = int.Parse(inputPlayLevel.GetComponent<InputField>().text);
            myWindowData.isUpdataTarget = true;
        }
        if(inputTargetTypeCounts.GetComponent<InputField>().text != null)
        { 
            myWindowData.targetCount = int.Parse(inputTargetTypeCounts.GetComponent<InputField>().text);
            myWindowData.isUpdataTarget = true;
        }
        myWindowData.targetTypeObj.GetComponent<Image>().sprite = sprites[targetTypeIndex];
        myWindowData.targetType = targetTypeIndex - 2;
        myWindowData.targetCountCountObj.GetComponent<Text>().text = "x" + myWindowData.targetCount;
    }

    //类型选择元素点击事件
    public void onGridTypeClick()
    {
        gridSize = Screen.width * (1 - 0.1f) / 10;

        //Debug.Log("currentIndex" + currentIndex);
        //隐藏元素类型选择列表
        gridTypeBackground.SetActive(false);

        if(startVertical >=0 && startHorizontal >= 0)
        {
            //计算当前鼠标点击的是哪个类型
            currentIndex = (int)((Input.mousePosition.x - Screen.width * 0.1f / 2) / gridSize);
            gridOfEditorManager[startVertical][startHorizontal].gridObject.GetComponent<Image>().sprite = sprites[currentIndex];
            gridOfEditorManager[startVertical][startHorizontal].spritesIndex = currentIndex;
        }
        else
        {
            targetTypeIndex = (int)((Input.mousePosition.x - Screen.width * 0.1f / 2) / gridSize);
            //编辑器更新目标类型
            targetTypeChoose.GetComponent<Image>().sprite = sprites[targetTypeIndex];
            myWindowData.isUpdataTarget = true;
        }
    }

    //格子点击事件
    public void onGridClick()
    {
        //获取编辑器对象管理器
        editorObjManager = SceneEditor.getGridTypeChoose();

        //显示元素选择内容列表
        gridTypeBackground.SetActive(true);

        //设置元素选择列表位置
        gridTypeBackgroundHeight = gridTypeBackground.GetComponent<RectTransform>().rect.height;
        gridTypeChoosePositionY = Input.mousePosition.y + (gridTypeBackgroundHeight + 50) / 2;
        gridTypeBackground.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, gridTypeChoosePositionY, 0.0f);

        //设置箭头位置
        editorObjManager.Arrows.GetComponent<RectTransform>().position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + (gridTypeBackgroundHeight + 50) / 4, 0.0f);

        //[2]计算格子所在边界，x为左边界，y为上边界
        y = Screen.height * 0.75f;
        x = leaveSize / 2;
        intervalPx = 1.0f;
        gridSize = (Screen.width - leaveSize - (9 - 1) * intervalPx) / 9;
        //[3]鼠标点中格子区域才会响应，记录初次点中的元素信息
        if (Input.mousePosition.x > x && Input.mousePosition.x < (x + gridSize * 9 + intervalPx * 8) && Input.mousePosition.y < y && Input.mousePosition.y > (y - ((9 * gridSize + intervalPx * 8))))
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
                    gridOfEditorManager[vertical][horizontal].spritesIndex = 0;
                    gridOfEditorManager[vertical][horizontal].gridObject.GetComponent<Image>().sprite = sprites[0];
                }
            }
        }
    }
}
