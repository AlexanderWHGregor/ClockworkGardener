using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemController : MonoBehaviour {

    private bool selected = false;

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

        //transform.position = new Vector2(currentX, currentY - distance);
        StartCoroutine(DropGem(distance));
    }

    // Gradually drop the gem    Same function used in "GemsAndCombos"
    private IEnumerator DropGem(int distance)
    {
        WaitForSeconds delay = new WaitForSeconds(0.01f); //Delay between every drop frame
        Vector2 start = transform.position;
        Vector2 end = new Vector2(transform.position.x, transform.position.y - distance);
        float lerpPercent = 0;

        while (lerpPercent <= 1)
        {
            transform.position = Vector2.Lerp(start, end, lerpPercent);
            lerpPercent += 0.05f; //Distance of every drop frame
            yield return delay;
        }

        transform.position = end;
    }
}
