using UnityEngine;
using System.IO;
using System.Text;

public class JsonUtil
{
    /// <summary>
    /// 读取json数据，根据配置显示具体横竖列数
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static GameData getGameDataFromJson(string path , string fileName)
    {
        string jsonPath = path + "/" + fileName;
        string jsonResult = File.ReadAllText(jsonPath, Encoding.UTF8);
        GameData obj = JsonUtility.FromJson<GameData>(jsonResult);
        GameData gameData = new GameData();
        gameData.horizontal = obj.horizontal;
        gameData.vertical = obj.vertical;
        return gameData;
    }
    /// <summary>
    /// 读取json数据，根据配置显示游戏关卡目标
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static EditorData getMyWindowDataFromJson(string path, string fileName)
    {
        string jsonPath = path + "/" + fileName;
        string jsonResult = File.ReadAllText(jsonPath, Encoding.UTF8);
        EditorData obj = JsonUtility.FromJson<EditorData>(jsonResult);
        EditorData editorData = new EditorData();
        editorData.playLevel = obj.playLevel;
        editorData.targetCounts = obj.targetCounts;
        editorData.targetType = obj.targetType;
        return editorData;
    }

    /// <summary>
    /// 将编辑器数据以Json格式导出到文件
    /// </summary>
    /// <param name="editorData"></param>
    /// <param name="fileSavePath"></param>
    public static void createPlayLevelData(EditorData editorData)
    {
        string fileSavePath = "D:/QcUsers/Win7/Administrator/Documents/D23/Assets/" + editorData.playLevel + ".json";
        string result = JsonUtility.ToJson(editorData);
        StreamWriter sw = new StreamWriter(fileSavePath, false,Encoding.UTF8);
        sw.Write(result);
        sw.Flush();
        sw.Close();
    }

    /// <summary>
    /// 读取配置数据，获取关卡数、目前类型、消除数量、步数以及游戏初次进入的内容
    /// </summary>
    /// <returns></returns>
    public static EditorData gridDataToList(int playLevel)
    {
        string jsonPath = "D:/QcUsers/Win7/Administrator/Documents/D23/Assets/" + playLevel + ".json";
        string jsonResult = File.ReadAllText(jsonPath, Encoding.UTF8);
        EditorData obj = JsonUtility.FromJson<EditorData>(jsonResult);
        EditorData editorData = new EditorData();
        editorData.gridOfEditorManagerData = obj.gridOfEditorManagerData;
        editorData.playLevel = obj.playLevel;
        editorData.stepCounts = obj.stepCounts;
        editorData.targetType = obj.targetType;
        editorData.targetCounts = obj.targetCounts;
        return editorData;
    }
}