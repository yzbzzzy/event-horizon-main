﻿using System.Collections.Generic;
using Constructor.Ships;
using GameDatabase;
using Services.Localization;
using Services.Reources;
using UnityEngine;
using Zenject;
using ViewModel;

namespace Gui.Constructor
{
    class ShipListContentFiller : MonoBehaviour, IContentFiller
    {
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IDatabase _database;

        [SerializeField] private ShipItem _itemPrefab;

        public void Initialize(IEnumerable<IShip> ships)
        {
            _itemPrefab.gameObject.SetActive(false);
            _ships.Clear();
            _ships.AddRange(ships);
        }

        public GameObject GetListItem(int index, int itemType, GameObject obj)
        {
            if (obj == null)
            {
                obj = Instantiate(_itemPrefab.gameObject);
            }

            var item = obj.GetComponent<ShipItem>();
            UpdateShip(item, _ships[index]);
            return obj;
        }

        public int GetItemCount()
        {
            return _ships.Count;
        }

        public int GetItemType(int index)
        {
            return 0;
        }

		private void UpdateShip(ShipItem item, IShip ship)
		{
			item.gameObject.SetActive(true);
			item.Initialize(ship, _resourceLocator, _localization);
		}

		private readonly List<IShip> _ships = new List<IShip>();
    }
}
