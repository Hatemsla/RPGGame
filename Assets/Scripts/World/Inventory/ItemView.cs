﻿using System.Collections.Generic;
using System.Linq;
using Leopotam.EcsLite;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using World.Inventory.Chest;
using World.Player;

namespace World.Inventory
{
    public sealed class ItemView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public EcsPackedEntity ItemIdx;
        public ItemObject itemObject;
        public Image itemImage;
        public RectTransform playerInventoryView;
        public RectTransform chestInventoryView;
        public RectTransform fastItemsView;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text itemDescription;
        [SerializeField] private TMP_Text itemCount;

        private Transform _parentBeforeDrag;
        private SceneData _sd;

        public string ItemName
        {
            get => itemName.text;
            set => itemName.text = value;
        }

        public string ItemDescription
        {
            get => itemDescription.text;
            set => itemDescription.text = value;
        }

        public string ItemCount
        {
            get => itemCount.text;
            set => itemCount.text = value;
        }

        private EcsWorld _world;
        private int _ownerEntity;
        private EcsPool<HasItems> _hasItems;
        private EcsPool<ItemComp> _itemsPool;
        private EcsPool<InventoryComp> _inventoryPool;
        private EcsPool<ChestComp> _chestPool;
        private EcsPool<PlayerComp> _playerPool;

        private ContentView _playerInventoryViewContent;
        private ContentView _chestInventoryViewContent;
        
        public void SetWorld(EcsWorld world, int entity, SceneData sd)
        {
            _world = world;
            _ownerEntity = entity;
            _hasItems = _world.GetPool<HasItems>();
            _itemsPool = _world.GetPool<ItemComp>();
            _inventoryPool = _world.GetPool<InventoryComp>();
            _chestPool = _world.GetPool<ChestComp>();
            _playerPool = _world.GetPool<PlayerComp>();
            _sd = sd;
        }

        public void SetViews(RectTransform playerInventoryView, RectTransform chestInventoryView, RectTransform fastItemsView)
        {
            this.playerInventoryView = playerInventoryView;
            this.chestInventoryView = chestInventoryView;
            this.fastItemsView = fastItemsView;

            _playerInventoryViewContent = playerInventoryView.GetComponentInChildren<ContentView>();
            _chestInventoryViewContent = chestInventoryView.GetComponentInChildren<ContentView>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if(!itemObject)
                    return;
                
                ref var hasItems = ref _hasItems.Get(_ownerEntity);

                ItemIdx.Unpack(_world, out var currentEntity);

                foreach (var itemPacked in hasItems.Entities)
                    if (itemPacked.Unpack(_world, out var unpackedEntity))
                    {
                        if (currentEntity == unpackedEntity)
                            itemObject.gameObject.SetActive(!itemObject.gameObject.activeSelf);
                        else
                            itemObject.gameObject.SetActive(false);
                    }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _parentBeforeDrag = transform.parent;
                transform.SetParent(transform.root);
                transform.SetAsLastSibling();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
                transform.position = Mouse.current.position.value;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (IsItemOutInventory(transform.position, playerInventoryView) || IsItemOutInventory(transform.position, chestInventoryView))
            {
                if (IsItemInsideInventory(transform.position, fastItemsView))
                {
                    SetFastItemView();
                    if(_ownerEntity == _playerInventoryViewContent.currentEntity)
                        transform.SetParent(_parentBeforeDrag);
                    else
                    {
                        MoveItem(_playerInventoryViewContent.currentEntity, _playerInventoryViewContent.transform);
                        var playerComp = _playerPool.Get(_playerInventoryViewContent.currentEntity);
                        var rot = itemObject.transform.localRotation;
                        itemObject.transform.SetParent(playerComp.Transform);
                        itemObject.transform.localRotation = rot;
                        itemObject.transform.position = playerComp.Transform.localPosition + playerComp.Transform.forward;
                    }
                }
                else if (IsItemInsideInventory(transform.position, chestInventoryView))
                {
                    MoveItem(_chestInventoryViewContent.currentEntity, _chestInventoryViewContent.transform);
                }
                else if (IsItemInsideInventory(transform.position, playerInventoryView))
                {
                    MoveItem(_playerInventoryViewContent.currentEntity, _playerInventoryViewContent.transform);
                }
                else
                {
                    DestroyItem();   
                }
            }
            else
            {
                transform.SetParent(_parentBeforeDrag);
            }
        }

