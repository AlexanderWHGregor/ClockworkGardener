using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Sin(Time.time * 1));
        if (Input.anyKey)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
