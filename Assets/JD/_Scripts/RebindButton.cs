using UnityEngine;
using UnityEngine.InputSystem; // Le coeur du système
using TMPro; // Pour changer le texte

public class RebindButton : MonoBehaviour
{
    [Header("Configuration")]
    // Ici, on glissera l'action qu'on veut changer (ex: Jump)
    public string actionName = "Jump"; 
    public TextMeshProUGUI buttonText; // Le texte du bouton

    private InputAction actionToRebind;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    void Start()
    {
        actionToRebind = GameManager.Instance.inputs.asset.FindActionMap("Gameplay").FindAction(actionName);
        
        UpdateDisplay();
    }

    // Cette fonction sera appelée quand on clique sur le bouton
    public void StartRebinding()
    {
        buttonText.text = "Appuyez..."; 
        actionToRebind.Disable(); // On éteint juste ce bouton là

        rebindOperation = actionToRebind.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => FinishRebinding())
            .Start();
    }

    void FinishRebinding()
    {
        rebindOperation.Dispose();
        actionToRebind.Enable(); // On rallume
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        // Affiche la touche (ex: "Space")
        int bindingIndex = actionToRebind.GetBindingIndexForControl(actionToRebind.controls[0]);
        buttonText.text = InputControlPath.ToHumanReadableString(
            actionToRebind.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}