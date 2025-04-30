using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Scriptable Objects/Difficulty")]
public class Difficulty : ScriptableObject
{
    [Header("General")]
    public string displayName;

    public int difficultyLevel;

    [Header("First Round Goal")]
    [Tooltip("The goal value for round 0 (before any pieces take over).")]
    public double firstRoundGoal = 15;

    [Header("Reference Curve (for comparison)")]
    [Tooltip("The goal for round 0 on the reference curve.")]
    public double referenceFirstRoundGoal = 15;
    [SerializeReference, InlineEditor]
    public List<FunctionPiece> referenceFunctionPieces = new List<FunctionPiece>();

    [Header("MetaCurrency Curve")]
    public float metacurrencyBase  = 1f;
    public float metacurrencyPower = 1.2f;
    public int maxMetaCurrencyPerLayer = 10;

    [Header("Round Goal Pieces")]
    [Tooltip("Define one or more piecewise segments.  Each takes over at its firstLayer.")]
    [SerializeReference, InlineEditor]
    public List<FunctionPiece> functionPieces = new List<FunctionPiece>();

    [Serializable]
    public abstract class FunctionPiece
    {
        [Tooltip("The map layer at which this segment begins.")]
        public int firstLayer;

        /// <summary>
        /// Evaluate this segment at <paramref name="currentLayer"/>, given it started at
        /// <paramref name="startingLayer"/> with value <paramref name="startingY"/>.
        /// </summary>
        public abstract double GetFunctionValue(int startingLayer, double startingY, int currentLayer);
    }

    [Serializable]
    public class PowerFunctionPiece : FunctionPiece
    {
        [Tooltip("Multiplier for the power term.")]
        public double baseValue = 1;
        [Tooltip("Exponent for the power term.")]
        public double exponent  = 1.2;

        public override double GetFunctionValue(int startingLayer, double startingY, int currentLayer)
        {
            double delta = currentLayer - startingLayer;
            return startingY + baseValue * Math.Pow(delta, exponent);
        }
    }

    [Serializable]
    public class ExpFunctionPiece : FunctionPiece
    {
        [Tooltip("Base of the exponential.")]
        public double expBase = 1.05;

        public override double GetFunctionValue(int startingLayer, double startingY, int currentLayer)
        {
            double delta = currentLayer - startingLayer;
            return startingY + Math.Pow(expBase, delta);
        }
    }

    /// <summary>
    /// Piecewise evaluation for your main curve.
    /// </summary>
    public double GetRoundGoal(int mapLayer)
    {
        if (mapLayer <= 0 || functionPieces == null || functionPieces.Count == 0)
            return firstRoundGoal;

        var segments = new List<FunctionPiece>(functionPieces);
        segments.Sort((a, b) => a.firstLayer.CompareTo(b.firstLayer));

        double y = firstRoundGoal;
        for (int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            if (mapLayer < seg.firstLayer)
                break;

            int nextStart = (i + 1 < segments.Count) ? segments[i + 1].firstLayer : mapLayer;
            int evalLayer = Math.Min(mapLayer, nextStart);

            y = seg.GetFunctionValue(seg.firstLayer, y, evalLayer);
            if (evalLayer == mapLayer)
                return y;
        }
        return firstRoundGoal;
    }

    /// <summary>
    /// Piecewise evaluation for the reference curve.
    /// </summary>
    public double GetReferenceRoundGoal(int mapLayer)
    {
        if (mapLayer <= 0 || referenceFunctionPieces == null || referenceFunctionPieces.Count == 0)
            return referenceFirstRoundGoal;

        var segments = new List<FunctionPiece>(referenceFunctionPieces);
        segments.Sort((a, b) => a.firstLayer.CompareTo(b.firstLayer));

        double y = referenceFirstRoundGoal;
        for (int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            if (mapLayer < seg.firstLayer)
                break;

            int nextStart = (i + 1 < segments.Count) ? segments[i + 1].firstLayer : mapLayer;
            int evalLayer = Math.Min(mapLayer, nextStart);

            y = seg.GetFunctionValue(seg.firstLayer, y, evalLayer);
            if (evalLayer == mapLayer)
                return y;
        }
        return referenceFirstRoundGoal;
    }

    public int GetMetaCurrencyForLayer(int layerNum)
    {
        int value = Mathf.CeilToInt(metacurrencyBase * Mathf.Pow(layerNum, metacurrencyPower));
        if (value > maxMetaCurrencyPerLayer)
        {
            value = maxMetaCurrencyPerLayer;
        }

        return value;
    }
}
