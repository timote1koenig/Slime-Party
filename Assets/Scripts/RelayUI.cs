
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RelayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text joinLabelText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text yourCodeText;

    public void Start()
    {
        SetStatus("Waiting...");
        yourCodeText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
    }

    public void SetStatus(string status)
    {
        statusText.text = "Status : " + status;
    }

    public void SetJoinCode(string code)
    {
        joinCodeInput.text = code;  
    }

    public void HostSelected()
    {
        hostButton.gameObject.SetActive(false);
        joinLabelText.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        joinCodeInput.gameObject.SetActive(false);
    }

    public void JoinSelected()
    {
        hostButton.gameObject.SetActive(false);
    }

    public void JoinSuccess()
    {
        joinLabelText.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        joinCodeInput.gameObject.SetActive(false);
    }

    public void SetHostCode(string code)
    {
        yourCodeText.text = "Your Code : " + code;
        yourCodeText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
    }
}
