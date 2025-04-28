#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

[CustomEditor(typeof(Difficulty))]
public class DifficultyEditor : OdinEditor
{
    private int   maxLayer         = 45;
    private int   tickInterval     = 5;
    private bool  useLogScale      = false;
    private bool  showReferenceCurve = true;

    public override void OnInspectorGUI()
    {
        // Draw the standard Odin inspector
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Round Goal Preview", EditorStyles.boldLabel);

        // Controls
        maxLayer           = EditorGUILayout.IntField("Max Layer",         maxLayer);
        tickInterval       = EditorGUILayout.IntField("Tick Interval",     tickInterval);
        useLogScale        = EditorGUILayout.Toggle("Log Y Scale",       useLogScale);
        showReferenceCurve = EditorGUILayout.Toggle("Show Reference Curve", showReferenceCurve);

        maxLayer     = Mathf.Max(1, maxLayer);
        tickInterval = Mathf.Clamp(tickInterval, 1, maxLayer);

        // Reserve space for graph
        Rect graphRect = GUILayoutUtility.GetRect(10, 200, 150, 450);
        DrawGraph(graphRect, useLogScale, showReferenceCurve);
    }

    private void DrawGraph(Rect rect, bool logScale, bool showRef)
    {
        var diff = (Difficulty)target;
        var mainList = diff.functionPieces;
        var refList  = diff.referenceFunctionPieces;

        // Nothing to draw?
        if ((mainList == null || mainList.Count == 0) && (!showRef || refList == null || refList.Count == 0))
        {
            EditorGUI.LabelField(rect, "No function pieces defined");
            return;
        }

        // 1) Sample both curves
        List<double> mainRaw = new List<double>(maxLayer + 1);
        List<double> refRaw  = new List<double>(maxLayer + 1);
        for (int i = 0; i <= maxLayer; i++)
        {
            mainRaw.Add(diff.GetRoundGoal(i));
            refRaw.Add(diff.GetReferenceRoundGoal(i));
        }

        // 2) Determine combined min/max for scaling
        double rawMin = double.MaxValue, rawMax = double.MinValue;
        foreach (var v in mainRaw) { rawMin = Math.Min(rawMin, v); rawMax = Math.Max(rawMax, v); }
        if (showRef)
        {
            foreach (var v in refRaw) { rawMin = Math.Min(rawMin, v); rawMax = Math.Max(rawMax, v); }
        }
        if (rawMin == rawMax) { rawMin -= 1; rawMax += 1; }

        // 3) Convert to plot values (log or linear)
        List<double> mainPlot = new List<double>(mainRaw.Count);
        List<double> refPlot  = new List<double>(refRaw.Count);

        if (logScale)
        {
            for (int i = 0; i < mainRaw.Count; i++)
            {
                mainPlot.Add(Math.Log10(Math.Max(1e-6, mainRaw[i])));
                refPlot .Add(Math.Log10(Math.Max(1e-6, refRaw[i])));
            }
            // recompute min/max on log scale
            double pMin = double.MaxValue, pMax = double.MinValue;
            foreach (var v in mainPlot) { pMin = Math.Min(pMin, v); pMax = Math.Max(pMax, v); }
            if (showRef)
                foreach (var v in refPlot) { pMin = Math.Min(pMin, v); pMax = Math.Max(pMax, v); }
            rawMin = pMin; rawMax = pMax;
        }
        else
        {
            mainPlot.AddRange(mainRaw);
            refPlot .AddRange(refRaw);
        }

        // 4) Draw background + axes
        EditorGUI.DrawRect(rect, Color.black);
        float margin = 20f;
        Rect graphArea = new Rect(
            rect.x + margin,
            rect.y + margin,
            rect.width  - 2 * margin,
            rect.height - 2 * margin
        );

        Handles.BeginGUI();
        Handles.color = Color.white;
        // Y axis
        Handles.DrawLine(new Vector3(graphArea.x,      graphArea.y),
                         new Vector3(graphArea.x,      graphArea.yMax));
        // X axis
        Handles.DrawLine(new Vector3(graphArea.x,      graphArea.yMax),
                         new Vector3(graphArea.xMax,   graphArea.yMax));

        // 5) Draw X ticks, labels, and main curve value at ticks
        for (int x = 0; x <= maxLayer; x += tickInterval)
        {
            float tx = (float)x / maxLayer;
            float px = Mathf.Lerp(graphArea.x, graphArea.xMax, tx);
            // tick
            Handles.DrawLine(new Vector3(px, graphArea.yMax),
                             new Vector3(px, graphArea.yMax + 4f));
            GUI.Label(new Rect(px + 2f, graphArea.yMax + 2f, 30f, 16f),
                      x.ToString(), EditorStyles.miniLabel);

            // main value at tick
            double vMain = mainRaw[x];
            double plotV = logScale ? Math.Log10(Math.Max(1e-6, vMain)) : vMain;
            float tyNorm = (float)((plotV - rawMin) / (rawMax - rawMin));
            float py     = Mathf.Lerp(graphArea.yMax, graphArea.y, tyNorm);
            GUI.Label(new Rect(px + 2f, py - 14f, 50f, 16f),
                      vMain.ToString("F0"), EditorStyles.miniLabel);
        }

        // Y min/max
        Handles.DrawLine(new Vector3(graphArea.x - 4f, graphArea.y),
                         new Vector3(graphArea.x,      graphArea.y));
        GUI.Label(new Rect(graphArea.x - margin, graphArea.y - 8f, margin, 16f),
                  rawMax.ToString(logScale ? "F2" : "F0"), EditorStyles.miniLabel);

        Handles.DrawLine(new Vector3(graphArea.x - 4f, graphArea.yMax),
                         new Vector3(graphArea.x,      graphArea.yMax));
        GUI.Label(new Rect(graphArea.x - margin, graphArea.yMax - 8f, margin, 16f),
                  rawMin.ToString(logScale ? "F2" : "F0"), EditorStyles.miniLabel);

        // 6) Plot main curve in green
        Handles.color = Color.green;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i < mainPlot.Count; i++)
        {
            float tx    = (float)i / maxLayer;
            float px    = Mathf.Lerp(graphArea.x, graphArea.xMax, tx);
            float tyNorm= (float)((mainPlot[i] - rawMin) / (rawMax - rawMin));
            float py    = Mathf.Lerp(graphArea.yMax, graphArea.y, tyNorm);

            if (i > 0)
                Handles.DrawLine(prev, new Vector3(px, py));
            prev = new Vector3(px, py);
        }

        // 7) Plot reference curve in red (if toggled)
        if (showRef)
        {
            Handles.color = Color.red;
            prev = Vector3.zero;
            for (int i = 0; i < refPlot.Count; i++)
            {
                float tx    = (float)i / maxLayer;
                float px    = Mathf.Lerp(graphArea.x, graphArea.xMax, tx);
                float tyNorm= (float)((refPlot[i] - rawMin) / (rawMax - rawMin));
                float py    = Mathf.Lerp(graphArea.yMax, graphArea.y, tyNorm);

                if (i > 0)
                    Handles.DrawLine(prev, new Vector3(px, py));
                prev = new Vector3(px, py);
            }
        }

        Handles.EndGUI();
    }
}
#endif
