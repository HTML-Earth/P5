using System;
using UnityEngine;

public class DriftCamera : MonoBehaviour
{
    [Serializable]
    public class AdvancedOptions
    {
        public bool updateCameraInUpdate;
        public bool updateCameraInFixedUpdate = true;
        public bool updateCameraInLateUpdate;
        public KeyCode switchViewKey = KeyCode.Space;
    }

    public float smoothing = 6f;
    public Transform lookAtTarget;
    public Transform positionTarget;
    public Transform sideView;
    public AdvancedOptions advancedOptions;

    bool m_ShowingSideView;

    void FixedUpdate ()
    {
        if(advancedOptions.updateCameraInFixedUpdate)
            UpdateCamera ();
    }

    void Update ()
    {
        if (Input.GetKeyDown (advancedOptions.switchViewKey))
            m_ShowingSideView = !m_ShowingSideView;

        if(advancedOptions.updateCameraInUpdate)
            UpdateCamera ();
    }

    void LateUpdate ()
    {
        if(advancedOptions.updateCameraInLateUpdate)
            UpdateCamera ();
    }

    void UpdateCamera ()
    {
        if (m_ShowingSideView)
        {
            transform.position = sideView.position;
            transform.rotation = sideView.rotation;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, positionTarget.position, Time.deltaTime * smoothing);
            transform.LookAt(lookAtTarget);
        }
    }
}
