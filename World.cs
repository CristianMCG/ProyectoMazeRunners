using Godot;
using System;
using Par;
using NodosK;
using System.Diagnostics;

public partial class World : Node2D
{
	
	Stopwatch stopwatch = new Stopwatch();

	bool selected = false;
	Label[] seleccionar = new Label[2];
	Flecha[] flechas = new Flecha[2];
	YaSeleccionado Ya = new YaSeleccionado();
	int fleX1 = 60, fleY1 = 58;
	int flePosY1 = 0, flePosY2 = 0; 
	int fleX2 = 1100, fleY2 = 58;
	int selected1 = 0, selected2 = 0;
	bool selected_minero = false;
	bool selected_tele = false;
	bool selected_tramp = false;
	bool selected_copion = false;
	bool selected_rapido = false;

	string[] personajes = new string[4];
	
	Muro[] muros = new Muro[3025];
	Pair[,] padre = new Pair[1000,1000];
	Nodo[] kruskall = new Nodo[6000];
	Nodo[] MatrixMin = new Nodo[6000];
	int[,] matrixFinal = new int[53,53];

	Minero minero = new Minero();
	int mineroX = 320, mineroY = 18;
	int mineroMX = 1, mineroMY = 1;
	bool minerof = false;

	Teleporter teletrans = new Teleporter();
	int teleX = 320, teleY = 18;
	int teleMX = 1, teleMY = 1;
	bool teletransf = false;

	Trampero tramper = new Trampero();
	int tramperX = 320, tramperY = 18;
	int tramperMX = 1, tramperMY = 1;
	bool tramperf = false;

	Random random = new Random();
	int itK = 0;
	int itM = 0;

	public override void _Ready()
	{
		
		for(int i = 0; i < 1000; i++)  
			for(int j = 0; j < 1000; j++)
			  padre[i,j] = new Pair(i,j);

		for(int i = 1; i <= 25; i++)  {
			for(int j = 1; j <= 25; j++)  {
				Pair p1 = new Pair(i,j);
				Pair p2 = new Pair(i + 1,j);
				Pair p3 = new Pair(i,j + 1);
				if(i + 1 <= 25) 
					kruskall[itK++] = new Nodo(p1, p2, random.Next(500));
				if(j + 1 <= 25)  
					kruskall[itK++] = new Nodo(p1, p3, random.Next(500));
			}
		}

		for(int i = 0; i < itK; i++)  {
			for(int j = 0; j < itK - 1; j++)  {
				if(Nodo.mayor(kruskall[j], kruskall[j + 1]))  {
					Nodo A = kruskall[j];
					kruskall[j] = kruskall[j + 1];
					kruskall[j + 1] = A;
				}
			}
		}

		for(int i = 0; i < itK; i++)  {
			Pair A = kruskall[i].GetFirst(), B = kruskall[i].GetSecond();
			if(conectados(A,B))  
			  continue;
			MatrixMin[itM++] = new Nodo(A,B,1);
			UNION(A,B);
		}

		for(int i = 0; i < itM; i++)  {
			Pair A = MatrixMin[i].GetFirst(), B = MatrixMin[i].GetSecond();
			int a = A.GetX(), b = A.GetY(), c = B.GetX(), d = B.GetY();
			matrixFinal[2 * a - 1,2 * b - 1] = 1;
			matrixFinal[2 * c - 1,2 * d - 1] = 1;
			if(a == c)  
				matrixFinal[2 * a - 1,((2 * b - 1) + (2 * d - 1)) / 2] = 1;
			else   
				matrixFinal[((2 * a - 1) + (2 * c - 1)) / 2,2 * b - 1] = 1;
		}
		
		var muro = GD.Load<PackedScene>("res://muro.tscn");
		for(int i = 0; i < 3025; i++)    {
			muros[i] = (Muro)(muro.Instantiate());
			AddChild(muros[i]);
		}

		var instLabel = GD.Load<PackedScene>("res://label.tscn");
		for(int i = 0; i < 2; i++)  {
			seleccionar[i] = (Label)(instLabel.Instantiate());
			AddChild(seleccionar[i]);
		}

		var instYa = GD.Load<PackedScene>("res://ya_seleccionado.tscn");
		Ya = (YaSeleccionado)(instYa.Instantiate());
		AddChild(Ya);
		Ya.Position = new Vector2(-50,-50);

		var instFlecha = GD.Load<PackedScene>("res://flecha.tscn");
		for(int i = 0; i < 2; i++)  {
			flechas[i] = (Flecha)(instFlecha.Instantiate());
			AddChild(flechas[i]);
		}
		
		var instMinero = GD.Load<PackedScene>("res://minero.tscn");
		minero = (Minero)(instMinero.Instantiate());
		AddChild(minero);

		var instTele = GD.Load<PackedScene>("res://teleporter.tscn");
		teletrans = (Teleporter)(instTele.Instantiate());
		AddChild(teletrans);

		var instTramper = GD.Load<PackedScene>("res://trampero.tscn");
		tramper = (Trampero)(instTramper.Instantiate());
		AddChild(tramper);

	}

