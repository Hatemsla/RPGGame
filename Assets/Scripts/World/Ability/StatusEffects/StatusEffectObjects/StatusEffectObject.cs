﻿using Leopotam.EcsLite;
using UnityEngine;
using Utils.ObjectsPool;

namespace World.Ability.StatusEffects.StatusEffectObjects
{
    public class StatusEffectObject : MonoBehaviour
    {
        public EcsPackedEntity PostEffectIdx;

        private protected EcsWorld _world;

        private protected SceneData _sd;
        private protected TimeService _ts;
        private protected PoolService _ps;

        private protected int _playerEntity;

        public void SetWorld(EcsWorld world, int entity, SceneData sd, TimeService ts, PoolService ps)
        {
            _world = world;
            _playerEntity = entity;

            _sd = sd;
            _ts = ts;
            _ps = ps;
        }
    }
}