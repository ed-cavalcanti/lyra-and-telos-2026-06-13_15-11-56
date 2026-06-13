using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum STATE
{
    DISABLED,
    WAITING,
    TYPING
}

public class DialogueSystem : MonoBehaviour
{
    public DialogueData dialogueData;
    STATE state;
    int currentText = 0;
    bool finished = false;
    [SerializeField] TypeTextAnimation typeText;

    // NOVO: Campo para arrastar o seu painel de UI no Inspector
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;

    private TarodevController.PlayerController playerController;
    public bool IsDialogueActive => state != STATE.DISABLED;

    void Awake()
    {
        playerController = FindAnyObjectByType<TarodevController.PlayerController>();
        if (typeText == null)
        {
            typeText = GetComponentInChildren<TypeTextAnimation>();
        }
        state = STATE.DISABLED;
    }

    void Update()
    {
        if (state == STATE.DISABLED) return;
        switch (state)
        {
            case STATE.WAITING:
                Waiting();
                break;
            case STATE.TYPING:
                Typing();
                break;
        }
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.talkScript == null || data.talkScript.Count == 0) return;

        dialogueData = data;
        currentText = 0;
        finished = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        if (playerController != null)
        {
            playerController.SetMovementLocked(true);
        }

        Next();
    }

    public void Next()
    {
        if (currentText < dialogueData.talkScript.Count)
        {
            if (nameText != null)
            {
                nameText.text = dialogueData.talkScript[currentText].name;
            }

            typeText.Play(dialogueData.talkScript[currentText].text);
            currentText++;
            if (currentText == dialogueData.talkScript.Count) finished = true;
            state = STATE.TYPING;
        }
    }

    void Waiting()
    {
        bool interactPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                               (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame); // Adicionado o E para fluidez!

        if (interactPressed)
        {
            if (finished)
            {
                state = STATE.DISABLED;
                if (dialoguePanel != null) dialoguePanel.SetActive(false);

                if (playerController != null)
                {
                    playerController.SetMovementLocked(false);
                }
            }
            else
            {
                Next();
            }
        }
    }

    void Typing()
    {
        bool interactPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        // Permite pular a animação do texto
        if (interactPressed)
        {
            typeText.Skip();
            state = STATE.WAITING;
        }
        else if (!typeText.isTyping) // Se a animação concluiu sozinha
        {
            state = STATE.WAITING;
        }
    }
}