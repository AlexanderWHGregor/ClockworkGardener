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
        public bool isChecked;
    }

    private Gem[,] allGems;                 // Stores all gem objects on the board       
    private Gem draggedGem;                 // Reference to the dragged gem
    private Gem draggedGemClone = new Gem();// Reference to the clone of the gem dragged
    private bool controllable = true;       // Lock of board to prevent user controls
    private Vector2 mousePos = Vector2.zero;// Vector of the mouse position
    private bool dragging = false;          // Boolean object to show dragging status

    public int width;                       // Board width
    public int height;                      // Board height
    public List<GameObject> gemResources = new List<GameObject>();  // List of gem prefabs

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
                if (i >= 2 && allGems[i - 2, j].resourceIndex == allGems[i - 1, j].resourceIndex)
                    possibleIndexes.Remove(allGems[i - 2, j].resourceIndex);
                if (j >= 2 && allGems[i, j - 2].resourceIndex == allGems[i, j - 1].resourceIndex)
                    possibleIndexes.Remove(allGems[i, j - 2].resourceIndex);

                // Randomly pick one from the gem resource set
                int index = possibleIndexes[Random.Range(0, possibleIndexes.Count)];

                // Creating the object
                GameObject gem = Instantiate(gemResources[index], pos, Quaternion.identity);
                gem.transform.name = "(" + i + "," + j + ") : " + index;
                gem.AddComponent<GemController>();
                allGems[i, j] = new Gem
                {
                    resourceIndex = index,
                    gemObject = gem,
                    dropDistance = 0,
                    gc = gem.GetComponent<GemController>(),
                    isChecked = false
                };
            }
        }
    }

    void Update()
    {
        // If the board is processing, control will be denied
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
            trackDragging();
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // TODO: drop the gem
            releaseGem();
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
        }
        else
        {
            // Select the gem at the mouse position
            draggedGem = allGems[pos_x, pos_y];

            GameObject gemObjectClone = Instantiate(draggedGem.gemObject,
            new Vector2(pos_x, pos_y), Quaternion.identity);
            gemObjectClone.AddComponent<GemController>();
            gemObjectClone.name = "(" + pos_x + "," + pos_y + ") : " + draggedGem.resourceIndex;

            draggedGemClone = new Gem
            {
                resourceIndex = draggedGem.resourceIndex,
                gemObject = gemObjectClone,
                dropDistance = 0,
                gc = gemObjectClone.GetComponent<GemController>(),
                isChecked = false
            };

            draggedGem.gc.select();

            // Change the transparency so user can see the gem underneath 
            Color32 color = draggedGem.gemObject.GetComponent<SpriteRenderer>().material.color;
            color.a = 100;
            draggedGem.gemObject.GetComponent<SpriteRenderer>().material.color = color;

            // Change reference in list
            allGems[pos_x, pos_y] = draggedGemClone;

        }
    }

    // Move the dragged gem toward the direction of the mouse 
    private void trackDragging()
    {
        //if (draggedGemClone.gemObject == null)
            //Debug.Log(draggedGemClone.resourceIndex);

        int draggedX = Mathf.RoundToInt(draggedGemClone.gemObject.transform.position.x);
        int draggedY = Mathf.RoundToInt(draggedGemClone.gemObject.transform.position.y);

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
        swapGems(draggedGemClone, allGems[draggedX, draggedY]);
    }

    // Swap two gems
    private void swapGems(Gem g1, Gem g2)
    {
        // TODO: Animation

        // Get their positions
        int x1 = Mathf.RoundToInt(g1.gemObject.transform.position.x);
        int y1 = Mathf.RoundToInt(g1.gemObject.transform.position.y);
        int x2 = Mathf.RoundToInt(g2.gemObject.transform.position.x);
        int y2 = Mathf.RoundToInt(g2.gemObject.transform.position.y);

        if (!(x1 == x2 && y1 == y2))
        {
            // Swap gem object positions
            Vector2 tempPos = g2.gemObject.transform.position;
            g2.gemObject.transform.position = g1.gemObject.transform.position;
            g1.gemObject.transform.position = tempPos;

            // Swap object name
            string tempName = g2.gemObject.name;
            g2.gemObject.name = g1.gemObject.name;
            g1.gemObject.name = tempName;

            // Swap gems stored in allGems list
            Gem tempGem = allGems[x1, y1];
            allGems[x1, y1] = allGems[x2, y2];
            allGems[x2, y2] = tempGem;
        }
    }

    // Release the dragged gem to current position
    private void releaseGem()
    {
        // Remove the reference to the dragged gem and destroy the object
        Destroy(draggedGem.gemObject);
        draggedGem = new Gem();

        // Check Match
        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                if (allGems[i, j].resourceIndex >= 0)
                    checkMatch(i, j);
            }
        }
        dropGems();
        //debugCheck();

        // Remove the reference to the clone
        draggedGemClone = new Gem();
    }

    // Check if there are at least 3 identical gems at (x,y)
    private void checkMatch(int x, int y)
    {
        Gem target = allGems[x, y];
        int index = target.resourceIndex;

        // Setting counters
        int up = 0, down = 0, left = 0, right = 0;

        // Four loops to detect the number of identical gems at each direction
        while (y + up + 1 < height && allGems[x,y + up + 1].resourceIndex == index)
            up++;
        while (y - down - 1 >= 0 && allGems[x, y - down - 1].resourceIndex == index)
            down++;
        while (x + right + 1 < width && allGems[x + right + 1,y].resourceIndex == index)
            right++;
        while (x - left - 1 >= 0 && allGems[x - left - 1, y].resourceIndex == index)
            left++;


        if (left + right >= 2 && left + right >= up + down)
        {
            for (int i = x - left; i <= x + right; i ++)
            {
                allGems[i,y].resourceIndex = -1;
                //Debug.Log("Destroy: " + i + " , " + y);
                Destroy(allGems[i,y].gemObject);
                allGems[i, y].gemObject = null;

                /*
                for (int k = y + 1; k < height; k ++)
                {
                    //Debug.Log(x + "," + k + "  caused by " + i + "," + y);
                    allGems[i,k].dropDistance++;
                }
                */
            }
        }else if (up + down >= 2)
        {
            for (int j = y - down; j <= y + up; j ++)
            {
                allGems[x,j].resourceIndex = -1;
                //Debug.Log("Destroy: " + x + " , " + j);
                Destroy(allGems[x,j].gemObject);
                allGems[x, j].gemObject = null;
            }
            /*
            for (int k = y + up + 1; k < height; k++)
            {
                allGems[x, k].dropDistance += up + down + 1;
            }
            */
        }
    }

    // Drop all gems based on the drop distance recorded
    private void dropGems()
    {
        /*
        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                Gem target = allGems[i, j];
                if (target.dropDistance > 0)
                {
                    if (j - target.dropDistance < 0)
                    {
                        Debug.Log("Drop: " + i + "," + j + " down " + target.dropDistance);
                    }
                    if (allGems[i, j - target.dropDistance].resourceIndex == -1)
                    {
                        target.gc.drop(target.dropDistance);
                        allGems[i, j - target.dropDistance] = target;
                        target.dropDistance = 0;

                        allGems[i, j].resourceIndex = -1;
                    }
                }
            }
        }
        */

        for (int i = 0; i < width; i++)
        {
            int distance = 0;
            for (int j = 0; j < height; j++)
            {
                if (allGems[i,j].resourceIndex == -1)
                {
                    if (allGems[i, j].gemObject != null)
                        allGems[i, j].gemObject.name += " : " + -1;
                    distance++;
                }else if (distance > 0)
                {
                    allGems[i, j].gc.drop(distance);

                    if (j - distance < 0)
                        Debug.Log("Error: (" + i + "," + j + ") dropping " + distance);
                    else
                    {
                        if (allGems[i, j - distance].gemObject != null)
                            Debug.Log("Position occupied by others");
                        else
                        {
                            allGems[i, j].gc.drop(distance);
                            allGems[i, j - distance].gemObject = allGems[i, j].gemObject;
                            allGems[i, j].gemObject = null;
                            allGems[i, j - distance].resourceIndex = allGems[i, j].resourceIndex;
                        }
                        
                    }

                    if (j + 1 < height && allGems[i, j + 1].resourceIndex == -1)
                        distance = 0;
                }
            }
        }
    }

    private void debugCheck()
    {
        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                if (allGems[i,j].resourceIndex == -1 &&
                    allGems[i,j].gemObject != null)
                {
                    Debug.Log(i + "," + j + " Index Error");
                    Debug.Log("Position: " + allGems[i, j].gemObject.transform.position);
                }

                if (allGems[i,j].gemObject != null &&
                    (allGems[i,j].gemObject.transform.position.x != i ||
                    allGems[i,j].gemObject.transform.position.y != j))
                {
                    Debug.Log(i + "," + j + " Position Error");
                }
            }
        }
    }
}
