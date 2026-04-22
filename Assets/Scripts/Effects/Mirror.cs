using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class Mirror : MonoBehaviour
{
    [SerializeField] private LayerMask _cullMask = ~0;
    [SerializeField] private Vector2 _scale;
    [SerializeField] private float _farClip = 1000;
    [SerializeField] private float _quality = 1;
    [SerializeField] private int _mirrorID;

    private static Dictionary<int, Mirror> _activeMirrors = new Dictionary<int, Mirror>();
    private bool _wasAdded;
    private RenderTexture _rt;
    private Material _mat;
    private Camera _probe;

    private void Start()
    {
        if (_probe == null)
            CreateProbe();
        CreateRenderTexture(Camera.main);

        _mat = new Material(Shader.Find("Unlit/Texture"));
        _mat.mainTexture = _rt;
    }

    private void OnEnable()
    {
        if (_activeMirrors.ContainsKey(_mirrorID))
        {
            Debug.LogWarning("KEY FOR MIRROR ALREADY CONTAINED " + _mirrorID + " CURRENT OBJECT - " + gameObject.name + " OTHER MIRROR" + _activeMirrors[_mirrorID]);
        }
        else
        {
            _activeMirrors.Add(_mirrorID, this);
            _wasAdded = true;
        }

        RenderPipelineManager.beginCameraRendering += PreRender;
    }

    private void OnDisable()
    {
        if (_wasAdded)
        {
            _activeMirrors.Remove(_mirrorID);
        }

        RemoveProbeAndPlane();
        RenderPipelineManager.beginCameraRendering -= PreRender;
    }

    private void PreRender(ScriptableRenderContext src, Camera cam)
    {
        if (cam.cameraType == CameraType.Reflection) return;
        if (_probe == null) CreateProbe();

        Vector3 normal = transform.forward;
        UpdateProbeSettings(cam);
        CreateRenderTexture(cam);
        UpdateProbeTransform(cam, normal);
        CalculateObliqueProjection(normal);
        var request = new UniversalRenderPipeline.SingleCameraRequest()
        {
            destination = _rt
        };

        RenderPipeline.SubmitRenderRequest<UniversalRenderPipeline.SingleCameraRequest>(_probe, request);
        if (_wasAdded)
        {
            Shader.SetGlobalTexture("_MirrorTex" + _mirrorID, _rt);
        }
    }

    private void CreateProbe()
    {
        var go = new GameObject();
        _probe = go.AddComponent<Camera>();
        go.transform.SetParent(transform);
    }

    private void UpdateProbeSettings(Camera cam)
    {
        _probe.CopyFrom(cam);
        _probe.cameraType = CameraType.Reflection;
        _probe.usePhysicalProperties = false;
        _probe.farClipPlane = _farClip;
        _probe.cullingMask = _cullMask;
        _probe.usePhysicalProperties = false;
        _probe.enabled = false;
    }

    private void CreateRenderTexture(Camera cam)
    {
        int width = (int)((float)cam.pixelWidth * _quality);
        int height = (int)((float)cam.pixelHeight * _quality);
        RenderTexture texture = _rt;
        if (!texture || texture.width != width || texture.height != height)
        {
            if (texture)
            {
                texture.Release();
            }
            _probe.targetTexture = new RenderTexture(width, height, 24);
            _probe.targetTexture.Create();
            _rt = _probe.targetTexture;
            if(_mat)
                _mat.mainTexture = _rt;
        }
        else
        {
            _probe.targetTexture = texture;
        }
    }

    private void UpdateProbeTransform(Camera cam, Vector3 normal)
    {
        Vector3 proj = normal * Vector3.Dot(
            normal, cam.transform.position - transform.position);
        _probe.transform.position = cam.transform.position - 2 * proj;

        Vector3 probeForward = Vector3.Reflect(cam.transform.forward, normal);
        Vector3 probeUp = Vector3.Reflect(cam.transform.up, normal);
        _probe.transform.LookAt(_probe.transform.position + probeForward, probeUp);
    }

    private void CalculateObliqueProjection(Vector3 normal)
    {
        Matrix4x4 viewMatrix = _probe.worldToCameraMatrix;
        Vector3 viewPosition = viewMatrix.MultiplyPoint(transform.position);
        Vector3 viewNormal = viewMatrix.MultiplyVector(normal);
        Vector4 plane = new Vector4(
            viewNormal.x, viewNormal.y, viewNormal.z,
            -Vector3.Dot(viewPosition, viewNormal));
        _probe.projectionMatrix = _probe.CalculateObliqueMatrix(plane);
    }

    private void RemoveProbeAndPlane()
    {
        if(_probe != null)
        {
            DestroyImmediate(_probe.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, _scale);
    }
}