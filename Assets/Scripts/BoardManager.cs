using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Script for the Board Manager to initiate the game board
    struct Gem
    {
        public int resourceIndex;           // Determines what the gem looks like               
        public GameObject gemObject;
        public GemController gc;            // Reference to gem handler
        public int dropDistance;            // Distance for gem to drop
    }

    private Gem[,] allGems;                 // Stores all gem objects on the board       
    private Gem draggedGem = new Gem();     // Reference to the clone of the gem dragged
    private bool controllable = true;       // Lock of board to prevent user controls
    private Vector2 mousePos = Vector2.zero;// Vector of the mouse position
    private bool dragging = false;          // Boolean object to show dragging status

    public int width; 					    // Board width
    public int height; 					    // Board height
    public List<GameObject> gemResources = new List<GameObject>(); 	// List of gem prefabs

    // Start is called before the first frame update 
    void Start()
    {
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
            createClone(); // Create a gem clone
        } // When holding primary key (dragging)
        else if (Input.GetMouseButton(0) && dragging)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            trackDragging();
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

    // Create a gem clone when dragging the gem
    void createClone()
    {
        // Get mouse position
        int pos_x = Mathf.RoundToInt(mousePos.x);
        int pos_y = Mathf.RoundToInt(mousePos.y);

        // Determine if the mouse is inside the board
        if (pos_x >= width || pos_x < 0 || pos_y > height || pos_y < 0)
        {
            Debug.Log("Click position out of board");
            return;
        }else
        {
            // Select the gem at the mouse position
            Gem target = allGems[pos_x, pos_y];
            target.gc.select();

            // Create the gem clone
            GameObject gemClone = Instantiate(gemResources[target.resourceIndex],
                new Vector2(pos_x, pos_y), Quaternion.identity);
            draggedGem.gemObject = gemClone;

            // Change the transparency so user can see the gem underneath 
            Color32 color = target.gemObject.GetComponent<SpriteRenderer>().material.color;
            color.a = 100;
            target.gemObject.GetComponent<SpriteRenderer>().material.color = color;

            // Change reference in list
            allGems[pos_x, pos_y] = draggedGem;
        }
    }

    private void trackDragging()
    {
        float draggedX = draggedGem.gemObject.transform.position.x;
        float draggedY = draggedGem.gemObject.transform.position.y;

        // Get mouse position
        int pos_x = Mathf.RoundToInt(mousePos.x);
        int pos_y = Mathf.RoundToInt(mousePos.y);

        // The gem to swap is the next gem at the direction
        // of the mouse 
        if (pos_x > draggedX && draggedX + 1 < width)
            draggedX++;
        else if (pos_x < draggedX && draggedX - 1 >= 0)
            draggedX--;
        else if (pos_y > draggedY && draggedY + 1 < height)
            draggedY++;
        else if (pos_y < draggedY && draggedY - 1 >= 0)
            draggedY--;

        // Get the target gem to swap with dragged gem
        Gem swapTarget = allGems[(int) draggedX, (int) draggedY];
        swapGems(draggedGem, swapTarget);
    }

    // Swap two gems
    private void swapGems(Gem g1, Gem g2)
    {
        // TODO: Animation

        // Get their positions
        int x1 = (int) g1.gemObject.transform.position.x;
        int y1 = (int) g1.gemObject.transform.position.y;
        int x2 = (int) g2.gemObject.transform.position.x;
        int y2 = (int) g2.gemObject.transform.position.y;

        // Swap gem object positions
        Vector2 temp = g2.gemObject.transform.position;
        g2.gemObject.transform.position = g1.gemObject.transform.position;
        g1.gemObject.transform.position = temp;

        // Swap gems stored in allGems list
        Gem tempGem = allGems[x1, y1];
        allGems[x1, y1] = allGems[x2, y2];
        allGems[x2, y2] = tempGem;
    }
}
