using UnityEngine;

namespace Extensions
{
	public class Vectors
	{
		public static Vector2 mod(Vector2 vec, int mod)
		{
			return new Vector2(vec.x % mod, vec.y % mod);
		}

		///<summary>
		///<para>Only use this when the first vector has only ONE component. Performs v1.c > v2.c, where c is the component.</para>
		///</summary>
		public static bool componentGreaterThan(Vector2 v1, Vector2 v2)
		{
			if (v1.x == 0)
			{
				return v1.y > v2.y;
			}
			return v1.x > v2.x;
		}

		///<summary>
		///<para>Only use this when the first vector has only ONE component. Performs v1.c >= v2.c, where c is the component.</para>
		///</summary>
		public static bool componentGreaterThanOrEqualTo(Vector2 v1, Vector2 v2)
		{
			if (v1.x == 0)
			{
				return v1.y > v2.y;
			}
			return v1.x > v2.x;
		}
	}