using DG.Tweening;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


public class LobbyPanel : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private LobbyUI _lobbyUIPrefab;
    [SerializeField] private float _spacing = 30f;
    [SerializeField] private Button _closeBtn;

    private List<LobbyUI> _lobbyRectlist;

    private RectTransform _rectTrm;
    private CanvasGroup _canvasGroup;

    private bool _isRefreshing;

    private void Awake()
    {
        _lobbyRectlist = new List<LobbyUI>();
        _rectTrm = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        _closeBtn.onClick.AddListener(Close);
    }

    private void Start()
    {
        float screenHeight = Screen.height;
        _rectTrm.anchoredPosition = new Vector2(0, screenHeight);
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
    }

    public void Open()
    {
        Debug.Log("Open");
        Sequence seq = DOTween.Sequence();
        seq.Append(_rectTrm.DOAnchorPos(new Vector2(0, 0), 0.8f));
        seq.Join(_canvasGroup.DOFade(1f, 0.8f));
        seq.AppendCallback(() =>
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        });
        RefreshList();
    }

    public async void RefreshList()
    {
        Debug.Log("Refresh");
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25; 
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field:QueryFilter.FieldOptions.AvailableSlots ,
                    op: QueryFilter.OpOptions.GT,
                    value:"0"),
                new QueryFilter(
                    field:QueryFilter.FieldOptions.IsLocked ,
                    op: QueryFilter.OpOptions.EQ,
                    value:"0"),
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

 
            ClearLobbies();

            foreach (Lobby lobby in lobbies.Results)
            {
                CreateLobbyUI(lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            throw;
        }

        _isRefreshing = false;
    }


    private void ClearLobbies()
    {
        foreach (LobbyUI ui in _lobbyRectlist)
        {
            Destroy(ui.gameObject);
        }

        _lobbyRectlist.Clear();
    }

    public void Close()
    {
        Debug.Log("Close");
        float screenHeight = Screen.height;
        Sequence seq = DOTween.Sequence();
        seq.Append(_rectTrm.DOAnchorPos(new Vector2(0, screenHeight), 0.8f));
        seq.Join(_canvasGroup.DOFade(0f, 0.8f));
        seq.AppendCallback(() =>
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        });
    }

    public void CreateLobbyUI(Lobby lobby)
    {
        LobbyUI ui = Instantiate(_lobbyUIPrefab, _scrollRect.content);

        ui.SetRoomTemplate(lobby);

        _lobbyRectlist.Add(ui);
        float offset = _spacing;

        for (int i = 0; i < _lobbyRectlist.Count; i++)
        {
            _lobbyRectlist[i].Rect.anchoredPosition = new Vector2(0, -offset);
            offset += _lobbyRectlist[i].Rect.sizeDelta.y + _spacing;
        }

        Vector2 contentSize = _scrollRect.content.sizeDelta;
        contentSize.y = offset;
        _scrollRect.content.sizeDelta = contentSize;
    }

}
