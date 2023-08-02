using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Camera ) )]
public class MultipeTargetCamera : MonoBehaviour
{
    public Camera cam;
    public List<Transform> targets;

    public Vector3 offset;
    public float smoothTime = 0.5f;

    public float minZoom = 40;
    public float maxZoom = 10;
    public float zoomLimiter = 50;

    private Vector3 velocity;

    private void Reset()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if ( targets.Count == 0 ) return;

        Move();
        Zoom();
    }

    private void Zoom()
    {
        var newZoom = Mathf.Lerp( maxZoom, minZoom, GetGreatestDistance() / zoomLimiter );
        cam.fieldOfView = Mathf.Lerp( cam.fieldOfView, newZoom, 1f );
    }

    private void Move()
    {
        var centerPoint = GetCenterPoint();
        var newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp( transform.position, newPosition, ref velocity, smoothTime );
    }

    private float GetGreatestDistance()
    {
        var bounds = new Bounds( targets[0].position, Vector3.zero );
        for ( int i = 0; i < targets.Count; i++ )
        {
            bounds.Encapsulate( targets[i].position );
        }
        return bounds.size.x;
    }

    private Vector3 GetCenterPoint()
    {
        if ( targets.Count == 1 ) return targets[0].position;
        var bounds = new Bounds( targets[0].position, Vector3.zero );
        for ( int i = 0; i < targets.Count; i++ )
        {
            bounds.Encapsulate( targets[i].position );
        }
        return bounds.center;
    }

    /*//ここから画面揺れ処理
    public void Shake( float duration, float magnitude )
    {
        StartCoroutine( DoShake( duration, magnitude ) );
    }

    private IEnumerator DoShake( float duration, float magnitude )
    {
        var pos = transform.localPosition;

        var elapsed = 0f;

        while ( elapsed < duration )
        {
            var x = pos.x + Random.Range( -1f, 1f ) * magnitude;
            var y = pos.y + Random.Range( -1f, 1f ) * magnitude;

            transform.localPosition = new Vector3( x, y, pos.z );

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = pos;
    }*/
}
