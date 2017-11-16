using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringManager
{
	public static string GetLeftMost(string src)
	{
		int t = src.IndexOf('_');
		if (t > 0)
		{
			return src.Substring(0, t);
		}

		return "";
	}
}
