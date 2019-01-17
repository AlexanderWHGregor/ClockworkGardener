using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    public List<GameObject> gems = new List<GameObject>();

    public int gridWidth;
    public int gridHeight;
    public GameObject gemPrefab;

    // Start is called before the first frame update
    void Start() {

        for (int y = 0; y < gridHeight; y++) {

          for (int x = 0; x < gridWidth; x++) {

            GameObject g = Instantiate(gemPrefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
            gems.Add(g);

          }

        }

    }

    // Update is called once per frame
    void Update() {

    }
}
