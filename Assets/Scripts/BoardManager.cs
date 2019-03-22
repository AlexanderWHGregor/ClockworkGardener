using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Script for the Board Manager to initiate the game board

    // Struct Gem
    struct Gem
    {
        public int resourceIndex;           // Index of the gem prefab used
        public GameObject gemObject;        // Game object of the gem
        public bool isSelected;             // True if the gem is dragged 
    }

    private Gem[,] allGems;                 // 2D array to store all gem objects on the board       
    private Gem draggedGem;                 // Reference to the dragged gem
    private Gem draggedGemClone = new Gem();// Reference to the clone of the gem dragged
    private bool lock1 = false;             // Lock for matching and dropping gems to wait for coroutines
    private int lock2 = 0;                  // Lock for dropByOne
    private int lock3 = 0;                  // Lock for dropping animation
    private Vector2 mousePos = Vector2.zero;// Vector of the mouse position
    private bool dragging = false;          // True if any gem is selected
    private bool matchFound = false;        // True if match gems are found

    public int width;                       // Board width
    public int height;                      // Board height
    public List<GameObject> gemResources = new List<GameObject>();  // List of all gem prefabs

    // Start is called before the first frame update 
    void Start()
    {
        SetUp();
    }

    // Create the board
    private void SetUp()
    {
        // Initialize gem array
        // Top row saved for new gems coming
        allGems = new Gem[width, height + 1];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create a gem at current position
                Vector2 pos = new Vector2(i, j);

                // Preparing for possible gems to allocate
                // Matched gems at the beginning should be avoided
                List<int> possibleIndexes = new List<int>();
                for (int k = 0; k < gemResources.Count; k++) possibleIndexes.Add(k);

                // Maximum 2 adjacent gems with identical prefabs are allowed
                if (i >= 2 && allGems[i - 2, j].resourceIndex == allGems[i - 1, j].resourceIndex)
                    possibleIndexes.Remove(allGems[i - 2, j].resourceIndex);
                if (j >= 2 && allGems[i, j - 2].resourceIndex == allGems[i, j - 1].resourceIndex)
                    possibleIndexes.Remove(allGems[i, j - 2].resourceIndex);

                // Randomly pick one from the gem resource set
                int index = possibleIndexes[Random.Range(0, possibleIndexes.Count)];

                // Creating the object
                GameObject gem = Instantiate(gemResources[index], pos, Quaternion.identity);
                gem.transform.name = "(" + i + "," + j + ") : " + index;
                allGems[i, j] = new Gem
                {
                    resourceIndex = index,
                    gemObject = gem,
                    isSelected = false
                };
            }
        }
    }

    void Update()
    {
        // If the board is processing, control will be denied
        if (lock1 || lock2 > 0 || lock3 > 0) return;

        // When press down the mouse primary key (start of dragging)
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
            StartCoroutine(releaseGem());
            dragging = false;
        }

        // Check selected gem
        if (draggedGem.gemObject != null && draggedGem.isSelected)
        {
            draggedGem.gemObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 5f);
            if (Input.GetMouseButtonUp(0))
                draggedGem.isSelected = false;
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
            dragging = true;

            // Select the gem at the mouse position
            draggedGem = allGems[pos_x, pos_y];

            // Create a clone on the board, whose position coordinates
            // should always be integers to remind the user of the 
            // actual position of the dragged gem on the board
            GameObject gemObjectClone = Instantiate(draggedGem.gemObject,
            new Vector2(pos_x, pos_y), Quaternion.identity);
            gemObjectClone.name = "(" + pos_x + "," + pos_y + ") : " + draggedGem.resourceIndex;

            draggedGemClone = new Gem
            {
                resourceIndex = draggedGem.resourceIndex,
                gemObject = gemObjectClone,
                isSelected = false
            };

            // Update the selecte status of the dragged gem
            draggedGem.isSelected = true;

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
        // Get the position of cloned gem
        int draggedX = Mathf.RoundToInt(draggedGemClone.gemObject.transform.position.x);
        int draggedY = Mathf.RoundToInt(draggedGemClone.gemObject.transform.position.y);

        // Compare the cloned gem position with mouse position
        int targetX = draggedX, targetY = draggedY;

        // Get mouse position
        int pos_x = Mathf.RoundToInt(mousePos.x);
        int pos_y = Mathf.RoundToInt(mousePos.y);

        // Move the cloned gem one step toward the mouse position
        if (pos_x > draggedX && draggedX + 1 < width)
            targetX++;
        else if (pos_x < draggedX && draggedX - 1 >= 0)
            targetX--;
        else if (pos_y > draggedY && draggedY + 1 < height)
            targetY++;
        else if (pos_y < draggedY && draggedY - 1 >= 0)
            targetY--;

        // Get the target gem to swap with dragged gem
        swapGems(draggedX,draggedY,targetX,targetY);
    }

    // Swap two gems
    private void swapGems(int x1, int y1, int x2, int y2)
    {
        // Choose not to do swap animation to prevent errors
        // caused by cloned gem not catching the speed of mouse

        // If the two gems are the same, return
        if (x1 == x2 && y1 == y2) return;

        // If empty, return
        if (allGems[x1, y1].gemObject == null ||
            allGems[x2, y2].gemObject == null)
            return;

        // Swap positions
        Vector2 tempPos = allGems[x1, y1].gemObject.transform.position;
        allGems[x1, y1].gemObject.transform.position =
            allGems[x2, y2].gemObject.transform.position;
        allGems[x2, y2].gemObject.transform.position = tempPos;

        // Swap gems in the array
        Gem temp = new Gem
        {
            resourceIndex = allGems[x1, y1].resourceIndex,
            gemObject = allGems[x1, y1].gemObject,
            isSelected = false
        };
        allGems[x1, y1] = new Gem
        {
            resourceIndex = allGems[x2, y2].resourceIndex,
            gemObject = allGems[x2, y2].gemObject,
            isSelected = false
        };
        allGems[x2, y2] = temp;
    }

    // Release the dragged gem to current position and start matching
    private IEnumerator releaseGem()
    {
        // Remove the reference to the dragged gem and destroy the object
        Destroy(draggedGem.gemObject);
        draggedGem = new Gem();
        draggedGem.isSelected = false;

        // Remove the reference to the clone
        draggedGemClone = new Gem();

        matchFound = true;

        // Keep matching until no match is found
        while (matchFound)
        {
            matchFound = false;

            // Check Match for every non-empty gem
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    // Index is -1 only if the gem is empty
                    if (allGems[i, j].resourceIndex >= 0)
                    {
                        StartCoroutine(checkMatch(i, j));
                        StartCoroutine(dropGems());

                        // Wait for processing before next match
                        while (lock1 || lock2 > 0 || lock3 > 0)
                            yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
    }

    // Check if there are at least 3 identical gems at (x,y)
    private IEnumerator checkMatch(int x, int y)
    {
        // Pause if other function is processing
        while (lock1 || lock2 > 0|| lock3 > 0)
            yield return new WaitForSeconds(0.1f);

        // List of the gems to be destroyed
        List<GameObject> toDestroy = new List<GameObject>();

        // Match lock
        lock1 = true;

        Gem target = allGems[x, y];
        int index = target.resourceIndex;

        // Setting counters
        int up = 0, down = 0, left = 0, right = 0;

        // Four loops to detect the number of identical gems at each direction
        while (y + up + 1 < height && allGems[x, y + up + 1].resourceIndex == index)
            up++;
        while (y - down - 1 >= 0 && allGems[x, y - down - 1].resourceIndex == index)
            down++;
        while (x + right + 1 < width && allGems[x + right + 1, y].resourceIndex == index)
            right++;
        while (x - left - 1 >= 0 && allGems[x - left - 1, y].resourceIndex == index)
            left++;

        // Check which direction has the most identical gems
        // if equal, horiziontal before vertical
        if (left + right >= 2 && left + right >= up + down)
        {
            matchFound = true;

            // Update the index of matched gems
            // Change the color of the gems to let the user see the matched gems
            for (int i = x - left; i <= x + right; i ++)
            {
                allGems[i,y].resourceIndex = -1;
                allGems[i,y].gemObject.GetComponent<SpriteRenderer>().color = Color.red;
                toDestroy.Add(allGems[i, y].gemObject);
            }
        }
        else if (up + down >= 2)
        {
            matchFound = true;

            for (int j = y - down; j <= y + up; j ++)
            {
                allGems[x,j].resourceIndex = -1;
                allGems[x,j].gemObject.GetComponent<SpriteRenderer>().color = Color.red;
                toDestroy.Add(allGems[x, j].gemObject);
            }
        }

        // Wait to make sure user have time to see the change of colors
        if (toDestroy.Count > 0) yield return new WaitForSeconds(0.5f);

        // Destroy the game objects of matched gems
        foreach (GameObject obj in toDestroy) Destroy(obj);

        // Match unlock
        lock1 = false;
    }

    // Drop all gems and fill the board
    private IEnumerator dropGems()
    {
        // Pause if other function is processing
        while (lock1 || lock2 > 0 || lock3 > 0)
            yield return new WaitForSeconds(0.1f);

        // Drop lock
        // Share the lock with matching because drop is 
        // always after matching, shouldn't be any conflict
        lock1 = true;

        // Start dropping for each column with empty positions
        for (int i = 0; i < width; i ++)
        {
            // Lowest empty position of the column
            Vector2 emptyPos = getLowestEmpty(i);

            // Drop 1 step one time, until no empty position left
            while (emptyPos.x >= 0)
            {
                // Generate a new gem at the top of this column
                allGems[i, height] = generateGem(i, height);

                // Drop one step for every gem in this column
                StartCoroutine(dropByOne(i,Mathf.RoundToInt(emptyPos.y)));

                // Update the lowest empty position
                emptyPos = getLowestEmpty(i);
            }
        }

        // If dropByOne is processing, wait
        while (lock2 > 0) yield return new WaitForSeconds(0.1f);

        // Unlock dropping
        lock1 = false;
    }

    // Randomly generate and return a new gem
    private Gem generateGem(int x, int y)
    {
        Vector2 pos = new Vector2(x, y);

        // Randomly pick a index of prefabs
        int index = Random.Range(0, gemResources.Count);

        // Create the new gem
        GameObject gem = Instantiate(gemResources[index], pos, Quaternion.identity);
        gem.transform.name = "(" + x + "," + y + ") : " + index;
        return new Gem
        {
            resourceIndex = index,
            gemObject = gem,
            isSelected = false,
        };
    }

    // Get the lowest empty position of the column
    private Vector2 getLowestEmpty(int col)
    {
        // Return the first empty position found from the 
        // bottom of the column
        for (int i = 0; i < height; i ++)
        {
            if (isEmpty(allGems[col, i])) return new Vector2(col, i); 
        }

        return new Vector2(-1,-1);
    }

    // Determine if the gem is empty
    private bool isEmpty(Gem gem)
    {
        // The gem is empty if the index is -1 
        // or the object is destroyed
        if (gem.gemObject == null) return true;
        if (gem.resourceIndex == -1) return true;

        return false;
    }

    // Drop all gems in the column by one 
    private IEnumerator dropByOne(int col, int row)
    {
        // Increment lock for dropByOne
        lock2 ++;

        // Drop every non-empty gem in this column and 
        // above this row
        for (int i = row + 1; i < height + 1; i ++)
        {
            if (!isEmpty(allGems[col,i]))
            {
                // Dropping animation starts
                StartCoroutine(dropSingleGemByOne(allGems[col, i].gemObject, col, i));

                // Duplicate the gem to the new position and 
                // destroy the original one
                allGems[col, i - 1] = new Gem
                {
                    resourceIndex = allGems[col,i].resourceIndex,
                    gemObject = allGems[col, i].gemObject,
                    isSelected = false
                };
                allGems[col,i-1].gemObject.name = "(" + col + "," + (i-1) + 
                ") : " + allGems[col,i-1].resourceIndex;

                allGems[col, i].gemObject = null;
                allGems[col, i].resourceIndex = -1;
            }
        }

        // Wait for all dropping animation to finish
        while (lock3 > 0)
            yield return new WaitForSeconds(0.1f);

        // Unlock dropByOne
        lock2 --;
    }

    // Drop single gem down by one slowly
    private IEnumerator dropSingleGemByOne(GameObject obj, int col, int i)
    {
        // Increment dropping animation lock
        lock3++;

        // Controls the delay between each drop
        WaitForSeconds delay = new WaitForSeconds(0.001f);

        // User lerp to show the position between start and end 
        float lerpPercent = 0;
        while (lerpPercent <= 1 && obj != null)
        {
            obj.transform.position = Vector2.Lerp(new Vector2(col, i),
                new Vector2(col, i - 1), lerpPercent);
            lerpPercent += 0.05f; // Speed of dropping
            yield return delay;
        }

        // Unlock dropping animation
        lock3--;
    }
}
