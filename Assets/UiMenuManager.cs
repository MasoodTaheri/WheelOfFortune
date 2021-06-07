using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiMenuManager : MonoBehaviour
{
    public GameObject DailyBonusPrefab;
     GameObject _DailyBonusPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DailyBonus_onclick()
    {
        _DailyBonusPrefab = Instantiate(DailyBonusPrefab,Vector3.zero,Quaternion.identity);

        _DailyBonusPrefab.GetComponent<RectTransform>().localScale = Vector3.zero;
        _DailyBonusPrefab.GetComponent<RectTransform>().gameObject.SetActive(true);
        _DailyBonusPrefab.transform.SetParent(this.gameObject.transform);
        _DailyBonusPrefab.GetComponent<RectTransform>().localPosition = Vector3.zero;

        LeanTween.scale(_DailyBonusPrefab.GetComponent<RectTransform>(), Vector3.one, 0.3f);
    }
}
