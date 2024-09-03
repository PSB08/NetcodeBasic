using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class JoinCodeConnectUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _joinCodeText;
    [SerializeField] private Button _joinBtn;

    private void Awake()
    {
        _joinBtn.onClick.AddListener(HandleJoinBtnClick);
    }

    private async void HandleJoinBtnClick()
    {
        string joinCode = _joinCodeText.text;
        await ClientSingletone.Instance.GameManager.StartClientWithJoinCode(joinCode);
    }
}
