﻿using System.Collections.Generic;
using Leopotam.EcsLite;
using TMPro;
using UnityEngine;
using World.Player;

namespace World.Inventory.Chest
{
    public sealed class ChestObject : MonoBehaviour
    {
        public List<ItemData> items;
        public List<ItemView> itemViews;

        public RectTransform chestInventoryView;
        public TMP_Text chestInventoryWeightText;

        public bool isOpen;

        private PlayerView _player;
        
        private EcsWorld _world;
        private int _chestEntity;
        private EcsPool<InventoryComp> _inventoryPool;
        private EcsPool<HasItems> _hasItems;

        private ContentView _chestInventoryViewContent;

        public void SetWorld(EcsWorld world, int entity)
        {
            _world = world;
            _chestEntity = entity;
            _inventoryPool = _world.GetPool<InventoryComp>();
            _hasItems = _world.GetPool<HasItems>();
        }

        public void SetView(RectTransform chestInventoryView)
        {
            this.chestInventoryView = chestInventoryView;
            _chestInventoryViewContent = chestInventoryView.GetComponentInChildren<ContentView>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            _player = other.GetComponentInParent<PlayerView>();
        }

        private void Update()
        {
            if (isOpen && _player)
            {
                if (chestInventoryView.gameObject.activeInHierarchy)
                {
                    chestInventoryView.gameObject.SetActive(false);

                    ref var hasItems = ref _hasItems.Get(_chestEntity);
                    
                    var validItems = new List<ItemView>();
                    
                    foreach (var item in itemViews)
                    {
                        if (item)
                        {
                            if (hasItems.Entities.Contains(item.ItemIdx))
                            {
                                item.transform.SetParent(transform);
                                validItems.Add(item);
                            }
                        }
                    }
                    
                    itemViews = new List<ItemView>(validItems);
                }
                else
                {
                    chestInventoryView.gameObject.SetActive(true);
                    _chestInventoryViewContent.currentEntity = _chestEntity;
                    
                    ref var inventory = ref _inventoryPool.Get(_chestEntity);
                    chestInventoryWeightText.text = $"Вес: {inventory.CurrentWeight}/{inventory.MaxWeight}";
                    
                    foreach (var item in itemViews)
                    {
                        if (item)
                        {
                            item.transform.SetParent(_chestInventoryViewContent.transform);
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponentInParent<PlayerView>())
            {
                _player = null;
                chestInventoryView.gameObject.SetActive(false);

                ref var hasItems = ref _hasItems.Get(_chestEntity);
                
                var validItems = new List<ItemView>();
                
                foreach (var item in itemViews)
                {
                    if (item && hasItems.Entities.Contains(item.ItemIdx))
                    {
                        item.transform.SetParent(transform);
                        validItems.Add(item);
                    }
                }

                itemViews = new List<ItemView>(validItems);
            }
        }
    }
}