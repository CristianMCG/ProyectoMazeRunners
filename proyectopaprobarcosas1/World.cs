using Godot;
using System;
using Par;
using NodosK;
using System.Diagnostics;
using System.Threading;

public partial class World : Node2D
{
	
	Stopwatch stopwatch = new Stopwatch();

	int cntmoves = 100;
	int turno = 3;

	Gold[] coins = new Gold[31];
	int cntC1 = 0, cntC2 = 0;

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
	int mineroX = -50, mineroY = -50;
	int mineroMX = -50, mineroMY = -50;
	bool minerof = false;

	Teleporter teletrans = new Teleporter();
	int teleX = -50, teleY = -50;
	int teleMX = -50, teleMY = -50;
	bool teletransf = false;

	Trampero tramper = new Trampero();
	int tramperX = -50, tramperY = -50;
	int tramperMX = -50, tramperMY = -50;
	bool tramperf = false;

	Copion imitador = new Copion();
	int imitadorX = -50, imitadorY = -50;
	int imitadorMX = -50, imitadorMY = -50;
	bool imitadorf = false;

	Rapido rapid = new Rapido();
	int rapidX = -50, rapidY = -50;
	int rapidMX = -50, rapidMY = -50;
	bool rapidf = false;

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

		var instCopion = GD.Load<PackedScene>("res://copion.tscn");
		imitador = (Copion)(instCopion.Instantiate());
		AddChild(imitador);

		var instRapido = GD.Load<PackedScene>("res://rapido.tscn");
		rapid = (Rapido)(instRapido.Instantiate());
		AddChild(rapid);

		var instGold = GD.Load<PackedScene>("res://gold.tscn");
		for(int i = 0; i < 31; i++)  {
			coins[i] = (Gold)(instGold.Instantiate());
			AddChild(coins[i]);
		}

