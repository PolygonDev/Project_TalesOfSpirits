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

	[Export]
	int speed = 300;
	[Export]
	float baseGravity = 27f;
	[Export]
	float jump_power = -35;
	[Export]
	float jump_speed = 160;


	float nowGravity = 0f;
	bool isJumping = false;
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	AnimationPlayer animplayer = null;
	Sprite sprite = null;
	RichTextLabel rtx = null;
	RichTextLabel zoomfac = null;
	RichTextLabel isinair = null;
	Camera2D cam = null;

	float zoomfactor = 0f;

	public override void _Ready()
	{
		animplayer = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<Sprite>("Sprite");
		rtx = GetNode<RichTextLabel>("Camera2D/movement");
		zoomfac = GetNode<RichTextLabel>("Camera2D/zoomfactor");
		isinair = GetNode<RichTextLabel>("Camera2D/isinair");
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

	public override void _PhysicsProcess(float delta)
	{
		zoomfac.Text = "zoom: " + cam.Zoom.x;
		rtx.Text = "move: " + movement.x;
		isinair.Text = "inAir: " + !IsOnFloor();

		if (movement.y <= 1200)
		{
			nowGravity *= 1.013f;
			movement.y += nowGravity;
		}

		if (IsOnCeiling())
		{
			nowGravity = baseGravity;
			movement.y = baseGravity;
		}

		if (IsOnFloor())
		{
			isJumping = false;
			nowGravity = baseGravity;
			movement.y = 0;
		}

		check_key_input();
		setAnimation();

		MoveAndSlideWithSnap(movement, new Vector2(0, 1), UP);
		movement.x = 0;
	}

	public void check_key_input()
	{

		if (Input.IsActionPressed(right))
		{
			if (isJumping)
			{
				movement.x = speed + jump_speed;
			}
			else
			{
				movement.x = speed;
			}
		}
		if (Input.IsActionPressed(left))
		{
			if (isJumping)
			{
				movement.x = -speed - jump_speed;
			}
			else
			{
				movement.x = -speed;
			}
		}
		if (Input.IsActionJustPressed(jump) && IsOnFloor())
		{
			isJumping = true;
			movement.y += jump_power * baseGravity;
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
}
