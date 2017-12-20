using UnityEngine;
using System.IO;
using System.Text;

public class JsonUtil
{
    //读取json数据，根据配置显示具体横竖列数
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

    //读取json数据，根据配置显示游戏关卡目标
    public static MyWindowData getMyWindowDataFromJson(string path, string fileName)
    {
        string jsonPath = path + "/" + fileName;
        string jsonResult = File.ReadAllText(jsonPath, Encoding.UTF8);
        MyWindowData obj = JsonUtility.FromJson<MyWindowData>(jsonResult);
        MyWindowData myWindowData = new MyWindowData();
        myWindowData.playLevel = obj.playLevel;
        myWindowData.targetCount = obj.targetCount;
        myWindowData.targetType = obj.targetType;
        return myWindowData;
    }
}