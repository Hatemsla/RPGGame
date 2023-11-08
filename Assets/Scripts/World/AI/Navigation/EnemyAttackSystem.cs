﻿using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using World.Player;
using Random = UnityEngine.Random;

namespace World.AI.Navigation
{
    public sealed class EnemyAttackSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<ZoneComp, HasEnemies>> _zoneFilter = default;
        private readonly EcsFilterInject<Inc<PlayerComp, RpgComp>> _playerFilter = default;
        private readonly EcsPoolInject<EnemyComp> _enemyPool = default;

        private readonly EcsWorldInject _world = default;
        
        public void Run(IEcsSystems systems)
        {
            foreach (var zoneEntity in _zoneFilter.Value)
            {
                ref var zoneComp = ref _zoneFilter.Pools.Inc1.Get(zoneEntity);
                ref var hasEnemiesComp = ref _zoneFilter.Pools.Inc2.Get(zoneEntity);

                foreach (var packedEnemy in hasEnemiesComp.Entities)
                {
                    if (packedEnemy.Unpack(_world.Value, out var unpackedEnemy))
                    {
                        ref var enemyComp = ref _enemyPool.Value.Get(unpackedEnemy);

                        foreach (var playerEntity in _playerFilter.Value)
                        {
                            ref var playerComp = ref _playerFilter.Pools.Inc1.Get(playerEntity);
                            ref var rpgComp = ref _playerFilter.Pools.Inc2.Get(playerEntity);

                            if (rpgComp.IsDead)
                            {
                                enemyComp.EnemyState = EnemyState.Patrol;
                                break;
                            }

                            var distanceToPlayer = Vector3.Distance(playerComp.Transform.position,
                                enemyComp.Agent.transform.position);

                            if (Math.Abs(distanceToPlayer) < 3f && enemyComp.EnemyState != EnemyState.Attack)
                            {
                                enemyComp.EnemyState = EnemyState.Attack;
                            }
                            else if (enemyComp.EnemyState == EnemyState.Attack && Math.Abs(distanceToPlayer) >= 3f)
                            {
                                enemyComp.EnemyState = EnemyState.Chase;
                            }

                            if (enemyComp.EnemyState == EnemyState.Attack)
                            {
                                enemyComp.Agent.SetDestination(playerComp.Transform.position);

                                if (enemyComp.EnemyView.currentAttackDelay >= enemyComp.AttackDelay)
                                {
                                    Debug.Log("Attack");
                                    rpgComp.Health -= Random.Range(enemyComp.MinDamage, enemyComp.MaxDamage);
                                    enemyComp.EnemyView.StartCoroutine(
                                        enemyComp.EnemyView.AttackDelay(enemyComp.AttackDelay));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}