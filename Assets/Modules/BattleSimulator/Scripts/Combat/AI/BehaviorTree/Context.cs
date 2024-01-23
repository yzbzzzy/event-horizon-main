﻿using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Scene;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree
{
	public class Context
	{
		private readonly IScene _scene;
		private readonly IShip _ship;
		private readonly ShipWeaponList _allWeapons;

		private IShip _targetShip;
		private float _elapsedTime;
		private float _targetUpdateTime = -60;
		private ShipWeaponList _selectedWeapons;
		private TargetList _targetList;
		private ThreatList _threatList;
		private float _targetListUpdateTime = -60;
		private float _threatListUpdateTime = -60;

		public IScene Scene => _scene;
		public IShip Ship => _ship;
		public IShip TargetShip => _targetShip;
		public ShipControls Controls { get; } = new();
		public IReadOnlyList<IShip> SecondaryTargets => _targetList?.Items ?? EmptyList<IShip>.Instance;
		public IReadOnlyList<IUnit> Threats => _threatList?.Units ?? EmptyList<IUnit>.Instance;
		public float TimeToCollision => _threatList != null ? _threatList.TimeToHit : float.MaxValue;

		public bool HaveWeapons => _allWeapons.Count > 0;
		public float AttackRangeMax => _allWeapons.RangeMax;
		public float AttackRangeMin => _allWeapons.RangeMin;

		public int FrameId { get; private set; }
		public float DeltaTime { get; private set; }
		public float Time => _elapsedTime;
		public float TimeSinceTargetUpdate => _elapsedTime - _targetUpdateTime;

		public bool RestoringEnergy { get; set; }
		public ShipWeaponList SelectedWeapons { get => _selectedWeapons ?? _allWeapons; set => _selectedWeapons = value; }

		public Context(IShip ship, IScene scene)
		{
			_scene = scene;
			_ship = ship;
			_allWeapons = new ShipWeaponList(ship);
		}

		public void Update(float deltaTime) 
		{
			_elapsedTime += deltaTime;
			DeltaTime = deltaTime;
			FrameId++;
		}

		public void UpdateTarget(IShip enemyShip)
		{
			_targetShip = enemyShip;
			_targetUpdateTime = _elapsedTime;
		}

		public void UpdateTargetList(float cooldown)
		{
			if (_targetList == null) 
				_targetList = new(_scene);

			if (_elapsedTime - _targetListUpdateTime < cooldown) return;
			_targetListUpdateTime = _elapsedTime;
			_targetList.Update(_ship, _targetShip);
		}

		public void UpdateThreatList(IThreatAnalyzer threatAnalyzer, float cooldown)
		{
			if (_threatList == null)
				_threatList = new(_scene);

			if (_elapsedTime - _threatListUpdateTime < cooldown) return;
			_threatListUpdateTime = _elapsedTime;
			_threatList.Update(_ship, threatAnalyzer);
		}
	}
}