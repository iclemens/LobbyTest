﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;



public class NetworkUIController : MonoBehaviour 
{
    public WatchedNetworkManager networkManager;
    public WatchedNetworkDiscovery networkDiscovery;

    // Shared objects
    [Header("Common")]
    public Toggle isServer;
    public Transform clientPanel;
    public Transform serverPanel;

    // Client panel
    [Header("Client")]
    public ListBox serverList;
    public Button connectButton;

    // Server panel
    [Header("Server")]
    public InputField experimentName;
    public ListBox clientList;
    public Button startButton;


    void Reinitialize()
    {
        //Debug.Log("Reinitialize");

        Debug.Log(networkDiscovery.hostId);
        if(networkDiscovery.hostId != -1 || networkDiscovery.running) {
            //Debug.Log("Stopping broadcast...");
            networkDiscovery.StopBroadcast();
            //Debug.Log("HostId: " + networkDiscovery.hostId.ToString());
        }

        if(isServer.isOn) {
            if(experimentName.text == "")
                networkDiscovery.broadcastData = "Untitled experiment";
            else
                networkDiscovery.broadcastData = experimentName.text;

            //Debug.Log("HostId: " + networkDiscovery.hostId);
            networkDiscovery.StartAsServer();

            networkManager.StartHost();
        } else {
            networkDiscovery.broadcastData = "Client";
            networkDiscovery.StartAsClient();
        }

        //Debug.Log("Finished, id: " + networkDiscovery.hostId.ToString());
    }


    void ShowPanel()
    {
        serverPanel.gameObject.SetActive(isServer.isOn);
        clientPanel.gameObject.SetActive(!isServer.isOn);           
    }


    void RefreshClientList()
    {
        clientList.items.Clear();
        foreach(var _conn in networkManager.connections) {
            if(_conn.address == "localServer") continue;

            if(_conn.address == "localClient")
                clientList.items.Add("localClient", "Local client");
            else
                clientList.items.Add(_conn.address, _conn.address);
        }
    }


	void Start () {
        #if UNITY_EDITOR
            isServer.isOn = false;
        #else
            isServer.isOn = true;
        #endif

        networkManager.ServerConnect += (NetworkConnection conn) => RefreshClientList();
        networkManager.ServerDisconnect += (NetworkConnection conn) => RefreshClientList();

        connectButton.onClick.AddListener(() => {
            Debug.Log("Click..." + serverList.selectedItem);
            if(networkManager.isNetworkActive)
                networkManager.StopClient();

            if(serverList.selectedItem != "") {
                Debug.Log("Connecting to: " + serverList.selectedItem);
                networkManager.networkAddress = serverList.selectedItem;
                networkManager.StartClient();
            }
        });

        networkDiscovery.OnNewServer += (sender, fromAddress, data) => {
            serverList.items.Clear();
            serverList.items.Add(
                fromAddress,
                data);
        };

        networkDiscovery.useNetworkManager = false;
        networkDiscovery.Initialize();

        ShowPanel();
        Reinitialize();

        isServer.onValueChanged.AddListener((v) => { ShowPanel(); Reinitialize(); });
        experimentName.onValueChanged.AddListener((v) => Reinitialize());
	}
    	
}
