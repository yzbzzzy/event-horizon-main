﻿using GameDatabase.DataModel;
using Services.Localization;
using Combat.Ai.BehaviorTree.Nodes;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree
{
	public class NodeBuilder
	{
		private readonly NodeFactory _factory;
		private readonly RequirementChecker _requirementChecker;
		private readonly ILocalization _localization;
		private readonly AiSettings _settings;
		private readonly IShip _ship;

		public NodeBuilder(IShip ship, AiSettings settings, ILocalization localization)
		{
			_ship = ship;
			_settings = settings;
			_localization = localization;
			_requirementChecker = new RequirementChecker(ship, settings);
			_factory = new NodeFactory(this);
			
		}

		public INode Build(BehaviorTreeNode model)
		{
			if (model == null || !model.Requirement.Create(_requirementChecker)) 
				return EmptyNode.Failure;

			return model.Create(_factory);
		}

		private class NodeFactory : IBehaviorTreeNodeFactory<INode>
		{
			private readonly NodeBuilder _builder;
			private System.Random _random;
			private IShip Ship => _builder._ship;
			private AiSettings Settings => _builder._settings;
			private ILocalization Localization => _builder._localization;
			private System.Random Random => _random ??= new();
			private float RandomValue => (float)Random.NextDouble();
			private float RandomRange(float min, float max) => min + (max - min) * RandomValue;

			public NodeFactory(NodeBuilder builder)
			{
				_builder = builder;
				_random = new();
			}

			public INode Create(BehaviorTreeNode_Undefined content) => EmptyNode.Success;
			public INode Create(BehaviorTreeNode_SubTree content) => _builder.Build(content.BehaviourTree?.RootNode);
			public INode Create(BehaviorTreeNode_Invertor content) => InvertorNode.Create(_builder.Build(content.Node));
			public INode Create(BehaviorTreeNode_ConstantResult content) => ConstantResultNode.Create(_builder.Build(content.Node), content.Result);
			public INode Create(BehaviorTreeNode_CompleteOnce content) => CompleteOnceNode.Create(_builder.Build(content.Node), content.Result);

			public INode Create(BehaviorTreeNode_Selector content)
			{
				var selector = new SelectorNode();
				for (int i = 0; i < content.Nodes.Count; ++i)
				{
					var node = _builder.Build(content.Nodes[i]);
					if (node == EmptyNode.Failure) continue;
					selector.Nodes.Add(node);
					if (node == EmptyNode.Success) break;
				}

				if (selector.Nodes.Count == 0) return EmptyNode.Failure;
				if (selector.Nodes.Count == 1) return selector.Nodes[0];
				return selector;
			}

			public INode Create(BehaviorTreeNode_Sequence content)
			{
				var sequence = new SequenceNode();
				for (int i = 0; i < content.Nodes.Count; ++i)
				{
					var node = _builder.Build(content.Nodes[i]);
					if (node == EmptyNode.Success) continue;
					sequence.Nodes.Add(node);
					if (node == EmptyNode.Failure) break;
				}

				if (sequence.Nodes.Count == 0) return EmptyNode.Success;
				if (sequence.Nodes.Count == 1) return sequence.Nodes[0];
				return sequence;
			}

			public INode Create(BehaviorTreeNode_Parallel content)
			{
				if (content.Nodes.Count == 0) return EmptyNode.Success;
				if (content.Nodes.Count == 1) return _builder.Build(content.Nodes[0]);

				// TODO: optimize
				var parallel = new ParallelNode();
				for (int i = 0; i < content.Nodes.Count; ++i)
				{
					var node = _builder.Build(content.Nodes[i]);
					parallel.Nodes.Add(node);
				}

				return parallel;
			}

			public INode Create(BehaviorTreeNode_RandomSelector content)
			{
				if (content.Nodes.Count == 0) return EmptyNode.Failure;
				if (content.Nodes.Count == 1) return _builder.Build(content.Nodes[0]);

				var random = new RandomSelectorNode(content.Cooldown, false);
				foreach (var child in content.Nodes)
					random.Add(_builder.Build(child));

				return random;
			}

			public INode Create(BehaviorTreeNode_RandomExecutor content)
			{
				if (content.Nodes.Count == 0) return EmptyNode.Failure;

				var random = new RandomSelectorNode(content.Cooldown, true);
				foreach (var child in content.Nodes)
					random.Add(_builder.Build(child));

				return random;
			}

			public INode Create(BehaviorTreeNode_FindEnemy content)
			{
				if (_builder._settings.DroneRange > 0)
					return new FindEnemyForDrone(content.MinCooldown, content.MaxCooldown,
						Settings.DroneRange, content.InAttackRange, content.IgnoreDrones);

				return new FindEnemyForShip(content.MinCooldown, content.MaxCooldown, content.InAttackRange, content.IgnoreDrones);
			}

			public INode Create(BehaviorTreeNode_MoveToAttackRange content) => new MoveToAttackRange(content.MinMaxLerp, content.Multiplier > 0 ? content.Multiplier : 1.0f);
			public INode Create(BehaviorTreeNode_Attack content) => new AttackNode(Settings.AiLevel);
			public INode Create(BehaviorTreeNode_FlyAroundMothership content) => new FlyAroundMothership(RandomRange(content.MinDistance, content.MaxDistance));
			public INode Create(BehaviorTreeNode_HaveEnoughEnergy content) => new HaveEnoughEnergy(content.FailIfLess);
			public INode Create(BehaviorTreeNode_SelectWeapon content) => SelectWeaponNode.Create(Ship, content.WeaponType);
			public INode Create(BehaviorTreeNode_SpawnDrones content) => new SpawnDronesNode(Ship);
			public INode Create(BehaviorTreeNode_Ram content) => new RamNode(Ship, content.UseShipSystems);
			public INode Create(BehaviorTreeNode_DetonateShip content) => new DetonateShipNode(Ship, content.InAttackRange);
			public INode Create(BehaviorTreeNode_IsLowOnHp content) => new IsLowOnHp(content.MinValue);
			public INode Create(BehaviorTreeNode_Vanish content) => new VanishNode();
			public INode Create(BehaviorTreeNode_MotherShipRetreated content) => new MothershipRetreated();
			public INode Create(BehaviorTreeNode_MotherShipDestroyed content) => new MothershipDestroyed();
			public INode Create(BehaviorTreeNode_GoBerserk content) => new GoBerserkNode();
			public INode Create(BehaviorTreeNode_TargetMothership content) => new TargetMothershipNode();
			public INode Create(BehaviorTreeNode_MaintainAttackRange content) => new MaintainAttackRange(content.MinMaxLerp, content.Tolerance);
			public INode Create(BehaviorTreeNode_IsWithinAttackRange content) => new IsWithinAttackRange(content.MinMaxLerp);
			public INode Create(BehaviorTreeNode_MothershipLowHp content) => new MothershipLowHpNode(content.MinValue);
			public INode Create(BehaviorTreeNode_IsControledByPlayer content) => new IsPlayerControlled();
			public INode Create(BehaviorTreeNode_Wait content) => new WaitNode(content.Cooldown, content.ResetIfInterrupted);
			public INode Create(BehaviorTreeNode_LookAtTarget content) => new LookAtTargetNode();
			public INode Create(BehaviorTreeNode_LookForSecondaryTargets content) => new UpdateSecondaryTargets(content.Cooldown);
			public INode Create(BehaviorTreeNode_LookForThreats content) => new UpdateThreats(content.Cooldown);
			public INode Create(BehaviorTreeNode_ActivateDevice content) => ActivateDeviceNode.Create(Ship, content.DeviceClass);
			public INode Create(BehaviorTreeNode_RechargeEnergy content) => new RechargeEnergy(content.FailIfLess, content.RestoreUntil);
			public INode Create(BehaviorTreeNode_SustainAim content) => new SustainAimNode(false);
			public INode Create(BehaviorTreeNode_ChargeWeapons content) => ChargeWeaponsNode.Create(Ship);
			public INode Create(BehaviorTreeNode_ShowMessage content) => new ShowMessageNode(Localization.Localize(content.Text), content.Color);
			public INode Create(BehaviorTreeNode_DebugLog content) => new DebugLogNode(content.Text);
			public INode Create(BehaviorTreeNode_Chase content) => new ChaseNode();
			public INode Create(BehaviorTreeNode_AvoidThreats content) => new AvoidThreatsNode();
			public INode Create(BehaviorTreeNode_HaveIncomingThreat content) => new HasIncomingThreatNode(content.TimeToCollision);
			public INode Create(BehaviorTreeNode_Stop content) => new StopShipNode();
			public INode Create(BehaviorTreeNode_UseRecoil content) => RecoilNode.Create(Ship);
			public INode Create(BehaviorTreeNode_DefendWithFronalShield content) => FrontalShieldNode.Create(Ship);
			public INode Create(BehaviorTreeNode_TrackControllableAmmo content) => TrackControllableAmmo.Create(Ship, true);
		}
	}
}
