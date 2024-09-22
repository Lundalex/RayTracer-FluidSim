using System.Collections;
using System.Collections.Generic;
using Unity.RenderStreaming;
using UnityEngine;

public class StreamSetter : MonoBehaviour
{
    public Broadcast broadcast;
    public GameObject fullScreenCamera;
    public GameObject smallCamera;
    public new GameObject audio;
    void Awake()
    {
        // Order is important
        if (fullScreenCamera != null) broadcast.AddComponent(fullScreenCamera.GetComponent<CameraStreamSender>());
        if (smallCamera != null) broadcast.AddComponent(smallCamera.GetComponent<CameraStreamSender>());
        if (audio != null) broadcast.AddComponent(audio.GetComponent<AudioStreamSender>());
    }
}
