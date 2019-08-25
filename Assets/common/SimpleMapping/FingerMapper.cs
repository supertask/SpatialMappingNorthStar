using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using static Leap.Finger;
using UnityEngine.SceneManagement;

public class FingerMapper : MonoBehaviour
{
    public GameObject leapProviderObj;
    public Transform player;
    //public GameObject makerPointsObj;
    public GameObject mappedPoints;
    public string sceneName;

    private LeapServiceProvider m_Provider;
    private HandUtil handUtil;
    private readonly float MAPPING_POINT_SIZE = 0.01f;

    void Start()
    {
        this.m_Provider = this.leapProviderObj.GetComponent<LeapServiceProvider>();
        this.handUtil = new HandUtil(player);
        this.Load();
    }

    void Update()
    {
        Frame frame = this.m_Provider.CurrentFrame;
        Hand[] hands = HandUtil.GetCorrectHands(frame); //0=LEFT, 1=RIGHT
        if (hands[HandUtil.LEFT] != null && hands[HandUtil.RIGHT] != null)
        {
            Hand leftHand = hands[HandUtil.LEFT];
            Hand rightHand = hands[HandUtil.RIGHT];
            if (this.handUtil.JustOpenedHandOn(hands, HandUtil.LEFT) )
            {
                //Moment that index finger of left hand is opened
                if (rightHand.Fingers[(int)FingerType.TYPE_INDEX].IsExtended) {
                    Vector3 markingPoint = HandUtil.ToVector3(rightHand.Fingers[(int)FingerType.TYPE_INDEX].TipPosition);
                }
            }
            else if (this.handUtil.JustOpenedHandOn(hands, HandUtil.RIGHT) )
            {
                //Moment that index finger of right hand is opened
                if (leftHand.Fingers[(int)FingerType.TYPE_INDEX].IsExtended) {
                    Vector3 markingPoint = HandUtil.ToVector3(leftHand.Fingers[(int)FingerType.TYPE_INDEX].TipPosition);
                    this.MarkPoint(markingPoint);
                    Debug.Log("Marked point at " + markingPoint);
                }
            }
        }

        if (SceneManager.GetActiveScene().name == this.sceneName)
        {
            if (Input.GetKeyUp(KeyCode.E)) {
                this.Save();
            }
            else if (Input.GetKeyUp(KeyCode.D)) {
                //foreach (Transform t in this.mappedPoints.transform) { Destroy(t.gameObject); }       
            }
        }


        //this.handUtil.SavePreviousFingers(hands);
        this.handUtil.SavePreviousHands(hands);
    }

    void MarkPoint(Vector3 point)
    {
        GameObject anObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anObj.transform.position = point;
        anObj.transform.parent = this.mappedPoints.transform;
        float size = MAPPING_POINT_SIZE;
        anObj.transform.localScale = new Vector3(size, size, size);
    }

    void Save()
    {
        List<Vector3> savingPosList = new List<Vector3>() { };
        foreach(Transform transform in this.mappedPoints.transform) {
            savingPosList.Add(transform.position);
        }
        PlayerPrefsX.SetVector3Array("mappedPosList", savingPosList.ToArray());
        Debug.Log("Saved mappedPosList");
    }

    /*
     * セーブデータを読み込む
     */
    public void Load()
    {
        if (PlayerPrefs.HasKey("mappedPosList"))
        {
            bool isShownMap = (SceneManager.GetActiveScene().name == this.sceneName);
            Vector3[] posList = PlayerPrefsX.GetVector3Array("mappedPosList");
            for (int i = 0; i < posList.Length; i++)
            {
                GameObject anObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //GameObject obj = Object.Instantiate(this.mappedPoints) as GameObject;
                anObj.transform.position = posList[i];
                Vector3 scale = new Vector3(MAPPING_POINT_SIZE, MAPPING_POINT_SIZE, MAPPING_POINT_SIZE);
                anObj.transform.localScale = scale;
                anObj.transform.parent = this.mappedPoints.transform;
                anObj.GetComponent<Renderer>().enabled = isShownMap;
            }
            Debug.Log("Loaded mappedPosList");
        }
    }
}
