using UnityEngine;

public interface IItem {
	float Lifetime { get; set; }
	void Use();
}

public interface IProjectile {
	Vector2 Start { get; set; }
	Vector2 Direction { get; set; }
	void OnCollision();
}

public abstract class Item : IItem
{
	private float _lifetime;

	public float Lifetime
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

	public Item(float lifetime)
	{
		Lifetime = lifetime;
	}

	public abstract void Use();
}

public class PowerupItem : Item
{
	public PowerupItem(float lifetime)
		: base(lifetime)
	{ }

	public override void Use()
	{
	}
}

public class InstantItem : Item
{
	public InstantItem()
		: base(0)
	{
		Use();
	}

	public override void Use()
	{
	}
}

public class ProjectileItem : Item, IProjectile
{
	public Vector2 Start { get; set; }
	public Vector2 Direction { get; set; }

	public ProjectileItem(float lifetime, Vector2 start, Vector2 dir)
		: base(lifetime)
	{
		Start = start;
		Direction = dir;
	}

	public override void Use()
	{
	}

	public void OnCollision()
	{
	}
}