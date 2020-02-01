using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlotManager : MonoBehaviour
{
    public static PlotManager Instance;

    private List<Plot> triggeredPlotPoints = new List<Plot>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public bool CheckPlot(Plot plot)
    {
        foreach (var triggered in triggeredPlotPoints)
            if (triggered == plot)
                return true;

        return false;
    }

    public void TriggerPlot(Plot plot)
    {
        triggeredPlotPoints.Add(plot);
    }

}
