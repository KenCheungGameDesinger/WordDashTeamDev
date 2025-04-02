using Assets;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Multiplay;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies.Models;

public class EntrySceneManager : MonoBehaviour
{

    static public EntrySceneManager Singleton { get; internal set; }

    public GameObject canvasObj;
    
    public GameObject loadingObj;

    Dictionary<string, GameObject> screenObjs = new Dictionary<string, GameObject> ();
    
    public string currentScreenName = "";
    
    public UnityTransport _transport;
    
    Lobby _connectedLobby;
    QueryResponse _lobbies;
    string JoinCodeKey = "j";
    private string _playerId;

    private async void Awake()
    {
        if(Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        GameSettings.playername = PlayerPrefs.GetString("PlayerName", "");
        MyNetwork.Singleton.SetPlayerName(GameSettings.playername);
        PlayerController.players.Clear();
        
        if(GlobalData.Singleton.email == "")
            await Authenticate();
    }

    // Start is called before the first frame update
    void Start()
    {
        var cnt = canvasObj.transform.childCount;
        for (var i = 0; i < cnt; i++) {
            var obj = canvasObj.transform.GetChild(i).gameObject;
            if (obj.name.EndsWith("Screen"))
            {
                screenObjs.Add(obj.name, obj);
                obj.SetActive(false);
            }
        }
        
        if(GlobalData.Singleton.email != "")
        {
            SetScreen("OpenScreen");
        }
        else
        {
            SetScreen("LoginScreen");
        }
        
        MyNetwork.Singleton.playerDatas.Clear();
        // MyNetwork.Singleton.playerNames.Clear();
        // MyNetwork.Singleton.playerTeams.Clear();
        NetworkManager.Singleton.Shutdown();
    }
    
    public void OnGetMysteryWordsComplete(bool isFail, List<object> param)
    {
        GlobalData.Singleton.mysteryWords.Clear();
        for(int i = 0; i < param.Count; i++)
        {
            MysteryWord word = new MysteryWord();
            word.SetWord(param[i] as Dictionary<string, object>);
            GlobalData.Singleton.mysteryWords.Add(word);
        }
    }

    public async void Search()
    {
        // await Authenticate();
        var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

        lobby.Data.ToList().ForEach(i => Debug.Log("ClassID : " + i.Key));
    }

    public async void CreateOrJoinLobby()
    {
        _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
    }

    private async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
    }

    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            if(GameSettings.lobbyId != "")
                await Lobbies.Instance.RemovePlayerAsync(GameSettings.lobbyId, AuthenticationService.Instance.PlayerId);
            
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            
            // lobby.Data.ToList().ForEach(i => Debug.LogError("ClassID : " + i.Key));
            
