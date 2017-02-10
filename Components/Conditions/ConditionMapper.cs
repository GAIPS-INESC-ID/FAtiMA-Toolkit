﻿using System;
using System.Collections.Generic;
using System.Linq;
using WellFormedNames;
using Utilities;
using IQueryable = WellFormedNames.IQueryable;
#if PORTABLE
using SerializationUtilities.Attributes;
#endif

namespace Conditions
{
	[Serializable]
	public class ConditionMapper<T> : IEnumerable<Pair<ConditionSet,T>>
	{
		private static readonly IEqualityComparer<Pair<ConditionSet, T>> EQUALITY_COMPARER = new ConditionMapperEquality();
		private HashSet<Pair<ConditionSet, T>> m_conditions = new HashSet<Pair<ConditionSet, T>>(EQUALITY_COMPARER);

		public int Count
		{
			get { return m_conditions.Count; }
		}

		public bool Add(ConditionSet conditionSet, T value)
		{
			if (conditionSet!=null && conditionSet.Count == 0)
				conditionSet = null;

			return m_conditions.Add(Tuples.Create(conditionSet, value));
		}

		public bool Remove(ConditionSet conditionSet, T value)
		{
			if (conditionSet.Count == 0)
				conditionSet = null;

			return m_conditions.Remove(Tuples.Create(conditionSet, value));
		}

		public void Clear()
		{
			m_conditions.Clear();
		}

	    public IEnumerable<Pair<T, SubstitutionSet>> MatchConditions(IQueryable db, Name perspective, SubstitutionSet constraints)
		{
			var constraintSet = new[] { constraints };
			foreach (var e in m_conditions)
			{
				if (e.Item1 == null)
				{
					yield return Tuples.Create(e.Item2, new SubstitutionSet(constraints));
					continue;
				}

			    foreach (var set in e.Item1.UnifyEvaluate(db, perspective, constraintSet))
			    {
                    Console.WriteLine(set.ToString());
			        yield return Tuples.Create(e.Item2, set);
			    }
			}
		}

		public IEnumerator<Pair<ConditionSet, T>> GetEnumerator()
		{
			return m_conditions.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override int GetHashCode()
		{
			const int BASE_HASHCODE = 0x3a74ed8a;
			return m_conditions.Aggregate(BASE_HASHCODE, (hash, pair) => hash ^ EQUALITY_COMPARER.GetHashCode(pair));
		}

		public override bool Equals(object obj)
		{
			ConditionMapper<T> map = obj as ConditionMapper<T>;
			if (map == null)
				return false;

			return m_conditions.SetEquals(map.m_conditions);
		}

		private class ConditionMapperEquality : IEqualityComparer<Pair<ConditionSet,T>>
		{
			public bool Equals(Pair<ConditionSet, T> x, Pair<ConditionSet, T> y)
			{
				return object.Equals(x.Item1, y.Item1) && object.Equals(x.Item2, y.Item2);
			}

			public int GetHashCode(Pair<ConditionSet, T> obj)
			{
				return obj.Item1 == null ? 0 : obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
			}
		}
	}
}