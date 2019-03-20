using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemController : MonoBehaviour {

    private bool selected = false;
    private Vector2 end;

    // Checks for mouse click and reset mouseGem once the drag is done
    private void Update()
    {
        if (!selected) return;

        // If get dragged, update position every frame
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 5f);

        if (Input.GetMouseButtonUp(0))
            selected = false;
    }

    public void select()
    {
        selected = true;
    }

    public void deselect()
    {
        selected = false;
    }

    public void drop(int distance)
    {
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentY = Mathf.RoundToInt(transform.position.y);

        end = new Vector2(currentX, currentY - distance);

        //Debug.Log("Before Drop: " + transform.position);

        //transform.position = new Vector2(currentX, currentY - distance);
        StartCoroutine(DropGem());
        //Debug.Log("End: " + end);
        transform.position = end;

        //Debug.Log("After Drop: " + transform.position);
    }

    // Gradually drop the gem    Same function used in "GemsAndCombos"
    private IEnumerator DropGem()
    {
        WaitForSeconds delay = new WaitForSeconds(0.01f); //Delay between every drop frame
        Vector2 start = transform.position;

        float lerpPercent = 0;

        while (lerpPercent <= 1)
        {
            transform.position = Vector2.Lerp(start, end, lerpPercent);
            lerpPercent += 0.05f; //Distance of every drop frame
            yield return delay;
        }
    }
}
