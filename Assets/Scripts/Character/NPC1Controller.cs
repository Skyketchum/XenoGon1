using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC1Controller : MonoBehaviour, Interact
{
    [SerializeField] NPCDialog dialog;

    private Rigidbody2D rb;

    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;
    NPCState state;

    float idleTimer = 0f;
    int currentPattern = 0;
  

    Character character;
    //ItemGiver iGiver;
    //xGiver xGiver;
    Healer helr;

    Xenogon _xenogon;

    private void Awake()
    {
        character = GetComponent<Character>();
        rb = GetComponent<Rigidbody2D>();
        helr = GetComponent<Healer>();

    }

    public IEnumerator Interact(Transform initiator)
    {
        

        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;

            character.LookTowards(initiator.position);

            Debug.Log("Interacting with NPC");
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));

        }
       if(helr != null)
        {
            yield return helr.Heal(initiator, dialog);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
        }



      //  StartCoroutine(character.Move(new Vector2(0,2)));
    }

    private void Update()
    {
       

        if(state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer > timeBetweenPattern)
            {

                idleTimer = 0f;

                if(movementPattern.Count > 0)
                StartCoroutine(Walk());
            }
        }

        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

      yield return  character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos) 
        currentPattern = (currentPattern + 1) % movementPattern.Count;
            state = NPCState.Idle;
    }

    public void Interact(Transform initiator, XenogonParty xenogonParty)
    {
        throw new System.NotImplementedException();
    }
}

   public enum NPCState
{
    Idle, Walking, Dialog
}

//old logic updated on 05/30/2022
/*[SerializeField] List<Sprite> sprites;


//SpriteAnimator spriteAnimator;

//  private void Start()
//  {
spriteAnimator = new SpriteAnimator(sprites, GetComponent<SpriteRenderer>());
spriteAnimator.Start();
    }

  

    */
    