	public override void _Process(double delta)
	{
		
		if(stopwatch.ElapsedMilliseconds > 2000)  {
			stopwatch.Stop();
			Ya.Position = new Vector2(-50,-50);
			stopwatch.Restart();
		}
		
		int cnt_muros = 0;
		for(int i = 0; i < 51; i++)  
			for(int j = 0; j < 51; j++)  
				if(matrixFinal[i,j] == 0)  
				  muros[cnt_muros++].Position = new Vector2(307 + i * 14,4 + j * 14);
		for( ;cnt_muros < 3025; cnt_muros++)  
			muros[cnt_muros].Position = new Vector2(-50,-50);
		
		if(!selected)    {
			seleccionar[0].Position = new Vector2(50,20);
			flechas[0].Position = new Vector2(fleX1,fleY1);
			seleccionar[1].Position = new Vector2(-300,-300);
			flechas[1].Position = new Vector2(-300,-300); 
			if(selected1 == 2)  {
				seleccionar[1].Position = new Vector2(1100,20);
				flechas[1].Position = new Vector2(fleX2,fleY2); 
			}
			if(Input.IsActionPressed("Abajo") && selected1 == 2 && flePosY2 + 1 <= 4)  {
				fleY2 += 26;
				flePosY2++;
				flechas[1].Position = new Vector2(fleX2,fleY2);
			}
			if(Input.IsActionPressed("Arriba") && selected1 == 2 && flePosY2 - 1 >= 0)  {
				fleY2 -= 26;
				flePosY2--;
				flechas[1].Position = new Vector2(fleX2,fleY2);
			}
			if(Input.IsActionPressed("Seleccionar") && selected1 == 2 && selected2 < 2)  {
				if(selected2 == 1)  {
					if(flePosY2 == 0 && selected_minero)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 1 && selected_tele)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 2 && selected_tramp)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 3 && selected_copion)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 4 && selected_rapido)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 0 && !selected_minero)   {
						personajes[3] = "minero";
						selected_minero = true;
						selected2++;
					}
					else if(flePosY2 == 1 && !selected_tele)  {
						personajes[3] = "teleporter";
						selected_tele = true;
						selected2++;
					}
					else if(flePosY2 == 2 && !selected_tramp)  {
						personajes[3] = "trampero";
						selected_tramp = true;
						selected2++;
					}
					else if(flePosY2 == 3 && !selected_copion)  {
						personajes[3] = "copion";
						selected_copion = true;
						selected2++;
					}
					else if(flePosY2 == 4 && !selected_rapido)  {
						personajes[3] = "rapido";
						selected_rapido = true;
						selected2++;
					}
				}
				if(selected2 == 0)  {
					if(flePosY2 == 0 && selected_minero)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 1 && selected_tele)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 2 && selected_tramp)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 3 && selected_copion)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 4 && selected_rapido)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY2 == 0 && !selected_minero)   {
						personajes[1] = "minero";
						selected_minero = true;
						selected2++;
					}
					else if(flePosY2 == 1 && !selected_tele)  {
						personajes[1] = "teleporter";
						selected_tele = true;
						selected2++;
					}
					else if(flePosY2 == 2 && !selected_tramp)  {
						personajes[1] = "trampero";
						selected_tramp = true;
						selected2++;
					}
					else if(flePosY2 == 3 && !selected_copion)  {
						personajes[1] = "copion";
						selected_copion = true;
						selected2++;
					}
					else if(flePosY2 == 4 && !selected_rapido)  {
						personajes[1] = "rapido";
						selected_rapido = true;
						selected2++;
					}
				}
			}
			if(Input.IsActionPressed("Abajo") && selected1 < 2 && flePosY1 + 1 <= 4)  {
				fleY1 += 26;
				flePosY1++;
				flechas[0].Position = new Vector2(fleX1,fleY1);
			}
			if(Input.IsActionPressed("Arriba") && selected1 < 2 && flePosY1 - 1 >= 0)  {
				fleY1 -= 26;
				flePosY1--;
				flechas[0].Position = new Vector2(fleX1,fleY1);
			}
			if(Input.IsActionPressed("Seleccionar") && selected1 < 2)  {			
				if(selected1 == 1)  {
					if(flePosY1 == 0 && selected_minero)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY1 == 1 && selected_tele)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY1 == 2 && selected_tramp)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY1 == 3 && selected_copion)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY1 == 4 && selected_rapido)  {
						Ya.Position = new Vector2(20,300);
						stopwatch.Start();
					}
					if(flePosY1 == 0 && !selected_minero)  {
						personajes[2] = "minero";
						selected_minero = true;
						selected1++;
					}
					else if(flePosY1 == 1 && !selected_tele)  {
						personajes[2] = "teleporter";
						selected_tele = true;
						selected1++;
					}
					else if(flePosY1 == 2 && !selected_tramp)  {
						personajes[2] = "trampero";
						selected_tramp = true;
						selected1++;
					}
					else if(flePosY1 == 3 && !selected_copion)  {
						personajes[2] = "copion";
						selected_copion = true;
						selected1++;
					}
					else if(flePosY1 == 4 && !selected_rapido)  {
						personajes[2] = "rapido";
						selected_rapido = true;
						selected1++;
					}
				}
				if(selected1 == 0)  {
					if(flePosY1 == 0)  {
						personajes[0] = "minero";
						selected_minero = true;
						selected1++;
					}
					else if(flePosY1 == 1)  {
						personajes[0] = "teleporter";
						selected_tele = true;
						selected1++;
					}
					else if(flePosY1 == 2)  {
						personajes[0] = "trampero";
						selected_tramp = true;
						selected1++;
					}
					else if(flePosY1 == 3)  {
						personajes[0] = "copion";
						selected_copion = true;
						selected1++;
					}
					else  {
						personajes[0] = "rapido";
						selected_rapido = true;
						selected1++;
					}
				}
			}
			if(selected1 == 2)  {
				seleccionar[0].Position = new Vector2(-100,-100);
				flechas[0].Position = new Vector2(-100,-100);
			}
			if(selected2 == 2)  {
				seleccionar[1].Position = new Vector2(-100,-100);
				flechas[1].Position = new Vector2(-100,-100);
				selected = true;
			}
		}

		if(Input.IsActionPressed("Abajo") && minerof)  {
			if(matrixFinal[mineroMX,mineroMY + 1] == 1)  {
				mineroY += 14;
				mineroMY++;
			}
		}
		if(Input.IsActionPressed("Arriba") && minerof)  {
			if(matrixFinal[mineroMX,mineroMY - 1] == 1)  {
				mineroY -= 14;
				mineroMY--;
			}
		}
		if(Input.IsActionPressed("Derecha") && minerof)  {
			if(matrixFinal[mineroMX + 1,mineroMY] == 1)  {
				mineroX += 14;
				mineroMX++;
			}
		}
		if(Input.IsActionPressed("Izquierda") && minerof)    {
			if(matrixFinal[mineroMX - 1,mineroMY] == 1)  {
				mineroX -= 14;
				mineroMX--;
			}
		}
		minero.Position = new Vector2(mineroX,mineroY);
		if(Input.IsActionPressed("Habilidad") && minerof)    {
			if(matrixFinal[mineroMX,mineroMY - 1] == 0 && mineroMY - 1 > 0)
			  matrixFinal[mineroMX,mineroMY - 1] = 1;
			if(matrixFinal[mineroMX,mineroMY + 1] == 0 && mineroMY + 1 < 50)
			  matrixFinal[mineroMX,mineroMY + 1] = 1;
			if(matrixFinal[mineroMX - 1,mineroMY] == 0 && mineroMX - 1 > 0)
			  matrixFinal[mineroMX - 1,mineroMY] = 1;
			if(matrixFinal[mineroMX + 1,mineroMY] == 0 && mineroMX + 1 < 50)
			  matrixFinal[mineroMX + 1,mineroMY] = 1;
		}

		if(Input.IsActionPressed("Abajo") && teletransf)  {
			if(matrixFinal[teleMX,teleMY + 1] == 1)  {
				teleY += 14;
				teleMY++;
			}
		}
		if(Input.IsActionPressed("Arriba") && teletransf)  {
			if(matrixFinal[teleMX,teleMY - 1] == 1)  {
				teleY -= 14;
				teleMY--;
			}
		}
		if(Input.IsActionPressed("Derecha") && teletransf)  {
			if(matrixFinal[teleMX + 1,teleMY] == 1)  {
				teleX += 14;
				teleMX++;
			}
		}
		if(Input.IsActionPressed("Izquierda") && teletransf)    {
			if(matrixFinal[teleMX - 1,teleMY] == 1)  {
				teleX -= 14;
				teleMX--;
			}
		}
		teletrans.Position = new Vector2(teleX,teleY);
		if(Input.IsActionPressed("Habilidad") && teletransf)  {
			int x = random.Next(60) % 49 + 1;
			int y = random.Next(60) % 49 + 1;
			if(matrixFinal[x - 1,y - 1] == 1) { 
			  teleMX = x - 1; teleMY = y - 1;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			else if(matrixFinal[x - 1,y] == 1)  { 
			  teleMX = x - 1; teleMY = y;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}  
			else if(matrixFinal[x - 1,y + 1] == 1)  { 
			  teleMX = x - 1; teleMY = y + 1;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			else if(matrixFinal[x,y - 1] == 1)  { 
			  teleMX = x; teleMY = y - 1;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			else if(matrixFinal[x,y] == 1)  { 
			  teleMX = x; teleMY = y;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			else if(matrixFinal[x,y + 1] == 1)  { 
			  teleMX = x; teleMY = y + 1;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			else if(matrixFinal[x + 1,y - 1] == 1)  { 
			  teleMX = x + 1; teleMY = y - 1;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			else if(matrixFinal[x + 1,y] == 1)  { 
			  teleMX = x + 1; teleMY = y;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			else   { 
			  teleMX = x + 1; teleMY = y + 1;
			  teleX = 306 + teleMX * 14; teleY = 4 + teleMY * 14;
			}
			teletrans.Position = new Vector2(teleMX,teleMY);
		}
	}


	public bool conectados(Pair A, Pair B)  {
		if(Pair.igual(Find(A), Find(B)))
			return true;
		return false;
	}

	public Pair Find(Pair A)  {
		int a = A.GetX(), b = A.GetY();
		Pair p = padre[a,b];
		if(Pair.igual(A, p))
		  return p;
		padre[a,b] = Find(p);
		return padre[a,b];
	}

	public void UNION(Pair A, Pair B)  {
		Pair p1 = Find(A), p2 = Find(B);
		int a = p1.GetX(), b = p1.GetY();
		padre[a,b] = p2;
	}

}


// 51x51 dim laberinto
// 14 y 14 correr cajas
//escala caja x = 1 y = 1
//14 y 14 correr minero
