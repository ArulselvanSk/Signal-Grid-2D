using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{   
    [SerializeField] private GameObject Door_Obj;
    [SerializeField] private GameObject InfoUI_Obj;
    [SerializeField] private TMP_Text InfoText_TMP;
    [SerializeField] private GameObject JumpInfoUI_Obj;
    [SerializeField] private TMP_Text JumpCounterText_TMP;
    [SerializeField] private GameObject TvScreenVisual_Obj;
    [SerializeField] private GameObject playerCamera_Obj;

    //TV
    private bool IsNearTv = false;
    private bool IsTvOn = false;
    //Table
    private bool IsNearTable = false;
    private bool IsScrouched = false;
    private bool IsKeyPicked = false;
    //Door
    private bool IsNearDoor = false;
    private bool IsDoorOpen = false;
    //Jump Area
    private int JumpCount = 0;
    private bool IsNearJumpArea = false;

    public void PlayerActionHandler(Transform collision)
    {
        string value = collision.gameObject.tag.ToString();

        if(value == "Table")
        {
            IsNearTable = !IsNearTable;
            HandleTableActions();
        }
        if(value == "TV")
        {
            IsNearTv = !IsNearTv;
            HandleTVActions();
        }
        if(value == "Door")
        {
            IsNearDoor = !IsNearDoor;
            HandleDoorActions();
        }
        if(value == "JumpArea")
        {
            HandleJumpAreaActions();
        }
    }

    public void Update()
    {
        bool PressedE = Input.GetKeyDown(KeyCode.E);
        bool PressedShift = Input.GetKeyDown(KeyCode.LeftShift);

        if(Input.GetKeyDown(KeyCode.X))
        {
            QuitApp();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            RestartApp();
        }

        if(PressedShift)
        {
            ShiftKeyPressedHandler();
        }

        if(PressedE)
        {
            EKeyPressedHandler();
        }
    }

    private void EKeyPressedHandler()
    {
        if(IsNearTable && IsScrouched)
        {
            IsKeyPicked = true;
            DisableScrouch();
            Debug.Log("Key Picked...");
            HandleTableActions();
        }
        if (IsNearTv)
        {
            IsTvOn = !IsTvOn;
            if (IsTvOn)
            {
                Debug.Log("TV Turned ON...");
                HandleTVActions();
            }
            else
            {
                Debug.Log("TV Turned OFF...");
                HandleTVActions();
            }
        }
        if(IsNearDoor)
        {
            IsDoorOpen = !IsDoorOpen;
            if(IsDoorOpen)
            {
                OpenCloseDoor(110f);
                Debug.Log("Door Opened...");
            }
            else
            {
                OpenCloseDoor(0f);
                Debug.Log("Door Closed...");
            }
        }
    }

    private void ShiftKeyPressedHandler()
    {
        if (IsNearTable && !IsScrouched && !IsKeyPicked)
        {
            EnableScrouch();
            HandleTableActions();
        }
        else if(IsNearTable && IsScrouched)
        {
            DisableScrouch();
            HandleTableActions();
        }
    }

    private void HandleJumpAreaActions()
    {        
        if(!IsNearJumpArea)
        {
            JumpCounterText_TMP.text = $"Jump Counter : {JumpCount}";
            JumpInfoUI_Obj.SetActive(true);
            IsNearJumpArea = true;
            Debug.Log("Reached Jump Area");
        }
    }

    private void HandleTVActions()
    {
        if(IsNearTv)
        {
            InfoUIHandler("Press 'E' to Turn ON/OFF TV!");
        }
        if(IsNearTv && IsTvOn)
        {
            TvScreenVisual_Obj.SetActive(true);
            Debug.Log("TV Turned ON Success...");
        }
        if(IsNearTv && !IsTvOn)
        {
            TvScreenVisual_Obj.SetActive(false);
            Debug.Log("TV Turned OFF Success...");
        }
        if(!IsNearTv)
        {
            IsTvOn = false;
            TvScreenVisual_Obj.SetActive(false);
            DisableInfoUI();
        }
    }

    private void HandleTableActions()
    {    
        if (IsNearTable && !IsScrouched && !IsKeyPicked)
        {
            InfoUIHandler("Press 'Shift' to crouch!");
        }
        if (IsNearTable && IsScrouched && !IsKeyPicked)
        {
            InfoUIHandler("Press 'E' to pick the key!");
        }
        if (IsNearTable && IsKeyPicked)
        {
            InfoUIHandler("Key taken!");
        }
        if (!IsNearTable)
        {
            DisableScrouch();
            DisableInfoUI();
        }
    }

    private void HandleDoorActions()
    {
        if(IsNearDoor && !IsKeyPicked)
        {
            InfoUIHandler("Find the key to open the door!");
        }
        if(IsNearDoor && IsKeyPicked)
        {
            InfoUIHandler("Press 'E' to Open/Close the door!");
        }
        if(!IsNearDoor)
        {
            DisableInfoUI();
        }
    }

    private void OpenCloseDoor(float rotation)
    {
        Door_Obj.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    public void PlayerJumpHandler()
    {
        JumpCount++;
        JumpCounterText_TMP.text = $"Jump Counter : {JumpCount}";
    }

    private void InfoUIHandler(string message)
    {
        InfoText_TMP.text = message;
        InfoUI_Obj.SetActive(true);
    }

    private void DisableInfoUI()
    {
        InfoText_TMP.text = string.Empty;
        InfoUI_Obj.SetActive(false);
    }

    public void EnableScrouch()
    {
        IsScrouched = true;
        playerCamera_Obj.transform.localPosition = new Vector3(playerCamera_Obj.transform.localPosition.x, - 0.7f, playerCamera_Obj.transform.localPosition.z);
    }
    public void DisableScrouch()
    {
        IsScrouched = false;
        playerCamera_Obj.transform.localPosition = new Vector3(playerCamera_Obj.transform.localPosition.x, 0f, playerCamera_Obj.transform.localPosition.z);
    }

    public void QuitApp()
    {
        Application.Quit();
    }
    public void RestartApp()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
    

