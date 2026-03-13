using System;
using System.Collections.Generic;

public static class Util {
	public static T WeightedRandom<T>(IList<KeyValuePair<T, float>> items, Random rng) {
		float total = 0f;

		for (int i = 0; i < items.Count; i++)
			total += items[i].Value;

		float r = (float)(rng.NextDouble() * total);

		for (int i = 0; i < items.Count; i++) {
			r -= items[i].Value;
			if (r <= 0)
				return items[i].Key;
		}

		return items[^1].Key;
	}
}

public enum Fade {
	Transparent = 0,
	Black = 1
}