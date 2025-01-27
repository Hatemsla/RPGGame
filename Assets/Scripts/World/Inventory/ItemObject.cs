﻿using Leopotam.EcsLite;
using ObjectsPool;
using UnityEngine;
using World.Configurations;
using World.Player;
using World.RPG;

namespace World.Inventory
{
    public abstract class ItemObject : MonoBehaviour
    {
        public EcsPackedEntity ItemIdx;
        
        public EcsWorld DefaultWorld;
        public EcsWorld EventWorld;
        public int playerEntity;
        public PoolService Ps;
        public TimeService Ts;
        public Configuration cf;
    }
}