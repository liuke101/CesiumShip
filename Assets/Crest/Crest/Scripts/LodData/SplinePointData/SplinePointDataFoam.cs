﻿// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

using Crest.Spline;
using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Foam tweakable param on spline points
    /// </summary>
    [AddComponentMenu("")]
    public class SplinePointDataFoam : SplinePointDataBase
    {
        /// <summary>
        /// The version of this asset. Can be used to migrate across versions. This value should
        /// only be changed when the editor upgrades the version.
        /// </summary>
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 0;
#pragma warning restore 414

        [Tooltip("Amount of foam emitted."), SerializeField]
        [DecoratedField, OnChange(nameof(NotifyOfSplineChange))]
        float _foamAmount = 1f;
        public float FoamAmount { get => _foamAmount; set => _foamAmount = value; }

        public override Vector2 GetData()
        {
            return new Vector2(_foamAmount, 0f);
        }
    }
}
