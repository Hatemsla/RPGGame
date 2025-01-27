﻿using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using ObjectsPool;
using World.Configurations;
using World.Player;
using World.RPG;

namespace World.Ability
{
    public class AbilityCastDelaySystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<PlayerComp, RpgComp>> _player = default;

        private readonly EcsPoolInject<AbilityComp> _abilityPool = default;
        private readonly EcsPoolInject<HasAbilities> _hasAbilities = default;

        private readonly EcsCustomInject<TimeService> _ts = default;
        private readonly EcsCustomInject<PoolService> _ps = default;
        private readonly EcsCustomInject<SceneData> _sd = default;
        private readonly EcsCustomInject<Configuration> _cf = default;

        private readonly EcsWorldInject _world = default;
        
        public void Run(IEcsSystems systems)
        {
            foreach (var playerEntity in _player.Value)
            {
                ref var hasAbilities = ref _hasAbilities.Value.Get(playerEntity);
                ref var rpg = ref _player.Pools.Inc2.Get(playerEntity);

                if (rpg.CastDelay > 0)
                {
                    rpg.CastDelay -= _ts.Value.DeltaTime;
                    
                    foreach (var abilityPacked in hasAbilities.Entities)
                    {
                        if (abilityPacked.Unpack(_world.Value, out var unpackedEntity))
                        {
                            ref var ability = ref _abilityPool.Value.Get(unpackedEntity);
                            
                            if(ability.CurrentDelay > 0)
                                continue;
                            
                            ActiveAbilityDelayView(unpackedEntity,
                                _cf.Value.abilityConfiguration.totalAbilityDelay, rpg.CastDelay);
                        }
                    }
                }
            }
        }
        
        public void ActiveAbilityDelayView(int unpackedEntity, float delayTime, float delayTimer)
        {
            foreach (var delayAbility in _sd.Value.uiSceneData.delayAbilityViews)
            {
                if (delayAbility.delayImage.fillAmount > 0)
                {
                    if (delayAbility.AbilityIdx.Unpack(_world.Value, out var delayUnpackedEntity))
                    {
                        if (delayUnpackedEntity == unpackedEntity)
                        {
                            delayAbility.delayImage.fillAmount -= (_ts.Value.DeltaTime / delayTime);
                            if (delayTimer >= 0.01)
                            {
                                delayAbility.delayTimer.text = $"{delayTimer:f1}";
                            }
                            else
                            {
                                delayAbility.delayTimer.text = "";
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
}