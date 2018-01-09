using UnityEngine;
using System.IO;
using System.Text;

public class JsonUtil
{
    /// <summary>
    /// 读取配置数据，获取关卡数、目前类型、消除数量、步数以及游戏初次进入的内容
    /// </summary>
    /// <returns></returns>
    public static EditorData getEditorData(int playLevel)
    {
        string jsonPath = Application.dataPath + "/" + playLevel + ".json";
        string jsonResult = File.ReadAllText(jsonPath, Encoding.UTF8);
        EditorData obj = JsonUtility.FromJson<EditorData>(jsonResult);
        EditorData editorData = new EditorData();
        editorData.gridData = obj.gridData;
        editorData.playLevel = obj.playLevel;
        editorData.stepCounts = obj.stepCounts;
        editorData.targetType = obj.targetType;
        editorData.targetCounts = obj.targetCounts;
        editorData.doorData = obj.doorData;
        editorData.iceData = obj.iceData;
        editorData.beanData = obj.beanData;
        editorData.basketData = obj.basketData;
        editorData.timboData = obj.timboData;
        return editorData;
    }

    /// <summary>
    /// 将编辑器数据以Json格式导出到文件
    /// </summary>
    /// <param name="editorData"></param>
    /// <param name="fileSavePath"></param>
    public static void createPlayLevelJsonData(EditorData editorData)
    {
        string fileSavePath = Application.dataPath + "/" + editorData.playLevel + ".json";
        string result = JsonUtility.ToJson(editorData);
        StreamWriter sw = new StreamWriter(fileSavePath, false,Encoding.UTF8);
        sw.Write(result);
        sw.Flush();
        sw.Close();
    }
}