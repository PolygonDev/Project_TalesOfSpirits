using Godot;
using System;

public class Player : KinematicBody2D
{
	//Input names
	String right = "right";
	String left = "left";
	String jump = "jump";
	String ui_home = "ui_home";
	String zoomin = "zoomin";
	String zoomout = "zoomout";

	Vector2 UP = new Vector2(0, -1);
	Vector2 DOWN = new Vector2(0, 1);

	[Export] int speed = 300;
	[Export] float baseGravity = 27f;
	[Export] float jump_power = 36f;
	[Export] float jump_speed = 170f;

	Vector2 globalVector = new Vector2(0, 1);
	Vector2 mathAngleToglobal = new Vector2(0, 0);

	float nowGravity = 0f;
	bool isJumping = false;
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	AnimationPlayer animplayer = null;
	AnimationPlayer rotateplayer = null;
	Sprite sprite = null;
	RichTextLabel rtx = null;
	RichTextLabel zoomfac = null;
	RichTextLabel isinair = null;
	RichTextLabel movvec = null;
	Camera2D cam = null;

	float zoomfactor = 0f;

	public override void _Ready()
	{
		animplayer = GetNode<AnimationPlayer>("AnimationPlayer");
		rotateplayer = GetNode<AnimationPlayer>("RotatePlayer");
		sprite = GetNode<Sprite>("Sprite");
		rtx = GetNode<RichTextLabel>("Camera2D/movement");
		zoomfac = GetNode<RichTextLabel>("Camera2D/zoomfactor");
		isinair = GetNode<RichTextLabel>("Camera2D/isinair");
		movvec = GetNode<RichTextLabel>("Camera2D/movvec");
		cam = GetNode<Camera2D>("Camera2D");

		nowGravity = baseGravity;
		zoomfactor = cam.Zoom.x;
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }

	Vector2 zoom = new Vector2(0, 0);
	Vector2 movement = new Vector2(0, 0);

	float jumpings = 0;

	public override void _PhysicsProcess(float delta)
	{
		if (movement.y <= 1200)
		{
			nowGravity *= 1.013f;
			movement.y += nowGravity * delta;
		}

		if (IsOnCeiling())
		{
			nowGravity = baseGravity * delta;
			movement.y = baseGravity * delta;
		}

		if (IsOnFloor())
		{
			isJumping = false;
			nowGravity = baseGravity * delta;
			movement.y = baseGravity * delta;
		}

		CheckKeyInput(delta);
		setAnimation();

		movement.y += nowGravity * DOWN.y;

		//Vectoren ins globale system umwandeln
		Vector2 vec1 = movement.Rotated(Rotation);
		Vector2 vec2 = UP.Rotated((float) (Rotation - (180 * (Math.PI / 180))));
		Vector2 vec3 = DOWN.Rotated((float)(Rotation - (180 * (Math.PI / 180))));

		//Debug texte
		zoomfac.Text = "zoom: " + cam.Zoom.x;
		rtx.Text = "move: " + movement.x;
		isinair.Text = "inAir: " + !IsOnFloor();
		movvec.Text = "globalrot: " + RotationDegrees;

		//Hier bewegt es sich dann wirklich mit den globalen vectoren da hier eben nur global bewegt wird
		MoveAndSlideWithSnap(vec1, vec2, vec3);

		//wenn fertig mit laufen nach rechts und links wird es auf 0 gesetzt
		movement.x = 0;
	}

	public void CheckKeyInput(float delta)
	{

		if (Input.IsActionPressed(right))
		{
			if (isJumping)
			{
				movement.x = speed*delta + jump_speed*delta;
			}
			else
			{
				movement.x = speed*delta;
			}
		}
		if (Input.IsActionPressed(left))
		{
			if (isJumping)
			{
				movement.x = -speed*delta - jump_speed*delta;
			}
			else
			{
				movement.x = -speed*delta;
			}
		}
		if (Input.IsActionJustPressed(jump) && IsOnFloor())
		{

			isJumping = true;
			movement.y -= jump_power * delta * 28;
			jumpings = movement.y;
		}
		if (Input.IsActionJustPressed(zoomin))
		{
			if (zoomfactor <= 1)
			{
				zoomfactor -= 0.1f;
			} 
			else
			{
				zoomfactor--;
			}
			
			cam.Zoom = new Vector2(zoomfactor, zoomfactor);
		}
		if (Input.IsActionJustPressed(zoomout))
		{
			if (zoomfactor < 1)
			{
				zoomfactor += 0.1f;
			}
			else
			{
				zoomfactor++;
			}
			
			cam.Zoom = new Vector2(zoomfactor, zoomfactor);
		}
		if (Input.IsActionPressed(ui_home))
		{

			GetTree().Quit();
		}
		if (Input.IsActionJustPressed("debug"))
		{
			rotateplayer.Play("gravityRotate");
			//RotationDegrees += 4f;
		}
	}

	public void setAnimation()
	{
		if (!IsOnFloor())
		{
			animplayer.Play("air");
		}
		else if (movement.x == 0)
		{
			animplayer.Play("idle");
		} 
		if (movement.x < 0)
		{
			sprite.FlipH = true;
			if (IsOnFloor()) animplayer.Play("walk");
		} 
		if (movement.x > 0)
		{
			sprite.FlipH = false;
			if (IsOnFloor()) animplayer.Play("walk");
		}
	}

	[Signal]
	public delegate void enter_area(KinematicBody2D player);

	[Signal]
	public delegate void exit_area(KinematicBody2D player);


	private void _event_enter(Area2D area)
	{
		foreach (Node cache in GetNode<Node2D>("/root/Game").GetChildren())
		{
			if (cache.Name.BeginsWith("GravityArea"))
			{
				Area2D grav = (Area2D) cache;
				grav.Call("_on_Player_enter_area", this);
			}
			Console.Out.WriteLine(cache);
		}
		Console.Out.WriteLine("Es gibt nichts mehr zu sehen");
		//EmitSignal(nameof(enter_area), this);
	}


	private void _event_exit(Area2D area)
	{
		foreach (Node cache in GetNode<Node2D>("/root/Game").GetChildren())
		{
			if (cache.Name.BeginsWith("GravityArea"))
			{
				Area2D grav = (Area2D)cache;
				grav.Call("_on_Player_exit_area", this);
			}
			Console.Out.WriteLine(cache);
		}
		Console.Out.WriteLine("Es regt rem esde");
		//EmitSignal(nameof(exit_area), this);
	}

	private void _on_GravityArea_call_player_animation(String animation, bool backwarts, float position)
	{
		animplayer.Play(animation);
	}
}