        private void SetFastItemView()
        {
            foreach (var ft in _sd.fastItemViews)
            {
                if (IsCursorOver(ft))
                {
                    ft.itemImage.sprite = itemImage.sprite;
                    ft.itemObject = itemObject;
                    ft.itemObject.ItemIdx = ItemIdx;
                    ft.itemName.text = ItemName;
                    ft.itemCount.text = ItemCount;
                    return;
                }
            }
        }
        
        private bool IsCursorOver(FastItemView ft)
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults(), ft);
        }

        private FastItemView IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, FastItemView ft)
        {
            return eventSystemRaysastResults
                .Select(curRaysastResult => curRaysastResult.gameObject.GetComponentInParent<FastItemView>())
                .FirstOrDefault(targetComp => targetComp && targetComp == ft);
        }

        private List<RaycastResult> GetEventSystemRaycastResults()
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }


        private void MoveItem(int otherEntity, Transform newParent)
        {
            if(otherEntity == _ownerEntity)
                return;
            
            ref var hasItemsOwner = ref _hasItems.Get(_ownerEntity);
            ref var hasItemsOther = ref _hasItems.Get(otherEntity);

            ref var ownerInventory = ref _inventoryPool.Get(_ownerEntity);
            ref var otherInventory = ref _inventoryPool.Get(otherEntity);

            ItemIdx.Unpack(_world, out var unpackedEntity);
            ref var item = ref _itemsPool.Get(unpackedEntity);

            if (otherEntity == _chestInventoryViewContent.currentEntity)
            {
                ref var chestComp = ref _chestPool.Get(otherEntity);
                
                chestComp.ChestObject.itemViews.Add(this);
            }

            ownerInventory.CurrentWeight -= item.Weight;
            otherInventory.CurrentWeight += item.Weight;
            
            ownerInventory.InventoryWeightView.inventoryWeightText.text = $"Вес: {ownerInventory.CurrentWeight}/{ownerInventory.MaxWeight}";
            otherInventory.InventoryWeightView.inventoryWeightText.text = $"Вес: {otherInventory.CurrentWeight}/{otherInventory.MaxWeight}";
            
            hasItemsOther.Entities.Add(ItemIdx);
            hasItemsOwner.Entities.Remove(ItemIdx);

            transform.SetParent(newParent);
            _ownerEntity = otherEntity;
        }

        private void DestroyItem()
        {
            if (ItemIdx.Unpack(_world, out var unpackedEntity))
            {
                ref var item = ref _itemsPool.Get(unpackedEntity);

                ref var inventory = ref _inventoryPool.Get(_ownerEntity);
                inventory.CurrentWeight -= item.Weight;
                inventory.InventoryWeightView.inventoryWeightText.text = $"Вес: {inventory.CurrentWeight}/{inventory.MaxWeight}";
                
                Destroy(itemObject != null ? itemObject.gameObject : null);
                
                _itemsPool.Del(unpackedEntity);
            }

            Destroy(transform.gameObject);
        }

        private bool IsItemOutInventory(Vector3 position, RectTransform view)
        {
            var minPosition = view.TransformPoint(view.rect.min);
            var maxPosition = view.TransformPoint(view.rect.max);

            return position.x < minPosition.x || position.x > maxPosition.x || position.y < minPosition.y ||
                   position.y > maxPosition.y;
        }
        
        private bool IsItemInsideInventory(Vector3 position, RectTransform view)
        {
            if (!view.gameObject.activeInHierarchy)
                return false;
            
            var minPosition = view.TransformPoint(view.rect.min);
            var maxPosition = view.TransformPoint(view.rect.max);

            return position.x >= minPosition.x && position.x <= maxPosition.x &&
                   position.y >= minPosition.y && position.y <= maxPosition.y;
        }

    }
}