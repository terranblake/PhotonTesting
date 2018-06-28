using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoped : MonoBehaviour
{
    public float scopeFOV = 15f;
    public Camera scopeCamera;
    public Material renderMaterial;
    public GameObject renderSurface;
    public KeyCode zoomIn;
    public KeyCode zoomOut;

    private RenderTexture cameraRender;

    void Awake()
    {
        if (scopeCamera != null)
            scopeFOV = scopeCamera.fieldOfView;
        else
        {
            Debug.LogError("scopeCamera Camera is not attached.");
            return;
        }
    }

    void Start()
    {
        SetRenderTexture();
        SetRenderSurface();
    }

    // Update is called once per frame
    void Update()
    {
        ScopeFOV();
    }

    void SetRenderTexture()
    {
        cameraRender = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
        scopeCamera.targetTexture = cameraRender;
    }

    void SetRenderSurface()
    {
        Renderer surfaceRenderer = renderSurface.GetComponent<Renderer>();
        if (surfaceRenderer != null)
            surfaceRenderer.material = renderMaterial;

        surfaceRenderer.material.mainTexture = cameraRender;
    }

    void ScopeFOV()
    {
        if (Input.GetKeyDown(zoomIn))
            scopeFOV += 0.5f;
        else if (Input.GetKeyDown(zoomOut))
            scopeFOV -= 0.5f;

        if (scopeCamera.fieldOfView != scopeFOV)
            scopeCamera.fieldOfView = Mathf.Lerp(scopeCamera.fieldOfView, scopeFOV, Time.deltaTime * 10);
    }
}
