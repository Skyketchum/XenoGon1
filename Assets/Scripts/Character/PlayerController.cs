using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] XenogonParty xenogonParty;

    public static PlayerController instance;

    private Vector2 input;
    private Character character;
    private Rigidbody2D rb;

    public int numOfSteps = 10;
    public int stepsMin = 10;
    public int stepsMax = 15;


    private void Awake()
    {
        character = GetComponent<Character>();
        rb = GetComponent<Rigidbody2D>();
        numOfSteps = UnityEngine.Random.Range(stepsMin, stepsMax);
    }


    public void HandleUpdate()
    {
        if (!character.IsMoving)
        { 
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();
       

        if (Input.GetKeyDown(KeyCode.Z))
            Interact();
        
    }

   

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractLayer);

        if(collider != null)
        {
            collider.GetComponent<Interact>()?.Interact(transform, xenogonParty);
            character.IsMoving = false;
        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        foreach(var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if(triggerable != null)
            {
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    public object CaptureState() //saves the player position
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            xenogonList = GetComponent<XenogonParty>().XenogonList.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state) //loads the player position
    {
        var saveData = (PlayerSaveData)state;

        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        // Restore Party
        GetComponent<XenogonParty>().XenogonList = saveData.xenogonList.Select(s => new Xenogon(s)).ToList();
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;

}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<XenogonSaveData> xenogonList;
}
