using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2i
{
	public int x;

	public int y;

	#region Constructor

	public Vector2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2i(Vector2 vector)
	{
		x = (int)vector.x;
		y = (int)vector.y;
	}

	public Vector2i(Vector3 vector)
	{
		x = (int)vector.x;
		y = (int)vector.y;
	}

	#endregion

	#region Overator

	public static Vector2i operator +(Vector2i a, Vector2i b)
	{
		return new Vector2i(a.x + b.x, a.y + b.y);
	}

	public static Vector2i operator -(Vector2i a, Vector2i b)
	{
		return new Vector2i(a.x - b.x, a.y - b.y);
	}
	
	public static Vector2i operator *(Vector2i a, float scale)
	{
		return new Vector2i((int)(a.x * scale), (int)(a.y * scale));
	}

	public static Vector2i operator *(Vector2i a, int scale)
	{
		return new Vector2i(a.x * scale, a.y * scale);
	}

	public static bool operator ==(Vector2i a, Vector2i b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator !=(Vector2i a, Vector2i b)
	{
		return !(a == b);
	}

	#endregion

	public override bool Equals(object obj)
	{
		if (this.GetType() != obj.GetType())
		{
			return false;
		}
		Vector2i vec = (Vector2i)obj;
		return x == vec.x && y == vec.y;
	}

	public override int GetHashCode()
	{
		return x ^ y;
	}

	public override string ToString()
	{
		return "(" + x.ToString() + ", " + y.ToString() + ")";
	}

	public static Vector2i zero
	{
		get { return new Vector2i(0, 0); }
	}
}
