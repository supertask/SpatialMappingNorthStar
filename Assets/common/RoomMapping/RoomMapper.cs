using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using Dummiesman;
using Leap.Unity;

[System.Serializable]
public class MappedObjectsPack
{
    public List<string> objNames;
    public List<Vector3> objPositions;
    public List<Quaternion> objRotations;
    public List<Vector3> objScales;
    
    public MappedObjectsPack() {
        this.objNames = new List<string>();
        this.objPositions = new List<Vector3>();
        this.objRotations = new List<Quaternion>();
        this.objScales = new List<Vector3>();
    }

    public static MappedObjectsPack CreateFromJSON(string json) {
        MappedObjectsPack pack = null;
        try {
            pack = JsonUtility.FromJson<MappedObjectsPack>(json);
        }
        catch (Exception e) {
            Debug.Log("JsonUtilityオブジェクトへの変換エラー" + e.Message);
        }
        return pack;
    }
    public string ToJSON() { return JsonUtility.ToJson(this); }
}

public class RoomMapper : MonoBehaviour
{
    public GameObject mapped;
    public GameObject selecting;
    public Transform player;
    public Material cubeMat;
    public Material objMat;
    public string mappedJsonName = "pack.json";

    public static readonly string SCENE_NAME = "RoomMapper";
    //public string SCANNED_OBJ_PATH ;
    public string MAPPED_OBJECTS_PACK_PATH;

    public static readonly string CUBE_PREFIX = "Cube";

    private int selectingIndex;
    private MappedObjectsPack pack;

    void Start()
    {
        //this.SCANNED_OBJ_PATH = Application.dataPath + "/Resources/ScannedObjects/";
        this.MAPPED_OBJECTS_PACK_PATH = Application.dataPath + "/Resources/MappedObjectsPack/";
        this.selectingIndex = 0;
        this.Load();

        //マップのオブジェクトを選択中に紐づけ
        this.SwitchSelectingObj();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == RoomMapper.SCENE_NAME)  {
            if (Input.GetKeyUp(KeyCode.M)) {
                Debug.Log("Key M");
                this.SwitchSelectingObj();
            }
            else if (Input.GetKeyUp(KeyCode.E)) {
                Debug.Log("Key E");
                this.Save(); //マッピング終了
            }
        }
    }

    /*
     * Cubeを作成
    public void CreateCube()
    {
        this.MapSelectingObjects(); //選択したオブジェクトをマッピングする
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = RoomMapper.SCANNED_OBJ_PREFIX + this.cubeIndex;
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        cube.transform.position = player.position +  player.rotation * Vector3.forward * 0.3f; //0.3m先
        cube.transform.parent = this.selecting.transform;
        cube.GetComponent<MeshRenderer>().material = this.cubeMat;
        this.cubeIndex++;
    }
     */

    /*
     * スキャンしたオブジェクトを作成し，選択中オブジェクトに加える
    public void CreateScannedObject(
        Transform parent,
        string filename,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale)
    {
        //this.MapSelectingObjects();
        string objPath = RoomMapper.SCANNED_OBJ_PATH + filename;
        if (File.Exists(objPath)) {
            Debug.Log("ERROR: no file, " + objPath);
            return;
        }

        GameObject newObj = new OBJLoader().Load(objPath);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        newObj.transform.localScale = scale;
        newObj.transform.parent = parent;

        //マテリアル対応付け
        foreach (Transform child in newObj.GetComponentsInChildren<Transform>())
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer != null) {
                child.gameObject.AddComponent<UVGenerator>();
                renderer.material = this.objMat;
            }
        }
    }
     */

    private Dictionary<string,GameObject> GetMappingObjects() {
        Dictionary<string, GameObject> mappingObjs = new Dictionary<string, GameObject>();
        foreach(Transform t in this.mapped.transform) {
            mappingObjs[t.name] = t.gameObject;
        }
        return mappingObjs;
    }

    /*
     * 選択中のオブジェクトをマップ済みオブジェクトへ登録する
     */
    private void MapSelectingObjects() {
        foreach(Transform child in this.selecting.transform) {
            child.parent = this.mapped.transform;
        }
    }

    /*
     * 選択中オブジェクトを切り替える
     */
    public void SwitchSelectingObj() {
        this.MapSelectingObjects();
        this.selectingIndex = (this.selectingIndex + 1) % this.mapped.transform.childCount;
        this.SelectMappedObject(selectingIndex);
    }

    public void SelectMappedObject(int mappedObjsIndex)
    {
        Transform selectedTransform = this.mapped.transform.GetChild(mappedObjsIndex);
        selectedTransform.parent = this.selecting.transform;

        //Switch Pinch Setting
        LeapRTS[] leaps = this.selecting.GetComponents<LeapRTS>();
        if (selectedTransform.name.IndexOf(RoomMapper.CUBE_PREFIX) >= 0)
        {
            leaps[0].enabled = false;
            leaps[1].enabled = true;
        }
        else {
            leaps[0].enabled = true;
            leaps[1].enabled = false;
        }
    }


    /*
     * データをPlayerPrefsXに保存する
     */
    public void Save()
    {
        this.MapSelectingObjects(); //選択したオブジェクトをマッピングする

        this.pack = new MappedObjectsPack();
        foreach (Transform transform in this.mapped.transform) {
            this.pack.objNames.Add(transform.gameObject.name);
            this.pack.objPositions.Add(transform.position);
            this.pack.objRotations.Add(transform.rotation);
            this.pack.objScales.Add(transform.localScale);
        }
        string savingData = this.pack.ToJSON();
        string packPath = this.MAPPED_OBJECTS_PACK_PATH + this.mappedJsonName;
        File.WriteAllText(packPath, savingData);
        Debug.Log("Data is saved in " + packPath);
    }

    /*
     * セーブデータを読み込む
     */
    public void Load()
    {
        bool is_shown_map = (SceneManager.GetActiveScene().name == RoomMapper.SCENE_NAME);

        string packPath = this.MAPPED_OBJECTS_PACK_PATH + this.mappedJsonName;
        string allText = File.ReadAllText(packPath);
        this.pack = MappedObjectsPack.CreateFromJSON(allText);

        Dictionary<string, GameObject> mappingObjs = this.GetMappingObjects();
        for (int i = 0; i < this.pack.objPositions.Count; i++)
        {
            string name = this.pack.objNames[i];
            if (mappingObjs.ContainsKey(name))
            {
                GameObject anObj = mappingObjs[name];
                anObj.transform.position = this.pack.objPositions[i];
                anObj.transform.rotation = this.pack.objRotations[i];
                anObj.transform.localScale = this.pack.objScales[i];
                anObj.transform.parent = this.mapped.transform;
            }
            else {
                Debug.Log("Error: object " + name + " doesn't exist.");
                continue;
            }
        }
    }


    void OnApplicationQuit()
    {
    }
}
