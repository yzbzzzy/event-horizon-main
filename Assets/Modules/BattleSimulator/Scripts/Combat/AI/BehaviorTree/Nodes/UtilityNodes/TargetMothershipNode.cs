﻿using Combat.Component.Unit.Classification;
using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TargetMothershipNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			if (context.Ship.Type.Class != UnitClass.Drone)
				return NodeState.Failure;

			var mothership = context.Ship.Type.Owner;
			if (!mothership.IsActive()) 
				return NodeState.Failure;

			context.TargetShip = mothership;
			return NodeState.Success;
		}
	}
}