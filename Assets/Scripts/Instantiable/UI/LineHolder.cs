using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineHolder : MonoBehaviour
{
    private GameObject connectObjectWithLine;
    private LineRenderer lineRenderer;
    private bool hasImported = false;

    public void ConnectWithLine(GameObject newObj)
    {
        connectObjectWithLine = newObj;
    }
    public void SetLineVisibility(bool set)
    {
        if (!hasImported)
        {
            lineRenderer = GetComponent<LineRenderer>();
            hasImported = true;        
        }
        lineRenderer.enabled = set;
        if (set)
        {
            UpdateLine();
        }
    }
    private void Update()
    {
        if (connectObjectWithLine != null)
        {
            UpdateLine();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void UpdateLine()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, connectObjectWithLine.transform.position);
    }
}