		for(int i = 0; i < 31; i++)  {
			int x = random.Next(60) % 45 + 3;
			int y = random.Next(60) % 45 + 3;
			if(matrixFinal[x - 1,y - 1] == 1)  {
				x--; y--;
			}
			else if(matrixFinal[x, y - 1] == 1)  
			  	y--;
			else if(matrixFinal[x + 1, y - 1] == 1)  {
				x++; y--;
			}
			else if(matrixFinal[x - 1,y] == 1)
				x--;
			else if(matrixFinal[x,y] == 1)
			  x += 0;
			else if(matrixFinal[x + 1,y] == 1)
			  x++;
			else if(matrixFinal[x - 1, y + 1] == 1)  {
				x--; y++;
			}
			else if(matrixFinal[x,y + 1] == 1)
				y++;
			else  {
				x++; y++;
			}
			matrixFinal[x,y] = 2;
		}

	}

	public override void _Process(double delta)
	{
		
		if(stopwatch.ElapsedMilliseconds > 2000)  {
			stopwatch.Stop();
			Ya.Position = new Vector2(-50,-50);
			stopwatch.Restart();
		}
		
		int cnt_muros = 0;
		int cnt_coins = 0;
		for(int i = 0; i < 51; i++)   { 
			for(int j = 0; j < 51; j++)  {  
				if(matrixFinal[i,j] == 0)  
				  muros[cnt_muros++].Position = new Vector2(307 + i * 14,4 + j * 14);
				if(matrixFinal[i,j] == 2)
				  coins[cnt_coins++].Position = new Vector2(307 + i * 14, 4 + j * 14);
			}
		}
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
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 1 && selected_tele)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 2 && selected_tramp)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 3 && selected_copion)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 4 && selected_rapido)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 0 && !selected_minero)   {
						personajes[3] = "minero";
						selected_minero = true;
						mineroMX = 49; mineroMY = 49;
						mineroX = 992; mineroY = 690;
						minero.Position = new Vector2(992,690); 
						selected2++;
					}
					else if(flePosY2 == 1 && !selected_tele)  {
						personajes[3] = "teleporter";
						selected_tele = true;
						teleMX = 49; teleMY = 49;
						teleX = 992; teleY = 690;
						teletrans.Position = new Vector2(992,690); 
						selected2++;
					}
					else if(flePosY2 == 2 && !selected_tramp)  {
						personajes[3] = "trampero";
						selected_tramp = true;
						tramperMX = 49; tramperMY = 49;
						tramperX = 992; tramperY = 690;
						tramper.Position = new Vector2(992,690); 
						selected2++;
					}
					else if(flePosY2 == 3 && !selected_copion)  {
						personajes[3] = "copion";
						selected_copion = true;
						imitadorMX = 49; imitadorMY = 49;
						imitadorX = 992; imitadorY = 690;
						imitador.Position = new Vector2(992,690); 
						selected2++;
					}
					else if(flePosY2 == 4 && !selected_rapido)  {
						personajes[3] = "rapido";
						selected_rapido = true;
						rapidMX = 49; rapidMY = 49;
						rapidX = 992; rapidY = 690;
						rapid.Position = new Vector2(992,690); 
						selected2++;
					}
				}
				if(selected2 == 0)  {
					if(flePosY2 == 0 && selected_minero)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 1 && selected_tele)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 2 && selected_tramp)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 3 && selected_copion)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 4 && selected_rapido)  {
						Ya.Position = new Vector2(1060,300);
						stopwatch.Start();
					}
					if(flePosY2 == 0 && !selected_minero)   {
						personajes[1] = "minero";
						selected_minero = true;
						mineroMX = 1; mineroMY = 49;
						mineroX = 320; mineroY = 690;
						minero.Position = new Vector2(320, 690);
						selected2++;
					}
					else if(flePosY2 == 1 && !selected_tele)  {
						personajes[1] = "teleporter";
						selected_tele = true;
						teleMX = 1; teleMY = 49;
						teleX = 320; teleY = 690;
						teletrans.Position = new Vector2(320, 690);
						selected2++;
					}
					else if(flePosY2 == 2 && !selected_tramp)  {
						personajes[1] = "trampero";
						selected_tramp = true;
						tramperMX = 1; tramperMY = 49;
						tramperX = 320; tramperY = 690;
						tramper.Position = new Vector2(320, 690);
						selected2++;
					}
					else if(flePosY2 == 3 && !selected_copion)  {
						personajes[1] = "copion";
						selected_copion = true;
						imitadorMX = 1; imitadorMY = 49;
						imitadorX = 320; imitadorY = 690;
						imitador.Position = new Vector2(320, 690);
						selected2++;
					}
					else if(flePosY2 == 4 && !selected_rapido)  {
						personajes[1] = "rapido";
						selected_rapido = true;
						rapidMX = 1; rapidMY = 49;
						rapidX = 320; rapidY = 690;
						rapid.Position = new Vector2(320, 690);
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
						mineroMX = 49; mineroMY = 1;
						mineroX = 992; mineroY = 18;
						minero.Position = new Vector2(992,18);
						selected1++;
					}
					else if(flePosY1 == 1 && !selected_tele)  {
						personajes[2] = "teleporter";
						selected_tele = true;
						teleMX = 49; teleMY = 1;
						teleX = 992; teleY = 18;
						teletrans.Position = new Vector2(992,18);
						selected1++;
					}
					else if(flePosY1 == 2 && !selected_tramp)  {
						personajes[2] = "trampero";
						selected_tramp = true;
						tramperMX = 49; tramperMY = 1;
						tramperX = 992; tramperY = 18;
						tramper.Position = new Vector2(992,18);
						selected1++;
					}
					else if(flePosY1 == 3 && !selected_copion)  {
						personajes[2] = "copion";
						selected_copion = true;
						imitadorMX = 49; imitadorMY = 1;
						imitadorX = 992; imitadorY = 18;
						imitador.Position = new Vector2(992,18);
						selected1++;
					}
					else if(flePosY1 == 4 && !selected_rapido)  {
						personajes[2] = "rapido";
						selected_rapido = true;
						rapidMX = 49; rapidMY = 1;
						rapidX = 992; rapidY = 18;
						rapid.Position = new Vector2(992,18);
						selected1++;
					}
				}
				if(selected1 == 0)  {
					if(flePosY1 == 0)  {
						personajes[0] = "minero";
						selected_minero = true;
						mineroX = 320; mineroY = 18;
						mineroMX = 1; mineroMY = 1;
						minero.Position = new Vector2(320,18);
						selected1++;
					}
					else if(flePosY1 == 1)  {
						personajes[0] = "teleporter";
						selected_tele = true;
						teleX = 320; teleY = 18;
						teletrans.Position = new Vector2(320,18);
						teleMX = 1; teleMY = 1;
						selected1++;
					}
					else if(flePosY1 == 2)  {
						personajes[0] = "trampero";
						selected_tramp = true;
						tramperX = 320; tramperY = 18;
						tramperMX = 1; tramperMY = 1;
						tramper.Position = new Vector2(320,18);
						selected1++;
					}
					else if(flePosY1 == 3)  {
						personajes[0] = "copion";
						selected_copion = true;
						imitadorX = 320; imitadorY = 18;
						imitadorMX = 1; imitadorMY = 1;
						imitador.Position = new Vector2(320,18);
						selected1++;
					}
					else  {
						personajes[0] = "rapido";
						selected_rapido = true;
						rapidX = 320; rapidY = 18;
						rapidMX = 1; rapidMY = 1;
						rapid.Position = new Vector2(320,18);
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

		if(selected && !minerof && !teletransf && !tramperf && !imitadorf && !rapidf)  
			cntmoves = 0;

		if(Input.IsActionPressed("Abajo") && minerof)  {
			if(matrixFinal[mineroMX,mineroMY + 1] != 0)  {
				mineroY += 14;
				mineroMY++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Arriba") && minerof)  {
			if(matrixFinal[mineroMX,mineroMY - 1] != 0)  {
				mineroY -= 14;
				mineroMY--;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Derecha") && minerof)  {
			if(matrixFinal[mineroMX + 1,mineroMY] != 0)  {
				mineroX += 14;
				mineroMX++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Izquierda") && minerof)    {
			if(matrixFinal[mineroMX - 1,mineroMY] != 0)  {
				mineroX -= 14;
				mineroMX--;
				cntmoves--;
			}
		}
		if(minerof)  
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
		if(minerof && matrixFinal[mineroMX,mineroMY] == 2)  {
			matrixFinal[mineroMX,mineroMY] = 1;
			for(int i = 0; i < 31; i++)  {
				if((coins[i].Position.X - 307) / 14 == mineroMX && (coins[i].Position.Y - 4) / 14 == mineroMY)  {
					if(personajes[0] == "minero" || personajes[2] == "minero")
						cntC1++;
					else
						cntC2++;
					coins[i].Position = new Vector2(-60,-60);
				}
			}
		}

		if(Input.IsActionPressed("Abajo") && teletransf)  {
			if(matrixFinal[teleMX,teleMY + 1] != 0)  {
				teleY += 14;
				teleMY++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Arriba") && teletransf)  {
			if(matrixFinal[teleMX,teleMY - 1] != 0)  {
				teleY -= 14;
				teleMY--;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Derecha") && teletransf)  {
			if(matrixFinal[teleMX + 1,teleMY] != 0)  {
				teleX += 14;
				teleMX++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Izquierda") && teletransf)    {
			if(matrixFinal[teleMX - 1,teleMY] != 0)  {
				teleX -= 14;
				teleMX--;
				cntmoves--;
			}
		}
		if(teletransf)  
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
			if(teletransf)				
				teletrans.Position = new Vector2(teleX,teleY);
		}
		if(teletransf && matrixFinal[teleMX,teleMY] == 2)  {
			matrixFinal[teleMX,teleMY] = 1;
			for(int i = 0; i < 31; i++)  {
				if((coins[i].Position.X - 307) / 14 == teleMX && (coins[i].Position.Y - 4) / 14 == teleMY)  {
					if(personajes[0] == "teleporter" || personajes[2] == "teleporter")
						cntC1++;
					else
						cntC2++;
					coins[i].Position = new Vector2(-60,-60);
				}
			}
		}

		if(Input.IsActionPressed("Abajo") && tramperf)  {
			if(matrixFinal[tramperMX,tramperMY + 1] != 0)  {
				tramperY += 14;
				tramperMY++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Arriba") && tramperf)  {
			if(matrixFinal[tramperMX,tramperMY - 1] != 0)  {
				tramperY -= 14;
				tramperMY--;
				cntmoves--;				
			}
		}
		if(Input.IsActionPressed("Derecha") && tramperf)  {
			if(matrixFinal[tramperMX + 1,tramperMY] != 0)  {
				tramperX += 14;
				tramperMX++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Izquierda") && tramperf)    {
			if(matrixFinal[tramperMX - 1,tramperMY] != 0)  {
				tramperX -= 14;
				tramperMX--;
				cntmoves--;
			}
		}
		if(tramperf)
		  tramper.Position = new Vector2(tramperX,tramperY);
		if(Input.IsActionPressed("Habilidad") && tramperf)  {

		}
		if(tramperf && matrixFinal[tramperMX,tramperMY] == 2)  {
			matrixFinal[tramperMX,tramperMY] = 1;
			for(int i = 0; i < 31; i++)  {
				if((coins[i].Position.X - 307) / 14 == tramperMX && (coins[i].Position.Y - 4) / 14 == tramperMY)  {
					if(personajes[0] == "trampero" || personajes[2] == "trampero")
						cntC1++;
					else
						cntC2++;
					coins[i].Position = new Vector2(-60,-60);
				}
			}
		}

		if(Input.IsActionPressed("Abajo") && imitadorf)  {
			if(matrixFinal[imitadorMX,imitadorMY + 1] != 0)  {
				imitadorY += 14;
				imitadorMY++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Arriba") && imitadorf)  {
			if(matrixFinal[imitadorMX,imitadorMY - 1] != 0)  {
				imitadorY -= 14;
				imitadorMY--;
				cntmoves--;				
			}
		}
		if(Input.IsActionPressed("Derecha") && imitadorf)  {
			if(matrixFinal[imitadorMX + 1,imitadorMY] != 0)  {
				imitadorX += 14;
				imitadorMX++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Izquierda") && imitadorf)    {
			if(matrixFinal[imitadorMX - 1,imitadorMY] != 0)  {
				imitadorX -= 14;
				imitadorMX--;
				cntmoves--;
			}
		}
		if(imitadorf)
		  imitador.Position = new Vector2(imitadorX,imitadorY);
		if(Input.IsActionPressed("Habilidad") && imitadorf)  {

		}
		if(imitadorf && matrixFinal[imitadorMX,imitadorMY] == 2)  {
			matrixFinal[imitadorMX,imitadorMY] = 1;
			for(int i = 0; i < 31; i++)  {
				if((coins[i].Position.X - 307) / 14 == imitadorMX && (coins[i].Position.Y - 4) / 14 == imitadorMY)  {
					if(personajes[0] == "copion" || personajes[2] == "copion")
						cntC1++;
					else
						cntC2++;
					coins[i].Position = new Vector2(-60,-60);
				}
			}
		}

		if(Input.IsActionPressed("Abajo") && rapidf)  {
			if(matrixFinal[rapidMX,rapidMY + 1] != 0)  {
				rapidY += 14;
				rapidMY++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Arriba") && rapidf)  {
			if(matrixFinal[rapidMX,rapidMY - 1] != 0)  {
				rapidY -= 14;
				rapidMY--;
				cntmoves--;				
			}
		}
		if(Input.IsActionPressed("Derecha") && rapidf)  {
			if(matrixFinal[rapidMX + 1,rapidMY] != 0)  {
				rapidX += 14;
				rapidMX++;
				cntmoves--;
			}
		}
		if(Input.IsActionPressed("Izquierda") && rapidf)    {
			if(matrixFinal[rapidMX - 1,rapidMY] != 0)  {
				rapidX -= 14;
				rapidMX--;
				cntmoves--;
			}
		}
		if(rapidf)
		  rapid.Position = new Vector2(rapidX,rapidY);
		if(Input.IsActionPressed("Habilidad") && rapidf)  
			cntmoves *= 2;
		if(rapidf && matrixFinal[rapidMX,rapidMY] == 2)  {
			matrixFinal[rapidMX,rapidMY] = 1;
			for(int i = 0; i < 31; i++)  {
				if((coins[i].Position.X - 307) / 14 == rapidMX && (coins[i].Position.Y - 4) / 14 == rapidMY)  {
					if(personajes[0] == "rapido" || personajes[2] == "rapido")
						cntC1++;
					else
						cntC2++;
					coins[i].Position = new Vector2(-60,-60);
				}
			}
		}

		if(cntmoves <= 0)   {
			turno = (turno + 1) % 4;
			minerof = false; teletransf = false; rapidf = false; imitadorf = false; tramperf = false;
			if(personajes[turno] == "minero")  {
				cntmoves = 20;
				minerof = true;
			}
			else if(personajes[turno] == "teleporter")  {
				cntmoves = 40;
				teletransf = true;
			}
			else if(personajes[turno] == "trampero")  {
				cntmoves = 50;
				tramperf = true;
			}
			else if(personajes[turno] == "copion")  {
				cntmoves = 20;
				imitadorf = true;
			}
			else  {
				cntmoves = 40;
				rapidf = true;
			}
		}

		Thread.Sleep(40);
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
