using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    private const string typeName = "UniqueGameName";
    private const string gameName = "RoomName";

    private bool isRefreshingHostList = false;
    private HostData[] hostList;
    private bool isServerStarted = false;
    private string serverTestMsg = "";
    private string masterServerError = "";
    private string masterServerRegisterMsg = "";
    private GameObject currentPlayer = null;

    public GameObject playerPrefab;

    void OnGUI()
    {
        if (!Network.isClient && !Network.isServer)
        {
            if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
                StartServer();

            if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
                RefreshHostList();

            if (hostList != null)
            {
                for (int i = 0; i < hostList.Length; i++)
                {
                    if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
                        JoinServer(hostList[i]);
                }
            }
        }

        if (isServerStarted)
        {
            GUI.Label(new Rect(10, 10, 300, 50), "Server is initialized successfully.");
            GUI.Label(new Rect(10, 70, 500, 50), serverTestMsg);
            GUI.Label(new Rect(10, 130, 500, 50), masterServerError);
            GUI.Label(new Rect(10, 190, 500, 50), masterServerRegisterMsg);
        }
    }

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        masterServerError = "Could not connect to master server: " + info;
    }

    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.RegistrationSucceeded)
            masterServerRegisterMsg = "Server registered";
        else
            masterServerRegisterMsg = "Server failed to register";

    }

    void OnDestroy()
    {
        Network.Disconnect();
        MasterServer.UnregisterHost();
    }

    IEnumerator NetworkTest()
    {
        while (true)
        {
            var result = Network.TestConnection();

            serverTestMsg = result.ToString();
            if (result != ConnectionTesterStatus.Undetermined)
                break;

            yield return null;
        }
    }

    private void StartServer()
    {
        Network.InitializeServer(5, 25000, !Network.HavePublicAddress());
        MasterServer.RegisterHost(typeName, gameName);
    }

    void OnServerInitialized()
    {
        SpawnPlayer();
        isServerStarted = true;
    }


    void Update()
    {
        if (isRefreshingHostList && MasterServer.PollHostList().Length > 0)
        {
            isRefreshingHostList = false;
            hostList = MasterServer.PollHostList();
        }

        if (currentPlayer == null)
            SpawnPlayer();
    }

    private void RefreshHostList()
    {
        if (!isRefreshingHostList)
        {
            isRefreshingHostList = true;
            MasterServer.RequestHostList(typeName);
        }
    }


    private void JoinServer(HostData hostData)
    {
        Network.Connect(hostData);
    }

    void OnConnectedToServer()
    {
        SpawnPlayer();
    }


    private void SpawnPlayer()
    {
        currentPlayer = Network.Instantiate(playerPrefab, new Vector3(Random.Range(-4.0f, 4.0f), Random.Range(-4.0f, 4.0f)), Quaternion.identity, 0) as GameObject;
    }
}
