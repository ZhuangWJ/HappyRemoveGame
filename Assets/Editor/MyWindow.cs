using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class MyWindow : EditorWindow
{
    private int playLevel; //第几关
    private int targetType;//消除类型
    private int targetCount;//消除数量
    private static string fileSavePath;

    private class MyWindowData //json存储对象
    {
        public int playLevel;
        public int targetCount;
        public int targetType;
    }

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MyWindow window = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow));
        window.Show();

    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        playLevel = EditorGUILayout.IntField("playLevel:", playLevel);
        targetType = EditorGUILayout.IntField("targetType:", targetType);
        targetCount = EditorGUILayout.IntField("targetCount:", targetCount);

        if (GUILayout.Button("execute"))
        {
            fileSavePath = "D:/QcUsers/Win7/Administrator/Documents/D23/Assets/" + playLevel+".json";

            MyWindowData myWindowData = new MyWindowData();
            myWindowData.playLevel = playLevel;
            myWindowData.targetType = targetType;
            myWindowData.targetCount = targetCount;
            string result = JsonUtility.ToJson(myWindowData);
            
            StreamWriter sw = new StreamWriter(fileSavePath, false);
            sw.Write(result);
            sw.Flush();
            sw.Close();
        }
    }
}