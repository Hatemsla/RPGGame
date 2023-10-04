﻿using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using World.Inventory;

namespace World.Player
{
    public class PlayerGetItemSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<PlayerInputComp, PlayerComp>> _player = default;
        
        private readonly EcsPoolInject<Item> _itemsPool = default;
        private readonly EcsPoolInject<HasItems> _hasItems = default;

        private readonly EcsCustomInject<Configuration> _cf = default;
        
        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _player.Value)
            {
                ref var input = ref _player.Pools.Inc1.Get(entity);
                ref var player = ref _player.Pools.Inc2.Get(entity);
                
                if (input.GetFirstItem)
                {
                    TryGetItem(0, entity);
                }

                if (input.GetSecondItem)
                {
                    TryGetItem(1, entity);
                }
                
                if (input.GetThirdItem)
                {
                    TryGetItem(2, entity);
                }
            }
        }

        private void TryGetItem(int abilityIdx, int entity)
        {
            ref var hasItems = ref _hasItems.Value.Get(entity);

            for (var i = 0; i < hasItems.Entities.Count; i++)
            {
                var itemEntity = hasItems.Entities[i];
                ref var item = ref _itemsPool.Value.Get(itemEntity);

                item.ItemView.gameObject.SetActive(i == abilityIdx);
            }
        }
    }
}