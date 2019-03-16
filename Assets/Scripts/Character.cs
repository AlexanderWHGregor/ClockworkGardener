using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    // Script for Character prefabs

	private Vector2 currentPosition; 		// 2D position of the character
	private static Character previousChar;  // Static variable for the last character selected
	private SpriteRenderer sr; 				// Controlling the rendering color of the character
	private bool isSelected; 				// Determine if the object is already selected 

	void Start()
	{
		sr = this.gameObject.GetComponent<SpriteRenderer> ();
		isSelected = false;
		previousChar = null;
	}

	// Click the object
	void OnMouseDown()
	{
		// Get the accurate coordiates of the object
		currentPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition); 

		// Rounding the coordinates in to nearest integer
		int x_pos = (int)Mathf.Round (currentPosition.x);
		int y_pos = (int)Mathf.Round (currentPosition.y);
		currentPosition = new Vector2 (x_pos, y_pos);

		if (!isSelected && previousChar == null) {
			// If it's the first object selected, make it red and 
			// store it into the variable previousChar
			isSelected = true;
			sr.color = Color.red;
			previousChar = this;
		} else if (previousChar != null) {
			// If it's the second object selected
			// swap the 2 characters and reset previousChar
			swap(this,previousChar);

			previousChar.deselect ();
			previousChar = null;
		} else 
		{
			// If the character is already selected
			// deselect it
			deselect ();	
		}
	}

	// Helper method to deselect the character
	private void deselect() { 
		isSelected = false; 
		sr.color = Color.white;
	}
		
	// Helper method to get current position of the character
	private Vector2 getPosition() { return currentPosition; }

	// Helper method to swap the position of 2 characters
	private void swap(Character c1, Character c2)
	{
		Vector2 tempPosition = c1.getPosition ();
		c1.gameObject.transform.position = c2.gameObject.transform.position;
		c2.gameObject.transform.position = tempPosition;
	}
}
