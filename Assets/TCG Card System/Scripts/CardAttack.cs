using System;
using TCG_Card_System.Scripts.Enums;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    [Serializable]
    public class CardAttack
    {
        public ECardAttack type;
        public GameObject prefab;
    }
}