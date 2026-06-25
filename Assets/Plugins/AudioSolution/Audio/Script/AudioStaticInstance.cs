using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioStaticInstance : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private AudioClipSO reference;
    [SerializeField]
    private AudioSource source;

    [HideInInspector] public AudioSettingSO setting;

    [HideInInspector] public TransformSO listener;
    

    private bool is3D;
    private bool isStatic;
    private float transitionDist;
    private float minSpatial;
    private float maxSpatial;
    void Start()
    {
        source.clip = reference.clips[Random.Range(0, reference.clips.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        if(is3D && !isStatic)
            CalculateSpatialBlend();
    }
    private void CalculateSpatialBlend()
    {
        Vector3 pos = listener.transform == null ? Vector3.zero : listener.transform.position;
        float dist = Vector3.Distance(pos, transform.position);
        if (dist <= source.minDistance)
            source.spatialBlend = minSpatial;
        else if (dist >= transitionDist)
            source.spatialBlend = maxSpatial;
        else
        {
            float distRatio = (dist - source.minDistance) / (transitionDist - source.maxDistance);
            float spatial = (maxSpatial - minSpatial) * distRatio + minSpatial;
            source.spatialBlend = spatial;
        }
    }
    public void Set3DValues(float maxDist, float minDist, float transitionPoint, float maxSpatial, float minSpatial, AudioRolloffMode rolloff)
    {
        is3D = true;
        isStatic = false;
        source.maxDistance = maxDist;
        source.minDistance = minDist;
        source.rolloffMode = rolloff;
        transitionDist = transitionPoint;
        this.minSpatial = minSpatial;
        this.maxSpatial = maxSpatial;
    }
    public void Set3DValues(float maxDist, float minDist, float spatial, AudioRolloffMode rolloff)
    {
        is3D = true;
        isStatic = true;
        source.maxDistance = maxDist;
        source.minDistance = minDist;
        source.rolloffMode = rolloff;
        source.spatialBlend = spatial;
    }

#if UNITY_EDITOR
    public void SetupAudioSource()
    {
        EditorUtility.SetDirty(source);
        source.loop = true;
        source.outputAudioMixerGroup = reference.mixGroup;
        if (reference.isStaticSpatial)
            Set3DValues(reference.maxDistance, reference.minDistance, reference.staticSpatial, reference.rolloffMode);
        else
            Set3DValues(reference.maxDistance, reference.minDistance, reference.transitionPoint, reference.maxSpatialBlend, reference.minSpatialBlend, reference.rolloffMode);
    }
#endif
}
