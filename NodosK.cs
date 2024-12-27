namespace NodosK;

using Par;

public class Nodo  {
  Pair a, b;
  int w;

  public Nodo(Pair A, Pair B, int _w)  {
    a = A;
    b = B;
    w = _w;
  }

  public static bool mayor(Nodo A, Nodo B)  {
    if(A.w > B.w)
      return true;
    return false;
  }

  public Pair GetFirst()  {
    return a;
  }

  public Pair GetSecond()  {
    return b;
  }

}