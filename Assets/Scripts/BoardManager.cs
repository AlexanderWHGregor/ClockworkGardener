using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Script for the Board Manager to initiate the game board
    struct Gem
    {
        public int resourceIndex;               
        public GameObject gemObject;
        public GemController gc;        // Reference to gem handler
        public int dropDistance;        // Distance for gem to drop
    }

    //private GameObject[,] allChars; 	// All character objects in the board
    //private int[,] allCharsIndex; 		// Indexes of the characters in all tiles
    private Gem[,] allGems;
    private Gem draggedGem = new Gem();
    private bool controllable = true;
    private Vector2 mousePos = Vector2.zero;
    private bool dragging = false;

    public int width; 					// Board width
    public int height; 					// Board height
    //public GameObject tilePrefab; 		// Empty tile prefabs
    public List<GameObject> gemResources = new List<GameObject>(); 	// List of character prefabs

    // Start is called before the first frame update 
    void Start()
    {
        //allChars = new GameObject[width, height];
        //allCharsIndex = new int[width, height];

        SetUp();
    }

    // Create the board using a 2D array storing all the tiles stating at (0,0)
    private void SetUp()
    {
        // 2 loops to create the game board from (0,0)
        // Camera position should be modified to make sure
        // the game board is at the center of the screen
        allGems = new Gem[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create a gem at current position
                Vector2 pos = new Vector2(i, j);

                // Preparing for possible gems to allocate
                List<int> possibleIndexes = new List<int>();
                for (int k = 0; k < gemResources.Count; k++) possibleIndexes.Add(k);

                // Avoid repeating 3 gems at the beginning
                if (i >= 2 && allGems[i-2,j].resourceIndex == allGems[i-1,j].resourceIndex)
                    possibleIndexes.Remove(allGems[i - 2,j].resourceIndex);
                if (j >= 2 && allGems[i,j - 2].resourceIndex == allGems[i,j - 1].resourceIndex)
                    possibleIndexes.Remove(allGems[i,j - 2].resourceIndex);

                // Randomly pick one from the gem resource set
                int index = possibleIndexes[Random.Range(0, possibleIndexes.Count)];

                // Creating the object
                GameObject gem = Instantiate(gemResources[index], pos, Quaternion.identity);
                gem.transform.name = "(" + i + "," + j + ")";
                gem.AddComponent<GemController>();
                allGems[i, j] = new Gem
                {
                    resourceIndex = index,
                    gemObject = gem,
                    dropDistance = 0,
                    gc = gem.GetComponent<GemController>()
                };
            }
        }
    }

    void Update()
    {
        if (!controllable)
        {
            Debug.Log("Control denied");
            return;
        }

        // When press down the mouse primary key (start of dragging)
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragging = true;
            // TODO: create a clone of gem for dragging
            createClone();
        } // When holding primary key (dragging)
        else if (Input.GetMouseButton(0) && dragging)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // TODO: swap the gem clone with the adjacent gems along
            //       the direction of the mouse
        }

        // When ending with primary key (end of dragging)
        if (Input.GetMouseButtonUp(0) && dragging)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // TODO: drop the gem
            //controllable = false; // No controls should be made when matching gems
            // TODO: match gems
            dragging = false;
        }
    }

    void createClone()
    {
        int pos_x = Mathf.RoundToInt(mousePos.x);
        int pos_y = Mathf.RoundToInt(mousePos.y);

        if (pos_x >= width || pos_x < 0 || pos_y > height || pos_y < 0)
        {
            Debug.Log("Click position out of board");
            return;
        }else
        {
            Gem target = allGems[pos_x, pos_y];
            target.gc.select();
            Color32 color = target.gemObject.GetComponent<Renderer>().material.color;
        }
    }
}
