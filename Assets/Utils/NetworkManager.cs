using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace TFG {

    public class NetworkManager : MonoBehaviour {

        public GameObject playerPrefab;

        private const string typeName = "TFG_Borja";
        private const string gameName = "RoomName";

        private HostData[] hostList;

        private void StartServer() {
            Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
            MasterServer.RegisterHost(typeName, gameName);
        }

        void OnServerInitialized() {
            Debug.Log("Server Initializied");
            SpawnPlayer();
        }

        void OnGUI() {
            if (!Network.isClient && !Network.isServer) {
                if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
                    StartServer();

                if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
                    RefreshHostList();

                if (hostList != null) {
                    for (int i = 0; i < hostList.Length; i++) {
                        if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
                            JoinServer(hostList[i]);
                    }
                }
            }
        }

        private void RefreshHostList() {
            MasterServer.RequestHostList(typeName);
        }

        void OnMasterServerEvent(MasterServerEvent msEvent) {
            if (msEvent == MasterServerEvent.HostListReceived)
                hostList = MasterServer.PollHostList();
        }

        private void JoinServer(HostData hostData) {
            Network.Connect(hostData);
        }

        void OnConnectedToServer() {
            Debug.Log("Server Joined");
            SpawnPlayer();
        }

        void SpawnPlayer() {
            Network.Instantiate(playerPrefab, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
            GameManager gameManager = GetComponent<GameManager>();
            if (gameManager) {
                gameManager.AddAllScenePlayers();
                gameManager.AssignCameraToPlayer();
            }
        }

    }
}