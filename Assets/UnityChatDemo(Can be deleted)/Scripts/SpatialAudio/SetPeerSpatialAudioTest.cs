using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPeerSpatialAudioTest : MonoBehaviour
{
    public int TestPeerID;
    public Vector3 PeerPosition;
    int dir=1;
    void Start()
    {
        PeerPosition = new Vector3(0,0,1);
    }


    void FixedUpdate() 
    {
        if (PeerPosition.x < -2) dir = 1;
        if (PeerPosition.x > 2) dir = -1;

        PeerPosition.x += 0.01f* dir;

        transform.position = PeerPosition;

        AudioSource audioSource = UnityChatSDK.Instance.SetPeerSpatialAudio(TestPeerID, true, PeerPosition.x, PeerPosition.y, PeerPosition.z);
        if (audioSource != null)
        {
            //do some audioSource settings...
        }
    }
}
