using Unity.VisualScripting;
using UnityEngine;

public class PressButton : MonoBehaviour
{
    public int ActVar = 0; 
    [Header("Interact type")]
   [SerializeField] public bool Hold = false;
   [SerializeField] public bool Toggle = false;
    [Header("Object select and actions")]
    [SerializeField] private GameObject Sqr;
    public enum Move {move, vanish, appear, flip }
    public Move action;
    [Header("Only if move")]
    [SerializeField] private GameObject MoveSpot;
    [SerializeField] public float TimeVar = 1f;
    public Vector3 Mover;
    public Vector3 OG;
    public bool isPress = false;
    public bool isMover = false;
    public bool isGhost = false;
    public bool isFlip = false;
    public bool Flippy = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        switch(action)
        {
            case Move.move:
                ActVar = 5;
                Debug.Log("move");
                break;
            case Move.vanish:
                ActVar = 1;
                Debug.Log("vanish");
                break;
            case Move.appear:
                ActVar = 3;
                Debug.Log("appear");
                break;
            case Move.flip:
                ActVar = 7;
                break;
        } 
        if (ActVar == 3)
        {
            isGhost = true;
        }
        if (isGhost == true && ActVar == 3)
        {
            Sqr.SetActive(false);
        }
    }

    private void Awake()
    {
       
    }
    // Update is called once per frame
    void Update()
    {
        if(Flippy == true)
        {
            
        }
        if (isPress == true && isMover == true)
        {
            Sqr.transform.position = Vector3.Lerp(Sqr.transform.position, Mover, Time.deltaTime * TimeVar);
        }
        else if (isPress == false && isMover == true)
        {
            Sqr.transform.position = Vector3.Lerp(Sqr.transform.position, OG, Time.deltaTime * TimeVar);
        }

        if (isGhost == true && Sqr != null)
        {
            Sqr.SetActive(false);
        }
        else if (isGhost == false && Sqr != null)
        {
            Sqr.SetActive(true);
        }
        
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Hold == true)
        {
            Debug.Log("is held");
            if (ActVar == 5)
            {
                isPress = true;
            }
           if (ActVar == 1)
            {
                isGhost = true;
            }
           if (ActVar == 3)
            {
                isGhost = false;
            }
           if (ActVar == 7)
            {
                isFlip = true;
            }
            if (isFlip == true)
            {
                Sqr.transform.Rotate(0f, 180f, 0f);
            }
        }
        if (collision.gameObject.CompareTag("Player") && Toggle == true)
        {
            Debug.Log("is toggle");
            if (ActVar == 5)
            {
                isPress = true;
            }
            if (ActVar == 1)
            {
                isGhost = true;
            }
            if (ActVar == 3)
            {
                isGhost = false;
            }
            if (ActVar == 7)
            {
                isFlip = true;
            }
            if (isFlip == true)
            {
                Sqr.transform.Rotate(0f, 180f, 0f);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Hold == true)
        {
            Debug.Log("is nolonger held");
            if (ActVar == 5)
            {
                isPress = false;
            }
            if (ActVar == 1)
            {
                isGhost = false;
            }
            if (ActVar == 3)
            {
                isGhost = true;
            }
            if (ActVar == 7)
            {
                isFlip = false;
            }
            if (isFlip == false)
            {
                Sqr.transform.Rotate(0f, -180f, 0f);
            }
        }
    }
}
