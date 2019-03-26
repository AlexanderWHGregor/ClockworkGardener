using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {

    private Transform bar;

    // Use this for initialization
    void Start()
    {
        bar = transform.Find("Bar");
    }

    public void SetSize(float size)
    {
        bar.localScale = new Vector3(size, 1f);
    }
}
