using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interact, ISavable
{
    [SerializeField] string trainerName;
    [SerializeField] Sprite sprite;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] NPCDialog dialog;
    [SerializeField] NPCDialog dialogAfterBattle;

   // [SerializeField] List<Vector2> movementPattern;
    //[SerializeField] float timeBetweenPattern;
  // [SerializeField] TrainerState state;

 //   float idleTimer = 0f;
   // int currentPattern = 0;

    Character character;
  //  private Rigidbody2D rb;


    public bool StopMoving = false;

    bool battleLost = false;

    private void Awake()
    {
        character = GetComponent<Character>();
       // rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.Defaultdirecton);
    }


    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {

        //showing exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //making trainer walk towards player

        var diff = player.transform.position - transform.position;

       var moveVec = diff - diff.normalized;

        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

       yield return character.Move(moveVec);

        //show dialog


       // state = TrainerState.Dialog;
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {

            Debug.Log("Starting Trainer Battle");
            GameController.Instance.StartTrainerBattle(this);

        }));
    }

    public void BattleLost()
    {

        battleLost = true;
        fov.gameObject.SetActive(false);
    }
    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;

        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
    public void Interact(Transform initiator, XenogonParty xenogonParty)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {

                Debug.Log("Starting Trainer Battle");
                GameController.Instance.StartTrainerBattle(this);
            }));

        }
        else
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));



            /*if (state == TrainerState.Idle)
            {
                state = TrainerState.Dialog;
                */
            character.LookTowards(initiator.position);

            Debug.Log("Interacting with Trainer");
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
               // idleTimer = 0f;
                    // state = TrainerState.Idle;
                }));

        }
    }
   private void Update()
    {
       /* if (state == TrainerState.Dialog)
        {
            character.IsMoving = false;
            character.Animator.IsMoving = false;
        }
        else if (state == TrainerState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        */
    

        character.HandleUpdate();

    }
/*
    IEnumerator Walk()
    {
        state = TrainerState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        state = TrainerState.Idle;
    }
    */
    public object CaptureState() //Saves if the trainer was already battled and if they lost and FoV disabling
    {
        return battleLost;
    }

    public void RestoreState(object state) //Loads if the trainer was already battled and if they lost already and FoV disabling
    {
        battleLost = (bool)state;

        if (battleLost)
            fov.gameObject.SetActive(false);
    }

    public string Name
    {
        get => trainerName;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}

public enum TrainerState
{
    Idle, Walking, Dialog
}

