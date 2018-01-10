using UnityEngine;
using System.Collections.Generic;

public class GridUIAttributeManager 
{
    private static GridUIAttributeManager gridUIAttributeManager = null;
    private GridUIAttributeManager() { }
    public static GridUIAttributeManager getInstance()
    {
        if (gridUIAttributeManager == null)
        {
            gridUIAttributeManager = new GridUIAttributeManager();
        }
        return gridUIAttributeManager;
    }

    /// <summary>
    /// 资源集合
    /// </summary>
    public List<Sprite> allSprites;

    /// <summary>
    /// 特效资源集合
    /// </summary>
    public List<Sprite> specialSprites;

    /// <summary>
    /// 游戏背景父对象
    /// </summary>
    public GameObject GameBackground;

    /// <summary>
    /// 格子内容父对象
    /// </summary>
    public GameObject Gird;

    /// <summary>
    /// 格子背景父对象
    /// </summary>
    public GameObject GridBg;

    /// <summary>
    /// 步数对象
    /// </summary>
    public GameObject stepCounts;

    /// <summary>
    /// 目标数量对象
    /// </summary>
    public GameObject targetCount;

    /// <summary>
    /// 胜利对象
    /// </summary>
    public GameObject great;

    /// <summary>
    /// 消除的数量
    /// </summary>
    public int deleteCounts;

    /// <summary>
    /// 游戏格子行列数
    /// </summary>
    public GameData gameData;

    /// <summary>
    /// 格子左右遗留的空隙
    /// </summary>
    public float leaveSize;

    /// <summary>
    /// 游戏格子大小
    /// </summary>
    public float gridSize;

    /// <summary>
    /// 两个格子中心点的距离 = gridSize + 间距intervalPx
    /// </summary>
    public float interval;

    /// <summary>
    /// 两个格子的间距
    /// </summary>
    public float intervalPx;

    /// <summary>
    /// 游戏格子内容的集合管理
    /// </summary>
    public List<List<GridBean>> gridListManager;

    /// <summary>
    /// 游戏背景的集合管理
    /// </summary>
    public List<List<GridBaseBean>> gridBaseListManager;

    /// <summary>
    /// 备用方块的集合管理
    /// </summary>
    public List<GridBean> gridDropList;

    /// <summary>
    /// 金豆荚格子所对应的对象
    /// </summary>
    public List<GridBean> gridOfBeanPodList;

    /// <summary>
    /// 雪块List
    /// </summary>
    public List<GridBean> frostingList = new List<GridBean>();

    /// <summary>
    /// 毒液List
    /// </summary>
    public List<GridBean> poisonList = new List<GridBean>();

    /// <summary>
    /// 生成金豆荚的步数
    /// </summary>
    public int createBeanPodStep;

    /// <summary>
    /// 是否生成金豆荚的累减变量
    /// </summary>
    public int isCreateBeanPod;

    /// <summary>
    /// 配置文件的对象
    /// </summary>
    public EditorData editorData;

    /// <summary>
    /// 配置文件格子内容解析后的数据
    /// </summary>
    public List<GridBean> gridDataList;

    /// <summary>
    /// 配置文件传送门解析后的数据
    /// </summary>
    public List<DoorBean> doorDataList;

    /// <summary>
    /// 配置文件冰块解析后的数据
    /// </summary>
    public List<IceBean> iceDataList;

    /// <summary>
    /// 配置文件金豆荚篮子解析后的数据
    /// </summary>
    public List<BasketBean> basketDataList;

    /// <summary>
    /// 配置文件树藤解析后的数据
    /// </summary>
    public List<TimboBean> timboDataList;
}