using Godot;
using System;

public partial class Teleporter : AnimatedSprite2D
{

	int cntMov, coolT;	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public int GetcntMov()  {return cntMov;}
	public int GetcoolT()  {return coolT;}

	public void setcntMov(int x)  {cntMov = x;}
	public void setcoolT(int x)  {coolT = x;}

}
