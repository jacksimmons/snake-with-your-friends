using UnityEngine;

public interface IItem {
	float Lifetime { get; set; }
	void Use();
}

public interface IProjectile {
	Vector2 Start { get; set; }
	Vector2 Direction { get; set; }
	void Collision();
}

public abstract class Item : IItem
{
	private float _lifetime;

	float IItem.Lifetime
	{
		get
		{
			return _lifetime;
		}
		set
		{
			if (value >= 0)
				_lifetime = value;
			else
				throw new System.Exception("Lifetime must be >= 0.");
		}
	}

	public abstract void Use();
}

public class Viagra : Item
{
	public override void Use()
	{
	}
}

public class SnakeOfSteel : Item
{
	public override void Use()
	{
	}
}

public class Fireball : Item, IProjectile
{
	Vector2 IProjectile.Start { get; set; }
	Vector2 IProjectile.Direction { get; set; }

	public override void Use()
	{
	}

	public void Collision()
	{
	}
}