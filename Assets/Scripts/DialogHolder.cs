using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogHolder : MonoBehaviour
{
  [SerializeField]
  private bool finishedDialog = false;
        public string dialogue;
    private DialogueManager dMan;

    public string[] dialogueLines;

    // Start is called before the first frame update
    void Start()
    {

      dMan = FindObjectOfType<DialogueManager>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
if(!finishedDialog)
{  if (other.gameObject.name == "Player") {



      if (!dMan.dialogActive) {

        dMan.dialogLines = dialogueLines;
        dMan.currentLine = 0;
        dMan.ShowDialogue();}
        finishedDialog = true;



        }

      }

    }
}
