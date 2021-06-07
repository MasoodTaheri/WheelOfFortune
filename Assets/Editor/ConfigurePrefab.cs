using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.IO;
public class ConfigurePrefab : EditorWindow
{
    [Serializable]
    public class PrefabData
    {
        public string text;
        public string color;
        public string image;
        private Texture2D _tex;
        public Texture2D tex
        {
            get
            {
                if (_tex == null)
                {
                    //Texture2D tex = null;
                    byte[] fileData;
                    string address = "Assets/EditorTools/" + image;

                    if (File.Exists(address))
                    {
                        fileData = File.ReadAllBytes(address);
                        _tex = new Texture2D(2, 2);
                        _tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                    }
                    else
                        Debug.Log("file not exit at " + address);
                }
                return _tex;
            }
        }

    }
    [Serializable]
    public class PrefabDataList
    {
        public List<PrefabData> Items;
    }


    public class FoundPrefabs
    {
        public GameObject prefab;
        public bool IsSelect;
        public string Address;
    }
    ConfigurePrefab window;
    string myString;
    string JsonAddress = "Assets/EditorTools/data.json";
    bool groupEnabled;
    //bool myBool = true;
    bool FindInAsset = true;
    bool FindInaddress = true;
    float myFloat = 1.23f;
    PrefabDataList prefabDataList;
    List<FoundPrefabs> foundPrefabs = new List<FoundPrefabs>();


    Vector2 scrollPos = new Vector2(200, 200);
    [MenuItem("Window/ConfigurePrefab")]
    static void Init()
    {
        ConfigurePrefab window = (ConfigurePrefab)EditorWindow.GetWindow(typeof(ConfigurePrefab));
        window.myString = "Assets/plugins";
        window.Show();
        //window.ReadJsonData();
    }

    void ReadJsonData()
    {
        //string s = File.ReadAllText("Assets/EditorTools/data.json");
        string s = File.ReadAllText(JsonAddress);
        s = "{\"Items\":" + s + "}";
        //Debug.Log(s);
        prefabDataList = Newtonsoft.Json.JsonConvert.DeserializeObject<PrefabDataList>(s);
        if (prefabDataList == null)
        {
            Debug.Log("prefabDataList is null");
            return;
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Search and configure prefabs ", EditorStyles.boldLabel);
        JsonAddress = EditorGUILayout.TextField("Json config address", JsonAddress);
        myString = EditorGUILayout.TextField("Specific address", myString);

        EditorGUILayout.BeginHorizontal();
        FindInaddress = EditorGUILayout.Toggle("find in Specific Address", FindInaddress);
        FindInAsset = EditorGUILayout.Toggle("find in Asset", FindInAsset);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Clear List", GUILayout.Width(150), GUILayout.Height(40)))
                foundPrefabs.Clear();
            Search();
            ApplyPrefabSelected();
        }
        EditorGUILayout.EndHorizontal();



        //if search could found any match now show it:
        EditorGUILayout.BeginHorizontal();
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(800), GUILayout.Height(foundPrefabs.Count * 25));
            {
                foreach (FoundPrefabs go in foundPrefabs)
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(50), GUILayout.Height(20)))
                            EditorGUIUtility.PingObject(go.prefab);
                        go.IsSelect = GUILayout.Toggle(go.IsSelect, go.prefab.name + "  @ " + go.Address);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndHorizontal();
    }

    void Search()
    {
        EditorGUI.BeginDisabledGroup((FindInaddress || FindInAsset) == false);
        if (GUILayout.Button("Search", GUILayout.Width(150), GUILayout.Height(40)))
        {
            List<string> SearchInFolderFilter = new List<string>();
            if (FindInaddress)
                SearchInFolderFilter.Add(myString);
            if (FindInAsset)
                SearchInFolderFilter.Add("Assets");

            string[] AllAssets = AssetDatabase.FindAssets("t:Prefab", SearchInFolderFilter.ToArray());
            foreach (string guid1 in AllAssets)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid1);
                GameObject t1 = (GameObject)AssetDatabase.LoadAssetAtPath(p, typeof(GameObject));
                if (t1 != null)
                    if (t1.GetComponent<Text>() != null)
                        foundPrefabs.Add(new FoundPrefabs { prefab = t1, IsSelect = false, Address = p });
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    void ApplyPrefabSelected()
    {
        ReadJsonData();
        bool AllowApply = false;
        foreach (FoundPrefabs go in foundPrefabs)
        {
            if (go.IsSelect)
                AllowApply = true;
        }

        //if (!AllowApply) return;
        EditorGUI.BeginDisabledGroup(AllowApply == false);
        //jumpHeight = EditorGUILayout.FloatField("Jump Height", jumpHeight);
        if (GUILayout.Button("Apply", GUILayout.Width(150), GUILayout.Height(40)))
        {
            Debug.Log("Apply");
            foreach (FoundPrefabs go in foundPrefabs)
            {
                if (go.IsSelect)
                {
                    PrefabData p = prefabDataList.Items.Find(x => x.text == go.prefab.GetComponent<Text>().text);
                    if (p != null)
                    {
                        Color c;
                        ColorUtility.TryParseHtmlString(p.color, out c);

                        go.prefab.GetComponent<Text>().color = c;
                        Image img = go.prefab.transform.GetChild(0).GetComponent<Image>();
                        img.sprite = Sprite.Create(p.tex, new Rect(0, 0, p.tex.width, p.tex.height), Vector2.zero, 100);
                        Debug.Log(go.prefab.name + " changed");

                        //PrefabUtility.SaveAsPrefabAssetAndConnect(go.prefab, go.Address, InteractionMode.UserAction);
                    }
                    else
                        Debug.LogError(go.prefab.name + " - Title=" + go.prefab.GetComponent<Text>().text + " is not in json data");

                }
            }
        }
        EditorGUI.EndDisabledGroup();
    }


}
