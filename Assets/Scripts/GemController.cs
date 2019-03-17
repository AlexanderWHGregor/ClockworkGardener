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
}