            GameSettings.lobbyId = lobby.Id;

            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[GlobalData.Singleton.classId].Value);

            SetTransformAsClient(a);
            
            NetworkManager.Singleton.StartClient();

            return lobby;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    private async Task<Lobby> CreateLobby()
    {
        try
        {
            const int maxPlayers = 8;

            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { GlobalData.Singleton.classId, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };

            var lobby = await Lobbies.Instance.CreateLobbyAsync("Useless Lobby Name", maxPlayers, options);

            GameSettings.lobbyId = lobby.Id;
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            
            _transport = GlobalData.Singleton.GetComponent<UnityTransport>();

            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
            
            NetworkManager.Singleton.StartHost();

            return lobby;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    IEnumerator StartHostInSec()
    {
        yield return new WaitForSeconds(1f);
        NetworkManager.Singleton.StartHost();
        loadingObj.SetActive(false);
    }

    private void SetTransformAsClient(JoinAllocation a)
    {    
        _transport = GlobalData.Singleton.GetComponent<UnityTransport>();
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnClickBackButton();
        if (Input.GetKeyDown(KeyCode.H))
            NetworkManager.Singleton.StartHost();
        if (Input.GetKeyDown(KeyCode.C))
            NetworkManager.Singleton.StartClient();
    }

    public void SetScreen(string screenName)
    {
        if(screenName == "OpenScreen")
        {
            StartCoroutine(WebServices.GetMysteryWords(OnGetMysteryWordsComplete));
        }
        
        if (!screenObjs.ContainsKey(screenName))
            return;

        if (screenObjs.ContainsKey(currentScreenName))
            screenObjs[currentScreenName].SetActive(false);

        if (screenObjs.ContainsKey(screenName))
            screenObjs[screenName].SetActive(true);

        currentScreenName = screenName;
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnClickBackButton()
    {
        switch (currentScreenName)
        {
            case "OpenScreen":
                SetScreen("LoginScreen");
                break;
            case "ExitScreen":
                SetScreen("LoginScreen");
                break;
            case "LoginScreen":
                SetScreen("ExitScreen");
                break;
            case "SignupScreen":
                SetScreen("LoginScreen");
                break;
            case "SettingsScreen":
                SetScreen("OpenScreen");
                break;
            case "ModeScreen":
                SetScreen("OpenScreen");
                break;
            case "MultiPlayerScreen":
                SetScreen("ModeScreen");
                break;
            case "ChooseLocationScreen":
                if (GameSettings.gameMode == EGameMode.SINGLE)
                    SetScreen("ModeScreen");
                else
                    SetScreen("MultiPlayerScreen");
                break;
            case "SinglePlayerScreen":
                SetScreen("ModeScreen");
                break;
            case "CompetitiveScreen":
                //SetScreen("MultiPlayerScreen");
                if(NetworkManager.Singleton.IsHost)
                {
                    MyNetwork.Singleton.playerNames.Clear();
                    MyNetwork.Singleton.playerTeams.Clear();
                }
                NetworkManager.Singleton.Shutdown();
                OnDestroy();
                SetScreen("OpenScreen");
                break;
            case "CooperativeScreen":
                //SetScreen("MultiPlayerScreen");
                if(NetworkManager.Singleton.IsHost)
                {
                    MyNetwork.Singleton.playerNames.Clear();
                    MyNetwork.Singleton.playerTeams.Clear();
                }
                NetworkManager.Singleton.Shutdown();
                OnDestroy();
                SetScreen("OpenScreen");
                break;
            case "WaitServerScreen":
                SetScreen("OpenScreen");
                break;
            case "ChooseCharacterScreen":
                //SetScreen("OpenScreen");
                NetworkManager.Singleton.Shutdown();
                OnDestroy();
                SetScreen("ModeScreen");
                break;
        }
    }
    
    public void OnClickSignupButton()
    {
        SetScreen("SignupScreen");
    }


    //OpenScreen
    public void OnClickStartButton()
    {
        SetScreen("ModeScreen");
        MyNetwork.Singleton.playerDatas.Clear();
        // MyNetwork.Singleton.playerNames.Clear();
        // MyNetwork.Singleton.playerTeams.Clear();
        NetworkManager.Singleton.Shutdown();
    }

    public async void OnClickTutorialButton()
    {
        SetScreen("WaitServerScreen");
        
        // _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
        Lobby lobby = await QuickJoinLobby();
            
        // Debug.LogError(lobby);
        if(lobby == null)
            SetScreen("OpenScreen");
        
    }
    
    private void OnDestroy()
    {
        StopAllCoroutines();
        try
        {
            StopAllCoroutines();
            if (_connectedLobby != null)
            {
                if (_connectedLobby.HostId == _playerId) Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                else Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
            }
        }
        catch (Exception e)
        {

        }
    }

    //ChooseLocationScreen
    public async void OnClickCreateGameButton()
    {
        if (GameSettings.gameMode == EGameMode.SINGLE)
            SetScreen("SinglePlayerScreen");
        else if (GameSettings.gameMode == EGameMode.COMPETITIVE)
            SetScreen("CompetitiveScreen");
        else if (GameSettings.gameMode == EGameMode.COOPERATIVE)
            SetScreen("CooperativeScreen");

        if (GameSettings.gameMode != EGameMode.SINGLE)
        {
            loadingObj.SetActive(true);
            // await CreateLobby();
            await CreateLobby();
            // StartCoroutine(StartHostInSec());
            loadingObj.SetActive(false);
        }
    }
    
    public void OnClickSettingsButton()
    {
        SetScreen("SettingsScreen");
    }

    //ModeScreen
    public void OnClickSinglePlayerButton()
    {
        GameSettings.gameMode = EGameMode.SINGLE;
        SetScreen("ChooseLocationScreen");
    }

    public void OnClickMultiPlayerButton()
    {
        SetScreen("MultiPlayerScreen");
    }

    //MultiPlayerScreen
    public void OnClickCompetitiveButton()
    {
        GameSettings.gameMode = EGameMode.COMPETITIVE;
        SetScreen("ChooseLocationScreen");
    }

    public void OnClickCooperativeButton()
    {
        GameSettings.gameMode = EGameMode.COOPERATIVE;
        SetScreen("ChooseLocationScreen");
    }

    //SinglePlayerScreen, CompetitiveScreen, CooperativeScreen
    public void OnClickStartGameButton()
    {
        // Debug.LogError(NetworkManager.Singleton.IsConnectedClient + " : " + NetworkManager.Singleton.IsApproved);
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            MyNetwork.Singleton.totalCount = SinglePlayerScreenManager.singleton.playerCount;
            SetScreen("ChooseCharacterScreen");
        }
        else
        {
            if(NetworkManager.Singleton.IsHost)
                MyNetwork.Singleton.GoChooseCharacterScreenClientRpc(NetworkManager.Singleton.ConnectedClients.Count);
        }
    }

    //ChooseCharacterScreen
    public void OnClickReadyButton()
    {
        if (!NetworkManager.Singleton.IsApproved)
        {
            SceneManager.LoadScene("GameScene" + GameSettings.locationId);
        }
        else
            MyNetwork.Singleton.SetReadyServerRpc(ChooseCharacterScreenManager.Singleton.selectedItem.id);
    }

}
