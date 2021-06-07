using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Server;
using Promises;


//## Constructors
//`GameplayApi()` _Default constructor to create a GameplayApi object_

//## Methods
//Name | Return type | Description
//--- | --- | ---
//Initialise | `Promise` | This method should be called in the beginning to initialize the API.Should be called after object creation.
//GetPlayerBalance | `Promise<Int64>` | Use this method to get Player's balance.
//GetInitialWin | `Promise<Int32>` | Use this method to get the Initial Win value, that should be multiplied after spin.
//GetMultiplier | `Promise<Int32>` | Use this method to get the value of the Multiplier.
//SetPlayerBalance | `Promise` | Use this method to set Player's balance.

public class WheelOfFortune : MonoBehaviour
{
    public GameObject Board;
    public List<int> prize;
    public List<AnimationCurve> animationCurves;
    public float AnglePerItem = 20;
    public bool IsSpin;
    public Text InitValueText;
    public Text Multiply;
    public Text FinalValue;
    public Button Spin;

    Server.API.GameplayApi serevr;
    public Promise<long> PlayerBalance;
    public Promise<int> PlayerInitialWin;
    public Promise<int> PlayerMultiplier;

    public float _PlayerBalance;
    public int _PlayerInitialWin;
    public int _PlayerMultiplier;
    // Start is called before the first frame update
    void Start()
    {
        FinalValue.text = "0";
        Multiply.text = "0";
        IsSpin = true;
        Spin.interactable = !IsSpin;
        serevr = new Server.API.GameplayApi();
        serevr.Initialise().Then(() =>
        {
            PlayerBalance = serevr.GetPlayerBalance();
            PlayerBalance.Then((x) =>
            {
                Debug.Log("PlayerBalance=" + x);
                _PlayerBalance = x;
            }).Then(() =>
            {
                PlayerInitialWin = serevr.GetInitialWin();
                PlayerInitialWin.Then((x) =>
                {
                    Debug.Log("PlayerInitialWin=" + x);
                    _PlayerInitialWin = x;
                    InitValueText.text = _PlayerInitialWin.ToString();
                }).Then(() =>
                {
                    PlayerMultiplier = serevr.GetMultiplier();
                    PlayerMultiplier.Then((x) =>
                    {
                        Debug.Log("PlayerMultiplier=" + x);
                        _PlayerMultiplier = x; IsSpin = false;
                        Spin.interactable = !IsSpin;
                    });
                });
            });
        }).Catch((e) => { Debug.Log(e.Message); });

        //a.SetPlayerBalance(125);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void StartSpin()
    {
        if (!IsSpin)
            StartCoroutine(Spining());
    }
    IEnumerator Spining()
    {
        FinalValue.text = "0";
        Multiply.text = "0";
        IsSpin = true;
        Spin.interactable = !IsSpin;
        int randomTime = Random.Range(1, 4);
        int itemNumber = 0;

        for (int i = 0; i < prize.Count; i++)
            if (prize[i] == _PlayerMultiplier) itemNumber = i;

        Debug.Log("itemNumber=" + itemNumber);

        float maxAngle = 360 * randomTime + (itemNumber * AnglePerItem);
        float timer = 0.0f;
        float startAngle = Board.transform.eulerAngles.z;

        maxAngle = maxAngle - startAngle;
        int animationCurveNumber = Random.Range(0, animationCurves.Count);
        //Debug.Log("Animation Curve No. : " + animationCurveNumber);

        while (timer < 5 * randomTime)
        {
            //to calculate rotation
            float angle = maxAngle * animationCurves[animationCurveNumber].Evaluate(timer / (randomTime * 5));
            Board.transform.eulerAngles = new Vector3(0.0f, 0.0f, angle + startAngle);
            timer += Time.deltaTime;
            yield return 0;
        }
        Board.transform.eulerAngles = new Vector3(0.0f, 0.0f, maxAngle + startAngle);

        Multiply.text = _PlayerMultiplier.ToString();
        Debug.Log("Prize: " + prize[itemNumber]);//use prize[itemNumnber] as per requirement
        StartCoroutine(IncValue());
    }
    IEnumerator IncValue()
    {
        yield return new WaitForSeconds(1);
        int prize = _PlayerInitialWin * _PlayerMultiplier;
        float cuvalue = 0;
        float duration = 3;//2 seconds
        float speed = prize / (30 * duration);
        float incvalue = speed;// * Time.deltaTime;
        while (cuvalue + incvalue < prize)
        {
            yield return null;
            cuvalue += incvalue;
            FinalValue.text = cuvalue.ToString();
        }
        cuvalue = prize;
        FinalValue.text = cuvalue.ToString();


        _PlayerBalance += prize;
        serevr.SetPlayerBalance((long)_PlayerBalance).Then(() =>
        {
            IsSpin = false;
            Spin.interactable = !IsSpin;
            Debug.Log("player New Balance is " + _PlayerBalance);
        }).Catch((e) => { Debug.Log(e.Message); });

    }

    public void Close()
    {
        GetComponent<RectTransform>().localScale = Vector3.one;
        LeanTween.scale(GetComponent<RectTransform>(), Vector3.zero, 0.3f).setOnComplete(() => { Destroy(this.gameObject); });
    }
}
