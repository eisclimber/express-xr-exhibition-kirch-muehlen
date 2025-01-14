/*
    Script Name: CoinWeight.cs
    Author: Kevin Koerner
    Refactoring & Integration: Luca Dreiling
    Purpose: Assigns a binary "weight" (fake/real) to a GameObject.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExPresSXR.Minigames.CoinScale
{
    public class CoinWeight : MonoBehaviour
    {
        /// <summary>
        /// Defines whether the coin is fake or real.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines whether the coin is fake or real.")]
        private bool _isFake;
        public bool isFake
        {
            get => _isFake;
            set => _isFake = value;
        }
    }
}