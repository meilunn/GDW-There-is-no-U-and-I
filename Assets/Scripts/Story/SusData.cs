using System.Collections.Generic;
using UnityEngine;

public enum SusAction {
	StealToiletPaper,
	BrewDecaf,
	SpillCoffee,
	MixCocktail,
	UseDrugs
}

public enum PenaltyFunction {
	Linear,
	Quadratic,
	Exponential,
	Const,
}

public class SusData : MonoBehaviour {
	public static Dictionary<TeammateController, int[]> SusLevels = new();

	public SusAction action;
	[SerializeField]
	private float basePenalty;
	[SerializeField]
	private float penaltyModifier;
	private readonly List<TeammateController> detectedTeammates = new();
	[SerializeField]
	[Tooltip("The function used to calculate the penalty based on the number of times the action has been performed. Linear: base + mod * count, Quadratic: base + mod * count^2, Exponential: base * mod^count")]
	private PenaltyFunction penaltyFunction;
	[Tooltip("The range within which the player can be detected by teammates when performing this action.")]
	public float range;
	[Tooltip("If true, the player will automatically inflict sus on all teammates in range without needing to be detected by them. Damage will only be inflicted once per iteration, not per teammate.")]
	public bool autoInflictOnEveryoneInRange = false;

	/// <summary>
	/// Calculates the penalty for a given teammate and increases their sus level for this action.
	/// </summary>
	/// <param name="teammate"></param>
	/// <returns>The amount of sus the player inflicts on the teammate</returns>
	public float OnDetect(TeammateController teammate) {
		if(!SusLevels.ContainsKey(teammate)) SusLevels[teammate] = new int[System.Enum.GetValues(typeof(SusAction)).Length];
		if((autoInflictOnEveryoneInRange && detectedTeammates.Count != 0) || detectedTeammates.Contains(teammate)) return 0;
		detectedTeammates.Add(teammate);

		int susLevel = SusLevels[teammate][(int)action]++;

		if(susLevel == 0) return basePenalty;
		
		float penalty = basePenalty + penaltyFunction switch {
			PenaltyFunction.Linear => penaltyModifier * susLevel,
			PenaltyFunction.Quadratic => penaltyModifier * susLevel * susLevel,
			PenaltyFunction.Exponential => basePenalty * Mathf.Pow(penaltyModifier, susLevel),
			_ => 0
		};

		// disable early if the penalty will be 0
		if(autoInflictOnEveryoneInRange) {
			Player.Instance.SusObjects.Remove(this);
		}

		return penalty;
	}

	public void Enable() {
		Player.Instance.SusObjects.Add(this);
	}

	public void Disable() {
		Player.Instance.SusObjects.Remove(this);
		detectedTeammates.Clear();
	}
}