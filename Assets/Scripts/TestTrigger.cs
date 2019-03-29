using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrigger : MonoBehaviour
{
public int test = 0;
void Update()
{
  Debug.Log("HELLO!");
  print("HELOO?");
}

  private void OnTriggerEnter2D(Collider2D other)
  {
    Debug.Log("TRIGGER ENTER!!!!!");
    test++;
  }
}
