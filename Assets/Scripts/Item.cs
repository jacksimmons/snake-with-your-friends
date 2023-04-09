public interface IItem {
	void Use();
}

public interface IProjectile {
	float Lifetime { get; set; }
	Vector2 Start { get; set; }
	Vector2 Direction { get; set; }
	void Collision();
}

public interface IPowerup {
	float Lifetime { get; set; }
}

public class Fireball : IProjectile
{
	Lifetime = 10;
	public void Use()
	{

	}
}

public class Viagra : IPowerup
{
	Lifetime = 45;
	public void Use()
	{

	}
}

public class SnakeOfSteel : IPowerup
{
	Lifetime = 25;
	public void Use()
	{

	}
}