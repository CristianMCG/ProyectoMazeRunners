using Godot;
using System;

public partial class Minero : AnimatedSprite2D
{

	int LP, cntMov, coolT;
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public int GetLP()  {return LP;}
	public int GetcntMov()  {return cntMov;}
	public int GetcoolT()  {return coolT;}

	public void setLP(int x)  {LP = x;}
	public void setcntMov(int x)  {cntMov = x;}
	public void setcoolT(int x)  {coolT = x;}

}
